import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormControl } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { SubSink } from 'subsink';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { ToasterService } from 'src/app/Core/services/toaster.service';
import { ConfirmationModalComponent } from 'src/app/shared/components/confirmation-modal/confirmation-modal.component';
import { OrderAdminClient, OrderPackageDto, AddOrderPackageCommand, UpdateOrderPackageCommand } from 'src/app/Core/services/NaqlahClient';
import { LanguageService } from 'src/app/Core/services/language.service';
import { PageHeaderComponent } from 'src/app/shared/components/page-header/page-header.component';

@Component({
  selector: 'app-types-control',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule, ConfirmationModalComponent, PageHeaderComponent],
  providers: [OrderAdminClient],
  templateUrl: './types-control.component.html',
  styleUrl: './types-control.component.css'
})
export class TypesControlComponent implements OnInit, OnDestroy {
  isModalOpen = false;
  isEditMode = false;
  editingItem: OrderPackageDto | null = null;
  isLoading = false;
  lang: string = 'ar';

  itemForm: FormGroup;
  searchControl = new FormControl('');

  items: OrderPackageDto[] = [];
  totalCount = 0;
  totalPages = 0;
  currentPage = 0;
  itemsPerPage = 10;

  showConfirmModal = false;
  confirmationTitle = '';
  confirmationMessage = '';
  private pendingAction?: () => void;

  private sub = new SubSink();

  constructor(
    private fb: FormBuilder,
    private toasterService: ToasterService,
    private orderAdminClient: OrderAdminClient,
    private languageService: LanguageService
  ) {
    this.itemForm = this.fb.group({
      arabicDescription: ['', [Validators.required, Validators.maxLength(200)]],
      englishDescription: ['', [Validators.required, Validators.maxLength(200)]],
      minWeightInKiloGram: [0, [Validators.required, Validators.min(0)]],
      maxWeightInKiloGram: [0, [Validators.required, Validators.min(0)]]
    });
  }

  ngOnInit(): void {
    this.lang = this.languageService.getLanguage();
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

    this.sub.sink = this.orderAdminClient.getAllOrderPackages(skip, this.itemsPerPage, searchTerm).subscribe({
      next: (response: any) => {
        this.items = response.data || [];
        this.totalCount = response.totalCount || 0;
        this.totalPages = response.totalPages || 0;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading order packages:', error);
        this.isLoading = false;
        this.toasterService.error('خطأ', 'حدث خطأ أثناء تحميل البيانات');
      }
    });
  }

  openModal(): void {
    this.isEditMode = false;
    this.editingItem = null;
    this.itemForm.reset({
      arabicDescription: '',
      englishDescription: '',
      minWeightInKiloGram: 0,
      maxWeightInKiloGram: 0
    });
    this.isModalOpen = true;
  }

  openEdit(item: OrderPackageDto): void {
    this.isEditMode = true;
    this.editingItem = item;
    this.itemForm.patchValue({
      arabicDescription: item.arabicDescription,
      englishDescription: item.englishDescription,
      minWeightInKiloGram: item.minWeightInKg,
      maxWeightInKiloGram: item.maxWeightInKg
    });
    this.isModalOpen = true;
  }

  closeModal(): void {
    this.isModalOpen = false;
    this.isEditMode = false;
    this.editingItem = null;
    this.itemForm.reset();
  }

  saveAction(): void {
    if (this.itemForm.invalid) {
      this.toasterService.error('خطأ', 'يرجى ملء جميع الحقول المطلوبة بشكل صحيح');
      return;
    }

    // Validate min < max
    const formValue = this.itemForm.value;
    if (formValue.minWeightInKiloGram > formValue.maxWeightInKiloGram) {
      this.toasterService.error('خطأ', 'الوزن الأدنى يجب أن يكون أقل من الوزن الأقصى');
      return;
    }

    if (this.isEditMode && this.editingItem) {
      this.performUpdate();
    } else {
      this.performAdd();
    }
  }

  private performAdd(): void {
    const value = this.itemForm.value;
    const command = new AddOrderPackageCommand();
    command.arabicDescription = value.arabicDescription;
    command.englishDescription = value.englishDescription;
    command.minWeightInKiloGram = value.minWeightInKiloGram;
    command.maxWeightInKiloGram = value.maxWeightInKiloGram;

    this.sub.sink = this.orderAdminClient.addOrderPackage(command).subscribe({
      next: () => {
        this.closeModal();
        this.toasterService.success('تمت الإضافة بنجاح', 'تمت إضافة نوع الشحنة بنجاح');
        this.loadItems();
      },
      error: (error) => {
        const errorMessage = error?.error?.detail || error?.message || 'حدث خطأ أثناء إضافة نوع الشحنة';
        this.toasterService.error('خطأ', errorMessage);
      }
    });
  }

  private performUpdate(): void {
    if (!this.editingItem) return;

    const value = this.itemForm.value;
    const command = new UpdateOrderPackageCommand();
    command.id = this.editingItem.id;
    command.arabicDescription = value.arabicDescription;
    command.englishDescription = value.englishDescription;
    command.minWeightInKiloGram = value.minWeightInKiloGram;
    command.maxWeightInKiloGram = value.maxWeightInKiloGram;

    this.sub.sink = this.orderAdminClient.updateOrderPackage(command).subscribe({
      next: () => {
        this.closeModal();
        this.toasterService.success('تم التحديث بنجاح', 'تم تحديث نوع الشحنة بنجاح');
        this.loadItems();
      },
      error: (error) => {
        const errorMessage = error?.error?.detail || error?.message || 'حدث خطأ أثناء تحديث نوع الشحنة';
        this.toasterService.error('خطأ', errorMessage);
      }
    });
  }

  confirmDelete(itemId: number): void {
    console.log('confirmDelete called with itemId:', itemId);
    this.confirmationTitle = 'تأكيد الحذف';
    this.confirmationMessage = 'هل أنت متأكد من حذف نوع الشحنة هذا؟';
    this.pendingAction = () => this.performDelete(itemId);
    this.showConfirmModal = true;
    console.log('showConfirmModal set to:', this.showConfirmModal);
  }

  private performDelete(itemId: number): void {
    console.log('performDelete called with itemId:', itemId);
    this.sub.sink = this.orderAdminClient.deleteOrderPackage(itemId).subscribe({
      next: (result) => {
        console.log('Delete successful:', result);
        this.toasterService.success('تم الحذف بنجاح', 'تم حذف نوع الشحنة بنجاح');
        this.loadItems();
      },
      error: (error) => {
        console.error('Delete error:', error);
        const errorMessage = error?.error?.detail || error?.message || 'حدث خطأ أثناء حذف نوع الشحنة';
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

  get displayStartCount(): number {
    if (this.totalCount === 0) return 0;
    return (this.currentPage * this.itemsPerPage) + 1;
  }

  get displayEndCount(): number {
    if (this.totalCount === 0) return 0;
    const endCount = (this.currentPage + 1) * this.itemsPerPage;
    return Math.min(endCount, this.totalCount);
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

  onConfirmationConfirmed(): void {
    console.log('onConfirmationConfirmed called');
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
