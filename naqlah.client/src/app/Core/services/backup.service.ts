import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { BackupAdminClient } from './NaqlahClient';

export interface ExportParams {
  module: string;
  from?: string | null;
  to?: string | null;
}

export interface ExportBlobResult {
  blob: Blob;
  fileName: string;
}

/**
 * Uses the generated BackupAdminClient to call the backup/export API.
 * Auth token is added by the auth interceptor.
 */
@Injectable({
  providedIn: 'root',
})
export class BackupService {
  constructor(private backupAdminClient: BackupAdminClient) {}

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

  triggerDownload(blob: Blob, fileName: string): void {
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    a.click();
    URL.revokeObjectURL(url);
  }
}
