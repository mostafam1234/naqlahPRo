import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormControl, FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { SubSink } from 'subsink';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { ToasterService } from 'src/app/Core/services/toaster.service';
import { ConfirmationModalComponent } from 'src/app/shared/components/confirmation-modal/confirmation-modal.component';
import {
  DiscountCodeAdminDto,
  AddDiscountCodeCommand,
  UpdateDiscountCodeCommand,
  DiscountCodeAdminClient
} from 'src/app/Core/services/NaqlahClient';
import { PermissionService } from 'src/app/shared/services/permission.service';

@Component({
  selector: 'app-discount-codes',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, TranslateModule, ConfirmationModalComponent],
  templateUrl: './discount-codes.component.html',
  styleUrls: ['./discount-codes.component.css']
})
export class DiscountCodesComponent implements OnInit, OnDestroy {
  showModal = false;
  showUpdateModal = false;
  editingItem: DiscountCodeAdminDto | null = null;
  isLoading = false;

  itemForm: FormGroup;
  searchControl = new FormControl('');
  filterActive: boolean | null = null;

  items: DiscountCodeAdminDto[] = [];

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
    private discountCodeClient: DiscountCodeAdminClient,
    private permissionService: PermissionService
  ) {
    this.itemForm = this.fb.group({
      code: ['', [Validators.required, Validators.maxLength(50)]],
      discountAmount: [null, [Validators.required, Validators.min(0.01)]],
      usageLimit: [null, [Validators.min(1)]],
      expiryDate: [null],
      minimumOrderAmount: [null, [Validators.min(0)]],
      isActive: [true]
    });
  }

  hasPermission(permission: string): boolean {
    return this.permissionService.hasPermission(permission);
  }

  ngOnInit(): void {
    this.permissionService.getPermissions().subscribe(() => {});
    this.loadItems();
    this.setupSearch();
  }

  ngOnDestroy(): void {
    this.sub.unsubscribe();
  }

  setupSearch(): void {
    this.sub.sink = this.searchControl.valueChanges
      .pipe(debounceTime(500), distinctUntilChanged())
      .subscribe(() => {
        this.currentPage = 0;
        this.loadItems();
      });
  }

  loadItems(): void {
    this.isLoading = true;
    const skip = this.currentPage * this.itemsPerPage;
    const searchTerm = this.searchControl.value || '';
    this.sub.sink = this.discountCodeClient
      .getAllDiscountCodes(skip, this.itemsPerPage, searchTerm, this.filterActive ?? undefined)
      .subscribe({
        next: (response: any) => {
          this.items = response.data;
          this.totalCount = response.totalCount;
          this.totalPages = response.totalPages;
          this.isLoading = false;
        },
        error: () => {
          this.isLoading = false;
        }
      });
  }

  setFilter(value: boolean | null): void {
    this.filterActive = value;
    this.currentPage = 0;
    this.loadItems();
  }

  openAdd(): void {
    this.editingItem = null;
    this.itemForm.reset({ isActive: true });
    this.showModal = true;
  }

  openEdit(item: DiscountCodeAdminDto): void {
    this.editingItem = item;
    this.itemForm.patchValue({
      code: item.code,
      discountAmount: item.discountAmount,
      usageLimit: item.usageLimit,
      expiryDate: item.expiryDate ? this.toDateInputValue(item.expiryDate) : null,
      minimumOrderAmount: item.minimumOrderAmount,
      isActive: item.isActive
    });
    this.showUpdateModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.showUpdateModal = false;
    this.editingItem = null;
    this.itemForm.reset({ isActive: true });
  }

  submit(): void {
    if (this.itemForm.invalid) return;
    const value = this.itemForm.value;
    const command = new AddDiscountCodeCommand();
    command.code = value.code;
    command.discountAmount = value.discountAmount;
    command.usageLimit = value.usageLimit || null;
    command.expiryDate = value.expiryDate ? new Date(value.expiryDate) : null;
    command.minimumOrderAmount = value.minimumOrderAmount ?? null;
    this.sub.sink = this.discountCodeClient.addDiscountCode(command).subscribe({
      next: () => {
        this.closeModal();
        this.toasterService.success('تمت الإضافة بنجاح', 'تمت إضافة كود الخصم بنجاح');
        this.loadItems();
      },
      error: (error: any) => {
        this.toasterService.error('خطأ', error?.message || 'حدث خطأ أثناء إضافة كود الخصم');
      }
    });
  }

  update(): void {
    if (this.itemForm.invalid) return;
    const value = this.itemForm.value;
    const command = new UpdateDiscountCodeCommand();
    command.id = this.editingItem!.id;
    command.code = value.code;
    command.discountAmount = value.discountAmount;
    command.usageLimit = value.usageLimit ?? null;
    command.expiryDate = value.expiryDate ? new Date(value.expiryDate) : null;
    command.minimumOrderAmount = value.minimumOrderAmount ?? null;
    command.isActive = value.isActive;
    this.sub.sink = this.discountCodeClient.updateDiscountCode(command).subscribe({
      next: () => {
        this.closeModal();
        this.toasterService.success('تم التحديث بنجاح', 'تم تحديث كود الخصم بنجاح');
        this.loadItems();
      },
      error: (error: any) => {
        this.toasterService.error('خطأ', error?.message || 'حدث خطأ أثناء تحديث كود الخصم');
      }
    });
  }

  confirmDelete(itemId: number): void {
    this.confirmationTitle = 'تأكيد الحذف';
    this.confirmationMessage = 'هل أنت متأكد من حذف هذا الكود؟';
    this.pendingAction = () => this.performDelete(itemId);
    this.showConfirmModal = true;
  }

  private performDelete(itemId: number): void {
    this.sub.sink = this.discountCodeClient.deleteDiscountCode(itemId).subscribe({
      next: () => {
        this.toasterService.success('تم الحذف بنجاح', 'تم حذف كود الخصم بنجاح');
        this.loadItems();
      },
      error: (error: any) => {
        this.toasterService.error('خطأ', error?.message || 'حدث خطأ أثناء حذف كود الخصم');
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
      for (let i = 1; i <= total; i++) pages.push(i);
    } else {
      pages.push(1);
      if (current <= 4) {
        for (let i = 2; i <= 5; i++) pages.push(i);
        pages.push(-1);
        pages.push(total);
      } else if (current >= total - 3) {
        pages.push(-1);
        for (let i = total - 4; i <= total; i++) pages.push(i);
      } else {
        pages.push(-1);
        for (let i = current - 1; i <= current + 1; i++) pages.push(i);
        pages.push(-1);
        pages.push(total);
      }
    }
    return pages;
  }

  get displayStartCount(): number {
    if (this.totalCount === 0) return 0;
    return this.currentPage * this.itemsPerPage + 1;
  }

  get displayEndCount(): number {
    if (this.totalCount === 0) return 0;
    return Math.min((this.currentPage + 1) * this.itemsPerPage, this.totalCount);
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

  isExpired(date: Date): boolean {
    return new Date(date) < new Date();
  }

  private toDateInputValue(date: Date): string {
    const d = new Date(date);
    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }
}
