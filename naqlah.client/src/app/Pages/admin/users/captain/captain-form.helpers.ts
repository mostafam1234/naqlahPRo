import { DatePipe } from '@angular/common';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AddDeliveryManDto, GetDeliveryManRequestDetailsDto } from 'src/app/Core/services/NaqlahClient';
import { DeliveryLicenseType, DeliveryType, VehicleOwnerType } from 'src/app/Core/enums/delivery.enums';

const PHONE_PATTERN = /^(\+966|0)?[5-9]\d{8}$/;

/** ربط نوع الصورة في الـ UI باسم الحقل في الفورم ومفتاح المعاينة */
export const CAPTAIN_IMAGE_MAP: Record<string, { formField: string; previewKey: string }> = {
  personal: { formField: 'personalImagePath', previewKey: 'personalImage' },
  frontIdentity: { formField: 'frontIdentityImagePath', previewKey: 'frontIdentityImage' },
  frontDrivingLicense: { formField: 'frontDrivingLicenseImagePath', previewKey: 'frontDrivingLicenseImage' },
  vehicleFront: { formField: 'vehicleFrontImagePath', previewKey: 'vehicleFrontImage' },
  vehicleSide: { formField: 'vehicleSideImagePath', previewKey: 'vehicleSideImage' },
  vehicleFrontLicense: { formField: 'vehicleFrontLicenseImagePath', previewKey: 'vehicleFrontLicenseImage' },
  vehicleFrontInsurance: { formField: 'vehicleFrontInsuranceImagePath', previewKey: 'vehicleFrontInsuranceImage' },
  ownerFrontIdentity: { formField: 'ownerFrontIdentityImagePath', previewKey: 'ownerFrontIdentityImage' },
  commercialRecord: { formField: 'commercialRecordImagePath', previewKey: 'commercialRecordImage' },
  rentContract: { formField: 'rentContractImagePath', previewKey: 'rentContractImage' }
};

export interface CaptainDocumentItem {
  type: string;
  label: string;
  optional?: boolean;
  ownerTypes?: VehicleOwnerType[];
}

/** كل صور الكابتن في تاب واحد — بالمسميات المعتمدة */
export const CAPTAIN_DOCUMENTS: CaptainDocumentItem[] = [
  { type: 'frontIdentity', label: 'صورة الهوية لسائق المركبة' },
  { type: 'frontDrivingLicense', label: 'صورة رخصة القيادة لسائق المركبة' },
  { type: 'ownerFrontIdentity', label: 'صورة الهوية لمالك المركبة', ownerTypes: [VehicleOwnerType.Resident, VehicleOwnerType.Renter] },
  { type: 'commercialRecord', label: 'صورة السجل التجاري', ownerTypes: [VehicleOwnerType.Company] },
  { type: 'vehicleFront', label: 'صورة أمامية للمركبة' },
  { type: 'vehicleSide', label: 'صورة جانبية للمركبة' },
  { type: 'vehicleFrontLicense', label: 'صورة رخصة سير المركبة (الاستمارة)' },
  { type: 'personal', label: 'صورة شخصية لسائق المركبة', optional: true },
  { type: 'vehicleFrontInsurance', label: 'صورة تأمين المركبة', optional: true },
  { type: 'rentContract', label: 'صورة وثيقة استئجار المركبة', ownerTypes: [VehicleOwnerType.Renter], optional: true }
];

export function getVisibleCaptainDocuments(ownerTypeId: number): CaptainDocumentItem[] {
  return CAPTAIN_DOCUMENTS.filter((doc) => {
    if (!doc.ownerTypes?.length) return true;
    return doc.ownerTypes.includes(ownerTypeId as VehicleOwnerType);
  });
}

export function isCaptainDocumentRequired(type: string, ownerTypeId: number): boolean {
  const doc = CAPTAIN_DOCUMENTS.find((d) => d.type === type);
  if (!doc || doc.optional) return false;
  if (doc.ownerTypes?.length && !doc.ownerTypes.includes(ownerTypeId as VehicleOwnerType)) return false;
  return true;
}

