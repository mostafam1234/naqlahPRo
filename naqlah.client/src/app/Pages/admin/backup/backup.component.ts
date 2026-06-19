import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { BackupService } from '../../../Core/services/backup.service';
import { ToasterService } from '../../../Core/services/toaster.service';
import { getDefaultSearchDateRange } from '../../../shared/utils/date-filter.helpers';

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
export class BackupComponent {
  modules = BACKUP_MODULES;
  groupedModules: { groupKey: string; items: BackupModuleOption[] }[];
  selectedModules = new Set<string>();
  fromDate: string | null = getDefaultSearchDateRange().fromDate;
  toDate: string | null = getDefaultSearchDateRange().toDate;
  exporting = false;
  progressMessage = '';
  errorMessage = '';

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

  export(): void {
    if (this.selectedModules.size === 0) {
      this.toasterService.warning(
        this.translate.instant('ADMIN.BACKUP.NO_MODULE_SELECTED') ?? 'Select at least one module',
        this.translate.instant('ADMIN.BACKUP.TITLE') ?? 'Backup'
      );
      return;
    }
    this.errorMessage = '';
    this.exporting = true;
    const list = Array.from(this.selectedModules);
    let done = 0;
    const total = list.length;

    const runNext = (index: number) => {
      if (index >= list.length) {
        this.exporting = false;
        this.progressMessage = '';
        this.toasterService.success(
          this.translate.instant('ADMIN.BACKUP.SUCCESS') ?? 'Export completed',
          this.translate.instant('ADMIN.BACKUP.TITLE') ?? 'Backup'
        );
        return;
      }
      const moduleKey = list[index];
      this.progressMessage = this.translate.instant('ADMIN.BACKUP.EXPORTING_PROGRESS', {
        current: index + 1,
        total,
      }) ?? `Exporting ${index + 1} of ${total}...`;

      this.backupService
        .exportModule({
          module: moduleKey,
          from: this.fromDate || null,
          to: this.toDate || null,
        })
        .subscribe({
          next: (result) => {
            this.backupService.triggerDownload(result.blob, result.fileName);
            done++;
            setTimeout(() => runNext(index + 1), 300);
          },
          error: (err) => {
            this.errorMessage =
              this.translate.instant('ADMIN.BACKUP.ERROR_MODULE', { module: moduleKey }) ??
              `Export failed for ${moduleKey}`;
            this.toasterService.error(this.errorMessage, this.translate.instant('ADMIN.BACKUP.TITLE') ?? 'Backup');
            this.exporting = false;
            this.progressMessage = '';
          },
        });
    };

    runNext(0);
  }
}
