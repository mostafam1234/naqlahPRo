import { CommonModule } from '@angular/common';
import { Component, OnDestroy } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import {
  BackupService,
  DatabaseOperationStatus,
} from '../../../Core/services/backup.service';
import { DatabaseRestoreSummary } from '../../../Core/services/NaqlahClient';
import { ToasterService } from '../../../Core/services/toaster.service';
import { getDefaultSearchDateRange } from '../../../shared/utils/date-filter.helpers';
import { SubSink } from 'subsink';

export interface BackupModuleOption {
  key: string;
  labelKey: string;
  group: string;
}

const BACKUP_MODULES: BackupModuleOption[] = [
  { key: 'Orders', labelKey: 'ADMIN.BACKUP.MODULES.ORDERS', group: 'ADMIN.BACKUP.GROUPS.ORDERS' },
  { key: 'OrderPackages', labelKey: 'ADMIN.BACKUP.MODULES.ORDER_PACKAGES', group: 'ADMIN.BACKUP.GROUPS.ORDERS' },
  { key: 'Vehicles', labelKey: 'ADMIN.BACKUP.MODULES.VEHICLES', group: 'ADMIN.BACKUP.GROUPS.SETTINGS' },
  { key: 'SystemUsers', labelKey: 'ADMIN.BACKUP.MODULES.SYSTEM_USERS', group: 'ADMIN.BACKUP.GROUPS.USERS' },
  { key: 'DeliveryMen', labelKey: 'ADMIN.BACKUP.MODULES.DELIVERY_MEN', group: 'ADMIN.BACKUP.GROUPS.USERS' },
  { key: 'MainCategories', labelKey: 'ADMIN.BACKUP.MODULES.MAIN_CATEGORIES', group: 'ADMIN.BACKUP.GROUPS.SETTINGS' },
  { key: 'WalletTransactions', labelKey: 'ADMIN.BACKUP.MODULES.WALLET_TRANSACTIONS', group: 'ADMIN.BACKUP.GROUPS.WALLET' },
  { key: 'Complains', labelKey: 'ADMIN.BACKUP.MODULES.COMPLAINS', group: 'ADMIN.BACKUP.GROUPS.TECH_SUPPORT' },
  { key: 'Suggestions', labelKey: 'ADMIN.BACKUP.MODULES.SUGGESTIONS', group: 'ADMIN.BACKUP.GROUPS.TECH_SUPPORT' },
  { key: 'Notifications', labelKey: 'ADMIN.BACKUP.MODULES.NOTIFICATIONS', group: 'ADMIN.BACKUP.GROUPS.SYSTEM' },
  { key: 'Regions', labelKey: 'ADMIN.BACKUP.MODULES.REGIONS', group: 'ADMIN.BACKUP.GROUPS.AREAS' },
  { key: 'Cities', labelKey: 'ADMIN.BACKUP.MODULES.CITIES', group: 'ADMIN.BACKUP.GROUPS.AREAS' },
  { key: 'Neighborhoods', labelKey: 'ADMIN.BACKUP.MODULES.NEIGHBORHOODS', group: 'ADMIN.BACKUP.GROUPS.AREAS' },
  { key: 'AssistantWorks', labelKey: 'ADMIN.BACKUP.MODULES.ASSISTANT_WORKS', group: 'ADMIN.BACKUP.GROUPS.SETTINGS' },
];

@Component({
  selector: 'app-backup',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule, PageHeaderComponent],
  templateUrl: './backup.component.html',
  styleUrl: './backup.component.css',
})
export class BackupComponent implements OnDestroy {
  activeTab: 'database' | 'excel' = 'database';

  modules = BACKUP_MODULES;
  groupedModules: { groupKey: string; items: BackupModuleOption[] }[];
  selectedModules = new Set<string>();
  fromDate: string | null = getDefaultSearchDateRange().fromDate;
  toDate: string | null = getDefaultSearchDateRange().toDate;

  exporting = false;
  progressMessage = '';
  errorMessage = '';

  fullBackupLoading = false;
  restoreLoading = false;
  selectedRestoreFile: File | null = null;
  restoreSummary: DatabaseRestoreSummary | null = null;
  restoreError = '';

  operationProgress = 0;
  operationCurrentItem = '';
  operationPhase: 'idle' | 'running' | 'completed' | 'failed' = 'idle';

