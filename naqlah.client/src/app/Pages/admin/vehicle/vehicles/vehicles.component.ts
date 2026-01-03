import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormControl, FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { PageHeaderComponent } from 'src/app/shared/components/page-header/page-header.component';
import { AddVehicleBrandCommand, AddVehicleTypeCommand, DeliveryManVehicleDto, UpdateVehicleBrandCommand, UpdateVehicleTypeCommand, VehicleAdminClient, ActiveCategoryDto, MainCategoryAdminLookupDto } from 'src/app/Core/services/NaqlahClient';
import { SubSink } from 'subsink';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { ImageService } from 'src/app/Core/services/image.service';
import { ToasterService } from 'src/app/Core/services/toaster.service';
import { ConfirmationModalComponent } from 'src/app/shared/components/confirmation-modal/confirmation-modal.component';


@Component({
  selector: 'app-vehicles',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, TranslateModule, PageHeaderComponent, ConfirmationModalComponent],
  templateUrl: './vehicles.component.html',
  styleUrls: ['./vehicles.component.css']
})
export class VehiclesComponent implements OnInit, OnDestroy {
  // UI State
  activeTab: 'brands' | 'types' = 'brands';
  showModal = false;
  showUpdateModal = false;
  editingItem: DeliveryManVehicleDto | null = null;
  isLoading = false;

  // Form Controls
  itemForm: FormGroup;
  searchControl = new FormControl('');

  // Data
  items: DeliveryManVehicleDto[] = [];
  mainCategories: MainCategoryAdminLookupDto[] = [];
  selectedIconFile: File | null = null;
  iconPreview: string | null = null;

  // Multi-Select Properties for Categories
  isCategoryMultiSelectOpen = false;
  categorySearchTerm = '';
  selectedCategories: MainCategoryAdminLookupDto[] = [];

  // Image Upload Properties
  selectedIconName = '';
  selectedIconSize = 0;
  isDragOver = false;
  iconError = '';

  // Pagination
  totalCount = 0;
  totalPages = 0;
  currentPage = 0;
  itemsPerPage = 10;

  // Confirmation Modal
  showConfirmModal = false;
  confirmationTitle = '';
  confirmationMessage = '';
  private pendingAction?: () => void;

  private sub = new SubSink();

  constructor(
    private fb: FormBuilder,
    private vehicleClient: VehicleAdminClient,
    private toasterService: ToasterService,
    private imageService: ImageService
  ) {
    this.itemForm = this.fb.group({
      arabicName: ['', [Validators.required, Validators.maxLength(100)]],
      englishName: ['', [Validators.required, Validators.maxLength(100)]],
      iconBase64: [''],
      mainCategoryIds: [[], []],
      cost: [0, [Validators.min(0)]]
    });
  }

  ngOnInit(): void {
    this.loadItems();
    this.setupSearch();
    this.loadMainCategories();
    this.setupDocumentClick();
  }

  private setupDocumentClick(): void {
    document.addEventListener('click', (event) => {
      const target = event.target as HTMLElement;
      if (!target.closest('.multi-select-container')) {
        this.isCategoryMultiSelectOpen = false;
      }
    });
  }

  ngOnDestroy(): void {
    this.sub.unsubscribe();
  }

  setupSearch(): void {
    this.sub.sink = this.searchControl.valueChanges
      .pipe(
        debounceTime(500),
        distinctUntilChanged()
      )
      .subscribe(() => {
        this.currentPage = 0;
        this.loadItems();
      });
  }

  loadItems(): void {
    this.isLoading = true;
    const skip = this.currentPage * this.itemsPerPage;
    const searchTerm = this.searchControl.value || '';

    const apiCall = this.activeTab === 'brands'
      ? this.vehicleClient.getVehiclesBrands(skip, this.itemsPerPage, searchTerm)
      : this.vehicleClient.getVehiclesTypes(skip, this.itemsPerPage, searchTerm);

    this.sub.sink = apiCall.subscribe({
      next: (response: any) => {
        console.log('API Response:', response);
        console.log('Items data:', response.data);
        this.items = response.data;
        this.totalCount = response.totalCount;
        this.totalPages = response.totalPages;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading items:', error);
        this.isLoading = false;
      }
    });
  }

