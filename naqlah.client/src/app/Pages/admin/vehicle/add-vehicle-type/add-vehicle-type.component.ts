import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { SubSink } from 'subsink';
import { finalize } from 'rxjs/operators';
import { PageHeaderComponent } from 'src/app/shared/components/page-header/page-header.component';
import {
  ActiveCategoryDto,
  AddVehicleTypeCommand,
  VehicleAdminClient,
  VehicleLoadCategoryLookupDto
} from 'src/app/Core/services/NaqlahClient';
import { ToasterService } from 'src/app/Core/services/toaster.service';
import {
  buildTypeForm,
  formatFileSize,
  hasInvalidTypeNumericFields,
  validateIconFile
} from '../vehicle-form.helpers';

@Component({
  selector: 'app-add-vehicle-type',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    TranslateModule,
    PageHeaderComponent,
    DecimalPipe
  ],
  templateUrl: './add-vehicle-type.component.html',
  styleUrl: './add-vehicle-type.component.css'
})
export class AddVehicleTypeComponent implements OnInit, OnDestroy {
  form: FormGroup;
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
    private router: Router
  ) {
    this.form = buildTypeForm(this.fb);
  }

  ngOnInit(): void {
    this.loadLookups();
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
      next: (categories) => (this.mainCategories = categories),
      error: () => this.toasterService.error('خطأ', 'تعذر تحميل أقسام الشحنات')
    });

    this.sub.sink = this.vehicleClient.getVehicleLoadCategoriesLookup().subscribe({
      next: (categories) => (this.loadCategories = categories),
      error: () => this.toasterService.error('خطأ', 'تعذر تحميل تصنيفات الحمولة')
    });
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
    const command = new AddVehicleTypeCommand();
    command.arabicName = value.arabicName;
    command.englishName = value.englishName;
    command.iconBase64 = value.iconBase64 || null;
    command.mainCategoryIds = value.mainCategoryIds ?? [];
    command.cost = value.cost ?? 0;
    command.serviceFee = value.serviceFee ?? 0;
    command.loadCategory = value.loadCategory ?? null;

    this.isSubmitting = true;
    this.sub.sink = this.vehicleClient
      .addVehicleType(command)
      .pipe(finalize(() => (this.isSubmitting = false)))
      .subscribe({
        next: () => {
          this.toasterService.success('تمت الإضافة', 'تمت إضافة نوع المركبة بنجاح');
          this.onCancel();
        },
        error: (error) => {
          this.toasterService.error('خطأ', error?.message || 'تعذر إضافة نوع المركبة');
        }
      });
  }
}
