import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import {
  AuditAdminClient,
  AuditLogDto,
  PagedResultOfAuditLogDto,
} from '../../../Core/services/NaqlahClient';
import { ToasterService } from '../../../Core/services/toaster.service';
import { AuditLogNotificationService } from '../../../shared/services/audit-log-notification.service';
import { AuditFiltersComponent, AuditFiltersModel } from './components/audit-filters/audit-filters.component';
import { AuditLogEntryComponent } from './components/audit-log-entry/audit-log-entry.component';
import { AuditPaginationComponent } from './components/audit-pagination/audit-pagination.component';
import { getDefaultSearchDateRange } from '../../../shared/utils/date-filter.helpers';

export interface UserAuditGroup {
  userKey: string;
  userId: number;
  userName: string;
  logs: AuditLogDto[];
}

@Component({
  selector: 'app-audit',
  standalone: true,
  imports: [
    CommonModule,
    TranslateModule,
    AuditFiltersComponent,
    AuditLogEntryComponent,
    AuditPaginationComponent,
  ],
  templateUrl: './audit.component.html',
  styleUrls: ['./audit.component.css'],
})
export class AuditComponent implements OnInit {
  loading = false;
  errorMessage = '';
  logs: AuditLogDto[] = [];
  totalCount = 0;
  totalPages = 0;
  currentPage = 0;
  pageSize = 15;
  expandedId: number | null = null;

  private auditLogNotificationService = inject(AuditLogNotificationService);

  filterModel: AuditFiltersModel = {
    fromDate: getDefaultSearchDateRange().fromDate,
    toDate: getDefaultSearchDateRange().toDate,
    entityType: '',
  };

  /** Entity types for dropdown (from backend). */
  entityTypes: string[] = [];

  constructor(
    private auditClient: AuditAdminClient,
    private toaster: ToasterService
  ) {}

  ngOnInit(): void {
    this.auditLogNotificationService.resetCount();
    this.auditClient.getAuditFilterOptions().subscribe({
      next: (options) => {
        this.entityTypes = Array.isArray(options.entityTypes) ? options.entityTypes : [];
      },
      error: () => {
        this.entityTypes = [];
      },
    });
    this.load();
  }

  load(): void {
    this.loading = true;
    this.errorMessage = '';
    const skip = this.currentPage * this.pageSize;
    const from = this.filterModel.fromDate ? new Date(this.filterModel.fromDate) : null;
    const to = this.filterModel.toDate ? new Date(this.filterModel.toDate) : null;
    this.auditClient
      .getAuditLogs(
        skip,
        this.pageSize,
        undefined,
        from,
        to,
        null,
        this.filterModel.entityType.trim() || null
      )
      .subscribe({
        next: (result: PagedResultOfAuditLogDto) => {
          this.logs = result.data ?? [];
          this.totalCount = result.totalCount ?? 0;
          this.totalPages = result.totalPages ?? 0;
          this.loading = false;
        },
        error: (err) => {
          this.errorMessage = err?.message ?? err?.error?.detail ?? 'ERROR';
          this.loading = false;
          this.toaster.error('ADMIN.AUDIT.ERROR_LOAD');
        },
      });
  }

  search(): void {
    this.currentPage = 0;
    this.load();
  }

  clearFilters(): void {
    const defaultDates = getDefaultSearchDateRange();
    this.filterModel = {
      fromDate: defaultDates.fromDate,
      toDate: defaultDates.toDate,
      entityType: '',
    };
    this.currentPage = 0;
    this.expandedId = null;
    this.load();
  }

  goToPage(page: number): void {
    this.currentPage = page;
    this.load();
  }

  toggleDetails(id: number): void {
    this.expandedId = this.expandedId === id ? null : id;
  }

  isExpanded(log: AuditLogDto): boolean {
    return this.expandedId === log.id;
  }

  trackByLogId(_: number, log: AuditLogDto): number {
    return log.id;
  }

  /** Group current logs by user so each user has their own module. */
  groupLogsByUser(): UserAuditGroup[] {
    const map = new Map<string, UserAuditGroup>();
    for (const log of this.logs) {
      const key = `u-${log.userId}`;
      const name = log.userName?.trim() || `User #${log.userId}`;
      if (!map.has(key)) {
        map.set(key, { userKey: key, userId: log.userId, userName: name, logs: [] });
      }
      map.get(key)!.logs.push(log);
    }
    return Array.from(map.values());
  }

  trackByUserKey(_: number, g: UserAuditGroup): string {
    return g.userKey;
  }
}