  loadMainCategories(): void {
    this.sub.sink = this.vehicleClient.getMainCategoriesLookup().subscribe({
      next: (categories) => {
        this.mainCategories = categories;
      },
      error: (error) => {
        console.error('Error loading main categories:', error);
      }
    });
  }

  async onIconFileSelected(event: any): Promise<void> {
    const result = await this.imageService.handleImageUpload(event, {
      maxSizeMB: 3,
      showErrorAlert: true
    });

    if (result?.success) {
      this.iconPreview = result.preview || null;
      this.itemForm.patchValue({ iconBase64: result.base64 });
    }
  }

  clearIcon(): void {
    this.selectedIconFile = null;
    this.iconPreview = null;
    this.itemForm.patchValue({ iconBase64: '' });
  }

  onCategoryChange(categoryId: number, event: any): void {
    const currentIds = this.itemForm.get('mainCategoryIds')?.value || [];

    if (event.target.checked) {
      // Add category if not exists
      if (!currentIds.includes(categoryId)) {
        currentIds.push(categoryId);
      }
    } else {
      // Remove category
      const index = currentIds.indexOf(categoryId);
      if (index > -1) {
        currentIds.splice(index, 1);
      }
    }

    this.itemForm.patchValue({ mainCategoryIds: currentIds });
  }

  isMainCategorySelected(categoryId: number): boolean {
    const currentIds = this.itemForm.get('mainCategoryIds')?.value || [];
    return currentIds.includes(categoryId);
  }

  setActiveTab(tab: 'brands' | 'types'): void {
    this.activeTab = tab;
    this.currentPage = 0;
    this.searchControl.setValue('');
    this.loadItems();
  }

  openAdd(): void {
    this.editingItem = null;
    this.itemForm.reset();
    this.selectedCategories = [];
    this.isCategoryMultiSelectOpen = false;
    this.categorySearchTerm = '';
    this.removeIcon();
    this.showModal = true;
  }

  openEdit(item: DeliveryManVehicleDto): void {
    debugger
    this.editingItem = item;

    this.selectedCategories = item.mainCategories?.map(cat => {
      const lookupDto = new MainCategoryAdminLookupDto();
      lookupDto.id = cat.id;
      lookupDto.name = (cat as any).name || cat.arabicName;
      return lookupDto;
    }) || [];

    // Extract all category IDs for form
    const categoryIds = item.mainCategories?.map(cat => cat.id) || [];

    this.itemForm.patchValue({
      arabicName: item.arabicName,
      englishName: item.englishName,
      mainCategoryIds: categoryIds,
      iconBase64: '', // Start with empty, will be set if user uploads new image
      cost: (item as any).cost || 0 // Add cost if available
    });

    // Reset multi-select state
    this.isCategoryMultiSelectOpen = false;
    this.categorySearchTerm = '';

    // Set icon preview if available
    if (item.iconImagePath && this.activeTab === 'types') {
      this.iconPreview = item.iconImagePath;
      this.selectedIconName = 'صورة محفوظة';
      this.selectedIconSize = 0; // We don't know the size from backend
      this.iconError = '';
    } else {
      this.removeIcon();
    }

    this.showUpdateModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.showUpdateModal = false;
    this.editingItem = null;
    this.itemForm.reset();

    // Reset multi-select
    this.selectedCategories = [];
    this.isCategoryMultiSelectOpen = false;
    this.categorySearchTerm = '';

    // Reset image upload
    this.removeIcon();
  }

  submit(): void {
    debugger;
    if (this.itemForm.invalid) return;

    const value = this.itemForm.value;

    let apiCall;
    if (this.activeTab === 'brands') {
      let command = new AddVehicleBrandCommand();
      command.arabicName = value.arabicName;
      command.englishName = value.englishName;
      apiCall = this.vehicleClient.addVehicleBrand(command);
    } else {
      let command = new AddVehicleTypeCommand();
      command.arabicName = value.arabicName;
      command.englishName = value.englishName;
      command.iconBase64 = value.iconBase64;
      command.mainCategoryIds = value.mainCategoryIds;
      command.cost = value.cost;
      apiCall = this.vehicleClient.addVehicleType(command);
     }

    this.sub.sink = apiCall.subscribe({
      next: () => {
        this.closeModal();
        this.toasterService.success('تمت الإضافة بنجاح', 'تمت إضافة المركبة بنجاح');
        this.loadItems();
      },
      error: (error) => {
        this.toasterService.error('خطأ', error?.message || 'حدث خطأ أثناء إضافة المركبة');
      }
    });
  }

