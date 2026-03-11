import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { AuditLogDto } from 'src/app/Core/services/NaqlahClient';
import { AuditDetailBlockComponent } from '../audit-detail-block/audit-detail-block.component';

@Component({
  selector: 'app-audit-log-entry',
  standalone: true,
  imports: [CommonModule, TranslateModule, AuditDetailBlockComponent],
  templateUrl: './audit-log-entry.component.html',
  styleUrls: ['./audit-log-entry.component.css'],
})
export class AuditLogEntryComponent {
  @Input() log!: AuditLogDto;
  @Input() expanded = false;
  /** When false, used inside a user module: hide user avatar/name, show only action + time + expand */
  @Input() showUserInfo = true;

  @Output() toggleDetails = new EventEmitter<number>();

  get displayName(): string {
    return this.log.userName || String(this.log.userId) || '—';
  }

  get initials(): string {
    const n = this.displayName.trim();
    if (!n) return '?';
    const parts = n.split(/\s+/);
    if (parts.length >= 2) return (parts[0][0] + parts[1][0]).toUpperCase();
    return n.slice(0, 2).toUpperCase();
  }

  formatDate(utc: Date | string): string {
    if (utc == null) return '—';
    const d = typeof utc === 'string' ? new Date(utc) : utc;
    return d.toLocaleString(undefined, { dateStyle: 'short', timeStyle: 'short' });
  }

  onToggle(e?: Event): void {
    e?.stopPropagation();
    this.toggleDetails.emit(this.log.id);
  }

  trackByDetailId(_: number, d: { id: number }): number {
    return d.id;
  }
}
