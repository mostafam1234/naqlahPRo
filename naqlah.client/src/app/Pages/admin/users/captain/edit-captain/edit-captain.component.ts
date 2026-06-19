import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { SubSink } from 'subsink';
import {
  DeliveryManAdminClient,
  GetDeliveryManRequestDetailsDto,
  VehicleAdminClient,
  VehicleTypeDto
} from 'src/app/Core/services/NaqlahClient';
import { PageHeaderComponent } from 'src/app/shared/components/page-header/page-header.component';
import { DeliveryLicenseType, DeliveryType, VehicleOwnerType } from 'src/app/Core/enums/delivery.enums';
import { ImageService } from 'src/app/Core/services/image.service';
import { ToasterService } from 'src/app/Core/services/toaster.service';
import { ConfirmationDialogService } from 'src/app/shared/services/confirmation-service';
import {
  applyOwnerValidators,
  buildCaptainForm,
  CAPTAIN_IMAGE_MAP,
  CaptainDocumentItem,
  formValueToApiDto,
  getVisibleCaptainDocuments,
  isCaptainDocumentRequired
} from '../captain-form.helpers';

interface WizardStep {
  key: string;
  titleKey: string;
  subtitleKey: string;
  fields: string[];
}

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
  currentStep = 0;
  deliveryManId!: number;
  readonly VehicleOwnerType = VehicleOwnerType;
  readonly DeliveryType = DeliveryType;

  imagesPreviews: Record<string, string | null> = {};

  deliveryTypes = [
    {
      value: DeliveryType.Resident,
      labelKey: 'ADMIN.PAGES.CAPTAIN_FORM.DELIVERY_RESIDENT_LABEL',
      descriptionKey: 'ADMIN.PAGES.CAPTAIN_FORM.DELIVERY_RESIDENT_DESC',
      icon: 'M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6'
    },
    {
      value: DeliveryType.Citizen,
      labelKey: 'ADMIN.PAGES.CAPTAIN_FORM.DELIVERY_CITIZEN_LABEL',
      descriptionKey: 'ADMIN.PAGES.CAPTAIN_FORM.DELIVERY_CITIZEN_DESC',
      icon: 'M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z'
    }
  ];

  deliveryLicenseTypes = [
    { value: DeliveryLicenseType.Public, labelKey: 'ADMIN.PAGES.CAPTAIN_FORM.LICENSE_PUBLIC' },
    { value: DeliveryLicenseType.Private, labelKey: 'ADMIN.PAGES.CAPTAIN_FORM.LICENSE_PRIVATE' }
  ];

  vehicleOwnerTypes = [
    { value: VehicleOwnerType.Resident, labelKey: 'ADMIN.PAGES.ADD_CAPTAIN_FIELDS.OWNER_RESIDENT' },
    { value: VehicleOwnerType.Company, labelKey: 'ADMIN.PAGES.ADD_CAPTAIN_FIELDS.OWNER_COMPANY' },
    { value: VehicleOwnerType.Renter, labelKey: 'ADMIN.PAGES.ADD_CAPTAIN_FIELDS.OWNER_LEASE' }
  ];

  wizardSteps: WizardStep[] = [
    {
      key: 'account',
      titleKey: 'ADMIN.PAGES.CAPTAIN_FORM.STEPS.ACCOUNT_TITLE',
      subtitleKey: 'ADMIN.PAGES.CAPTAIN_FORM.STEPS.ACCOUNT_SUBTITLE',
      fields: ['email', 'fullName', 'phoneNumber', 'identityNumber', 'birthDate', 'deliveryType', 'deliveryLicenseType']
    },
    {
      key: 'vehicle',
      titleKey: 'ADMIN.PAGES.CAPTAIN_FORM.STEPS.VEHICLE_TITLE',
      subtitleKey: 'ADMIN.PAGES.CAPTAIN_FORM.STEPS.VEHICLE_SUBTITLE',
      fields: ['vehicleTypeId', 'vehicleBrandId', 'vehiclePlateNumber', 'vehicleOwnerTypeId', 'vehicleOwnerName']
    },
    {
      key: 'documents',
      titleKey: 'ADMIN.PAGES.CAPTAIN_FORM.STEPS.DOCUMENTS_TITLE',
      subtitleKey: 'ADMIN.PAGES.CAPTAIN_FORM.STEPS.DOCUMENTS_SUBTITLE',
      fields: []
    }
  ];

  vehicleTypes: VehicleTypeDto[] = [];
  vehicleBrands: VehicleTypeDto[] = [];
  private subs = new SubSink();

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private route: ActivatedRoute,
    private translate: TranslateService,
    private deliveryManClient: DeliveryManAdminClient,
    private datePipe: DatePipe,
    private vehicleClient: VehicleAdminClient,
    private imageService: ImageService,
    private toasterService: ToasterService,
    private confirmationDialog: ConfirmationDialogService
  ) {
    this.captainForm = buildCaptainForm(this.fb, true);
    this.subs.sink = this.captainForm.get('vehicleOwnerTypeId')!.valueChanges.subscribe(() =>
      applyOwnerValidators(this.captainForm)
    );
    applyOwnerValidators(this.captainForm);
  }

  ngOnInit(): void {
    this.subs.sink = this.route.params.subscribe((params) => {
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

  get visibleDocuments(): CaptainDocumentItem[] {
    const ownerType = Number(this.captainForm.get('vehicleOwnerTypeId')?.value);
    return getVisibleCaptainDocuments(ownerType);
  }

  get totalRequiredCount(): number {
    return this.getRequiredFields().length;
  }

  get completedRequiredCount(): number {
    return this.getRequiredFields().filter((field) => this.isRequiredFieldComplete(field)).length;
  }

  get progressPercent(): number {
    const total = this.totalRequiredCount;
    if (total === 0) return 0;
    return Math.round((this.completedRequiredCount / total) * 100);
  }

  private getRequiredFields(): string[] {
    applyOwnerValidators(this.captainForm);
    return Object.keys(this.captainForm.controls).filter((name) =>
      this.captainForm.get(name)?.hasValidator(Validators.required)
    );
  }

  private isRequiredFieldComplete(fieldName: string): boolean {
    const control = this.captainForm.get(fieldName);
    if (!control) return false;
    const value = control.value;
    if (value === null || value === undefined || value === '') return false;
    return control.valid;
  }

  loadDeliveryManDetails(): void {
    this.isLoading = true;
    this.subs.sink = this.deliveryManClient.getDeliveryManDetails(this.deliveryManId).subscribe({
      next: (details) => {
        this.populateForm(details);
        this.isLoading = false;
      },
      error: (error) => {
        this.isLoading = false;
        this.toasterService.error(
          this.translate.instant('ADMIN.PAGES.CAPTAIN_FORM.TOAST.ERROR_TITLE'),
          error?.errorMessage || this.translate.instant('ADMIN.PAGES.CAPTAIN_FORM.TOAST.LOAD_ERROR_MSG')
        );
        setTimeout(() => this.router.navigate(['/admin/users/captains']), 2000);
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

  selectDeliveryType(value: DeliveryType): void {
    this.captainForm.patchValue({ deliveryType: value });
    this.captainForm.get('deliveryType')?.markAsDirty();
    this.captainForm.get('deliveryType')?.markAsTouched();
  }

  isDeliveryTypeSelected(value: DeliveryType): boolean {
    return Number(this.captainForm.get('deliveryType')?.value) === value;
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

  async onImageSelected(event: Event, imageType: string): Promise<void> {
    const result = await this.imageService.handleImageUpload(event, { maxSizeMB: 5, showErrorAlert: true });
    if (!result?.success) return;

    const mapping = CAPTAIN_IMAGE_MAP[imageType];
    if (!mapping) return;

    this.imagesPreviews[mapping.previewKey] = result.preview || null;
    this.captainForm.patchValue({ [mapping.formField]: result.base64 });
    this.captainForm.get(mapping.formField)?.markAsTouched();
  }

  removeImage(imageType: string): void {
    const mapping = CAPTAIN_IMAGE_MAP[imageType];
    if (!mapping) return;

    this.imagesPreviews[mapping.previewKey] = null;
    this.captainForm.patchValue({ [mapping.formField]: '' });
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.captainForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  getFieldError(fieldName: string): string {
    const field = this.captainForm.get(fieldName);
    if (field?.errors?.['required']) return this.translate.instant('VALIDATION.REQUIRED');
    if (field?.errors?.['email']) return this.translate.instant('VALIDATION.EMAIL');
    if (field?.errors?.['minlength']) return this.translate.instant('VALIDATION.MIN_LENGTH');
    if (field?.errors?.['pattern']) return this.translate.instant('VALIDATION.PATTERN');
    return '';
  }

  private getStepFields(stepIndex: number): string[] {
    const step = this.wizardSteps[stepIndex];
    if (step.key === 'documents') {
      applyOwnerValidators(this.captainForm);
      return this.visibleDocuments
        .filter((doc) => isCaptainDocumentRequired(doc.type, Number(this.captainForm.get('vehicleOwnerTypeId')?.value)))
        .map((doc) => CAPTAIN_IMAGE_MAP[doc.type].formField);
    }
    return [...step.fields];
  }

  private validateStep(stepIndex: number): boolean {
    const fields = this.getStepFields(stepIndex);
    let valid = true;

    fields.forEach((field) => {
      const control = this.captainForm.get(field);
      control?.markAsTouched();
      if (control?.invalid) valid = false;
    });

    return valid;
  }

  goToStep(index: number): void {
    if (index >= 0 && index < this.wizardSteps.length) {
      this.currentStep = index;
    }
  }

  nextStep(): void {
    if (this.currentStep < this.wizardSteps.length - 1) {
      this.currentStep++;
    }
  }

  prevStep(): void {
    if (this.currentStep > 0) {
      this.currentStep--;
    }
  }

  onSubmit(): void {
    applyOwnerValidators(this.captainForm);
    this.captainForm.markAllAsTouched();

    if (this.captainForm.invalid) {
      for (let i = 0; i < this.wizardSteps.length; i++) {
        if (!this.validateStep(i)) {
          this.currentStep = i;
          break;
        }
      }
      this.toasterService.error(
        this.translate.instant('ADMIN.PAGES.CAPTAIN_FORM.TOAST.FORM_INVALID_TITLE'),
        this.translate.instant('ADMIN.PAGES.CAPTAIN_FORM.TOAST.FORM_INVALID_MSG')
      );
      return;
    }

    this.isSubmitting = true;
    const body = formValueToApiDto(this.captainForm.getRawValue(), this.datePipe, true);

    this.subs.sink = this.deliveryManClient.updateDeliveryMan(this.deliveryManId, body).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.toasterService.success(
          this.translate.instant('ADMIN.PAGES.CAPTAIN_FORM.TOAST.SUCCESS_TITLE'),
          this.translate.instant('ADMIN.PAGES.CAPTAIN_FORM.TOAST.EDIT_SUCCESS_MSG')
        );
        setTimeout(() => this.router.navigate(['/admin/users/captains']), 1500);
      },
      error: (error) => {
        this.isSubmitting = false;
        this.toasterService.error(
          this.translate.instant('ADMIN.PAGES.CAPTAIN_FORM.TOAST.ERROR_TITLE'),
          error?.errorMessage || this.translate.instant('ADMIN.PAGES.CAPTAIN_FORM.TOAST.EDIT_ERROR_MSG')
        );
      }
    });
  }

  onCancel(): void {
    if (!this.captainForm.dirty) {
      this.router.navigate(['/admin/users/captains']);
      return;
    }

    this.subs.sink = this.confirmationDialog.confirm({
      title: this.translate.instant('ADMIN.PAGES.CAPTAIN_FORM.CANCEL_DIALOG.TITLE'),
      message: this.translate.instant('ADMIN.PAGES.CAPTAIN_FORM.CANCEL_DIALOG.MESSAGE_EDIT'),
      confirmText: this.translate.instant('ADMIN.PAGES.CAPTAIN_FORM.CANCEL_DIALOG.CONFIRM'),
      cancelText: this.translate.instant('ADMIN.PAGES.CAPTAIN_FORM.CANCEL_DIALOG.CANCEL'),
      confirmColor: 'warn',
      icon: 'cancel',
      iconColor: 'text-red-500'
    }).subscribe((confirmed) => {
      if (confirmed) {
        this.router.navigate(['/admin/users/captains']);
      }
    });
  }
}
