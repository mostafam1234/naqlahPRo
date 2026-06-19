import { CommonModule, NgClass, NgFor, NgIf } from '@angular/common';
import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  OnDestroy,
  OnInit
} from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { of } from 'rxjs';
import {
  catchError,
  distinctUntilChanged,
  filter,
  finalize,
  map,
  switchMap,
  tap
} from 'rxjs/operators';
import { DeliveryManAdminClient, GetDeliveryManRequestDetailsDto } from '../../../../../Core/services/NaqlahClient';
import { ToasterService } from '../../../../../Core/services/toaster.service';
import { ConfirmationModalComponent } from '../../../../../shared/components/confirmation-modal/confirmation-modal.component';
import { SubSink } from 'subsink';
import {
  getCaptainDetailDocuments,
  getVehicleOwnerTypeLabelKey
} from '../../captain/captain-form.helpers';

enum DeliveryRequesState {
  Pending = 1,
  Approved = 2,
  Rejected = 3,
  Blocked = 4,
  Suspended = 5
}

interface InfoItem {
  label: string;
  value: string;
  badge?: 'success' | 'danger' | 'warning' | 'neutral' | 'primary';
  warn?: boolean;
}

interface DocumentViewItem {
  key: string;
  labelKey: string;
  url: string | null;
  optional?: boolean;
}

