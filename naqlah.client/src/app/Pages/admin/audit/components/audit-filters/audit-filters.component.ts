import { CommonModule } from '@angular/common';
import { Component, EventEmitter, HostListener, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';

export interface AuditFiltersModel {
  fromDate: string | null;
  toDate: string | null;
  entityType: string;
}

@Component({
  selector: 'app-audit-filters',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule],
  templateUrl: './audit-filters.component.html',
  styleUrls: ['./audit-filters.component.css'],
})
export class AuditFiltersComponent {
  @Input() loading = false;
  @Input() model!: AuditFiltersModel;
  /** Entity types for dropdown (from backend). */
  @Input() entityTypes: string[] = [];

  @Output() search = new EventEmitter<void>();
  @Output() clear = new EventEmitter<void>();

  entityTypeDropdownOpen = false;
  entityTypeSearch = '';

  get filteredEntityTypes(): string[] {
    const q = (this.entityTypeSearch || '').trim().toLowerCase();
    if (!q) return this.entityTypes;
    return this.entityTypes.filter((t) => t.toLowerCase().includes(q));
  }

  get entityTypeDisplayText(): string {
    const v = this.model.entityType?.trim();
    if (!v) return '';
    return v;
  }

  openEntityTypeDropdown(): void {
    this.entityTypeDropdownOpen = true;
    this.entityTypeSearch = '';
  }

  closeEntityTypeDropdown(): void {
    this.entityTypeDropdownOpen = false;
    this.entityTypeSearch = '';
  }

  selectEntityType(type: string): void {
    this.model.entityType = type;
    this.closeEntityTypeDropdown();
  }

  clearEntityType(): void {
    this.model.entityType = '';
    this.closeEntityTypeDropdown();
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: Event): void {
    const target = event.target as HTMLElement;
    if (target?.closest?.('.audit-entity-type-dropdown')) return;
    this.closeEntityTypeDropdown();
  }

  onSearch(): void {
    this.search.emit();
  }

  onClear(): void {
    this.clear.emit();
  }
}
