import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { AuditLogDetailDto } from 'src/app/Core/services/NaqlahClient';
import { AUDIT_CHANGE_TYPE_KEYS } from '../../constants/audit.constants';

@Component({
  selector: 'app-audit-detail-block',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  templateUrl: './audit-detail-block.component.html',
  styleUrls: ['./audit-detail-block.component.css'],
})
export class AuditDetailBlockComponent {
  @Input() detail!: AuditLogDetailDto;

  readonly changeTypeKeys = AUDIT_CHANGE_TYPE_KEYS;

  changeTypeLabel(changeType: number): string {
    return this.changeTypeKeys[changeType] ?? '—';
  }

  formatJson(value: string | null): string {
    if (value == null || value === '') return '—';
    try {
      const o = typeof value === 'string' ? JSON.parse(value) : value;
      return JSON.stringify(o, null, 2);
    } catch {
      return value;
    }
  }

  getChangeTypeClass(changeType: number): string {
    switch (changeType) {
      case 1: return 'badge-insert';
      case 2: return 'badge-update';
      case 3: return 'badge-delete';
      default: return '';
    }
  }
}
