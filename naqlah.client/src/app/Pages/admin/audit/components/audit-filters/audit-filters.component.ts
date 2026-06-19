import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { FormalSelectComponent } from 'src/app/shared/components/formal-select/formal-select.component';
import { SelectOption } from 'src/app/shared/models/select-option.model';

export interface AuditFiltersModel {
  fromDate: string | null;
  toDate: string | null;
  entityType: string;
}

@Component({
  selector: 'app-audit-filters',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule, FormalSelectComponent],
  templateUrl: './audit-filters.component.html',
  styleUrls: ['./audit-filters.component.css'],
})
export class AuditFiltersComponent {
  @Input() loading = false;
  @Input() model!: AuditFiltersModel;
  @Input() entityTypes: string[] = [];

  @Output() search = new EventEmitter<void>();
  @Output() clear = new EventEmitter<void>();

  constructor(private translate: TranslateService) {}

  get entityTypeOptions(): SelectOption[] {
    const allLabel = this.translate.instant('ADMIN.AUDIT.ALL_ENTITY_TYPES');
    return [
      { value: '', label: allLabel },
      ...this.entityTypes.map((type) => ({ value: type, label: type }))
    ];
  }

  get entityTypePlaceholder(): string {
    return this.translate.instant('ADMIN.AUDIT.ALL_ENTITY_TYPES');
  }

  onSearch(): void {
    this.search.emit();
  }

  onClear(): void {
    this.clear.emit();
  }
}
