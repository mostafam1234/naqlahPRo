import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { SubSink } from 'subsink';
import { DeliveryManAdminClient, GetDeliveryManRequestDetailsDto, VehicleAdminClient, VehicleTypeDto } from 'src/app/Core/services/NaqlahClient';
import { PageHeaderComponent } from 'src/app/shared/components/page-header/page-header.component';
import { DeliveryLicenseType, DeliveryType, VehicleOwnerType } from 'src/app/Core/enums/delivery.enums';
import { ImageService } from 'src/app/Core/services/image.service';
import { ToasterService } from 'src/app/Core/services/toaster.service';
import {
  applyOwnerValidators,
  buildCaptainForm,
  CAPTAIN_IMAGE_MAP,
  CaptainDocumentItem,
  formValueToApiDto,
  getVisibleCaptainDocuments,
  isCaptainDocumentRequired
} from '../captain-form.helpers';

@Component({
  selector: 'app-edit-captain',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule, PageHeaderComponent],
  providers: [DatePipe, DeliveryManAdminClient, VehicleAdminClient],
  templateUrl: './edit-captain.component.html',
  styleUrl: './edit-captain.component.css'
})
export class EditCaptainComponent implements OnInit, OnDestroy {
  captainForm: FormGroup;
  isLoading = false;
  isSubmitting = false;
  deliveryManId!: number;
  deliveryManDetails: GetDeliveryManRequestDetailsDto | null = null;
  readonly VehicleOwnerType = VehicleOwnerType;

  imagesPreviews: Record<string, string | null> = {};
  formSections = {
    personalInfo: true,
    identityInfo: true,
    drivingLicense: true,
    vehicleInfo: true,
    documentsUpload: true
  };

  deliveryTypes = [
    { value: DeliveryType.Resident, label: 'مقيم' },
    { value: DeliveryType.Citizen, label: 'مواطن' }
  ];

  deliveryLicenseTypes = [
    { value: DeliveryLicenseType.Public, label: 'رخصة عامة' },
    { value: DeliveryLicenseType.Private, label: 'رخصة خاصة' }
  ];

  vehicleTypes: VehicleTypeDto[] = [];
  vehicleBrands: VehicleTypeDto[] = [];
  private subs = new SubSink();

