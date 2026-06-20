import { HttpClient } from '@angular/common/http';
import { Inject, Injectable, Optional } from '@angular/core';
import { Observable, map } from 'rxjs';
import { API_BASE_URL, BackupAdminClient, DatabaseRestoreSummary } from './NaqlahClient';

export type { DatabaseRestoreSummary };

export interface ExportParams {
  module: string;
  from?: string | null;
  to?: string | null;
}

export interface ExportBlobResult {
  blob: Blob;
  fileName: string;
}

export interface DatabaseOperationStatus {
  jobId: string;
  operation: string;
  phase: 'running' | 'completed' | 'failed' | string;
  progressPercent: number;
  currentItem: string;
  completedItems: number;
  totalItems: number;
  summary?: DatabaseRestoreSummary;
  errorMessage?: string;
  downloadFileName?: string;
}

function normalizeRestoreSummary(raw: unknown): DatabaseRestoreSummary {
  const data = (raw ?? {}) as Record<string, unknown>;
  return DatabaseRestoreSummary.fromJS({
    totalTables: data['totalTables'] ?? data['TotalTables'] ?? 0,
    tablesProcessed: data['tablesProcessed'] ?? data['TablesProcessed'] ?? 0,
    tablesChanged: data['tablesChanged'] ?? data['TablesChanged'] ?? 0,
    rowsInserted: data['rowsInserted'] ?? data['RowsInserted'] ?? 0,
    rowsSkipped: data['rowsSkipped'] ?? data['RowsSkipped'] ?? 0,
    batchesExecuted: data['batchesExecuted'] ?? data['BatchesExecuted'] ?? 0,
  });
}

function normalizeOperationStatus(raw: Record<string, unknown>): DatabaseOperationStatus {
  const summaryRaw = raw['summary'] ?? raw['Summary'];
  return {
    jobId: String(raw['jobId'] ?? raw['JobId'] ?? ''),
    operation: String(raw['operation'] ?? raw['Operation'] ?? ''),
    phase: String(raw['phase'] ?? raw['Phase'] ?? 'running'),
    progressPercent: Number(raw['progressPercent'] ?? raw['ProgressPercent'] ?? 0),
    currentItem: String(raw['currentItem'] ?? raw['CurrentItem'] ?? ''),
    completedItems: Number(raw['completedItems'] ?? raw['CompletedItems'] ?? 0),
    totalItems: Number(raw['totalItems'] ?? raw['TotalItems'] ?? 0),
    summary: summaryRaw ? normalizeRestoreSummary(summaryRaw) : undefined,
    errorMessage: (raw['errorMessage'] ?? raw['ErrorMessage']) as string | undefined,
    downloadFileName: (raw['downloadFileName'] ?? raw['DownloadFileName']) as string | undefined,
  };
}

@Injectable({
  providedIn: 'root',
})
export class BackupService {
  constructor(
    private backupAdminClient: BackupAdminClient,
    private http: HttpClient,
    @Optional() @Inject(API_BASE_URL) private apiBaseUrl: string
  ) {}

  private get baseUrl(): string {
    return (this.apiBaseUrl ?? '').replace(/\/$/, '');
  }

  exportModule(params: ExportParams): Observable<ExportBlobResult> {
    const from = params.from ? new Date(params.from) : null;
    const to = params.to ? new Date(params.to) : null;

    return this.backupAdminClient.export(params.module, from, to).pipe(
      map((fileResponse) => ({
        blob: fileResponse.data,
        fileName: fileResponse.fileName ?? `${params.module}_export.xlsx`,
      }))
    );
  }

  exportFullDatabaseDirect(): Observable<ExportBlobResult> {
    return this.backupAdminClient.exportFullDatabase().pipe(
      map((fileResponse) => ({
        blob: fileResponse.data,
        fileName: fileResponse.fileName ?? `Naqlah_FullBackup_${new Date().toISOString().slice(0, 10)}.sql`,
      }))
    );
  }

  restoreFullDatabaseDirect(file: File): Observable<DatabaseRestoreSummary> {
    return this.backupAdminClient.restoreFullDatabase({ data: file, fileName: file.name }).pipe(
      map((summary) => normalizeRestoreSummary(summary))
    );
  }

  startFullDatabaseBackup(): Observable<string> {
    return this.http
      .post<Record<string, string>>(`${this.baseUrl}/api/BackupAdmin/StartFullDatabaseBackup`, {})
      .pipe(map((res) => res['jobId'] ?? res['JobId'] ?? ''));
  }

  startFullDatabaseRestore(file: File): Observable<string> {
    const formData = new FormData();
    formData.append('file', file, file.name);

    return this.http
      .post<Record<string, string>>(`${this.baseUrl}/api/BackupAdmin/StartFullDatabaseRestore`, formData)
      .pipe(map((res) => res['jobId'] ?? res['JobId'] ?? ''));
  }

  trackOperation(jobId: string): Observable<DatabaseOperationStatus> {
    return new Observable<DatabaseOperationStatus>((subscriber) => {
      const poll = () => {
        this.http
          .get<Record<string, unknown>>(`${this.baseUrl}/api/BackupAdmin/OperationStatus/${jobId}`)
          .subscribe({
            next: (raw) => {
              const status = normalizeOperationStatus(raw);
              subscriber.next(status);
              if (status.phase === 'completed' || status.phase === 'failed') {
                subscriber.complete();
                return;
              }
              timerId = window.setTimeout(poll, 600);
            },
            error: (err) => subscriber.error(err),
          });
      };

      let timerId = window.setTimeout(poll, 0);
      return () => window.clearTimeout(timerId);
    });
  }

  downloadBackupJob(jobId: string): Observable<ExportBlobResult> {
    return this.http
      .get(`${this.baseUrl}/api/BackupAdmin/OperationDownload/${jobId}`, {
        observe: 'response',
        responseType: 'blob',
      })
      .pipe(
        map((response) => {
          const disposition = response.headers.get('content-disposition') ?? '';
          const match = /filename\*?=(?:UTF-8''|")?([^";]+)/i.exec(disposition);
          const fileName = match?.[1]?.trim() ?? 'Naqlah_FullBackup.sql';
          return {
            blob: response.body ?? new Blob(),
            fileName: decodeURIComponent(fileName),
          };
        })
      );
  }

  triggerDownload(blob: Blob, fileName: string): void {
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    a.click();
    URL.revokeObjectURL(url);
  }
}
