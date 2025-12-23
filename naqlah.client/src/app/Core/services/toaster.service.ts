import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export interface ToastMessage {
  id: string;
  type: 'success' | 'error' | 'warning' | 'info';
  title: string;
  message?: string;
  duration?: number;
}

@Injectable({
  providedIn: 'root'
})
export class ToasterService {
  private static instance: ToasterService;
  private toastsSubject = new BehaviorSubject<ToastMessage[]>([]);
  public toasts$ = this.toastsSubject.asObservable();

  constructor() {
    console.log('🔥 ToasterService Constructor - Instance:', this);
    console.log('🔥 ToasterService Constructor - BehaviorSubject:', this.toastsSubject);
    
    // Ensure singleton
    if (ToasterService.instance) {
      console.log('🔥 ToasterService - Returning existing instance');
      return ToasterService.instance;
    }
    ToasterService.instance = this;
  }

  private generateId(): string {
    return Math.random().toString(36).substr(2, 9);
  }

  private showToast(toast: Omit<ToastMessage, 'id'>): void {
    const id = this.generateId();
    const newToast: ToastMessage = {
      id,
      ...toast,
      duration: toast.duration || 4000 // Default to 4000 if undefined
    };

    console.log('🍞 ToasterService: Showing toast:', newToast);
    console.log('🍞 ToasterService: Toast duration:', newToast.duration);
    const currentToasts = this.toastsSubject.value;
    console.log('🍞 ToasterService: Current toasts before:', currentToasts);
    console.log('🍞 ToasterService: BehaviorSubject before next:', this.toastsSubject);
    
    const newArray = [...currentToasts, newToast];
    console.log('🍞 ToasterService: New array to emit:', newArray);
    
    this.toastsSubject.next(newArray);
    
    console.log('🍞 ToasterService: BehaviorSubject after next:', this.toastsSubject.value);
    console.log('🍞 ToasterService: Toasts updated, count:', newArray.length);

    // Auto remove after duration
    console.log('🍞 ToasterService: Setting timeout for', newToast.duration, 'ms');
    setTimeout(() => {
      console.log('🍞 ToasterService: Removing toast after timeout:', id);
      this.removeToast(id);
    }, newToast.duration);
  }

  success(title: string, message?: string, duration?: number): void {
    console.log('🔥 ToasterService.success called with:', { title, message, duration });
    this.showToast({ type: 'success', title, message, duration });
    console.log('🔥 ToasterService.success completed');
  }

  error(title: string, message?: string, duration?: number): void {
    this.showToast({ type: 'error', title, message, duration });
  }

  warning(title: string, message?: string, duration?: number): void {
    this.showToast({ type: 'warning', title, message, duration });
  }

  info(title: string, message?: string, duration?: number): void {
    this.showToast({ type: 'info', title, message, duration });
  }

  removeToast(id: string): void {
    const currentToasts = this.toastsSubject.value;
    this.toastsSubject.next(currentToasts.filter(toast => toast.id !== id));
  }

  static getInstance(): ToasterService {
    return ToasterService.instance;
  }
}