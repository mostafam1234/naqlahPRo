import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToasterService, ToastMessage } from 'src/app/Core/services/toaster.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-toaster',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div style="position: fixed; top: 20px; right: 20px; z-index: 999999;" class="space-y-2">
      <div
        *ngFor="let toast of toasts"
        [class]="getToastClass(toast.type)"
        class="min-w-80 rounded-lg shadow-xl p-4 transform transition-all duration-300 ease-in-out animate-slide-in"
      >
        <div class="flex items-start">
          <div class="flex-shrink-0">
            <i [class]="getIconClass(toast.type)" class="text-lg"></i>
          </div>
          <div class="mr-3 flex-1">
            <h4 class="text-sm font-medium">{{ toast.title }}</h4>
            <p *ngIf="toast.message" class="text-sm mt-1">{{ toast.message }}</p>
          </div>
          <button
            (click)="removeToast(toast.id)"
            class="flex-shrink-0 mr-2 text-sm hover:opacity-75 transition-opacity"
          >
            <i class="bi bi-x-lg"></i>
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    @keyframes slide-in {
      from {
        transform: translateX(100%);
        opacity: 0;
      }
      to {
        transform: translateX(0);
        opacity: 1;
      }
    }
    
    .animate-slide-in {
      animation: slide-in 0.3s ease-out;
    }
  `]
})
export class ToasterComponent implements OnInit, OnDestroy {
  toasts: ToastMessage[] = [];
  private subscription?: Subscription;

  constructor(private toasterService: ToasterService) {
    console.log('🍞 ToasterComponent Constructor called!');
    console.log('🍞 ToasterComponent - ToasterService instance:', this.toasterService);
    console.log('🍞 ToasterComponent - Observable:', this.toasterService.toasts$);
  }

  ngOnInit(): void {
    console.log('🍞 ToasterComponent initialized');
    
    // Try to get static instance
    const staticInstance = ToasterService.getInstance();
    console.log('🍞 Static instance:', staticInstance);
    console.log('🍞 Injected instance:', this.toasterService);
    console.log('🍞 Are they the same?', staticInstance === this.toasterService);
    
    // Use the static instance if available
    const serviceToUse = staticInstance || this.toasterService;
    console.log('🍞 Using service:', serviceToUse);
    
    this.subscription = serviceToUse.toasts$.subscribe(toasts => {
      console.log('🍞 Received toasts:', toasts);
      console.log('🍞 Toasts array length:', toasts.length);
      this.toasts = toasts;
      console.log('🍞 Component toasts updated:', this.toasts);
    });
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
  }

  getToastClass(type: string): string {
    switch (type) {
      case 'success':
        return 'bg-green-50 text-green-800 border border-green-200';
      case 'error':
        return 'bg-red-50 text-red-800 border border-red-200';
      case 'warning':
        return 'bg-yellow-50 text-yellow-800 border border-yellow-200';
      case 'info':
        return 'bg-blue-50 text-blue-800 border border-blue-200';
      default:
        return 'bg-gray-50 text-gray-800 border border-gray-200';
    }
  }

  getIconClass(type: string): string {
    switch (type) {
      case 'success':
        return 'bi bi-check-circle-fill text-green-500';
      case 'error':
        return 'bi bi-x-circle-fill text-red-500';
      case 'warning':
        return 'bi bi-exclamation-triangle-fill text-yellow-500';
      case 'info':
        return 'bi bi-info-circle-fill text-blue-500';
      default:
        return 'bi bi-info-circle-fill text-gray-500';
    }
  }

  removeToast(id: string): void {
    this.toasterService.removeToast(id);
  }
}