  update(): void {
    debugger;
    if (this.itemForm.invalid) return;
    const itemId = this.editingItem?.id;
    const value = this.itemForm.value;

    let apiCall;
    if (this.activeTab === 'brands') {
      let command = new UpdateVehicleBrandCommand();
      command.vehicleBrandId = itemId;
      command.arabicName = value.arabicName;
      command.englishName = value.englishName;
      apiCall = this.vehicleClient.updateVehicleBrand(command);
    } else {
      let command = new UpdateVehicleTypeCommand();
      command.vehicleTypeId = itemId;
      command.arabicName = value.arabicName;
      command.englishName = value.englishName;
      // Only send iconBase64 if user uploaded a new image
      command.iconBase64 = value.iconBase64 || null;
      command.mainCategoryIds = value.mainCategoryIds;
      command.cost = value.cost || 0;
      apiCall = this.vehicleClient.updateVehicleType(command);
    }

    this.sub.sink = apiCall.subscribe({
      next: () => {
        this.closeModal();
        this.toasterService.success('تم التحديث بنجاح', 'تم تحديث المركبة بنجاح');
        this.loadItems();
      },
      error: (error) => {
        this.toasterService.error('خطأ', error?.message || 'حدث خطأ أثناء التحديث');
      }
    });
  }

  confirmDelete(itemId: number): void {
    const itemType = this.activeTab === 'brands' ? 'الماركة' : 'نوع المركبة';

    this.confirmationTitle = 'تأكيد الحذف';
    this.confirmationMessage = `هل أنت متأكد من حذف ${itemType}؟`;
    this.pendingAction = () => this.performDelete(itemId);
    this.showConfirmModal = true;
  }

  private performDelete(itemId: number): void {
    const apiCall = this.activeTab === 'brands'
      ? this.vehicleClient.deleteVehicleBrand(itemId)
      : this.vehicleClient.deleteVehicleType(itemId);

    this.sub.sink = apiCall.subscribe({
      next: () => {
        this.toasterService.success('تم الحذف بنجاح', 'تم حذف العنصر بنجاح');
        this.loadItems();
      },
      error: (error) => {
        this.toasterService.error('خطأ', error?.message || 'حدث خطأ أثناء الحذف');
      }
    });
  }

  changePage(page: number): void {
    const backendPage = page - 1;
    if (backendPage >= 0 && backendPage < this.totalPages) {
      this.currentPage = backendPage;
      this.loadItems();
    }
  }

  get displayCurrentPage(): number {
    return this.currentPage + 1;
  }

  get visiblePages(): number[] {
    const current = this.displayCurrentPage;
    const total = this.totalPages;
    const pages: number[] = [];

    if (total <= 7) {
      for (let i = 1; i <= total; i++) {
        pages.push(i);
      }
    } else {
      pages.push(1);
      if (current <= 4) {
        for (let i = 2; i <= 5; i++) {
          pages.push(i);
        }
        pages.push(-1);
        pages.push(total);
      } else if (current >= total - 3) {
        pages.push(-1);
        for (let i = total - 4; i <= total; i++) {
          pages.push(i);
        }
      } else {
        pages.push(-1);
        for (let i = current - 1; i <= current + 1; i++) {
          pages.push(i);
        }
        pages.push(-1);
        pages.push(total);
      }
    }
    return pages;
  }

  get displayStartCount(): number {
    if (this.totalCount === 0) return 0;
    return (this.currentPage * this.itemsPerPage) + 1;
  }

  get displayEndCount(): number {
    if (this.totalCount === 0) return 0;
    const endCount = (this.currentPage + 1) * this.itemsPerPage;
    return Math.min(endCount, this.totalCount);
  }

  getAbsoluteDifference(a: number, b: number): number {
    return Math.abs(a - b);
  }

  goBack(): void {
    window.history.back();
  }

  get currentTabTitle(): string {
    return this.activeTab === 'brands' ? 'ADMIN.VEHICLESMENUE.BRANDS' : 'ADMIN.VEHICLESMENUE.TYPES';
  }

