import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import Swal from 'sweetalert2';

@Injectable({
  providedIn: 'root'
})
export class ConfirmationDialogService {

  // Quick methods for common confirmation dialogs
  confirmApprove(captainName: string): Observable<boolean> {
    return new Observable<boolean>((observer) => {
      Swal.fire({
        title: 'تأكيد الموافقة',
        text: `هل أنت متأكد من موافقة طلب الكابتن "${captainName}"؟`,
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#10b981',
        cancelButtonColor: '#6b7280',
        confirmButtonText: 'موافقة',
        cancelButtonText: 'إلغاء',
        reverseButtons: true,
        customClass: {
          popup: 'swal-rtl'
        }
      }).then((result) => {
        observer.next(result.isConfirmed);
        observer.complete();
      });
    });
  }

  confirmReject(captainName: string): Observable<boolean> {
    return new Observable<boolean>((observer) => {
      Swal.fire({
        title: 'تأكيد الرفض',
        text: `هل أنت متأكد من رفض طلب الكابتن "${captainName}"؟`,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#ef4444',
        cancelButtonColor: '#6b7280',
        confirmButtonText: 'رفض',
        cancelButtonText: 'إلغاء',
        reverseButtons: true,
        customClass: {
          popup: 'swal-rtl'
        }
      }).then((result) => {
        observer.next(result.isConfirmed);
        observer.complete();
      });
    });
  }

  confirmBlock(captainName: string): Observable<boolean> {
    return new Observable<boolean>((observer) => {
      Swal.fire({
        title: 'تأكيد الحظر',
        text: `هل أنت متأكد من حظر الكابتن "${captainName}"؟ هذا الإجراء سيمنع الكابتن من استخدام التطبيق.`,
        icon: 'error',
        showCancelButton: true,
        confirmButtonColor: '#dc2626',
        cancelButtonColor: '#6b7280',
        confirmButtonText: 'حظر',
        cancelButtonText: 'إلغاء',
        reverseButtons: true,
        customClass: {
          popup: 'swal-rtl'
        }
      }).then((result) => {
        observer.next(result.isConfirmed);
        observer.complete();
      });
    });
  }

  confirmSuspend(captainName: string): Observable<boolean> {
    return new Observable<boolean>((observer) => {
      Swal.fire({
        title: 'تأكيد التعليق',
        text: `هل أنت متأكد من تعليق حساب الكابتن "${captainName}"؟`,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#f59e0b',
        cancelButtonColor: '#6b7280',
        confirmButtonText: 'تعليق',
        cancelButtonText: 'إلغاء',
        reverseButtons: true,
        customClass: {
          popup: 'swal-rtl'
        }
      }).then((result) => {
        observer.next(result.isConfirmed);
        observer.complete();
      });
    });
  }

  // Success messages
  showSuccess(message: string): void {
    Swal.fire({
      title: 'نجح العملية!',
      text: message,
      icon: 'success',
      confirmButtonColor: '#10b981',
      confirmButtonText: 'حسناً',
      customClass: {
        popup: 'swal-rtl'
      },
      timer: 3000,
      timerProgressBar: true
    });
  }

  // Error messages
  showError(message: string): void {
    Swal.fire({
      title: 'حدث خطأ!',
      text: message,
      icon: 'error',
      confirmButtonColor: '#ef4444',
      confirmButtonText: 'حسناً',
      customClass: {
        popup: 'swal-rtl'
      }
    });
  }

  // Loading dialog
  showLoading(message: string = 'جارِ المعالجة...'): void {
    Swal.fire({
      title: message,
      allowOutsideClick: false,
      allowEscapeKey: false,
      showConfirmButton: false,
      customClass: {
        popup: 'swal-rtl'
      },
      didOpen: () => {
        Swal.showLoading();
      }
    });
  }

  // Close loading
  closeLoading(): void {
    Swal.close();
  }
}
