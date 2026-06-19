import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { SubSink } from 'subsink';
import { finalize } from 'rxjs/operators';
import { PageHeaderComponent } from 'src/app/shared/components/page-header/page-header.component';
import {
  ActiveCategoryDto,
  DeliveryManVehicleDto,
  UpdateVehicleTypeCommand,
  VehicleAdminClient,
  VehicleLoadCategoryLookupDto
} from 'src/app/Core/services/NaqlahClient';
import { ToasterService } from 'src/app/Core/services/toaster.service';
import {
  buildTypeForm,
  formatFileSize,
  hasInvalidTypeNumericFields,
  loadVehicleTypeById,
  mapMainCategoriesToSelected,
  validateIconFile
} from '../vehicle-form.helpers';

@Component({
  selector: 'app-edit-vehicle-type',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    TranslateModule,
    PageHeaderComponent,
    DecimalPipe
  ],
  templateUrl: './edit-vehicle-type.component.html',
  styleUrl: './edit-vehicle-type.component.css'
})
export class EditVehicleTypeComponent implements OnInit, OnDestroy {
  form: FormGroup;
  typeId = 0;
  isLoading = true;
  isSubmitting = false;

  mainCategories: ActiveCategoryDto[] = [];
  loadCategories: VehicleLoadCategoryLookupDto[] = [];
  selectedCategories: ActiveCategoryDto[] = [];
  categorySearchTerm = '';

  iconPreview: string | null = null;
  selectedIconName = '';
  selectedIconSize = 0;
  isDragOver = false;
  iconError = '';

  private sub = new SubSink();

  constructor(
    private fb: FormBuilder,
    private vehicleClient: VehicleAdminClient,
    private toasterService: ToasterService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.form = buildTypeForm(this.fb);
  }

  ngOnInit(): void {
    this.typeId = Number(this.route.snapshot.paramMap.get('id'));
    this.loadLookups();

    const stateItem = history.state?.item as DeliveryManVehicleDto | undefined;
    if (stateItem?.id === this.typeId) {
      this.patchForm(stateItem);
      this.isLoading = false;
      return;
    }

    this.sub.sink = loadVehicleTypeById(this.vehicleClient, this.typeId)
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe((item) => {
        if (!item) {
          this.toasterService.error('خطأ', 'لم يتم العثور على نوع المركبة');
          this.onCancel();
          return;
        }
        this.patchForm(item);
      });
  }

  ngOnDestroy(): void {
    this.sub.unsubscribe();
  }

  get filteredCategories(): ActiveCategoryDto[] {
    const term = this.categorySearchTerm.trim().toLowerCase();
    if (!term) return this.mainCategories;
    return this.mainCategories.filter((c) => c.name?.toLowerCase().includes(term));
  }

  get selectedLoadCategoryName(): string {
    const id = this.form.get('loadCategory')?.value;
    if (id == null) return 'غير محدد';
    return this.loadCategories.find((c) => c.id === id)?.name ?? 'غير محدد';
  }

  private loadLookups(): void {
    this.sub.sink = this.vehicleClient.getMainCategoriesLookup().subscribe({
      next: (categories) => (this.mainCategories = categories)
    });

    this.sub.sink = this.vehicleClient.getVehicleLoadCategoriesLookup().subscribe({
      next: (categories) => (this.loadCategories = categories)
    });
  }

  private patchForm(item: DeliveryManVehicleDto): void {
    this.selectedCategories = mapMainCategoriesToSelected(item);

    this.form.patchValue({
      arabicName: item.arabicName,
      englishName: item.englishName,
      mainCategoryIds: this.selectedCategories.map((c) => c.id),
      iconBase64: '',
      cost: item.cost ?? 0,
      serviceFee: item.serviceFee ?? 0,
      loadCategory: item.loadCategory ?? null
    });

    if (item.iconImagePath) {
      this.iconPreview = item.iconImagePath;
      this.selectedIconName = 'صورة محفوظة';
      this.selectedIconSize = 0;
    }
  }

  selectLoadCategory(id: number | null): void {
    this.form.patchValue({ loadCategory: id });
  }

  isLoadCategorySelected(id: number): boolean {
    return this.form.get('loadCategory')?.value === id;
  }

  isCategorySelected(category: ActiveCategoryDto): boolean {
    return this.selectedCategories.some((c) => c.id === category.id);
  }

  toggleCategory(category: ActiveCategoryDto): void {
    const index = this.selectedCategories.findIndex((c) => c.id === category.id);
    if (index > -1) {
      this.selectedCategories.splice(index, 1);
    } else {
      this.selectedCategories.push(category);
    }
    this.form.patchValue({ mainCategoryIds: this.selectedCategories.map((c) => c.id) });
  }

  onIconSelect(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (file) this.handleIconFile(file);
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver = false;
    const file = event.dataTransfer?.files?.[0];
    if (file) this.handleIconFile(file);
  }

  private handleIconFile(file: File): void {
    const error = validateIconFile(file);
    if (error) {
      this.iconError = error;
      return;
    }

    this.iconError = '';
    this.selectedIconName = file.name;
    this.selectedIconSize = file.size;

    const reader = new FileReader();
    reader.onload = (e) => {
      this.iconPreview = e.target?.result as string;
      this.form.patchValue({ iconBase64: this.iconPreview });
    };
    reader.readAsDataURL(file);
  }

  removeIcon(): void {
    this.iconPreview = null;
    this.selectedIconName = '';
    this.selectedIconSize = 0;
    this.iconError = '';
    this.form.patchValue({ iconBase64: '' });
  }

  formatFileSize(bytes: number): string {
    return formatFileSize(bytes);
  }

  onCancel(): void {
    this.router.navigate(['/admin/vehicles'], { queryParams: { tab: 'types' } });
  }

  onSubmit(): void {
    if (this.form.invalid || hasInvalidTypeNumericFields(this.form)) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.value;
    const command = new UpdateVehicleTypeCommand();
    command.vehicleTypeId = this.typeId;
    command.arabicName = value.arabicName;
    command.englishName = value.englishName;
    command.iconBase64 = value.iconBase64 || null;
    command.mainCategoryIds = value.mainCategoryIds ?? [];
    command.cost = value.cost ?? 0;
    command.serviceFee = value.serviceFee ?? 0;
    command.loadCategory = value.loadCategory ?? null;

    this.isSubmitting = true;
    this.sub.sink = this.vehicleClient
      .updateVehicleType(command)
      .pipe(finalize(() => (this.isSubmitting = false)))
      .subscribe({
        next: () => {
          this.toasterService.success('تم التحديث', 'تم تحديث نوع المركبة بنجاح');
          this.onCancel();
        },
        error: (error) => {
          this.toasterService.error('خطأ', error?.message || 'تعذر تحديث نوع المركبة');
        }
      });
  }
}
