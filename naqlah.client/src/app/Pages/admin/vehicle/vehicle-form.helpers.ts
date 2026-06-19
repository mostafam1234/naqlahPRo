import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Observable, map } from 'rxjs';
import {
  ActiveCategoryDto,
  DeliveryManVehicleDto,
  VehicleAdminClient,
  VehicleLoadCategoryLookupDto
} from 'src/app/Core/services/NaqlahClient';
import { SelectOption } from 'src/app/shared/models/select-option.model';

export function buildBrandForm(fb: FormBuilder): FormGroup {
  return fb.group({
    arabicName: ['', [Validators.required, Validators.maxLength(100)]],
    englishName: ['', [Validators.required, Validators.maxLength(100)]]
  });
}

export function buildTypeForm(fb: FormBuilder): FormGroup {
  return fb.group({
    arabicName: ['', [Validators.maxLength(100)]],
    englishName: ['', [Validators.maxLength(100)]],
    iconBase64: [''],
    mainCategoryIds: [[] as number[]],
    cost: [0, [Validators.min(0)]],
    serviceFee: [0, [Validators.min(0)]],
    loadCategory: [null as number | null]
  });
}

export function mapLoadCategoriesToOptions(
  categories: VehicleLoadCategoryLookupDto[]
): SelectOption[] {
  return categories.map((category) => ({
    value: String(category.id),
    label: category.name
  }));
}

export function mapMainCategoriesToSelected(
  item: DeliveryManVehicleDto
): ActiveCategoryDto[] {
  return (
    item.mainCategories?.map((cat) => {
      const dto = new ActiveCategoryDto();
      dto.id = cat.id;
      dto.name = cat.name || cat.arabicName;
      return dto;
    }) ?? []
  );
}

export function loadVehicleTypeById(
  client: VehicleAdminClient,
  id: number
): Observable<DeliveryManVehicleDto | null> {
  return client.getVehiclesTypes(0, 5000, '').pipe(
    map((result) => result.data?.find((item) => item.id === id) ?? null)
  );
}

export function loadVehicleBrandById(
  client: VehicleAdminClient,
  id: number
): Observable<DeliveryManVehicleDto | null> {
  return client.getVehiclesBrands(0, 5000, '').pipe(
    map((result) => result.data?.find((item) => item.id === id) ?? null)
  );
}

export function formatFileSize(bytes: number): string {
  if (bytes === 0) return '0 Bytes';
  const k = 1024;
  const sizes = ['Bytes', 'KB', 'MB', 'GB'];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return `${parseFloat((bytes / Math.pow(k, i)).toFixed(2))} ${sizes[i]}`;
}

export function validateIconFile(file: File): string | null {
  if (!file.type.startsWith('image/')) {
    return 'يرجى اختيار ملف صورة صحيح';
  }
  if (file.size > 2 * 1024 * 1024) {
    return 'حجم الصورة يجب أن يكون أقل من 2 ميجابايت';
  }
  return null;
}

export function hasInvalidTypeNumericFields(form: FormGroup): boolean {
  const cost = form.get('cost');
  const serviceFee = form.get('serviceFee');
  return !!(cost?.invalid || serviceFee?.invalid);
}