@Component({
  selector: 'app-captain-action',
  standalone: true,
  imports: [CommonModule, NgFor, NgClass, NgIf, TranslateModule, ConfirmationModalComponent],
  templateUrl: './captain-action.component.html',
  styleUrl: './captain-action.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CaptainActionComponent implements OnInit, OnDestroy {
  deliveryManDetails: GetDeliveryManRequestDetailsDto | null = null;
  isLoading = false;
  deliveryManId = 0;
  previewImage: string | null = null;
  previewLabel = '';

  displayName = 'غير متوفر';
  avatarUrl: string | null = null;
  deliveryTypeLabel = 'غير متوفر';
  personalInfo: InfoItem[] = [];
  licenseInfo: InfoItem[] = [];
  vehicleInfo: InfoItem[] = [];
  summaryInfo: InfoItem[] = [];
  documentItems: DocumentViewItem[] = [];

  actionInProgress = {
    approve: false,
    reject: false,
    suspend: false,
    block: false
  };

  showConfirmModal = false;
  confirmationTitle = '';
  confirmationMessage = '';
  private pendingAction?: () => void;
  private sub = new SubSink();

  constructor(
    private route: ActivatedRoute,
    private deliveryManClient: DeliveryManAdminClient,
    private toasterService: ToasterService,
    private translate: TranslateService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.sub.add(
      this.route.paramMap.pipe(
        map((params) => Number(params.get('id'))),
        filter((id) => id > 0),
        distinctUntilChanged(),
        tap((id) => {
          this.deliveryManId = id;
          this.isLoading = true;
          this.deliveryManDetails = null;
          this.resetViewState();
          this.cdr.markForCheck();
        }),
        switchMap((id) =>
          this.deliveryManClient.getDeliveryManDetails(id).pipe(
            catchError((error) => {
              const errorMessage =
                error?.error?.detail ||
                error?.error?.errorMessage ||
                error?.error?.title ||
                'حدث خطأ في جلب البيانات';
              this.toasterService.error('خطأ', errorMessage);
              return of(null);
            }),
            finalize(() => {
              this.isLoading = false;
              this.cdr.markForCheck();
            })
          )
        )
      ).subscribe((response) => {
        this.deliveryManDetails = response;
        this.buildViewState();
        this.cdr.markForCheck();
      })
    );
  }

  ngOnDestroy(): void {
    this.sub.unsubscribe();
  }

  private reloadDetails(): void {
    if (!this.deliveryManId) return;

    this.isLoading = true;
    this.cdr.markForCheck();

    this.sub.add(
      this.deliveryManClient.getDeliveryManDetails(this.deliveryManId).pipe(
        catchError((error) => {
          const errorMessage =
            error?.error?.detail ||
            error?.error?.errorMessage ||
            'حدث خطأ في جلب البيانات';
          this.toasterService.error('خطأ', errorMessage);
          return of(null);
        }),
        finalize(() => {
          this.isLoading = false;
          this.cdr.markForCheck();
        })
      ).subscribe((response) => {
        this.deliveryManDetails = response;
        this.buildViewState();
        this.cdr.markForCheck();
      })
    );
  }

  private resetViewState(): void {
    this.displayName = 'غير متوفر';
    this.avatarUrl = null;
    this.deliveryTypeLabel = 'غير متوفر';
    this.personalInfo = [];
    this.licenseInfo = [];
    this.vehicleInfo = [];
    this.summaryInfo = [];
    this.documentItems = [];
  }

  private buildViewState(): void {
    const d = this.deliveryManDetails;
    if (!d) {
      this.resetViewState();
      return;
    }

    this.displayName = d.fullName || 'غير متوفر';
    this.avatarUrl = d.personalImagePath || d.frontIdentityImagePath || null;
    this.deliveryTypeLabel = this.getDeliveryTypeLabel(d.deliveryType);

    this.personalInfo = [
      { label: 'الاسم الكامل', value: d.fullName || '—' },
      { label: 'رقم الهاتف', value: d.phoneNumber || '—' },
      { label: 'البريد الإلكتروني', value: d.email || '—' },
      { label: 'رقم الهوية', value: d.identityNumber || '—' },
      { label: 'تاريخ الميلاد', value: this.formatDate(d.birthDate) },
      { label: 'العنوان', value: d.address || '—' },
      { label: 'نوع الكابتن', value: this.deliveryTypeLabel, badge: 'primary' },
      { label: 'الحالة', value: d.active ? 'نشط' : 'غير نشط', badge: d.active ? 'success' : 'danger' },
      { label: 'حالة الطلب', value: d.stateName || this.getStateLabel(d.state), badge: 'neutral' }
    ];

    this.licenseInfo = [
      { label: 'نوع الرخصة', value: this.getDeliveryLicenseTypeLabel(d.deliveryLicenseType) },
      { label: 'تاريخ انتهاء الهوية', value: this.formatDate(d.identityExpirationDate) },
      {
        label: 'حالة الهوية',
        value: this.isIdentityValid(d) ? 'سارية' : 'منتهية / غير محددة',
        badge: this.isIdentityValid(d) ? 'success' : 'danger'
      },
      { label: 'تاريخ انتهاء رخصة القيادة', value: this.formatDate(d.drivingLicenseExpirationDate) },
      {
        label: 'حالة رخصة القيادة',
        value: this.isLicenseValid(d) ? 'سارية' : 'منتهية / غير محددة',
        badge: this.isLicenseValid(d) ? 'success' : 'danger'
      }
    ];

    this.vehicleInfo = [
      { label: 'نوع المركبة', value: d.vehicleType || '—' },
      { label: 'ماركة المركبة', value: d.vehicleModel || '—' },
      { label: 'رقم اللوحة', value: d.vehiclePlateNumber || '—' },
      { label: 'نوع مالك المركبة', value: this.translate.instant(getVehicleOwnerTypeLabelKey(d.vehicleOwnerTypeId)) },
      { label: 'اسم المالك', value: d.vehicleOwnerName || '—' },
      { label: 'رقم هوية المالك', value: d.vehicleOwnerIdentityNumber || '—' },
      { label: 'رقم السجل التجاري', value: d.commercialRecordNumber || '—' },
      { label: 'الرقم الضريبي', value: d.taxNumber || '—' },
      { label: 'حساب بنك المالك', value: d.ownerBankAccountNumber || '—' },
      { label: 'انتهاء الاستمارة', value: this.formatDate(d.vehicleLicenseExpirationDate) },
      { label: 'انتهاء التأمين', value: this.formatDate(d.vehicleInsuranceExpirationDate) }
    ];

    this.summaryInfo = [
      { label: 'معرّف الكابتن', value: `#${d.deliveryManId}` },
      { label: 'معرّف المستخدم', value: `#${d.userId}` },
      { label: 'معرّف المركبة', value: d.vehicleId ? `#${d.vehicleId}` : '—' },
      { label: 'البريد الإلكتروني', value: d.email || '—' },
      {
        label: 'اكتمال التسجيل',
        value: d.hasIncompleteRegistration ? 'بيانات غير مكتملة' : 'بيانات مكتملة',
        warn: d.hasIncompleteRegistration,
        badge: d.hasIncompleteRegistration ? undefined : 'success'
      }
    ];

    this.documentItems = getCaptainDetailDocuments(d).map((doc) => ({
      key: doc.key,
      labelKey: doc.labelKey,
      url: doc.getUrl(d) || null,
      optional: doc.optional
    }));
  }

  trackDocument(_index: number, doc: DocumentViewItem): string {
    return doc.key;
  }

  trackInfoItem(_index: number, item: InfoItem): string {
    return item.label;
  }

  goBack(): void {
    window.history.back();
  }

  approveDeliveryMan(): void {
    this.openConfirm('تأكيد الموافقة', `هل أنت متأكد من موافقة طلب الكابتن "${this.displayName}"؟`, () =>
      this.executeStateChange(DeliveryRequesState.Approved, 'approve')
    );
  }

  rejectDeliveryMan(): void {
    this.openConfirm('تأكيد الرفض', `هل أنت متأكد من رفض طلب الكابتن "${this.displayName}"؟`, () =>
      this.executeStateChange(DeliveryRequesState.Rejected, 'reject')
    );
  }

  suspendDeliveryMan(): void {
    this.openConfirm('تأكيد التعليق', `هل أنت متأكد من تعليق حساب الكابتن "${this.displayName}"؟`, () =>
      this.executeStateChange(DeliveryRequesState.Suspended, 'suspend')
    );
  }

  blockDeliveryMan(): void {
    this.openConfirm('تأكيد الحظر', `هل أنت متأكد من حظر الكابتن "${this.displayName}"؟`, () =>
      this.executeStateChange(DeliveryRequesState.Blocked, 'block')
    );
  }

  private openConfirm(title: string, message: string, action: () => void): void {
    if (!this.deliveryManDetails) return;
    this.confirmationTitle = title;
    this.confirmationMessage = message;
    this.pendingAction = action;
    this.showConfirmModal = true;
    this.cdr.markForCheck();
  }

  private executeStateChange(state: DeliveryRequesState, actionType: keyof typeof this.actionInProgress): void {
    if (!this.deliveryManDetails) return;
    this.actionInProgress[actionType] = true;
    this.cdr.markForCheck();

    this.sub.add(
      this.deliveryManClient
        .updateDeliveryManState(this.deliveryManDetails.deliveryManId, state as number)
        .pipe(
          finalize(() => {
            this.actionInProgress[actionType] = false;
            this.cdr.markForCheck();
          })
        )
        .subscribe({
          next: () => {
            this.reloadDetails();
            this.toasterService.success('نجحت العملية', this.getActionSuccessMessage(actionType));
          },
          error: (error) => {
            const errorMessage =
              error?.error?.detail || error?.error?.errorMessage || 'حدث خطأ في تحديث حالة الكابتن';
            this.toasterService.error('خطأ', errorMessage);
          }
        })
    );
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

  formatDate(date: Date | string | null | undefined): string {
    if (!date) return '—';
    const d = typeof date === 'string' ? new Date(date) : date;
    return isNaN(d.getTime()) ? '—' : d.toLocaleDateString('ar-EG');
  }

  getDeliveryTypeLabel(deliveryType?: string | null): string {
    if (!deliveryType) return 'غير متوفر';
    const type = deliveryType.toLowerCase();
    if (type.includes('resident') || type.includes('مقيم')) return 'مقيم';
    if (type.includes('citizen') || type.includes('مواطن')) return 'مواطن';
    return deliveryType;
  }

  getDeliveryLicenseTypeLabel(licenseType?: string | null): string {
    if (!licenseType) return 'غير متوفر';
    const type = licenseType.toLowerCase();
    if (type.includes('public') || type.includes('عامة')) return 'رخصة عامة';
    if (type.includes('private') || type.includes('خاصة')) return 'رخصة خاصة';
    return licenseType;
  }

  isIdentityValid(details: GetDeliveryManRequestDetailsDto): boolean {
    if (!details.identityExpirationDate) return false;
    return new Date(details.identityExpirationDate) > new Date();
  }

  isLicenseValid(details: GetDeliveryManRequestDetailsDto): boolean {
    if (!details.drivingLicenseExpirationDate) return false;
    return new Date(details.drivingLicenseExpirationDate) > new Date();
  }

  getStateLabel(state?: string | null): string {
    switch (state) {
      case 'New':
      case 'Pending': return 'قيد المراجعة';
      case 'Approved': return 'موافق عليه';
      case 'Rejected': return 'مرفوض';
      case 'Suspended': return 'معلق';
      case 'Blocked': return 'محظور';
      default: return state || '—';
    }
  }

  openPreview(url: string | null | undefined, labelKey: string): void {
    if (!url) return;
    this.previewImage = url;
    this.previewLabel = this.translate.instant(labelKey);
    this.cdr.markForCheck();
  }

  closePreview(): void {
    this.previewImage = null;
    this.previewLabel = '';
    this.cdr.markForCheck();
  }

  onConfirmationConfirmed(): void {
    this.showConfirmModal = false;
    this.pendingAction?.();
    this.pendingAction = undefined;
    this.cdr.markForCheck();
  }

  onConfirmationCancelled(): void {
    this.showConfirmModal = false;
    this.pendingAction = undefined;
    this.cdr.markForCheck();
  }
}
