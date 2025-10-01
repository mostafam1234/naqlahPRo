import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormControl, FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { PageHeaderComponent } from 'src/app/shared/components/page-header/page-header.component';
import { MainCategoryAdminClient, MainCategoryAdminDto, AddMainAdminCategory, UpdateMainAdminCategory } from 'src/app/Core/services/NaqlahClient';
import { SubSink } from 'subsink';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-main-category',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, TranslateModule, PageHeaderComponent],
  providers: [MainCategoryAdminClient],
  templateUrl: './main-category.component.html',
  styleUrls: ['./main-category.component.css']
})
export class MainCategoryComponent implements OnInit, OnDestroy {
  // UI State
  showModal = false;
  showUpdateModal = false;
  editingItem: MainCategoryAdminDto | null = null;
  isLoading = false;

  // Form Controls
  itemForm: FormGroup;
  searchControl = new FormControl('');

  // Data
  items: MainCategoryAdminDto[] = [];

  // Multi-Select Properties
  isMultiSelectOpen = false;
  serviceSearchTerm = '';
  selectedServices: any[] = [];
  availableServices: any[] = [
    { id: 1, name: 'نقل البضائع' },
    { id: 2, name: 'توصيل الطلبات' },
    { id: 3, name: 'النقل السريع' },
    { id: 4, name: 'النقل المبرد' },
    { id: 5, name: 'نقل الأثاث' },
    { id: 6, name: 'التوصيل المجدول' }
  ];

  // Image Upload Properties
  selectedImagePreview: string | null = null;
  selectedImageName = '';
  selectedImageSize = 0;
  selectedImageFile: File | null = null;
  isDragOver = false;
  imageError = '';

  // Pagination
  totalCount = 0;
  totalPages = 0;
  currentPage = 0;
  itemsPerPage = 10;

  private sub = new SubSink();

  constructor(
    private fb: FormBuilder,
    private mainCategoryClient: MainCategoryAdminClient
  ) {
    this.itemForm = this.fb.group({
      arabicName: ['', [Validators.required, Validators.maxLength(100)]],
      englishName: ['', [Validators.required, Validators.maxLength(100)]],
      services: [[], [Validators.required]]
    });
  }

  ngOnInit(): void {
    this.loadItems();
    this.setupSearch();
    this.setupDocumentClick();
  }

