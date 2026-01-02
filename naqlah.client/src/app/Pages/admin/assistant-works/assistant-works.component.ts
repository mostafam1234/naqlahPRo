import { Component, OnInit, OnDestroy, Inject, Optional } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormControl, FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { PageHeaderComponent } from 'src/app/shared/components/page-header/page-header.component';
import { SubSink } from 'subsink';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { ToasterService } from 'src/app/Core/services/toaster.service';
import { ConfirmationModalComponent } from 'src/app/shared/components/confirmation-modal/confirmation-modal.component';
import { AddAssistantWorkCommand, AssistantWorkAdminDto, AssistantWorkClient, UpdateAssistantWorkCommand } from 'src/app/Core/services/NaqlahClient';


@Component({
  selector: 'app-assistant-works',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, TranslateModule, PageHeaderComponent, ConfirmationModalComponent],
  providers: [AssistantWorkClient],
  templateUrl: './assistant-works.component.html',
  styleUrls: ['./assistant-works.component.css']
})
export class AssistantWorksComponent implements OnInit, OnDestroy {
  // UI State
  showModal = false;
  showUpdateModal = false;
  editingItem: AssistantWorkAdminDto | null = null;
  isLoading = false;

  // Form Controls
  itemForm: FormGroup;
  searchControl = new FormControl('');

  // Data
  items: AssistantWorkAdminDto[] = [];

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
    private assistantWorkClient: AssistantWorkClient,
    private toasterService: ToasterService
  ) {
    this.itemForm = this.fb.group({
      arabicName: ['', [Validators.required, Validators.maxLength(100)]],
      englishName: ['', [Validators.required, Validators.maxLength(100)]],
      cost: [0, [Validators.required, Validators.min(0)]]
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

    this.sub.sink = this.assistantWorkClient.getAllAssistantWorks(skip, this.itemsPerPage, searchTerm).subscribe({
      next: (response: any) => {
        this.items = response.data || [];
        this.totalCount = response.totalCount || 0;
        this.totalPages = response.totalPages || 0;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading assistant works:', error);
        this.isLoading = false;
        this.toasterService.error('خطأ', 'حدث خطأ أثناء تحميل أعمال المساعدين');
      }
    });
  }

  openAdd(): void {
    this.editingItem = null;
    this.itemForm.reset();
    this.itemForm.patchValue({ cost: 0 });
    this.showModal = true;
  }

  openEdit(item: AssistantWorkAdminDto): void {
    this.editingItem = item;
    this.itemForm.patchValue({
      arabicName: item.arabicName,
      englishName: item.englishName,
      cost: item.cost
    });
    this.showUpdateModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.showUpdateModal = false;
    this.editingItem = null;
    this.itemForm.reset();
    this.itemForm.patchValue({ cost: 0 });
  }

  submit(): void {
    if (this.itemForm.invalid) return;
    this.performAdd();
  }

  private performAdd(): void {
    const value = this.itemForm.value;
    const command = new AddAssistantWorkCommand();
    command.arabicName = value.arabicName;
    command.englishName = value.englishName;
    command.cost = value.cost;

    this.sub.sink = this.assistantWorkClient.addAssistantWork(command).subscribe({
      next: () => {
        this.closeModal();
        this.toasterService.success('تمت الإضافة بنجاح', 'تمت إضافة عمل المساعد بنجاح');
        this.loadItems();
      },
      error: (error) => {
        this.toasterService.error('خطأ', error?.message || 'حدث خطأ أثناء إضافة عمل المساعد');
      }
    });
  }

  update(): void {
    if (this.itemForm.invalid) return;

    const itemId = this.editingItem?.id;
    const value = this.itemForm.value;
    const command = new UpdateAssistantWorkCommand();
    command.id = itemId!;
    command.arabicName = value.arabicName;
    command.englishName = value.englishName;
    command.cost = value.cost;

    this.sub.sink = this.assistantWorkClient.updateAssistantWork(command).subscribe({
      next: () => {
        this.closeModal();
        this.toasterService.success('تم التحديث بنجاح', 'تم تحديث عمل المساعد بنجاح');
        this.loadItems();
      },
      error: (error) => {
        this.toasterService.error('خطأ', error?.message || 'حدث خطأ أثناء تحديث عمل المساعد');
      }
    });
  }

  confirmDelete(itemId: number): void {
    this.confirmationTitle = 'تأكيد الحذف';
    this.confirmationMessage = 'هل أنت متأكد من حذف عمل المساعد هذا؟';
    this.pendingAction = () => this.performDelete(itemId);
    this.showConfirmModal = true;
  }

  private performDelete(itemId: number): void {
    this.sub.sink = this.assistantWorkClient.deleteAssistantWork(itemId).subscribe({
      next: () => {
        this.toasterService.success('تم الحذف بنجاح', 'تم حذف عمل المساعد بنجاح');
        this.loadItems();
      },
      error: (error) => {
        this.toasterService.error('خطأ', error?.message || 'حدث خطأ أثناء حذف عمل المساعد');
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

