import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-confirmation-modal',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div *ngIf="show" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div class="na-confirm-modal rounded-lg shadow-xl max-w-md w-full mx-4" style="background: var(--shell-surface); border: 1px solid var(--shell-surface-border)">
        <div class="p-6 text-center">
          <div class="na-confirm-modal__icon-wrap w-12 h-12 rounded-full flex items-center justify-center mx-auto mb-4" style="background: color-mix(in srgb, #ef4444 12%, var(--shell-surface))">
            <i class="bi bi-exclamation-triangle text-red-600 text-xl"></i>
          </div>

          <h3 class="na-confirm-modal__title text-lg font-bold mb-2" style="color: var(--shell-text)">{{ title }}</h3>
          <p class="na-confirm-modal__message mb-6" style="color: var(--shell-text-muted)">{{ message }}</p>

          <div class="flex justify-center space-x-3 space-x-reverse">
            <button
              (click)="cancel()"
              class="na-confirm-modal__cancel px-4 py-2 rounded-lg"
              style="background: var(--shell-muted-surface); color: var(--shell-text-muted)">
              إلغاء
            </button>
            <button
              (click)="confirm()"
              class="px-4 py-2 bg-red-600 hover:bg-red-700 text-white rounded-lg">
              تأكيد
            </button>
          </div>
        </div>
      </div>
    </div>
  `
})
export class ConfirmationModalComponent {
  @Input() show = false;
  @Input() title = 'تأكيد الحذف';
  @Input() message = 'هل أنت متأكد من حذف هذا العنصر؟';
  
  @Output() confirmed = new EventEmitter<void>();
  @Output() cancelled = new EventEmitter<void>();

  confirm(): void {
    this.confirmed.emit();
  }

  cancel(): void {
    this.cancelled.emit();
  }
}