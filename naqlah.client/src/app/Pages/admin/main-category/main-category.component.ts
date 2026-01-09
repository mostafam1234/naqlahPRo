import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormControl, FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { PageHeaderComponent } from 'src/app/shared/components/page-header/page-header.component';
import { MainCategoryAdminClient, MainCategoryAdminDto, AddMainAdminCategory, UpdateMainAdminCategory } from 'src/app/Core/services/NaqlahClient';
import { SubSink } from 'subsink';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { ToasterService } from 'src/app/Core/services/toaster.service';
import { ConfirmationModalComponent } from 'src/app/shared/components/confirmation-modal/confirmation-modal.component';
import { ImageService } from 'src/app/Core/services/image.service';

@Component({
  selector: 'app-main-category',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, TranslateModule, PageHeaderComponent, ConfirmationModalComponent],
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

  // Confirmation Modal
  showConfirmModal = false;
  confirmationTitle = '';
  confirmationMessage = '';
  private pendingAction?: () => void;

  private sub = new SubSink();

  constructor(
    private fb: FormBuilder,
    private mainCategoryClient: MainCategoryAdminClient,
    private toasterService: ToasterService,
    private imageService: ImageService
  ) {
    this.itemForm = this.fb.group({
      arabicName: ['', [Validators.required, Validators.maxLength(100)]],
      englishName: ['', [Validators.required, Validators.maxLength(100)]],
      imageBase64: ['', [Validators.required]]
    });
  }

  ngOnInit(): void {
    this.loadItems();
    this.setupSearch();
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
    this.removeImage();
    this.showModal = true;
  }

  openEdit(item: MainCategoryAdminDto): void {
    this.editingItem = item;
    this.itemForm.patchValue({
      arabicName: item.arabicName,
      englishName: item.englishName,
      imageBase64: '' // Empty - will only be set if user uploads new image
    });
    
    // Set preview to show existing image
    if (item.imagePath) {
      this.selectedImagePreview = item.imagePath;
      // Don't set file info since it's an existing image from server
      this.selectedImageFile = null;
      this.selectedImageName = '';
      this.selectedImageSize = 0;
    } else {
      this.removeImage();
    }
    
    this.showUpdateModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.showUpdateModal = false;
    this.editingItem = null;
    this.itemForm.reset();
    this.removeImage();
  }

  submit(): void {
    // Mark all fields as touched to show validation errors
    this.itemForm.markAllAsTouched();
    
    if (this.itemForm.invalid) {
      // Check which field is invalid
      if (this.itemForm.get('imageBase64')?.invalid) {
        this.toasterService.error('خطأ', 'يرجى رفع صورة الفئة');
      }
      return;
    }

    this.performAdd();
  }

  private performAdd(): void {
    const value = this.itemForm.value;
    
    // Validate image is present
    if (!value.imageBase64 || value.imageBase64.trim() === '') {
      this.toasterService.error('خطأ', 'يرجى رفع صورة الفئة');
      return;
    }
    
    const command = new AddMainAdminCategory();
    command.arabicName = value.arabicName;
    command.englishName = value.englishName;
    command.imageBase64 = value.imageBase64;

    console.log('Sending command:', {
      arabicName: command.arabicName,
      englishName: command.englishName,
      hasImage: !!command.imageBase64,
      imageLength: command.imageBase64?.length || 0
    });

    this.sub.sink = this.mainCategoryClient.addMainCategoryAdmin(command).subscribe({
      next: () => {
        this.closeModal();
        this.toasterService.success('تمت الإضافة بنجاح', 'تمت إضافة الفئة الرئيسية بنجاح');
        this.loadItems();
      },
      error: (error) => {
        console.error('Error adding category:', error);
        
        // Extract error message from backend response
        let errorMessage = 'حدث خطأ أثناء إضافة الفئة';
        
        if (error?.error) {
          // Check for ProblemDetail format (detail, title, or errorMessage)
          if (error.error.detail) {
            errorMessage = error.error.detail;
          } else if (error.error.errorMessage) {
            errorMessage = error.error.errorMessage;
          } else if (error.error.title) {
            errorMessage = error.error.title;
          } else if (error.error.message) {
            errorMessage = error.error.message;
          } else if (typeof error.error === 'string') {
            errorMessage = error.error;
          }
        } else if (error?.message) {
          errorMessage = error.message;
        }
        
        this.toasterService.error('خطأ', errorMessage);
      }
    });
  }

  update(): void {
    if (this.itemForm.invalid) return;

    const itemId = this.editingItem?.id;
    const value = this.itemForm.value;
    const command = new UpdateMainAdminCategory();
    command.id = itemId;
    command.arabicName = value.arabicName;
    command.englishName = value.englishName;
    // Only send imageBase64 if user uploaded a new image (not empty string)
    // If empty string, send null so backend keeps existing image
    command.imageBase64 = value.imageBase64 && value.imageBase64.trim() !== '' ? value.imageBase64 : null;

    this.sub.sink = this.mainCategoryClient.updateMainCategoryAdmin(command).subscribe({
      next: () => {
        this.closeModal();
        this.toasterService.success('تم التحديث بنجاح', 'تم تحديث الفئة الرئيسية بنجاح');
        this.loadItems();
      },
      error: (error) => {
        console.error('Error updating category:', error);
        
        // Extract error message from backend response
        let errorMessage = 'حدث خطأ أثناء تحديث الفئة';
        
        if (error?.error) {
          // Check for ProblemDetail format (detail or title)
          if (error.error.detail) {
            errorMessage = error.error.detail;
          } else if (error.error.title) {
            errorMessage = error.error.title;
          } else if (error.error.message) {
            errorMessage = error.error.message;
          } else if (typeof error.error === 'string') {
            errorMessage = error.error;
          }
        } else if (error?.message) {
          errorMessage = error.message;
        }
        
        this.toasterService.error('خطأ', errorMessage);
      }
    });
  }

  confirmDelete(itemId: number): void {
    this.confirmationTitle = 'تأكيد الحذف';
    this.confirmationMessage = 'هل أنت متأكد من حذف هذه الفئة؟';
    this.pendingAction = () => this.performDelete(itemId);
    this.showConfirmModal = true;
  }

  private performDelete(itemId: number): void {
    this.sub.sink = this.mainCategoryClient.deleteMainCategoryAdmin(itemId).subscribe({
      next: () => {
        this.toasterService.success('تم الحذف بنجاح', 'تم حذف الفئة بنجاح');
        this.loadItems();
      },
      error: (error) => {
        console.error('Error deleting category:', error);
        
        // Extract error message from backend response
        let errorMessage = 'حدث خطأ أثناء حذف الفئة';
        
        if (error?.error) {
          // Check for ProblemDetail format (detail or title)
          if (error.error.detail) {
            errorMessage = error.error.detail;
          } else if (error.error.title) {
            errorMessage = error.error.title;
          } else if (error.error.message) {
            errorMessage = error.error.message;
          } else if (typeof error.error === 'string') {
            errorMessage = error.error;
          }
        } else if (error?.message) {
          errorMessage = error.message;
        }
        
        this.toasterService.error('خطأ', errorMessage);
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

  // Image Upload Methods
  async onImageSelect(event: Event): Promise<void> {
    const result = await this.imageService.handleImageUpload(event, {
      maxSizeMB: 2,
      allowedTypes: ['image/png', 'image/jpg', 'image/jpeg'],
      showErrorAlert: true
    });

    if (!result?.success) {
      this.imageError = result?.error || 'فشل في رفع الصورة';
      return;
    }

    // Clear any previous errors
    this.imageError = '';

    // Set preview
    this.selectedImagePreview = result.preview || null;
    
    // Set form value with Base64 - ensure it's a string
    const base64Value = result.base64 || '';
    this.itemForm.patchValue({ imageBase64: base64Value });
    this.itemForm.get('imageBase64')?.markAsTouched();
    this.itemForm.get('imageBase64')?.updateValueAndValidity();
    
    // Verify the value was set
    console.log('Form imageBase64 value after setting:', {
      formValue: this.itemForm.get('imageBase64')?.value,
      isValid: this.itemForm.get('imageBase64')?.valid,
      base64Length: base64Value.length
    });
    
    // Get file info for display
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      const file = input.files[0];
      this.selectedImageFile = file;
      this.selectedImageName = file.name;
      this.selectedImageSize = file.size;
    }
    
    console.log('Image selected:', {
      hasBase64: !!result.base64,
      base64Length: result.base64?.length || 0,
      fileName: this.selectedImageName
    });
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver = false;
  }

  async onDrop(event: DragEvent): Promise<void> {
    event.preventDefault();
    this.isDragOver = false;
    const files = event.dataTransfer?.files;
    if (files && files.length > 0) {
      const file = files[0];
      const result = await this.imageService.convertFileToBase64(file, {
        maxSizeMB: 2,
        allowedTypes: ['image/png', 'image/jpg', 'image/jpeg'],
        showErrorAlert: true
      });

      if (!result?.success) {
        this.imageError = result?.error || 'فشل في رفع الصورة';
        return;
      }

      // Clear any previous errors
      this.imageError = '';

      // Set preview
      this.selectedImagePreview = result.preview || null;
      
      // Set form value with Base64 - ensure it's a string
      const base64Value = result.base64 || '';
      this.itemForm.patchValue({ imageBase64: base64Value });
      this.itemForm.get('imageBase64')?.markAsTouched();
      this.itemForm.get('imageBase64')?.updateValueAndValidity();
      
      // Verify the value was set
      console.log('Form imageBase64 value after drop:', {
        formValue: this.itemForm.get('imageBase64')?.value,
        isValid: this.itemForm.get('imageBase64')?.valid,
        base64Length: base64Value.length
      });
      
      // Get file info for display
      this.selectedImageFile = file;
      this.selectedImageName = file.name;
      this.selectedImageSize = file.size;
      
      console.log('Image dropped:', {
        hasBase64: !!result.base64,
        base64Length: result.base64?.length || 0,
        fileName: this.selectedImageName
      });
    }
  }


  removeImage(): void {
    this.selectedImageFile = null;
    this.selectedImagePreview = null;
    this.selectedImageName = '';
    this.selectedImageSize = 0;
    this.imageError = '';
    // Clear imageBase64 - if editing, this will keep existing image (null = keep existing)
    // If adding, this will require user to upload an image
    this.itemForm.patchValue({ imageBase64: '' });
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