  get currentTabDescription(): string {
    return this.activeTab === 'brands' ? 'ADMIN.VEHICLESMENUE.BRANDS_DESC' : 'ADMIN.VEHICLESMENUE.TYPES_DESC';
  }

  get addButtonText(): string {
    return this.activeTab === 'brands' ? 'ADMIN.VEHICLESMENUE.ADD_BUTTON_BRAND' : 'ADMIN.VEHICLESMENUE.ADD_BUTTON';
  }

  get tableTitle(): string {
    return this.activeTab === 'brands' ? 'ADMIN.VEHICLESMENUE.TABLE_TITLE_BRAND' : 'ADMIN.VEHICLESMENUE.TABLE_TITLE';
  }

  get noDataText(): string {
    return this.activeTab === 'brands' ? 'ADMIN.VEHICLESMENUE.NO_DATA_BRAND' : 'ADMIN.VEHICLESMENUE.NO_DATA';
  }

  get modalTitleAdd(): string {
    return this.activeTab === 'brands' ? 'ADMIN.PAGES.VEHICLES.ADD_BRAND_VEHICLE' : 'ADMIN.PAGES.VEHICLES.ADD_TYPE_VEHICLE';
  }

  get modalTitleEdit(): string {
    return this.activeTab === 'brands' ? 'ADMIN.PAGES.VEHICLES.EDIT_BRAND_VEHICLE' : 'ADMIN.PAGES.VEHICLES.EDIT_TYPE_VEHICLE';
  }

  // Multi-Select Methods
  toggleMultiSelect(): void {
    this.isCategoryMultiSelectOpen = !this.isCategoryMultiSelectOpen;
  }

  get filteredCategories(): MainCategoryAdminLookupDto[] {
    if (!this.categorySearchTerm) {
      return this.mainCategories;
    }
    return this.mainCategories.filter(category =>
      category.name?.toLowerCase().includes(this.categorySearchTerm.toLowerCase())
    );
  }

  isCategorySelected(category: MainCategoryAdminLookupDto): boolean {
    return this.selectedCategories.some(selected => selected.id === category.id);
  }

  toggleCategory(category: MainCategoryAdminLookupDto): void {
    const index = this.selectedCategories.findIndex(selected => selected.id === category.id);
    if (index > -1) {
      this.selectedCategories.splice(index, 1);
    } else {
      this.selectedCategories.push(category);
    }
    this.itemForm.patchValue({ mainCategoryIds: this.selectedCategories.map(c => c.id) });
  }

  removeCategory(category: MainCategoryAdminLookupDto, event: Event): void {
    event.stopPropagation();
    const index = this.selectedCategories.findIndex(selected => selected.id === category.id);
    if (index > -1) {
      this.selectedCategories.splice(index, 1);
      this.itemForm.patchValue({ mainCategoryIds: this.selectedCategories.map(c => c.id) });
    }
  }

  // Image Upload Methods
  onIconSelect(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.handleIconFile(file);
    }
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
    const files = event.dataTransfer?.files;
    if (files && files.length > 0) {
      this.handleIconFile(files[0]);
    }
  }

  private handleIconFile(file: File): void {
    this.iconError = '';

    // Validate file type
    if (!file.type.startsWith('image/')) {
      this.iconError = 'يرجى اختيار ملف صورة صحيح';
      return;
    }

    // Validate file size (2MB)
    if (file.size > 2 * 1024 * 1024) {
      this.iconError = 'حجم الصورة يجب أن يكون أقل من 2 ميجابايت';
      return;
    }

    this.selectedIconFile = file;
    this.selectedIconName = file.name;
    this.selectedIconSize = file.size;

    // Create preview
    const reader = new FileReader();
    reader.onload = (e) => {
      this.iconPreview = e.target?.result as string;
      this.itemForm.patchValue({ iconBase64: this.iconPreview });
    };
    reader.readAsDataURL(file);
  }

  removeIcon(): void {
    this.selectedIconFile = null;
    this.iconPreview = null;
    this.selectedIconName = '';
    this.selectedIconSize = 0;
    this.iconError = '';
    this.itemForm.patchValue({ iconBase64: '' });
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
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
}
