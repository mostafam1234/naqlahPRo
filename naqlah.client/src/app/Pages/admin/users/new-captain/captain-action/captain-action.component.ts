import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { DeliveryManAdminClient, GetDeliveryManRequestDetailsDto } from '../../../../../Core/services/NaqlahClient';
import { LanguageService } from '../../../../../Core/services/language.service';
import { ConfirmationDialogService } from '../../../../../shared/services/confirmation-dialog.service';
import { SubSink } from 'subsink';

// Enum للحالات
enum DeliveryRequesState {
  Pending = 1,
  Approved = 2,
  Rejected = 3,
  Blocked = 4,
  Suspended = 5  // إضافة حالة معلق
}

@Component({
  selector: 'app-captain-action',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  templateUrl: './captain-action.component.html',
  styleUrl: './captain-action.component.css'
})
export class CaptainActionComponent implements OnInit, OnDestroy {
  deliveryManDetails: GetDeliveryManRequestDetailsDto | null = null;
  isLoading = false;
  deliveryManId: number = 0;
  lang: string = 'ar';

  // حالة الأزرار لمنع الضغط المتكرر
  actionInProgress = {
    approve: false,
    reject: false,
    suspend: false,
    block: false
  };

  private sub = new SubSink();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private deliveryManClient: DeliveryManAdminClient,
    private languageService: LanguageService,
    private confirmationService: ConfirmationDialogService
  ) {}

  ngOnInit() {
    this.route.params.subscribe(params => {
      this.deliveryManId = +params['id'];
      if (this.deliveryManId) {
        this.loadDeliveryManDetails();
      }
    });
  }

  loadDeliveryManDetails() {
    this.isLoading = true;

    this.sub.sink = this.deliveryManClient
      .getDeliveryManDetails(this.deliveryManId)
      .subscribe({
        next: (response) => {
          this.deliveryManDetails = response;
          this.isLoading = false;
        },
        error: (error) => {
          console.error('خطأ في جلب تفاصيل الكابتن:', error);
          this.confirmationService.showError('حدث خطأ في جلب البيانات');
          this.isLoading = false;
        }
      });
  }

  goBack() {
    this.router.navigate(['/admin/users/newCaptain']);
  }

  approveDeliveryMan() {
    console.log('approveDeliveryMan called');
    if (!this.deliveryManDetails || this.actionInProgress.approve) return;

    const captainName = this.deliveryManDetails.fullName || 'الكابتن';
    console.log('Opening confirmation dialog for:', captainName);

    this.sub.sink = this.confirmationService.confirmApprove(captainName)
      .subscribe({
        next: (confirmed) => {
          console.log('Confirmation result:', confirmed);
          if (confirmed) {
            this.executeStateChange(DeliveryRequesState.Approved, 'approve');
          }
        },
        error: (error) => {
          console.error('Error in confirmation dialog:', error);
        }
      });
  }

  rejectDeliveryMan() {
    if (!this.deliveryManDetails || this.actionInProgress.reject) return;

    const captainName = this.deliveryManDetails.fullName || 'الكابتن';

    this.sub.sink = this.confirmationService.confirmReject(captainName)
      .subscribe((confirmed) => {
        if (confirmed) {
          this.executeStateChange(DeliveryRequesState.Rejected, 'reject');
        }
      });
  }

  suspendDeliveryMan() {
    if (!this.deliveryManDetails || this.actionInProgress.suspend) return;

    const captainName = this.deliveryManDetails.fullName || 'الكابتن';

    this.sub.sink = this.confirmationService.confirmSuspend(captainName)
      .subscribe((confirmed) => {
        if (confirmed) {
          this.executeStateChange(DeliveryRequesState.Suspended, 'suspend');
        }
      });
  }

  blockDeliveryMan() {
    if (!this.deliveryManDetails || this.actionInProgress.block) return;

    const captainName = this.deliveryManDetails.fullName || 'الكابتن';

    this.sub.sink = this.confirmationService.confirmBlock(captainName)
      .subscribe((confirmed) => {
        if (confirmed) {
          this.executeStateChange(DeliveryRequesState.Blocked, 'block');
        }
      });
  }

  private executeStateChange(state: DeliveryRequesState, actionType: keyof typeof this.actionInProgress) {
    if (!this.deliveryManDetails) return;

    // تعطيل الزر أثناء المعالجة
    this.actionInProgress[actionType] = true;

    const deliveryManId = this.deliveryManDetails.deliveryManId;
    const stateNumber = state as number;

    console.log('إرسال طلب تحديث الحالة:', {
      deliveryManId: deliveryManId,
      state: stateNumber,
      originalState: state
    });

    this.sub.sink = this.deliveryManClient
      .updateDeliveryManState(deliveryManId, stateNumber)
      .subscribe({
        next: (response) => {
          console.log('نجح تحديث حالة الكابتن:', response);
          if (this.deliveryManDetails) {
            this.deliveryManDetails.state = this.getStateStringFromEnum(state);
          }
          this.actionInProgress[actionType] = false;

          // إظهار رسالة نجاح باستخدام SweetAlert2
          const successMessage = this.getActionSuccessMessage(actionType);
          this.confirmationService.showSuccess(successMessage);
        },
        error: (error) => {
          console.error('خطأ في تحديث حالة الكابتن:', error);
          console.error('تفاصيل الخطأ:', {
            message: error.message,
            status: error.status,
            statusText: error.statusText,
            url: error.url,
            error: error.error
          });

          // إعادة تفعيل الزر
          this.actionInProgress[actionType] = false;

          // إظهار رسالة خطأ باستخدام SweetAlert2
          const errorMessage = error.error?.message || error.message || 'حدث خطأ في تحديث حالة الكابتن';
          this.confirmationService.showError(errorMessage);
        }
      });
  }

  private getStateStringFromEnum(state: DeliveryRequesState): string {
    switch (state) {
      case DeliveryRequesState.Pending: return 'Pending';
      case DeliveryRequesState.Approved: return 'Approved';
      case DeliveryRequesState.Rejected: return 'Rejected';
      case DeliveryRequesState.Blocked: return 'Blocked';
      case DeliveryRequesState.Suspended: return 'Suspended';
      default: return 'Pending';
    }
  }

  private getActionSuccessMessage(actionType: string): string {
    switch (actionType) {
      case 'approve': return 'تم قبول طلب الكابتن بنجاح';
      case 'reject': return 'تم رفض طلب الكابتن';
      case 'suspend': return 'تم تعليق حساب الكابتن';
      case 'block': return 'تم حظر الكابتن';
      default: return 'تم تحديث حالة الكابتن';
    }
  }

  formatDate(date: Date | string | null): string {
    if (!date) return 'غير محدد';
    const d = typeof date === 'string' ? new Date(date) : date;
    return d.toLocaleDateString('ar-EG');
  }

  getDeliveryLicenseTypeLabel(licenseType: string): string {
    if (!licenseType) return 'غير متوفر';
    const type = licenseType.toLowerCase();
    if (type.includes('public') || type.includes('عامة')) return 'رخصة عامة';
    if (type.includes('private') || type.includes('خاصة')) return 'رخصة خاصة';
    return licenseType;
  }

  isIdentityValid(): boolean {
    if (!this.deliveryManDetails || !this.deliveryManDetails.identityExpirationDate) return false;
    const expirationDate = new Date(this.deliveryManDetails.identityExpirationDate);
    return expirationDate > new Date();
  }

  isLicenseValid(): boolean {
    if (!this.deliveryManDetails || !this.deliveryManDetails.drivingLicenseExpirationDate) return false;
    const expirationDate = new Date(this.deliveryManDetails.drivingLicenseExpirationDate);
    return expirationDate > new Date();
  }

  getStateLabel(state: string): string {
    switch (state) {
      case 'Pending': return 'قيد المراجعة';
      case 'Approved': return 'موافق عليه';
      case 'Rejected': return 'مرفوض';
      case 'Suspended': return 'معلق';
      case 'Blocked': return 'محظور';
      default: return state;
    }
  }

  getStateClass(state: string): string {
    switch (state) {
      case 'Pending': return 'bg-yellow-100 text-yellow-800';
      case 'Approved': return 'bg-green-100 text-green-800';
      case 'Rejected': return 'bg-red-100 text-red-800';
      case 'Suspended': return 'bg-orange-100 text-orange-800';
      case 'Blocked': return 'bg-gray-100 text-gray-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  }

  ngOnDestroy() {
    this.sub.unsubscribe();
  }
}