export interface CaptainDetailDocument {
  key: string;
  label: string;
  optional?: boolean;
  show?: (details: GetDeliveryManRequestDetailsDto) => boolean;
  getUrl: (details: GetDeliveryManRequestDetailsDto) => string | null | undefined;
}

export function getCaptainDetailDocuments(details: GetDeliveryManRequestDetailsDto | null): CaptainDetailDocument[] {
  if (!details) return [];

  const ownerType = details.vehicleOwnerTypeId ?? 0;

  const all: CaptainDetailDocument[] = [
    { key: 'frontIdentity', label: 'صورة الهوية لسائق المركبة', getUrl: (d) => d.frontIdentityImagePath },
    { key: 'frontDrivingLicense', label: 'صورة رخصة القيادة لسائق المركبة', getUrl: (d) => d.frontDrivingLicenseImagePath },
    {
      key: 'ownerFrontIdentity',
      label: 'صورة الهوية لمالك المركبة',
      show: () => ownerType === VehicleOwnerType.Resident || ownerType === VehicleOwnerType.Renter,
      getUrl: (d) => d.ownerFrontIdentityImagePath
    },
    {
      key: 'commercialRecord',
      label: 'صورة السجل التجاري',
      show: () => ownerType === VehicleOwnerType.Company,
      getUrl: (d) => d.commercialRecordImagePath
    },
    { key: 'vehicleFront', label: 'صورة أمامية للمركبة', getUrl: (d) => d.vehicleFrontImagePath },
    { key: 'vehicleSide', label: 'صورة جانبية للمركبة', getUrl: (d) => d.vehicleSideImagePath },
    { key: 'vehicleFrontLicense', label: 'صورة رخصة سير المركبة (الاستمارة)', getUrl: (d) => d.vehicleFrontLicenseImagePath },
    { key: 'personal', label: 'صورة شخصية لسائق المركبة', optional: true, getUrl: (d) => d.personalImagePath },
    { key: 'vehicleFrontInsurance', label: 'صورة تأمين المركبة', optional: true, getUrl: (d) => d.vehicleFrontInsuranceImagePath },
    {
      key: 'rentContract',
      label: 'صورة وثيقة استئجار المركبة',
      optional: true,
      show: () => ownerType === VehicleOwnerType.Renter,
      getUrl: (d) => d.rentContractImagePath
    }
  ];

  return all.filter((doc) => !doc.show || doc.show(details));
}

export function getVehicleOwnerTypeLabel(ownerTypeId: number | null | undefined): string {
  switch (ownerTypeId) {
    case VehicleOwnerType.Resident: return 'مالك فرد';
    case VehicleOwnerType.Company: return 'شركة / مؤسسة';
    case VehicleOwnerType.Renter: return 'مستأجر';
    default: return 'غير محدد';
  }
}

export function buildCaptainForm(fb: FormBuilder, isEdit: boolean): FormGroup {
  return fb.group({
    email: ['', isEdit ? [Validators.email] : [Validators.required, Validators.email]],
    password: ['', isEdit ? [Validators.minLength(6)] : [Validators.required, Validators.minLength(6)]],
    fullName: ['', [Validators.required, Validators.minLength(3)]],
    address: [''],
    phoneNumber: ['', [Validators.required, Validators.pattern(PHONE_PATTERN)]],
    identityNumber: ['', [Validators.required, Validators.pattern(/^\d{10}$/)]],
    birthDate: ['', Validators.required],
    deliveryType: [DeliveryType.Resident, Validators.required],
    active: [true],
    identityExpirationDate: [''],
    deliveryLicenseType: [DeliveryLicenseType.Private, Validators.required],
    drivingLicenseExpirationDate: [''],
    vehicleTypeId: ['', Validators.required],
    vehicleBrandId: ['', Validators.required],
    vehiclePlateNumber: ['', Validators.required],
    vehicleOwnerTypeId: [VehicleOwnerType.Resident, Validators.required],
    vehicleOwnerName: ['', Validators.required],
    vehicleOwnerIdentityNumber: [''],
    commercialRecordNumber: [''],
    vehicleLicenseExpirationDate: [''],
    vehicleInsuranceExpirationDate: [''],
    personalImagePath: [''],
    frontIdentityImagePath: ['', Validators.required],
    frontDrivingLicenseImagePath: ['', Validators.required],
    vehicleFrontImagePath: ['', Validators.required],
    vehicleSideImagePath: ['', Validators.required],
    vehicleFrontLicenseImagePath: ['', Validators.required],
    vehicleFrontInsuranceImagePath: [''],
    ownerFrontIdentityImagePath: [''],
    commercialRecordImagePath: [''],
    rentContractImagePath: ['']
  });
}

