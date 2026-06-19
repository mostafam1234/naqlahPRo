import { Component, ElementRef, EventEmitter, HostListener, Input, OnChanges, OnDestroy, OnInit, Output, SimpleChanges, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NgClass, NgFor, NgIf } from '@angular/common';
import { Subscription } from 'rxjs';
import { SelectOption } from '../../models/select-option.model';
import { FormalDropdownCoordinatorService } from '../../services/formal-dropdown-coordinator.service';
import {
  attachFormalDropdownPanelToBody,
  detachFormalDropdownPanelFromBody,
  positionFormalDropdownPanel,
  resetFormalDropdownPanel
} from '../../utils/formal-dropdown-position.helper';

@Component({
  selector: 'app-formal-multi-select',
  standalone: true,
  imports: [NgClass, NgIf, NgFor, FormsModule],
  templateUrl: './formal-multi-select.component.html',
  styleUrl: './formal-multi-select.component.css'
})
export class FormalMultiSelectComponent implements OnInit, OnDestroy, OnChanges {
  @Input() label = '';
  @Input() placeholder = 'الكل';
  @Input() options: SelectOption[] = [];
  @Input() values: string[] = [];
  @Input() cachedSelected: SelectOption[] = [];
  @Input() disabled = false;
  @Input() loading = false;
  @Input() minSearchLength = 2;
  @Input() remoteSearch = false;
  @Input() searchPlaceholder = 'ابحث...';

  @Output() valuesChange = new EventEmitter<string[]>();
  @Output() searchChange = new EventEmitter<string>();
  @Output() cachedSelectedChange = new EventEmitter<SelectOption[]>();
  @Output() panelOpen = new EventEmitter<void>();

  isOpen = false;
  searchTerm = '';

  @ViewChild('triggerBtn') triggerBtn?: ElementRef<HTMLButtonElement>;
  @ViewChild('panel') panel?: ElementRef<HTMLDivElement>;

  private readonly instanceId = this.coordinator.createInstanceId();
  private coordinatorSub?: Subscription;

  constructor(private coordinator: FormalDropdownCoordinatorService) {}

  ngOnInit(): void {
    this.coordinatorSub = this.coordinator.activeChange$.subscribe((activeId) => {
      if (activeId !== this.instanceId) this.close(false);
    });
  }

  ngOnDestroy(): void {
    this.coordinatorSub?.unsubscribe();
    detachFormalDropdownPanelFromBody(this.panel?.nativeElement);
    if (this.isOpen) this.coordinator.notifyClose(this.instanceId);
  }

  get hasSelection(): boolean {
    return this.values.length > 0;
  }

  get displayOptions(): SelectOption[] {
    const map = new Map<string, SelectOption>();
    for (const opt of this.cachedSelected) map.set(opt.value, opt);
    for (const opt of this.options) map.set(opt.value, opt);
    return Array.from(map.values());
  }

  get filteredOptions(): SelectOption[] {
    if (this.remoteSearch) return this.displayOptions;
    const q = this.searchTerm.trim().toLowerCase();
    if (!q) return this.displayOptions;
    return this.displayOptions.filter((opt) => {
      const label = opt.label.toLowerCase();
      const hint = opt.hint?.toLowerCase() ?? '';
      const meta = opt.meta?.toLowerCase() ?? '';
      return label.includes(q) || hint.includes(q) || meta.includes(q);
    });
  }

  get displaySummary(): string {
    if (!this.hasSelection) return this.placeholder;
    if (this.values.length === 1) return this.getOptionLabel(this.values[0]);
    const first = this.getOptionLabel(this.values[0]);
    return `${first} +${this.values.length - 1}`;
  }

  get searchHint(): string {
    if (!this.remoteSearch) return '';
    if (this.searchTerm.trim().length < this.minSearchLength) {
      return `اكتب ${this.minSearchLength} أحرف على الأقل للبحث`;
    }
    if (this.loading) return 'جاري البحث...';
    if (this.filteredOptions.length === 0) return 'لا توجد نتائج';
    return '';
  }

  get selectedOptions(): SelectOption[] {
    return this.values
      .map((v) => this.cachedSelected.find((o) => o.value === v) ?? this.options.find((o) => o.value === v))
      .filter((o): o is SelectOption => !!o);
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (!this.isOpen) return;
    if (changes['loading'] || changes['options']) {
      this.schedulePanelPosition();
    }
  }

  toggle(): void {
    if (this.disabled) return;
    if (this.isOpen) {
      this.close();
      return;
    }
    this.coordinator.notifyOpen(this.instanceId);
    this.isOpen = true;
    this.searchTerm = '';
    this.panelOpen.emit();
    this.schedulePanelPosition();
  }

  private schedulePanelPosition(): void {
    setTimeout(() => this.updatePanelPosition(), 0);
  }

  private updatePanelPosition(): void {
    const trigger = this.triggerBtn?.nativeElement;
    const panel = this.panel?.nativeElement;
    if (!trigger || !panel || !this.isOpen) return;
    attachFormalDropdownPanelToBody(panel);
    positionFormalDropdownPanel(trigger, panel);
  }

  close(notifyCoordinator = true): void {
    if (!this.isOpen) return;
    const panel = this.panel?.nativeElement;
    if (panel) resetFormalDropdownPanel(panel);
    detachFormalDropdownPanelFromBody(panel);
    this.isOpen = false;
    this.searchTerm = '';
    if (notifyCoordinator) this.coordinator.notifyClose(this.instanceId);
  }

  onSearchInput(): void {
    if (this.remoteSearch) this.searchChange.emit(this.searchTerm);
  }

  isSelected(value: string): boolean {
    return this.values.includes(value);
  }

  toggleOption(opt: SelectOption): void {
    const next = this.isSelected(opt.value)
      ? this.values.filter((v) => v !== opt.value)
      : [...this.values, opt.value];

    const cacheMap = new Map(this.cachedSelected.map((o) => [o.value, o]));
    cacheMap.set(opt.value, opt);
    const nextCache = next
      .map((v) => cacheMap.get(v))
      .filter((o): o is SelectOption => !!o);

    this.valuesChange.emit(next);
    this.cachedSelectedChange.emit(nextCache);
  }

  clearAll(event: Event): void {
    event.stopPropagation();
    this.valuesChange.emit([]);
    this.cachedSelectedChange.emit([]);
  }

  removeValue(value: string, event: Event): void {
    event.stopPropagation();
    const next = this.values.filter((v) => v !== value);
    const nextCache = this.cachedSelected.filter((o) => next.includes(o.value));
    this.valuesChange.emit(next);
    this.cachedSelectedChange.emit(nextCache);
  }

  getOptionLabel(value: string): string {
    return this.cachedSelected.find((o) => o.value === value)?.label
      ?? this.options.find((o) => o.value === value)?.label
      ?? value;
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: Event): void {
    const target = event.target as HTMLElement;
    if (target?.closest?.('.na-formal-multi-select')) return;
    if (target?.closest?.('.na-formal-select__panel')) return;
    this.close();
  }

  @HostListener('window:resize')
  @HostListener('window:scroll')
  @HostListener('document:scroll', ['$event'])
  onViewportChange(): void {
    if (this.isOpen) this.updatePanelPosition();
  }
}