  private setupDocumentClick(): void {
    document.addEventListener('click', (event) => {
      const target = event.target as HTMLElement;
      if (!target.closest('.multi-select-container')) {
        this.isMultiSelectOpen = false;
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

    this.sub.sink = this.mainCategoryClient.getAllMainCategories(skip, this.itemsPerPage, searchTerm).subscribe({
      next: (response: any) => {
        console.log('Full response:', response);
        this.items = response.data;
        console.log(this.items)
        this.totalCount = response.totalCount;
        this.totalPages = response.totalPages;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading main categories:', error);
        this.isLoading = false;
      }
    });
  }

  openAdd(): void {
    this.editingItem = null;
    this.itemForm.reset();
    this.selectedServices = [];
    this.isMultiSelectOpen = false;
    this.serviceSearchTerm = '';
    this.removeImage();
    this.showModal = true;
  }

  openEdit(item: MainCategoryAdminDto): void {
    this.editingItem = item;
    this.itemForm.patchValue({
      arabicName: item.arabicName,
      englishName: item.englishName
    });
    this.showUpdateModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.showUpdateModal = false;
    this.editingItem = null;
    this.itemForm.reset();

    // Reset multi-select
    this.selectedServices = [];
    this.isMultiSelectOpen = false;
    this.serviceSearchTerm = '';

    // Reset image upload
    this.removeImage();
  }

  submit(): void {
    if (this.itemForm.invalid) return;

    const value = this.itemForm.value;
    const command = new AddMainAdminCategory();
    command.arabicName = value.arabicName;
    command.englishName = value.englishName;

    this.sub.sink = this.mainCategoryClient.addMainCategoryAdmin(command).subscribe({
      next: () => {
        this.closeModal();
        Swal.fire({
          title: 'تمت إضافة الفئة',
          text: 'تمت إضافة الفئة الرئيسية بنجاح',
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
          text: error?.message || 'حدث خطأ أثناء إضافة الفئة',
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
    const command = new UpdateMainAdminCategory();
    command.id = itemId;
    command.arabicName = value.arabicName;
    command.englishName = value.englishName;

    this.sub.sink = this.mainCategoryClient.updateMainCategoryAdmin(command).subscribe({
      next: () => {
        this.closeModal();
        Swal.fire({
          title: 'تم تحديث الفئة',
          text: 'تم تحديث الفئة الرئيسية بنجاح',
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
          text: error?.message || 'حدث خطأ أثناء تحديث الفئة',
          icon: 'error',
          confirmButtonText: 'موافق',
          timer: 3000
        });
      }
    });
  }

  confirmDelete(itemId: number): void {
    Swal.fire({
      title: 'هل أنت متأكد؟',
      text: 'سيتم حذف هذه الفئة بشكل دائم',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'نعم، احذف',
      cancelButtonText: 'إلغاء'
    }).then((result) => {
      if (result.isConfirmed) {
        this.sub.sink = this.mainCategoryClient.deleteMainCategoryAdmin(itemId).subscribe({
          next: () => {
            Swal.fire({
              title: 'تم الحذف',
              text: 'تم حذف الفئة بنجاح',
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
              text: error?.message || 'حدث خطأ أثناء حذف الفئة',
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

  goBack(): void {
    window.history.back();
  }

  // Multi-Select Methods
  toggleMultiSelect(): void {
    this.isMultiSelectOpen = !this.isMultiSelectOpen;
  }

  get filteredServices(): any[] {
    if (!this.serviceSearchTerm) {
      return this.availableServices;
    }
    return this.availableServices.filter(service =>
      service.name.toLowerCase().includes(this.serviceSearchTerm.toLowerCase())
    );
  }

  isServiceSelected(service: any): boolean {
    return this.selectedServices.some(selected => selected.id === service.id);
  }

  toggleService(service: any): void {
    const index = this.selectedServices.findIndex(selected => selected.id === service.id);
    if (index > -1) {
      this.selectedServices.splice(index, 1);
    } else {
      this.selectedServices.push(service);
    }
    this.itemForm.patchValue({ services: this.selectedServices });
  }

  removeService(service: any, event: Event): void {
    event.stopPropagation();
    const index = this.selectedServices.findIndex(selected => selected.id === service.id);
    if (index > -1) {
      this.selectedServices.splice(index, 1);
      this.itemForm.patchValue({ services: this.selectedServices });
    }
  }

  // Image Upload Methods
  onImageSelect(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.handleImageFile(file);
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
      this.handleImageFile(files[0]);
    }
  }

  private handleImageFile(file: File): void {
    this.imageError = '';

    // Validate file type
    if (!file.type.startsWith('image/')) {
      this.imageError = 'يرجى اختيار ملف صورة صحيح';
      return;
    }

    // Validate file size (2MB)
    if (file.size > 2 * 1024 * 1024) {
      this.imageError = 'حجم الصورة يجب أن يكون أقل من 2 ميجابايت';
      return;
    }

    this.selectedImageFile = file;
    this.selectedImageName = file.name;
    this.selectedImageSize = file.size;

    // Create preview
    const reader = new FileReader();
    reader.onload = (e) => {
      this.selectedImagePreview = e.target?.result as string;
    };
    reader.readAsDataURL(file);

    this.itemForm.patchValue({ image: file });
  }

  removeImage(): void {
    this.selectedImageFile = null;
    this.selectedImagePreview = null;
    this.selectedImageName = '';
    this.selectedImageSize = 0;
    this.imageError = '';
    this.itemForm.patchValue({ image: null });
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }
}