export function applyOwnerValidators(form: FormGroup): void {
  const ownerType = Number(form.get('vehicleOwnerTypeId')?.value);
  const ownerFrontId = form.get('ownerFrontIdentityImagePath');
  const commercialRecord = form.get('commercialRecordImagePath');

  ownerFrontId?.clearValidators();
  commercialRecord?.clearValidators();

  if (ownerType === VehicleOwnerType.Resident || ownerType === VehicleOwnerType.Renter) {
    ownerFrontId?.setValidators([Validators.required]);
  } else if (ownerType === VehicleOwnerType.Company) {
    commercialRecord?.setValidators([Validators.required]);
  }

  ownerFrontId?.updateValueAndValidity({ emitEvent: false });
  commercialRecord?.updateValueAndValidity({ emitEvent: false });
}

function toApiDate(datePipe: DatePipe, value: unknown): string | undefined {
  if (!value) return undefined;
  const d = new Date(value as string);
  if (isNaN(d.getTime())) return undefined;
  return datePipe.transform(d, 'yyyy-MM-dd') || undefined;
}

function parseDeliveryType(value: unknown): number {
  const parsed = Number(value);
  if (parsed === DeliveryType.Resident || parsed === DeliveryType.Citizen) {
    return parsed;
  }
  return DeliveryType.Resident;
}

/** تحويل قيمة الفورم إلى DTO جاهز للـ API */
export function formValueToApiDto(
  formValue: Record<string, unknown>,
  datePipe: DatePipe,
  isEdit: boolean
): AddDeliveryManDto {
  return AddDeliveryManDto.fromJS({
    ...formValue,
    birthDate: toApiDate(datePipe, formValue['birthDate']),
    identityExpirationDate: toApiDate(datePipe, formValue['identityExpirationDate']),
    drivingLicenseExpirationDate: toApiDate(datePipe, formValue['drivingLicenseExpirationDate']),
    vehicleLicenseExpirationDate: toApiDate(datePipe, formValue['vehicleLicenseExpirationDate']),
    vehicleInsuranceExpirationDate: toApiDate(datePipe, formValue['vehicleInsuranceExpirationDate']),
    deliveryType: parseDeliveryType(formValue['deliveryType']),
    deliveryLicenseType: Number(formValue['deliveryLicenseType']) || DeliveryLicenseType.Private,
    vehicleTypeId: formValue['vehicleTypeId'] ? Number(formValue['vehicleTypeId']) : null,
    vehicleBrandId: formValue['vehicleBrandId'] ? Number(formValue['vehicleBrandId']) : null,
    vehicleOwnerTypeId: formValue['vehicleOwnerTypeId'] ? Number(formValue['vehicleOwnerTypeId']) : null,
    active: isEdit ? (formValue['active'] as boolean) ?? true : false,
    backIdentityImagePath: null,
    backDrivingLicenseImagePath: null,
    vehicleBackLicenseImagePath: null,
    vehicleBackInsuranceImagePath: null,
    ownerBackIdentityImagePath: null,
    androidDevice: null,
    iosDevice: null
  });
}
