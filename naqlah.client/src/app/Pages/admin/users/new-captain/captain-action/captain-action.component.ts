import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { DeliveryManAdminClient, GetDeliveryManRequestDetailsDto } from '../../../../../Core/services/NaqlahClient';
import { LanguageService } from '../../../../../Core/services/language.service';
import { ConfirmationModalComponent } from '../../../../../shared/components/confirmation-modal/confirmation-modal.component';
import { ToasterService } from '../../../../../Core/services/toaster.service';
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
  imports: [CommonModule, TranslateModule, ConfirmationModalComponent],
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

  // Confirmation Modal Properties
  showConfirmModal = false;
  confirmationTitle = '';
  confirmationMessage = '';
  private pendingAction?: () => void;

  private sub = new SubSink();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private deliveryManClient: DeliveryManAdminClient,
    private languageService: LanguageService,
    private toasterService: ToasterService
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
          
          // Extract error message from backend response
          let errorMessage = 'حدث خطأ في جلب البيانات';
          if (error?.error) {
            if (error.error.detail) {
              errorMessage = error.error.detail;
            } else if (error.error.errorMessage) {
              errorMessage = error.error.errorMessage;
            } else if (error.error.title) {
              errorMessage = error.error.title;
            } else if (error.error.message) {
              errorMessage = error.error.message;
            } else if (typeof error.error === 'string') {
              errorMessage = error.error;
            }
          } else if (error?.message) {
            errorMessage = error.message;
          }
          
          this.toasterService.error('خطأ', errorMessage);
          this.isLoading = false;
        }
      });
  }

  goBack() {
    this.router.navigate(['/admin/users/newCaptain']);
  }

  approveDeliveryMan() {
    if (!this.deliveryManDetails || this.actionInProgress.approve) return;

    const captainName = this.deliveryManDetails.fullName || 'الكابتن';
    this.confirmationTitle = 'تأكيد الموافقة';
    this.confirmationMessage = `هل أنت متأكد من موافقة طلب الكابتن "${captainName}"؟`;
    this.pendingAction = () => this.executeStateChange(DeliveryRequesState.Approved, 'approve');
    this.showConfirmModal = true;
  }

  rejectDeliveryMan() {
    if (!this.deliveryManDetails || this.actionInProgress.reject) return;

    const captainName = this.deliveryManDetails.fullName || 'الكابتن';
    this.confirmationTitle = 'تأكيد الرفض';
    this.confirmationMessage = `هل أنت متأكد من رفض طلب الكابتن "${captainName}"؟`;
    this.pendingAction = () => this.executeStateChange(DeliveryRequesState.Rejected, 'reject');
    this.showConfirmModal = true;
  }

  suspendDeliveryMan() {
    if (!this.deliveryManDetails || this.actionInProgress.suspend) return;

    const captainName = this.deliveryManDetails.fullName || 'الكابتن';
    this.confirmationTitle = 'تأكيد التعليق';
    this.confirmationMessage = `هل أنت متأكد من تعليق حساب الكابتن "${captainName}"؟`;
    this.pendingAction = () => this.executeStateChange(DeliveryRequesState.Suspended, 'suspend');
    this.showConfirmModal = true;
  }

  blockDeliveryMan() {
    if (!this.deliveryManDetails || this.actionInProgress.block) return;

    const captainName = this.deliveryManDetails.fullName || 'الكابتن';
    this.confirmationTitle = 'تأكيد الحظر';
    this.confirmationMessage = `هل أنت متأكد من حظر الكابتن "${captainName}"؟ هذا الإجراء سيمنع الكابتن من استخدام التطبيق.`;
    this.pendingAction = () => this.executeStateChange(DeliveryRequesState.Blocked, 'block');
    this.showConfirmModal = true;
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
          this.actionInProgress[actionType] = false;

          // Reload delivery man details to get updated state and active status
          this.loadDeliveryManDetails();

          // إظهار رسالة نجاح باستخدام Toaster
          const successMessage = this.getActionSuccessMessage(actionType);
          this.toasterService.success('نجح العملية', successMessage);
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

          // Extract error message from backend response
          let errorMessage = 'حدث خطأ في تحديث حالة الكابتن';
          if (error?.error) {
            if (error.error.detail) {
              errorMessage = error.error.detail;
            } else if (error.error.errorMessage) {
              errorMessage = error.error.errorMessage;
            } else if (error.error.title) {
              errorMessage = error.error.title;
            } else if (error.error.message) {
              errorMessage = error.error.message;
            } else if (typeof error.error === 'string') {
              errorMessage = error.error;
            }
          } else if (error?.message) {
            errorMessage = error.message;
          }
          
          this.toasterService.error('خطأ', errorMessage);
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

  // Confirmation Modal Methods
  onConfirmationConfirmed(): void {
    this.showConfirmModal = false;
    if (this.pendingAction) {
      this.pendingAction();
      this.pendingAction = undefined;
    }
  }

  onConfirmationCancelled(): void {
    this.showConfirmModal = false;
    this.pendingAction = undefined;
  }

  ngOnDestroy() {
    this.sub.unsubscribe();
  }
}
