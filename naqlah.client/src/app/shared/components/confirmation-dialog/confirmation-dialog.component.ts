import { Component, Input } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { CommonModule } from '@angular/common';

export interface ConfirmationDialogData {
  title: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  confirmColor?: 'primary' | 'accent' | 'warn';
  icon?: string;
  iconColor?: string;
}

@Component({
  selector: 'app-confirmation-dialog',
  standalone: true,
  imports: [
    CommonModule,
    TranslateModule
  ],
  template: `
    <div class="confirmation-dialog bg-white rounded-lg shadow-2xl max-w-md w-full mx-auto" dir="rtl">
      <!-- Header -->
      <div class="dialog-header flex items-center gap-3 p-6 border-b border-gray-200 bg-gradient-to-l from-gray-50 to-gray-100">
        <div *ngIf="data.icon"
             [class]="'text-2xl ' + (data.iconColor || 'text-orange-500')"
             [innerHTML]="getIconHtml(data.icon)">
        </div>
        <h2 class="text-xl font-semibold text-gray-800 flex-1">
          {{ data.title }}
        </h2>
      </div>

      <!-- Content -->
      <div class="dialog-content p-6">
        <p class="text-gray-600 leading-relaxed text-base">
          {{ data.message }}
        </p>
      </div>

      <!-- Actions -->
      <div class="dialog-actions flex justify-end gap-3 p-6 border-t border-gray-200 bg-gray-50">
        <button
          type="button"
          (click)="cancel()"
          class="px-6 py-2 text-gray-600 hover:text-gray-800 hover:bg-gray-100 rounded-lg transition-colors border border-gray-300">
          {{ data.cancelText || 'إلغاء' }}
        </button>
        <button
          type="button"
          (click)="confirm()"
          [class]="getConfirmButtonClass()"
          class="px-6 py-2 rounded-lg font-medium transition-all transform hover:scale-105 text-white">
          {{ data.confirmText || 'تأكيد' }}
        </button>
      </div>
    </div>
  `,
  styles: [`
    .confirmation-dialog {
      min-width: 400px;
      max-width: 500px;
      animation: slideIn 0.3s ease-out;
    }

    @keyframes slideIn {
      from {
        opacity: 0;
        transform: translateY(-20px) scale(0.95);
      }
      to {
        opacity: 1;
        transform: translateY(0) scale(1);
      }
    }

    .dialog-header {
      background: linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%);
    }

    .dialog-actions {
      background: #f8fafc;
    }

    button:hover {
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
    }

    @media (max-width: 640px) {
      .confirmation-dialog {
        min-width: 320px;
        max-width: 90vw;
      }

      .dialog-actions {
        flex-direction: column;
      }

      .dialog-actions button {
        width: 100%;
      }
    }
  `]
})
export class ConfirmationDialogComponent {
  @Input() data!: ConfirmationDialogData;

  onConfirm: () => void = () => {};
  onCancel: () => void = () => {};

  confirm(): void {
    this.onConfirm();
  }

  cancel(): void {
    this.onCancel();
  }

  getConfirmButtonClass(): string {
    const baseClass = 'px-6 py-2 rounded-lg font-medium transition-all transform hover:scale-105 text-white';

    switch (this.data.confirmColor) {
      case 'primary':
        return baseClass + ' bg-blue-600 hover:bg-blue-700';
      case 'accent':
        return baseClass + ' bg-orange-500 hover:bg-orange-600';
      case 'warn':
        return baseClass + ' bg-red-600 hover:bg-red-700';
      default:
        return baseClass + ' bg-red-600 hover:bg-red-700';
    }
  }

  getIconHtml(iconName: string): string {
    // Simple icon mapping - you can expand this or use icon fonts
    const iconMap: { [key: string]: string } = {
      'check_circle': '✓',
      'cancel': '✕',
      'block': '🚫',
      'pause_circle': '⏸'
    };

    return iconMap[iconName] || '⚠';
  }
}
