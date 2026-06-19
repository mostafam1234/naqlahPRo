import { Component, ElementRef, EventEmitter, HostListener, Input, OnDestroy, OnInit, Output, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NgFor, NgIf } from '@angular/common';
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
  selector: 'app-formal-select',
  standalone: true,
  imports: [NgIf, NgFor, FormsModule],
  templateUrl: './formal-select.component.html',
  styleUrl: './formal-select.component.css'
})
export class FormalSelectComponent implements OnInit, OnDestroy {
  @Input() label = '';
  @Input() placeholder = 'الكل';
  @Input() options: SelectOption[] = [];
  @Input() value = '';
  @Input() searchable = true;
  @Input() disabled = false;
  @Input() loading = false;
  @Input() clearable = true;

  @Output() valueChange = new EventEmitter<string>();
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

  get filteredOptions(): SelectOption[] {
    const q = this.searchTerm.trim().toLowerCase();
    if (!q) return this.options;
    return this.options.filter((opt) => {
      const label = opt.label.toLowerCase();
      const hint = opt.hint?.toLowerCase() ?? '';
      return label.includes(q) || hint.includes(q);
    });
  }

  get selectedOption(): SelectOption | undefined {
    return this.options.find((opt) => opt.value === this.value);
  }

  get displayText(): string {
    return this.selectedOption?.label ?? '';
  }

  get hasSelection(): boolean {
    return !!this.value && this.value !== 'all' && this.value !== '';
  }

  toggle(): void {
    if (this.disabled || this.loading) return;
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

  select(optionValue: string): void {
    this.valueChange.emit(optionValue);
    this.close();
  }

  clearSelection(event: Event): void {
    event.stopPropagation();
    const empty = this.options.find((o) => o.value === 'all' || o.value === '')?.value ?? '';
    this.valueChange.emit(empty);
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: Event): void {
    const target = event.target as HTMLElement;
    if (target?.closest?.('.na-formal-select')) return;
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
