import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-confirmation-modal',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div *ngIf="show" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div class="bg-white rounded-lg shadow-xl max-w-md w-full mx-4">
        <div class="p-6 text-center">
          <div class="w-12 h-12 bg-red-100 rounded-full flex items-center justify-center mx-auto mb-4">
            <i class="bi bi-exclamation-triangle text-red-600 text-xl"></i>
          </div>
          
          <h3 class="text-lg font-bold text-gray-900 mb-2">{{ title }}</h3>
          <p class="text-gray-600 mb-6">{{ message }}</p>
          
          <div class="flex justify-center space-x-3 space-x-reverse">
            <button
              (click)="cancel()"
              class="px-4 py-2 text-gray-600 bg-gray-100 hover:bg-gray-200 rounded-lg">
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