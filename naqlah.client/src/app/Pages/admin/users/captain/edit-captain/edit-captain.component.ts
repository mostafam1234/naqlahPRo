import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { SubSink } from 'subsink';
import Swal from 'sweetalert2';
import { DeliveryManAdminClient, GetDeliveryManRequestDetailsDto, DeliveryManVehicleDto, VehicleAdminClient, VehicleTypeDto } from 'src/app/Core/services/NaqlahClient';
import { PageHeaderComponent } from 'src/app/shared/components/page-header/page-header.component';
import { DeliveryType, DeliveryLicenseType, VehicleOwnerType } from 'src/app/Core/enums/delivery.enums';
import { ImageService } from 'src/app/Core/services/image.service';

@Component({
  selector: 'app-edit-captain',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule, PageHeaderComponent],
  providers: [DatePipe, DeliveryManAdminClient, VehicleAdminClient],
  templateUrl: './edit-captain.component.html',
  styleUrl: './edit-captain.component.css'
})
export class EditCaptainComponent implements OnInit, OnDestroy {
  captainForm!: FormGroup;
  isLoading = false;
  isSubmitting = false;
  deliveryManId!: number;
  deliveryManDetails: GetDeliveryManRequestDetailsDto | null = null;

  // Image previews
  imagesPreviews = {
    personalImage: null as string | null,
    frontIdentityImage: null as string | null,
    backIdentityImage: null as string | null,
    frontDrivingLicenseImage: null as string | null,
    backDrivingLicenseImage: null as string | null,
    // Vehicle images
    vehicleFrontImage: null as string | null,
    vehicleSideImage: null as string | null,
    vehicleFrontLicenseImage: null as string | null,
    vehicleBackLicenseImage: null as string | null,
    vehicleFrontInsuranceImage: null as string | null,
    vehicleBackInsuranceImage: null as string | null
  };

  // Form sections visibility
  formSections = {
    personalInfo: true,
    identityInfo: true,
    drivingLicense: true,
    vehicleInfo: true,
    documentsUpload: true
  };

  // Enums for templates
  deliveryTypes = [
    { value: DeliveryType.Resident, label: 'مقيم' },
    { value: DeliveryType.Citizen, label: 'مواطن' }
  ];

  deliveryLicenseTypes = [
    { value: DeliveryLicenseType.Public, label: 'رخصة عامة' },
    { value: DeliveryLicenseType.Private, label: 'رخصة خاصة' },
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
    private imageService: ImageService
  ) {
    this.initializeForm();
  }

  ngOnInit(): void {
    // Get delivery man ID from route
    this.route.params.subscribe(params => {
      this.deliveryManId = +params['id'];
      if (this.deliveryManId) {
        this.loadDeliveryManDetails();
      }
    });

    this.GetVehicleTypesLookUp();
    this.GetVehicleBrandsLookUp();
  }

  ngOnDestroy(): void {
    this.subs.unsubscribe();
  }

  GetVehicleBrandsLookUp(): void {
    this.subs.sink = this.vehicleClient.getVehiclesBrandLookup().subscribe({
      next: (brands) => {
        this.vehicleBrands = brands;
      },
      error: (err) => {
        console.error('Error fetching vehicle brands:', err);
      }
    });
  }

