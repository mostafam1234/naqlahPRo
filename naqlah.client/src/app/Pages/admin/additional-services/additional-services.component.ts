import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormControl, FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { SubSink } from 'subsink';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { ToasterService } from 'src/app/Core/services/toaster.service';
import { PageHeaderComponent } from 'src/app/shared/components/page-header/page-header.component';
import { ConfirmationModalComponent } from 'src/app/shared/components/confirmation-modal/confirmation-modal.component';
import { PermissionService } from 'src/app/shared/services/permission.service';
import {
  AdditionalServiceClient,
  AdditionalServiceAdminDto,
  AddAdditionalServiceCommand,
  UpdateAdditionalServiceCommand
} from 'src/app/Core/services/NaqlahClient';

@Component({
  selector: 'app-additional-services',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, TranslateModule, PageHeaderComponent, ConfirmationModalComponent],
  providers: [AdditionalServiceClient],
  templateUrl: './additional-services.component.html',
  styleUrls: ['./additional-services.component.css']
})
export class AdditionalServicesComponent implements OnInit, OnDestroy {
  showModal = false;
  showUpdateModal = false;
  editingItem: AdditionalServiceAdminDto | null = null;
  isLoading = false;

  itemForm: FormGroup;
  searchControl = new FormControl('');

  items: AdditionalServiceAdminDto[] = [];
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
    private client: AdditionalServiceClient,
    private toaster: ToasterService,
    private permissionService: PermissionService
  ) {
    this.itemForm = this.fb.group({
      arabicName: ['', [Validators.required, Validators.maxLength(200)]],
      englishName: ['', [Validators.required, Validators.maxLength(200)]],
      unitPrice: [0, [Validators.required, Validators.min(0)]]
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
    const search = this.searchControl.value || undefined;

    this.sub.sink = this.client.getAll(skip, this.itemsPerPage, search).subscribe({
      next: (res: any) => {
        this.items = res.data || [];
        this.totalCount = res.totalCount || 0;
        this.totalPages = res.totalPages || 0;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
        this.toaster.error('خطأ', 'حدث خطأ أثناء تحميل الخدمات الإضافية');
      }
    });
  }

  openAdd(): void {
    this.editingItem = null;
    this.itemForm.reset();
    this.itemForm.patchValue({ unitPrice: 0 });
    this.showModal = true;
  }

  openEdit(item: AdditionalServiceAdminDto): void {
    this.editingItem = item;
    this.itemForm.patchValue({
      arabicName: item.arabicName,
      englishName: item.englishName,
      unitPrice: item.unitPrice
    });
    this.showUpdateModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.showUpdateModal = false;
    this.editingItem = null;
    this.itemForm.reset();
    this.itemForm.patchValue({ unitPrice: 0 });
  }

  submit(): void {
    if (this.itemForm.invalid) return;
    const v = this.itemForm.value;
    const command = new AddAdditionalServiceCommand();
    command.arabicName = v.arabicName;
    command.englishName = v.englishName;
    command.unitPrice = v.unitPrice;

    this.sub.sink = this.client.add(command).subscribe({
      next: () => {
        this.closeModal();
        this.toaster.success('تمت الإضافة بنجاح', 'تمت إضافة الخدمة الإضافية بنجاح');
        this.loadItems();
      },
      error: (err: any) => {
        this.toaster.error('خطأ', err?.message || 'حدث خطأ أثناء إضافة الخدمة');
      }
    });
  }

  update(): void {
    if (this.itemForm.invalid) return;
    const v = this.itemForm.value;
    const command = new UpdateAdditionalServiceCommand();
    command.id = this.editingItem!.id;
    command.arabicName = v.arabicName;
    command.englishName = v.englishName;
    command.unitPrice = v.unitPrice;

    this.sub.sink = this.client.update(command).subscribe({
      next: () => {
        this.closeModal();
        this.toaster.success('تم التحديث بنجاح', 'تم تحديث الخدمة الإضافية بنجاح');
        this.loadItems();
      },
      error: (err: any) => {
        this.toaster.error('خطأ', err?.message || 'حدث خطأ أثناء تحديث الخدمة');
      }
    });
  }

  confirmDelete(id: number): void {
    this.confirmationTitle = 'تأكيد الحذف';
    this.confirmationMessage = 'هل أنت متأكد من حذف هذه الخدمة الإضافية؟';
    this.pendingAction = () => this.performDelete(id);
    this.showConfirmModal = true;
  }

  private performDelete(id: number): void {
    this.sub.sink = this.client.delete(id).subscribe({
      next: () => {
        this.toaster.success('تم الحذف بنجاح', 'تم حذف الخدمة الإضافية بنجاح');
        this.loadItems();
      },
      error: (err: any) => {
        this.toaster.error('خطأ', err?.message || 'حدث خطأ أثناء حذف الخدمة');
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

  get displayCurrentPage(): number { return this.currentPage + 1; }

  get displayStartCount(): number {
    if (this.totalCount === 0) return 0;
    return this.currentPage * this.itemsPerPage + 1;
  }

  get displayEndCount(): number {
    if (this.totalCount === 0) return 0;
    return Math.min((this.currentPage + 1) * this.itemsPerPage, this.totalCount);
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

  goBack(): void { window.history.back(); }

  onConfirmationConfirmed(): void {
    this.showConfirmModal = false;
    if (this.pendingAction) { this.pendingAction(); this.pendingAction = undefined; }
  }

  onConfirmationCancelled(): void {
    this.showConfirmModal = false;
    this.pendingAction = undefined;
  }
}
