import { Injectable, ApplicationRef, ComponentRef, createComponent, EnvironmentInjector, inject } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { ConfirmationDialogComponent, ConfirmationDialogData } from '../components/confirmation-dialog/confirmation-dialog.component';

@Injectable({
  providedIn: 'root'
})
export class ConfirmationDialogService {
  private environmentInjector = inject(EnvironmentInjector);
  private appRef = inject(ApplicationRef);

  confirm(data: ConfirmationDialogData): Observable<boolean> {
    const subject = new Subject<boolean>();

    // Create component
    const componentRef: ComponentRef<ConfirmationDialogComponent> = createComponent(
      ConfirmationDialogComponent,
      {
        environmentInjector: this.environmentInjector
      }
    );

    // Set data
    componentRef.instance.data = data;

    // Create overlay
    const overlay = document.createElement('div');
    overlay.className = 'fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4';
    overlay.style.zIndex = '9999';

    // Handle result
    const handleResult = (result: boolean) => {
      subject.next(result);
      subject.complete();
      this.appRef.detachView(componentRef.hostView);
      componentRef.destroy();
      document.body.removeChild(overlay);
    };

    // Set up component methods
    componentRef.instance.onConfirm = () => handleResult(true);
    componentRef.instance.onCancel = () => handleResult(false);

    // Add click outside to close
    overlay.addEventListener('click', (event) => {
      if (event.target === overlay) {
        handleResult(false);
      }
    });

    // Attach to DOM
    overlay.appendChild(componentRef.location.nativeElement);
    document.body.appendChild(overlay);
    this.appRef.attachView(componentRef.hostView);

    return subject.asObservable();
  }

  // Quick methods for common confirmation dialogs
  confirmApprove(captainName: string): Observable<boolean> {
    return this.confirm({
      title: 'تأكيد الموافقة',
      message: `هل أنت متأكد من موافقة طلب الكابتن "${captainName}"؟`,
      confirmText: 'موافقة',
      cancelText: 'إلغاء',
      confirmColor: 'primary',
      icon: 'check_circle',
      iconColor: 'text-green-500'
    });
  }

  confirmReject(captainName: string): Observable<boolean> {
    return this.confirm({
      title: 'تأكيد الرفض',
      message: `هل أنت متأكد من رفض طلب الكابتن "${captainName}"؟`,
      confirmText: 'رفض',
      cancelText: 'إلغاء',
      confirmColor: 'warn',
      icon: 'cancel',
      iconColor: 'text-red-500'
    });
  }

  confirmBlock(captainName: string): Observable<boolean> {
    return this.confirm({
      title: 'تأكيد الحظر',
      message: `هل أنت متأكد من حظر الكابتن "${captainName}"؟ هذا الإجراء سيمنع الكابتن من استخدام التطبيق.`,
      confirmText: 'حظر',
      cancelText: 'إلغاء',
      confirmColor: 'warn',
      icon: 'block',
      iconColor: 'text-red-600'
    });
  }

  confirmSuspend(captainName: string): Observable<boolean> {
    return this.confirm({
      title: 'تأكيد التعليق',
      message: `هل أنت متأكد من تعليق حساب الكابتن "${captainName}"؟`,
      confirmText: 'تعليق',
      cancelText: 'إلغاء',
      confirmColor: 'accent',
      icon: 'pause_circle',
      iconColor: 'text-orange-500'
    });
  }
}