  GetVehicleTypesLookUp(): void {
    this.subs.sink = this.vehicleClient.getVehiclesTypesLookup().subscribe({
      next: (types) => {
        this.vehicleTypes = types;
      },
      error: (err) => {
        console.error('Error fetching vehicle types:', err);
      }
    });
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
        console.error('Error loading delivery man details:', error);
        this.isLoading = false;
        Swal.fire({
          title: 'خطأ!',
          text: 'حدث خطأ أثناء تحميل بيانات الكابتن',
          icon: 'error',
          confirmButtonText: 'موافق'
        }).then(() => {
          this.router.navigate(['/admin/users/captain']);
        });
      }
    });
  }

  private populateForm(details: GetDeliveryManRequestDetailsDto): void {
    // Format dates for date inputs - dates come as Date objects from the DTO
    const formatDateForInput = (date: Date | null | undefined): string => {
      if (!date) return '';
      // If it's already a Date object, use it directly
      const dateObj = date instanceof Date ? date : new Date(date);
      if (isNaN(dateObj.getTime())) return '';
      return this.datePipe.transform(dateObj, 'yyyy-MM-dd') || '';
    };

    this.captainForm.patchValue({
      fullName: details.fullName || '',
      phoneNumber: details.phoneNumber || '',
      identityNumber: details.identityNumber || '',
      deliveryType: details.deliveryType || DeliveryType.Citizen,
      address: details.address || '',
      identityExpirationDate: formatDateForInput(details.identityExpirationDate),
      deliveryLicenseType: details.deliveryLicenseType || DeliveryLicenseType.Private,
      drivingLicenseExpirationDate: formatDateForInput(details.drivingLicenseExpirationDate),
      // Vehicle fields - these might need to be fetched separately if not in DTO
      vehicleTypeId: (details as any).vehicleTypeId || '',
      vehicleBrandId: (details as any).vehicleBrandId || '',
      vehiclePlateNumber: details.vehiclePlateNumber || '',
      vehicleOwnerTypeId: (details as any).vehicleOwnerTypeId || VehicleOwnerType.Resident,
      vehicleLicenseExpirationDate: formatDateForInput((details as any).vehicleLicenseExpirationDate),
      vehicleInsuranceExpirationDate: formatDateForInput((details as any).vehicleInsuranceExpirationDate),
    });

    // Set image previews
    this.imagesPreviews.personalImage = details.personalImagePath || null;
    this.imagesPreviews.frontIdentityImage = details.frontIdentityImagePath || null;
    this.imagesPreviews.backIdentityImage = details.backIdentityImagePath || null;
    this.imagesPreviews.frontDrivingLicenseImage = details.frontDrivingLicenseImagePath || null;
    this.imagesPreviews.backDrivingLicenseImage = details.backDrivingLicenseImagePath || null;
    this.imagesPreviews.vehicleFrontImage = (details as any).vehicleFrontImagePath || null;
    this.imagesPreviews.vehicleSideImage = (details as any).vehicleSideImagePath || null;
    this.imagesPreviews.vehicleFrontLicenseImage = (details as any).vehicleFrontLicenseImagePath || null;
    this.imagesPreviews.vehicleBackLicenseImage = (details as any).vehicleBackLicenseImagePath || null;
    this.imagesPreviews.vehicleFrontInsuranceImage = (details as any).vehicleFrontInsuranceImagePath || null;
    this.imagesPreviews.vehicleBackInsuranceImage = (details as any).vehicleBackInsuranceImagePath || null;
  }

  private initializeForm(): void {
    this.captainForm = this.fb.group({
      // User Account Information
      email: [''], // Optional for edit, can be fetched separately if needed
      password: [''], // Optional for edit - only required if changing password
      
      // Personal Information
      fullName: ['', [Validators.required, Validators.minLength(3)]],
      address: ['', [Validators.required, Validators.minLength(10)]],
      phoneNumber: ['', [Validators.required, Validators.pattern(/^(\+966|0)?[5-9]\d{8}$/)]],
      identityNumber: ['', [Validators.required, Validators.pattern(/^\d{10}$/)]],
      deliveryType: [DeliveryType.Citizen, Validators.required],
      active: [true],

      // Identity Information
      identityExpirationDate: ['', Validators.required],

      // Driving License Information
      deliveryLicenseType: [DeliveryLicenseType.Private, Validators.required],
      drivingLicenseExpirationDate: ['', Validators.required],

      // Vehicle Section
      vehicleTypeId: ['', Validators.required],
      vehicleBrandId: ['', Validators.required],
      vehiclePlateNumber: ['', Validators.required],
      vehicleFrontImagePath: [''],
      vehicleSideImagePath: [''],
      vehicleFrontLicenseImagePath: [''],
      vehicleBackLicenseImagePath: [''],
      vehicleLicenseExpirationDate: [''],
      vehicleFrontInsuranceImagePath: [''],
      vehicleBackInsuranceImagePath: [''],
      vehicleInsuranceExpirationDate: [''],
      vehicleOwnerTypeId: [VehicleOwnerType.Resident, Validators.required],

      // Document Images
      personalImagePath: [''],
      frontIdentityImagePath: [''],
      backIdentityImagePath: [''],
      frontDrivingLicenseImagePath: [''],
      backDrivingLicenseImagePath: ['']
    });
  }

  // Toggle form sections
  toggleSection(section: string): void {
    switch (section) {
      case 'personalInfo':
        this.formSections.personalInfo = !this.formSections.personalInfo;
        break;
      case 'identityInfo':
        this.formSections.identityInfo = !this.formSections.identityInfo;
        break;
      case 'drivingLicense':
        this.formSections.drivingLicense = !this.formSections.drivingLicense;
        break;
      case 'vehicleInfo':
        this.formSections.vehicleInfo = !this.formSections.vehicleInfo;
        break;
      case 'documentsUpload':
        this.formSections.documentsUpload = !this.formSections.documentsUpload;
        break;
    }
  }

  // Form validation helpers
  isFieldInvalid(fieldName: string): boolean {
    const field = this.captainForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  getFieldError(fieldName: string): string {
    const field = this.captainForm.get(fieldName);
    if (field?.errors) {
      if (field.errors['required']) return `${fieldName} مطلوب`;
      if (field.errors['minlength']) return `${fieldName} قصير جداً`;
      if (field.errors['pattern']) return `${fieldName} غير صحيح`;
    }
    return '';
  }

  // Form submission
  onSubmit(): void {
    if (this.captainForm.valid) {
      this.isSubmitting = true;

      Swal.fire({
        title: 'تأكيد التعديل',
        text: 'هل أنت متأكد من حفظ التعديلات؟',
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#10b981',
        cancelButtonColor: '#ef4444',
        confirmButtonText: 'نعم، احفظ التعديلات',
        cancelButtonText: 'إلغاء',
        backdrop: true,
        allowOutsideClick: false
      }).then((result) => {
        if (result.isConfirmed) {
          this.updateDeliveryMan();
        } else {
          this.isSubmitting = false;
        }
      });
    } else {
      Object.keys(this.captainForm.controls).forEach(key => {
        this.captainForm.get(key)?.markAsTouched();
      });

      Swal.fire({
        title: 'خطأ في البيانات',
        text: 'يرجى تصحيح الأخطاء في النموذج قبل المتابعة',
        icon: 'error',
        confirmButtonText: 'موافق'
      });
    }
  }

  private updateDeliveryMan(): void {
    // Format dates for backend
    const formatDateForBackend = (date: any): string | null => {
      if (!date) return null;
      const dateObj = new Date(date);
      if (isNaN(dateObj.getTime())) return null;
      return this.datePipe.transform(dateObj, 'yyyy-MM-dd') || null;
    };

    const rawFormData = this.captainForm.value;

    const updateData = {
      ...rawFormData,
      identityExpirationDate: formatDateForBackend(rawFormData.identityExpirationDate),
      drivingLicenseExpirationDate: formatDateForBackend(rawFormData.drivingLicenseExpirationDate),
      vehicleLicenseExpirationDate: formatDateForBackend(rawFormData.vehicleLicenseExpirationDate),
      vehicleInsuranceExpirationDate: formatDateForBackend(rawFormData.vehicleInsuranceExpirationDate),
    };

    // TODO: Replace with actual update API call when available
    // For now, this is a placeholder structure
    console.log('Update data:', updateData);

    // Example API call structure (uncomment when API is available):
    /*
    this.subs.sink = this.deliveryManClient.updateDeliveryMan(this.deliveryManId, updateData).subscribe({
      next: (result) => {
        this.isSubmitting = false;
        Swal.fire({
          title: 'تمت العملية بنجاح!',
          text: 'تم تحديث بيانات الكابتن بنجاح',
          icon: 'success',
          confirmButtonText: 'موافق'
        }).then(() => {
          this.router.navigate(['/admin/users/captain']);
        });
      },
      error: (error) => {
        this.isSubmitting = false;
        Swal.fire({
          title: 'خطأ!',
          text: 'حدث خطأ أثناء تحديث بيانات الكابتن',
          icon: 'error',
          confirmButtonText: 'موافق'
        });
      }
    });
    */

    // Temporary success message for testing
    setTimeout(() => {
      this.isSubmitting = false;
      Swal.fire({
        title: 'تمت العملية بنجاح!',
        text: 'تم تحديث بيانات الكابتن بنجاح',
        icon: 'success',
        confirmButtonText: 'موافق'
      }).then(() => {
        this.router.navigate(['/admin/users/captain']);
      });
    }, 1000);
  }

  // Image upload handlers
  async onImageSelected(event: Event, imageType: string): Promise<void> {
    const result = await this.imageService.handleImageUpload(event, {
      maxSizeMB: 5,
      showErrorAlert: true
    });

    if (!result?.success) {
      return;
    }

    // Set preview
    this.setImagePreview(imageType, result.preview || null);

    // Set form value with Base64
    const fieldName = this.getFormFieldName(imageType);
    if (fieldName) {
      this.captainForm.patchValue({ [fieldName]: result.base64 });
    }
  }

  private setImagePreview(imageType: string, preview: string | null): void {
    switch (imageType) {
      case 'personal':
        this.imagesPreviews.personalImage = preview;
        break;
      case 'frontIdentity':
        this.imagesPreviews.frontIdentityImage = preview;
        break;
      case 'backIdentity':
        this.imagesPreviews.backIdentityImage = preview;
        break;
      case 'frontDrivingLicense':
        this.imagesPreviews.frontDrivingLicenseImage = preview;
        break;
      case 'backDrivingLicense':
        this.imagesPreviews.backDrivingLicenseImage = preview;
        break;
      case 'vehicleFront':
        this.imagesPreviews.vehicleFrontImage = preview;
        break;
      case 'vehicleSide':
        this.imagesPreviews.vehicleSideImage = preview;
        break;
      case 'vehicleFrontLicense':
        this.imagesPreviews.vehicleFrontLicenseImage = preview;
        break;
      case 'vehicleBackLicense':
        this.imagesPreviews.vehicleBackLicenseImage = preview;
        break;
      case 'vehicleFrontInsurance':
        this.imagesPreviews.vehicleFrontInsuranceImage = preview;
        break;
      case 'vehicleBackInsurance':
        this.imagesPreviews.vehicleBackInsuranceImage = preview;
        break;
    }
  }

  private getFormFieldName(imageType: string): string {
    switch (imageType) {
      case 'personal':
        return 'personalImagePath';
      case 'frontIdentity':
        return 'frontIdentityImagePath';
      case 'backIdentity':
        return 'backIdentityImagePath';
      case 'frontDrivingLicense':
        return 'frontDrivingLicenseImagePath';
      case 'backDrivingLicense':
        return 'backDrivingLicenseImagePath';
      case 'vehicleFront':
        return 'vehicleFrontImagePath';
      case 'vehicleSide':
        return 'vehicleSideImagePath';
      case 'vehicleFrontLicense':
        return 'vehicleFrontLicenseImagePath';
      case 'vehicleBackLicense':
        return 'vehicleBackLicenseImagePath';
      case 'vehicleFrontInsurance':
        return 'vehicleFrontInsuranceImagePath';
      case 'vehicleBackInsurance':
        return 'vehicleBackInsuranceImagePath';
      default:
        return '';
    }
  }

  removeImage(imageType: string): void {
    // Clear preview
    this.setImagePreview(imageType, null);
    
    // Clear form value
    const fieldName = this.getFormFieldName(imageType);
    if (fieldName) {
      this.captainForm.patchValue({ [fieldName]: '' });
    }
  }

  onCancel(): void {
    if (this.captainForm.dirty) {
      Swal.fire({
        title: 'هل أنت متأكد؟',
        text: 'سيتم فقدان جميع التعديلات',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#ef4444',
        cancelButtonColor: '#6b7280',
        confirmButtonText: 'نعم، إلغاء',
        cancelButtonText: 'الرجوع للنموذج'
      }).then((result) => {
        if (result.isConfirmed) {
          this.router.navigate(['/admin/users/captain']);
        }
      });
    } else {
      this.router.navigate(['/admin/users/captain']);
    }
  }

  // Form step definitions for progress indicator
  formSteps = [
    {
      key: 'personalInfo',
      title: 'البيانات الشخصية',
      icon: 'M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z'
    },
    {
      key: 'identityInfo',
      title: 'الهوية',
      icon: 'M10 6H5a2 2 0 00-2 2v9a2 2 0 002 2h14a2 2 0 002-2V8a2 2 0 00-2-2h-5m-4 0V5a2 2 0 114 0v1m-4 0a2 2 0 104 0m-5 8a2 2 0 100-4 2 2 0 000 4zm0 0c1.306 0 2.417.835 2.83 2M9 14a3.001 3.001 0 00-2.83 2M15 11h3m-3 4h2'
    },
    {
      key: 'drivingLicense',
      title: 'رخصة القيادة',
      icon: 'M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z'
    }
  ];

  // Get step class for progress indicator
  getStepClass(stepKey: string): string {
    const isActive = this.formSections[stepKey as keyof typeof this.formSections];
    const hasErrors = this.hasStepErrors(stepKey);

    if (hasErrors) {
      return 'bg-red-500 text-white';
    } else if (this.isStepCompleted(stepKey)) {
      return 'bg-green-500 text-white';
    } else if (isActive) {
      return 'bg-primary-500 text-white';
    } else {
      return 'bg-neutral-300 text-neutral-600';
    }
  }

  // Check if step has errors
  private hasStepErrors(stepKey: string): boolean {
    const stepFields = this.getStepFields(stepKey);
    return stepFields.some(field => this.isFieldInvalid(field));
  }

  // Check if step is completed
  private isStepCompleted(stepKey: string): boolean {
    const stepFields = this.getStepFields(stepKey);
    const requiredFields = stepFields.filter(field => {
      const control = this.captainForm.get(field);
      return control?.hasError('required') !== undefined;
    });

    return requiredFields.every(field => {
      const control = this.captainForm.get(field);
      return control?.valid;
    });
  }

  // Get fields for each step
  private getStepFields(stepKey: string): string[] {
    const fieldMap = {
      personalInfo: ['email', 'fullName', 'phoneNumber', 'identityNumber', 'deliveryType', 'address'],
      identityInfo: ['identityExpirationDate'],
      drivingLicense: ['deliveryLicenseType', 'drivingLicenseExpirationDate'],
      vehicleInfo: [],
      documentsUpload: []
    };

    return fieldMap[stepKey as keyof typeof fieldMap] || [];
  }
}
