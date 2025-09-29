import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormControl } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { PageHeaderComponent } from 'src/app/shared/components/page-header/page-header.component';
import { AddVehicleBrandCommand, AddVehicleTypeCommand, DeliveryManVehicleDto, UpdateVehicleBrandCommand, UpdateVehicleTypeCommand, VehicleAdminClient, ActiveCategoryDto, MainCategoryAdminLookupDto } from 'src/app/Core/services/NaqlahClient';
import { SubSink } from 'subsink';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import Swal from 'sweetalert2';
import { ImageService } from 'src/app/Core/services/image.service';


@Component({
  selector: 'app-vehicles',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule, PageHeaderComponent],
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

  // Pagination
  totalCount = 0;
  totalPages = 0;
  currentPage = 0;
  itemsPerPage = 10;

  private sub = new SubSink();

  constructor(
    private fb: FormBuilder,
    private vehicleClient: VehicleAdminClient,
    private imageService: ImageService
  ) {
    this.itemForm = this.fb.group({
      arabicName: ['', [Validators.required, Validators.maxLength(100)]],
      englishName: ['', [Validators.required, Validators.maxLength(100)]],
      iconBase64: [''],
      mainCategoryIds: [[], []]
    });
  }

  ngOnInit(): void {
    this.loadItems();
    this.setupSearch();
    this.loadMainCategories();
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
    this.clearIcon();
    this.showModal = true;
  }

  openEdit(item: DeliveryManVehicleDto): void {
    debugger
    this.editingItem = item;
    this.itemForm.patchValue({
      arabicName: item.arabicName,
      englishName: item.englishName,
      mainCategoryIds: [] // TODO: Update when DTO is updated
    });
    
    this.clearIcon(); // TODO: Set icon preview when DTO is updated
    
    this.showUpdateModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.showUpdateModal = false;
    this.editingItem = null;
    this.itemForm.reset();
    this.clearIcon();
  }

  submit(): void {
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
      apiCall = this.vehicleClient.addVehicleType(command);
     }

    this.sub.sink = apiCall.subscribe({
      next: () => {
        this.closeModal();
        Swal.fire({
          title: 'تمت إضافة المركبة',
          text: 'تمت إضافة المركبة بنجاح',
          icon: 'success',
          confirmButtonText: 'موافق',
          timer: 3000
        }).then(() => {
          this.loadItems();
        });
      },
      error: (error) => {
        Swal.fire({
          title: 'خطأ',
          text: error?.message || 'حدث خطأ أثناء إضافة المركبة',
          icon: 'error',
          confirmButtonText: 'موافق',
          timer: 3000
        });
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
      command.iconBase64 = value.iconBase64;
      command.mainCategoryIds = value.mainCategoryIds;
      apiCall = this.vehicleClient.updateVehicleType(command);
    }

    this.sub.sink = apiCall.subscribe({
      next: () => {
        this.closeModal();
        Swal.fire({
          title: 'تم تحديث الطلب',
          text: 'تم تحديث الطلب بنجاح',
          icon: 'success',
          confirmButtonText: 'موافق',
          timer: 3000
        }).then(() => {
          this.loadItems();
        });
      },
      error: (error) => {
        Swal.fire({
          title: 'خطأ',
          text: error?.message || 'حدث خطأ أثناء إلغاء الطلب',
          icon: 'error',
          confirmButtonText: 'موافق',
          timer: 3000
        });
      }
    });
  }

  confirmDelete(itemId: number): void {
    let ApiCall;
    if (this.activeTab === 'brands') {
      ApiCall = this.vehicleClient.deleteVehicleBrand(itemId);
    } else {
      ApiCall = this.vehicleClient.deleteVehicleType(itemId);
    }
    
    Swal.fire({
      title: 'هل أنت متأكد؟',
      text: 'سيتم حذف هذا العنصر بشكل دائم',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'نعم، احذف',
      cancelButtonText: 'إلغاء'
    }).then((result) => {
      if (result.isConfirmed) {
        this.sub.sink = ApiCall.subscribe({
          next: () => {
            Swal.fire({
              title: 'تم الحذف',
              text: 'تم حذف العنصر بنجاح',
              icon: 'success',
              confirmButtonText: 'موافق',
              timer: 3000
            }).then(() => {
              this.loadItems();
            });
          },
          error: (error) => {
            Swal.fire({
              title: 'خطأ',
              text: error?.message || 'حدث خطأ أثناء إلغاء الطلب',
              icon: 'error',
              confirmButtonText: 'موافق',
              timer: 3000
            });
          }
        });
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
    return this.activeTab === 'brands' ? 'ADMIN.PAGES.VEHICLE_BRANDS.ADD_MODAL.TITLE' : 'ADMIN.PAGES.VEHICLE_TYPES.ADD_MODAL.TITLE';
  }

  get modalTitleEdit(): string {
    return this.activeTab === 'brands' ? 'ADMIN.PAGES.VEHICLE_BRANDS.EDIT_MODAL.TITLE' : 'ADMIN.PAGES.VEHICLE_TYPES.EDIT_MODAL.TITLE';
  }
}