  private sub = new SubSink();
  private progressTimer: ReturnType<typeof setInterval> | null = null;

  constructor(
    private backupService: BackupService,
    private toasterService: ToasterService,
    private translate: TranslateService
  ) {
    const byGroup = new Map<string, BackupModuleOption[]>();
    BACKUP_MODULES.forEach((m) => {
      const arr = byGroup.get(m.group) ?? [];
      arr.push(m);
      byGroup.set(m.group, arr);
    });
    this.groupedModules = Array.from(byGroup.entries()).map(([groupKey, items]) => ({ groupKey, items }));
  }

  ngOnDestroy(): void {
    this.stopSmoothProgress();
    this.sub.unsubscribe();
  }

  setTab(tab: 'database' | 'excel'): void {
    this.activeTab = tab;
  }

  toggleModule(key: string): void {
    if (this.selectedModules.has(key)) {
      this.selectedModules.delete(key);
    } else {
      this.selectedModules.add(key);
    }
  }

  selectAll(): void {
    this.modules.forEach((m) => this.selectedModules.add(m.key));
  }

  clearAll(): void {
    this.selectedModules.clear();
  }

  exportFullDatabase(): void {
    this.resetOperationState();
    this.fullBackupLoading = true;
    this.operationPhase = 'running';
    this.startSmoothProgress();

    this.sub.sink = this.backupService.exportFullDatabaseDirect().subscribe({
      next: (result) => {
        this.completeProgress();
        this.fullBackupLoading = false;
        this.backupService.triggerDownload(result.blob, result.fileName);
        this.toasterService.success(
          this.translate.instant('ADMIN.BACKUP.FULL_BACKUP_SUCCESS'),
          this.translate.instant('ADMIN.BACKUP.TITLE')
        );
      },
      error: (err) => {
        this.stopSmoothProgress();
        this.fullBackupLoading = false;
        this.operationPhase = 'failed';
        const msg = this.extractErrorMessage(err) ?? this.translate.instant('ADMIN.BACKUP.FULL_BACKUP_ERROR');
        this.restoreError = msg;
        this.toasterService.error(msg, this.translate.instant('ADMIN.BACKUP.TITLE'));
      },
    });
  }

  onRestoreFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;
    this.selectedRestoreFile = file;
    this.restoreSummary = null;
    this.restoreError = '';
  }

  restoreFullDatabase(): void {
    if (!this.selectedRestoreFile) {
      this.toasterService.warning(
        this.translate.instant('ADMIN.BACKUP.RESTORE_FILE_REQUIRED'),
        this.translate.instant('ADMIN.BACKUP.TITLE')
      );
      return;
    }

    this.resetOperationState();
    this.restoreLoading = true;
    this.restoreSummary = null;
    this.restoreError = '';
    this.operationPhase = 'running';

    this.sub.sink = this.backupService.startFullDatabaseRestore(this.selectedRestoreFile).subscribe({
      next: (jobId) => this.watchOperation(jobId, 'restore'),
      error: (err) => {
        this.restoreLoading = false;
        this.operationPhase = 'failed';
        this.restoreError = this.extractErrorMessage(err) ?? this.translate.instant('ADMIN.BACKUP.RESTORE_ERROR');
        this.toasterService.error(this.restoreError, this.translate.instant('ADMIN.BACKUP.TITLE'));
      },
    });
  }

  private watchOperation(jobId: string, mode: 'backup' | 'restore'): void {
    this.operationPhase = 'running';

    this.sub.sink = this.backupService.trackOperation(jobId).subscribe({
      next: (status) => this.applyOperationStatus(status),
      complete: () => {
        if (this.operationPhase === 'failed') {
          this.restoreLoading = false;
          this.fullBackupLoading = false;
          const msg =
            this.restoreError ||
            this.translate.instant(
              mode === 'backup' ? 'ADMIN.BACKUP.FULL_BACKUP_ERROR' : 'ADMIN.BACKUP.RESTORE_ERROR'
            );
          this.toasterService.error(msg, this.translate.instant('ADMIN.BACKUP.TITLE'));
          return;
        }

        if (mode === 'restore') {
          this.finishRestore();
        }
      },
      error: (err) => {
        this.restoreLoading = false;
        this.fullBackupLoading = false;
        this.operationPhase = 'failed';
        this.restoreError = this.extractErrorMessage(err) ?? this.translate.instant('ADMIN.BACKUP.RESTORE_ERROR');
        this.toasterService.error(this.restoreError, this.translate.instant('ADMIN.BACKUP.TITLE'));
      },
    });
  }

  private applyOperationStatus(status: DatabaseOperationStatus): void {
    this.operationProgress = status.progressPercent ?? 0;
    this.operationCurrentItem = status.currentItem ?? '';
    this.operationPhase = (status.phase as typeof this.operationPhase) ?? 'running';

    if (status.phase === 'failed') {
      this.restoreError = status.errorMessage ?? this.translate.instant('ADMIN.BACKUP.RESTORE_ERROR');
    }

    if (status.phase === 'completed' && status.summary) {
      this.restoreSummary = status.summary;
    }
  }

  private finishRestore(): void {
    this.restoreLoading = false;
    this.operationProgress = 100;
    this.operationPhase = 'completed';
    this.toasterService.success(
      this.translate.instant('ADMIN.BACKUP.RESTORE_SUCCESS'),
      this.translate.instant('ADMIN.BACKUP.TITLE')
    );
  }

  /** Smooth progress for sync backup download (no server-side job). */
  private startSmoothProgress(): void {
    this.stopSmoothProgress();
    this.operationProgress = 3;
    this.progressTimer = setInterval(() => {
      if (this.operationProgress >= 97) {
        return;
      }
      const remaining = 97 - this.operationProgress;
      const step = Math.max(0.5, remaining * 0.06);
      this.operationProgress = Math.min(97, Math.round((this.operationProgress + step) * 10) / 10);
    }, 350);
  }

  private completeProgress(): void {
    this.stopSmoothProgress();
    this.operationProgress = 100;
    this.operationPhase = 'completed';
  }

  private stopSmoothProgress(): void {
    if (this.progressTimer) {
      clearInterval(this.progressTimer);
      this.progressTimer = null;
    }
  }

  private extractErrorMessage(err: unknown): string | null {
    const e = err as { error?: unknown; message?: string };
    if (typeof e?.error === 'string' && e.error.trim()) {
      return e.error;
    }
    if (e?.error && typeof e.error === 'object') {
      const detail = (e.error as { detail?: string; title?: string }).detail
        ?? (e.error as { title?: string }).title;
      if (detail?.trim()) {
        return detail;
      }
    }
    if (e?.message?.trim()) {
      return e.message;
    }
    return null;
  }

  private resetOperationState(): void {
    this.operationProgress = 0;
    this.operationCurrentItem = '';
    this.operationPhase = 'idle';
    this.restoreError = '';
  }

  export(): void {
    if (this.selectedModules.size === 0) {
      this.toasterService.warning(
        this.translate.instant('ADMIN.BACKUP.NO_MODULE_SELECTED'),
        this.translate.instant('ADMIN.BACKUP.TITLE')
      );
      return;
    }

    this.errorMessage = '';
    this.exporting = true;
    const list = Array.from(this.selectedModules);
    const total = list.length;

    const runNext = (index: number) => {
      if (index >= list.length) {
        this.exporting = false;
        this.progressMessage = '';
        this.toasterService.success(
          this.translate.instant('ADMIN.BACKUP.SUCCESS'),
          this.translate.instant('ADMIN.BACKUP.TITLE')
        );
        return;
      }

      const moduleKey = list[index];
      this.progressMessage = this.translate.instant('ADMIN.BACKUP.EXPORTING_PROGRESS', {
        current: index + 1,
        total,
      });

      this.backupService
        .exportModule({
          module: moduleKey,
          from: this.fromDate || null,
          to: this.toDate || null,
        })
        .subscribe({
          next: (result) => {
            this.backupService.triggerDownload(result.blob, result.fileName);
            setTimeout(() => runNext(index + 1), 300);
          },
          error: () => {
            this.errorMessage = this.translate.instant('ADMIN.BACKUP.ERROR_MODULE', { module: moduleKey });
            this.toasterService.error(this.errorMessage, this.translate.instant('ADMIN.BACKUP.TITLE'));
            this.exporting = false;
            this.progressMessage = '';
          },
        });
    };

    runNext(0);
  }
}
