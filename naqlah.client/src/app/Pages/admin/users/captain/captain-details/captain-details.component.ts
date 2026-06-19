import { DatePipe, NgClass, NgFor, NgIf } from '@angular/common';
import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  OnDestroy,
  OnInit
} from '@angular/core';
import { ActivatedRoute, RouterModule } from '@angular/router';
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
import { PageHeaderComponent } from 'src/app/shared/components/page-header/page-header.component';
import { ConfirmationModalComponent } from 'src/app/shared/components/confirmation-modal/confirmation-modal.component';
import { ToasterService } from 'src/app/Core/services/toaster.service';
import {
  DeliveryManActiveHistoryResponseDto,
  DeliveryManAdminClient,
  GetDeliveryManRequestDetailsDto
} from 'src/app/Core/services/NaqlahClient';
import { SubSink } from 'subsink';
import {
  getCaptainDetailDocuments,
  getVehicleOwnerTypeLabelKey
} from '../captain-form.helpers';
import { CaptainActiveHistoryModalComponent } from '../captain-active-history-modal/captain-active-history-modal.component';
import { triggerFileDownload, mapFileResponse } from '../../../orders/captain-orders.helpers';

interface InfoItem {
  label: string;
  value: string;
  badge?: 'success' | 'danger' | 'warning' | 'neutral' | 'primary';
}

interface DocumentViewItem {
  key: string;
  labelKey: string;
  url: string | null;
  optional?: boolean;
}