  formSteps = [
    { key: 'personalInfo', title: 'البيانات الشخصية', icon: 'M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z' },
    { key: 'identityInfo', title: 'الهوية', icon: 'M10 6H5a2 2 0 00-2 2v9a2 2 0 002 2h14a2 2 0 002-2V8a2 2 0 00-2-2h-5m-4 0V5a2 2 0 114 0v1m-4 0a2 2 0 104 0m-5 8a2 2 0 100-4 2 2 0 000 4zm0 0c1.306 0 2.417.835 2.83 2M9 14a3.001 3.001 0 00-2.83 2M15 11h3m-3 4h2' },
    { key: 'drivingLicense', title: 'رخصة القيادة', icon: 'M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z' },
    { key: 'vehicleInfo', title: 'المركبة', icon: 'M9 17a2 2 0 11-4 0 2 2 0 014 0zM19 17a2 2 0 11-4 0 2 2 0 014 0z' },
    { key: 'documentsUpload', title: 'الوثائق', icon: 'M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12' }
  ];

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private route: ActivatedRoute,
    private translate: TranslateService,
    private deliveryManClient: DeliveryManAdminClient,
    private datePipe: DatePipe,
    private vehicleClient: VehicleAdminClient,
    private imageService: ImageService,
    private toasterService: ToasterService
  ) {
    this.captainForm = buildCaptainForm(this.fb, true);
    this.subs.sink = this.captainForm.get('vehicleOwnerTypeId')!.valueChanges.subscribe(() =>
      applyOwnerValidators(this.captainForm)
    );
    applyOwnerValidators(this.captainForm);
  }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.deliveryManId = +params['id'];
      if (this.deliveryManId) this.loadDeliveryManDetails();
    });
    this.subs.sink = this.vehicleClient.getVehiclesBrandLookup().subscribe({
      next: (brands) => { this.vehicleBrands = brands; }
    });
    this.subs.sink = this.vehicleClient.getVehiclesTypesLookup().subscribe({
      next: (types) => { this.vehicleTypes = types; }
    });
  }

  ngOnDestroy(): void {
    this.subs.unsubscribe();
  }

  loadDeliveryManDetails(): void {
    this.isLoading = true;
    this.subs.sink = this.deliveryManClient.getDeliveryManDetails(this.deliveryManId).subscribe({
      next: (details) => {
        this.deliveryManDetails = details;
        this.populateForm(details);
        this.isLoading = false;
      },
      error: (error) => {
        this.isLoading = false;
        this.toasterService.error('خطأ', error?.errorMessage || 'حدث خطأ أثناء تحميل بيانات الكابتن');
        setTimeout(() => this.router.navigate(['/admin/users/captain']), 2000);
      }
    });
  }

  private populateForm(details: GetDeliveryManRequestDetailsDto): void {
    const toInputDate = (date: Date | null | undefined): string => {
      if (!date) return '';
      const dateObj = date instanceof Date ? date : new Date(date);
      return isNaN(dateObj.getTime()) ? '' : (this.datePipe.transform(dateObj, 'yyyy-MM-dd') || '');
    };

    let deliveryLicenseTypeValue = DeliveryLicenseType.Private;
    if (details.deliveryLicenseType?.toLowerCase().includes('public')) {
      deliveryLicenseTypeValue = DeliveryLicenseType.Public;
    }

    let deliveryTypeValue = DeliveryType.Resident;
    const deliveryTypeText = details.deliveryType?.toLowerCase() ?? '';
    if (deliveryTypeText.includes('citizen') || deliveryTypeText.includes('مواطن')) {
      deliveryTypeValue = DeliveryType.Citizen;
    }

    const d = details as GetDeliveryManRequestDetailsDto & {
      birthDate?: Date | null;
      vehicleOwnerTypeId?: number | null;
      vehicleOwnerName?: string | null;
      vehicleOwnerIdentityNumber?: string | null;
      commercialRecordNumber?: string | null;
      vehicleLicenseExpirationDate?: Date | null;
      vehicleInsuranceExpirationDate?: Date | null;
      ownerFrontIdentityImagePath?: string | null;
      commercialRecordImagePath?: string | null;
      rentContractImagePath?: string | null;
    };

    this.captainForm.patchValue({
      email: details.email || '',
      fullName: details.fullName || '',
      phoneNumber: details.phoneNumber || '',
      identityNumber: details.identityNumber || '',
      birthDate: toInputDate(d.birthDate),
      deliveryType: deliveryTypeValue,
      address: details.address || '',
      identityExpirationDate: toInputDate(details.identityExpirationDate),
      deliveryLicenseType: deliveryLicenseTypeValue,
      drivingLicenseExpirationDate: toInputDate(details.drivingLicenseExpirationDate),
      vehicleTypeId: details.vehicleTypeId || null,
      vehicleBrandId: details.vehicleBrandId || null,
      vehiclePlateNumber: details.vehiclePlateNumber || '',
      vehicleOwnerTypeId: d.vehicleOwnerTypeId ?? VehicleOwnerType.Resident,
      vehicleOwnerName: d.vehicleOwnerName || '',
      vehicleOwnerIdentityNumber: d.vehicleOwnerIdentityNumber || '',
      commercialRecordNumber: d.commercialRecordNumber || '',
      vehicleLicenseExpirationDate: toInputDate(d.vehicleLicenseExpirationDate),
      vehicleInsuranceExpirationDate: toInputDate(d.vehicleInsuranceExpirationDate),
      frontIdentityImagePath: details.frontIdentityImagePath || '',
      frontDrivingLicenseImagePath: details.frontDrivingLicenseImagePath || '',
      personalImagePath: details.personalImagePath || '',
      vehicleFrontImagePath: details.vehicleFrontImagePath || '',
      vehicleSideImagePath: details.vehicleSideImagePath || '',
      vehicleFrontLicenseImagePath: details.vehicleFrontLicenseImagePath || '',
      vehicleFrontInsuranceImagePath: details.vehicleFrontInsuranceImagePath || '',
      ownerFrontIdentityImagePath: d.ownerFrontIdentityImagePath || '',
      commercialRecordImagePath: d.commercialRecordImagePath || '',
      rentContractImagePath: d.rentContractImagePath || ''
    });

    this.imagesPreviews['personalImage'] = details.personalImagePath || null;
    this.imagesPreviews['frontIdentityImage'] = details.frontIdentityImagePath || null;
    this.imagesPreviews['frontDrivingLicenseImage'] = details.frontDrivingLicenseImagePath || null;
    this.imagesPreviews['vehicleFrontImage'] = details.vehicleFrontImagePath || null;
    this.imagesPreviews['vehicleSideImage'] = details.vehicleSideImagePath || null;
    this.imagesPreviews['vehicleFrontLicenseImage'] = details.vehicleFrontLicenseImagePath || null;
    this.imagesPreviews['vehicleFrontInsuranceImage'] = details.vehicleFrontInsuranceImagePath || null;
    this.imagesPreviews['ownerFrontIdentityImage'] = d.ownerFrontIdentityImagePath || null;
    this.imagesPreviews['commercialRecordImage'] = d.commercialRecordImagePath || null;
    this.imagesPreviews['rentContractImage'] = d.rentContractImagePath || null;

    applyOwnerValidators(this.captainForm);
  }

  isOwnerCompany(): boolean {
    return Number(this.captainForm.get('vehicleOwnerTypeId')?.value) === VehicleOwnerType.Company;
  }

  isOwnerRenter(): boolean {
    return Number(this.captainForm.get('vehicleOwnerTypeId')?.value) === VehicleOwnerType.Renter;
  }

  isOwnerIndividual(): boolean {
    const t = Number(this.captainForm.get('vehicleOwnerTypeId')?.value);
    return t === VehicleOwnerType.Resident || t === VehicleOwnerType.Renter;
  }

  isFieldRequired(fieldName: string): boolean {
    return !!this.captainForm.get(fieldName)?.hasValidator(Validators.required);
  }

  get visibleDocuments(): CaptainDocumentItem[] {
    const ownerType = Number(this.captainForm.get('vehicleOwnerTypeId')?.value);
    return getVisibleCaptainDocuments(ownerType);
  }

  isImageRequired(imageType: string): boolean {
    const ownerType = Number(this.captainForm.get('vehicleOwnerTypeId')?.value);
    return isCaptainDocumentRequired(imageType, ownerType);
  }

  getPreviewKey(imageType: string): string {
    return CAPTAIN_IMAGE_MAP[imageType]?.previewKey ?? imageType;
  }

  getUploadInputId(imageType: string): string {
    return `edit-upload-${imageType}`;
  }

  toggleSection(section: string): void {
    const key = section as keyof typeof this.formSections;
    if (key in this.formSections) this.formSections[key] = !this.formSections[key];
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.captainForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  getFieldError(fieldName: string): string {
    const field = this.captainForm.get(fieldName);
    if (field?.errors?.['required']) return this.translate.instant('VALIDATION.REQUIRED');
    if (field?.errors?.['pattern']) return this.translate.instant('VALIDATION.PATTERN');
    return '';
  }

  onSubmit(): void {
    applyOwnerValidators(this.captainForm);
    this.captainForm.markAllAsTouched();

    if (this.captainForm.invalid) {
      this.toasterService.error('بيانات ناقصة أو غير صحيحة', 'يرجى تعبئة جميع الحقول المطلوبة');
      return;
    }

    this.isSubmitting = true;
    const body = formValueToApiDto(this.captainForm.getRawValue(), this.datePipe, true);

    this.subs.sink = this.deliveryManClient.updateDeliveryMan(this.deliveryManId, body).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.toasterService.success('تمت العملية بنجاح', 'تم تحديث بيانات الكابتن بنجاح');
        setTimeout(() => this.router.navigate(['/admin/users/captain']), 1500);
      },
      error: (error) => {
        this.isSubmitting = false;
        this.toasterService.error('خطأ', error?.errorMessage || 'حدث خطأ أثناء تحديث بيانات الكابتن');
      }
    });
  }

  async onImageSelected(event: Event, imageType: string): Promise<void> {
    const result = await this.imageService.handleImageUpload(event, { maxSizeMB: 5, showErrorAlert: true });
    if (!result?.success) return;

    const mapping = CAPTAIN_IMAGE_MAP[imageType];
    if (!mapping) return;

    this.imagesPreviews[mapping.previewKey] = result.preview || null;
    this.captainForm.patchValue({ [mapping.formField]: result.base64 });
  }

  removeImage(imageType: string): void {
    const mapping = CAPTAIN_IMAGE_MAP[imageType];
    if (!mapping) return;

    this.imagesPreviews[mapping.previewKey] = null;
    this.captainForm.patchValue({ [mapping.formField]: '' });
  }

  onCancel(): void {
    this.router.navigate(['/admin/users/captain']);
  }

  getStepClass(stepKey: string): string {
    if (this.formSections[stepKey as keyof typeof this.formSections]) {
      const activeColors: Record<string, string> = {
        personalInfo: 'bg-primary-500',
        identityInfo: 'bg-emerald-500',
        drivingLicense: 'bg-purple-500',
        vehicleInfo: 'bg-orange-500',
        documentsUpload: 'bg-cyan-500'
      };
      return `${activeColors[stepKey] ?? 'bg-primary-500'} text-white`;
    }
    return 'bg-neutral-300 text-neutral-600';
  }
}