@Component({
  selector: 'app-captain-details',
  standalone: true,
  imports: [
    NgFor, NgClass, NgIf, TranslateModule, RouterModule, PageHeaderComponent,
    DatePipe, ConfirmationModalComponent, CaptainActiveHistoryModalComponent
  ],
  templateUrl: './captain-details.component.html',
  styleUrl: './captain-details.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CaptainDetailsComponent implements OnInit, OnDestroy {
  captainDetails: GetDeliveryManRequestDetailsDto | null = null;
  deliveryManId = 0;
  isLoading = false;
  isTogglingActive = false;
  previewImage: string | null = null;
  previewLabel = '';

  showHistoryModal = false;
  isLoadingHistory = false;
  isExportingHistory = false;
  activeHistory: DeliveryManActiveHistoryResponseDto | null = null;

  showConfirmModal = false;
  confirmationTitle = '';
  confirmationMessage = '';
  private pendingActiveValue: boolean | null = null;

  displayName = 'غير متوفر';
  avatarUrl: string | null = null;
  deliveryTypeLabel = 'غير متوفر';
  personalInfo: InfoItem[] = [];
  licenseInfo: InfoItem[] = [];
  vehicleInfo: InfoItem[] = [];
  documentItems: DocumentViewItem[] = [];

  private subs = new SubSink();

  constructor(
    private route: ActivatedRoute,
    private deliveryManClient: DeliveryManAdminClient,
    private toasterService: ToasterService,
    private translate: TranslateService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.subs.add(
      this.route.paramMap.pipe(
        map((params) => Number(params.get('id'))),
        filter((id) => id > 0),
        distinctUntilChanged(),
        tap((id) => {
          this.deliveryManId = id;
          this.isLoading = true;
          this.captainDetails = null;
          this.resetViewState();
          this.cdr.markForCheck();
        }),
        switchMap((id) =>
          this.deliveryManClient.getDeliveryManDetails(id).pipe(
            catchError(() => of(null)),
            finalize(() => {
              this.isLoading = false;
              this.cdr.markForCheck();
            })
          )
        )
      ).subscribe((response) => {
        this.captainDetails = response;
        this.buildViewState();
        this.cdr.markForCheck();
      })
    );
  }

  ngOnDestroy(): void {
    this.subs.unsubscribe();
  }

  get isApprovedCaptain(): boolean {
    const state = this.captainDetails?.state?.toLowerCase() ?? '';
    return state === 'approved' || this.captainDetails?.stateName?.includes('موافق') === true;
  }

  requestToggleActive(): void {
    if (!this.captainDetails || !this.isApprovedCaptain || this.isTogglingActive) return;

    const nextActive = !this.captainDetails.active;
    this.pendingActiveValue = nextActive;
    this.confirmationTitle = nextActive ? 'تفعيل الكابتن' : 'إيقاف الكابتن';
    this.confirmationMessage = nextActive
      ? `هل تريد تفعيل الكابتن "${this.displayName}" وجعله نشطاً؟`
      : `هل تريد إيقاف الكابتن "${this.displayName}" وجعله غير نشط؟`;
    this.showConfirmModal = true;
    this.cdr.markForCheck();
  }

  onConfirmationConfirmed(): void {
    this.showConfirmModal = false;
    if (this.pendingActiveValue === null) return;

    const active = this.pendingActiveValue;
    this.pendingActiveValue = null;
    this.setActiveStatus(active);
    this.cdr.markForCheck();
  }

  onConfirmationCancelled(): void {
    this.showConfirmModal = false;
    this.pendingActiveValue = null;
    this.cdr.markForCheck();
  }

  private setActiveStatus(active: boolean): void {
    this.isTogglingActive = true;
    this.cdr.markForCheck();

    this.subs.add(
      this.deliveryManClient.setDeliveryManActiveStatus(this.deliveryManId, active).pipe(
        catchError((error) => {
          const msg = error?.error?.detail || error?.error?.errorMessage || 'تعذر تغيير حالة الكابتن';
          this.toasterService.error('خطأ', msg);
          return of(null);
        }),
        finalize(() => {
          this.isTogglingActive = false;
          this.cdr.markForCheck();
        })
      ).subscribe((result) => {
        if (!result) return;

        if (this.captainDetails) {
          this.captainDetails.active = result.active;
        }
        this.buildViewState();

        if (result.statusChanged) {
          this.toasterService.success('تم', `تم تحديث الحالة إلى ${result.activeStatusName}`);
          if (this.showHistoryModal) {
            this.loadActiveHistory(false);
          }
        } else {
          this.toasterService.success('تم', `الكابتن بالفعل ${result.activeStatusName}`);
        }
        this.cdr.markForCheck();
      })
    );
  }

  openActiveHistory(): void {
    this.showHistoryModal = true;
    this.loadActiveHistory(true);
    this.cdr.markForCheck();
  }

  closeActiveHistory(): void {
    this.showHistoryModal = false;
    this.cdr.markForCheck();
  }

  exportActiveHistory(): void {
    if (!this.deliveryManId || this.isExportingHistory) return;

    this.isExportingHistory = true;
    this.cdr.markForCheck();

    this.subs.add(
      this.deliveryManClient.exportDeliveryManActiveHistory(this.deliveryManId).pipe(
        map((file) => mapFileResponse(file, `CaptainActiveHistory_${this.deliveryManId}_${Date.now()}.xlsx`)),
        catchError(() => {
          this.toasterService.error('خطأ', 'تعذر تصدير سجل النشاط');
          return of(null);
        }),
        finalize(() => {
          this.isExportingHistory = false;
          this.cdr.markForCheck();
        })
      ).subscribe((result) => {
        if (!result) return;
        triggerFileDownload(result.blob, result.fileName);
        this.toasterService.success('تم', 'تم تصدير سجل النشاط بنجاح');
      })
    );
  }

  private loadActiveHistory(showLoader: boolean): void {
    if (showLoader) {
      this.isLoadingHistory = true;
      this.activeHistory = null;
      this.cdr.markForCheck();
    }

    this.subs.add(
      this.deliveryManClient.getDeliveryManActiveHistory(this.deliveryManId).pipe(
        catchError(() => {
          this.toasterService.error('خطأ', 'تعذر تحميل سجل النشاط');
          return of(null);
        }),
        finalize(() => {
          this.isLoadingHistory = false;
          this.cdr.markForCheck();
        })
      ).subscribe((data) => {
        this.activeHistory = data;
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
    this.documentItems = [];
  }

  private buildViewState(): void {
    const d = this.captainDetails;
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
      {
        label: 'الحالة',
        value: d.active ? 'نشط' : 'غير نشط',
        badge: d.active ? 'success' : 'danger'
      }
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

    this.documentItems = getCaptainDetailDocuments(d).map((doc) => ({
      key: doc.key,
      labelKey: doc.labelKey,
      url: doc.getUrl(d) || null,
      optional: doc.optional
    }));
  }

  trackInfoItem(_index: number, item: InfoItem): string {
    return item.label;
  }

  trackDocument(_index: number, doc: DocumentViewItem): string {
    return doc.key;
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

  formatDate(date: Date | string | null | undefined): string {
    if (!date) return '—';
    const dateObj = typeof date === 'string' ? new Date(date) : date;
    return isNaN(dateObj.getTime()) ? '—' : dateObj.toLocaleDateString('ar-EG');
  }

  isIdentityValid(details: GetDeliveryManRequestDetailsDto): boolean {
    if (!details.identityExpirationDate) return false;
    return new Date(details.identityExpirationDate) > new Date();
  }

  isLicenseValid(details: GetDeliveryManRequestDetailsDto): boolean {
    if (!details.drivingLicenseExpirationDate) return false;
    return new Date(details.drivingLicenseExpirationDate) > new Date();
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

  goBack(): void {
    window.history.back();
  }
}
