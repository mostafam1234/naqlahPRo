import { NgClass, NgFor, NgIf } from '@angular/common';
import { Component, HostListener, OnInit, OnDestroy } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { LanguageService } from 'src/app/Core/services/language.service';
import { SystemUserAdminClient, SystemUserAdminDto } from 'src/app/Core/services/NaqlahClient';
import { SubSink } from 'subsink';
import { FormControl } from '@angular/forms';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { ToasterService } from 'src/app/Core/services/toaster.service';
import { ConfirmationModalComponent } from 'src/app/shared/components/confirmation-modal/confirmation-modal.component';
import { ReactiveFormsModule } from '@angular/forms';
import { PageHeaderComponent } from 'src/app/shared/components/page-header/page-header.component';

@Component({
  selector: 'app-systme-users',
  standalone: true,
  imports: [NgFor, NgClass, NgIf, TranslateModule, RouterModule, ReactiveFormsModule, ConfirmationModalComponent, PageHeaderComponent],
  providers: [SystemUserAdminClient],
  templateUrl: './systme-users.component.html',
  styleUrl: './systme-users.component.css'
})
export class SystmeUsersComponent implements OnInit, OnDestroy {
  lang: string = 'ar';
  viewMode: 'cards' | 'table' = 'table';
  isLoading = false;
  searchControl = new FormControl('');
  
  constructor(
    private languageService: LanguageService,
    private router: Router,
    private systemUserClient: SystemUserAdminClient,
    private toasterService: ToasterService) {}

  activeTab = 'all';
  currentPage = 0;
  itemsPerPage = 9;
  totalCount = 0;
  totalPages = 0;
  openDropdownId: number = null;
  users: SystemUserAdminDto[] = [];
  
  // Confirmation Modal
  showConfirmModal = false;
  confirmationTitle = '';
  confirmationMessage = '';
  private pendingAction?: () => void;
  private sub = new SubSink();

  get language(){
    return this.languageService.getLanguage();
  }

  ngOnInit(): void {
    this.loadUsers();
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
        this.loadUsers();
      });
  }

  loadUsers(): void {
    this.isLoading = true;
    const skip = this.currentPage * this.itemsPerPage;
    const searchTerm = this.searchControl.value || '';
    const roleFilter = this.activeTab === 'all' ? null : this.activeTab;

    this.sub.sink = this.systemUserClient.getAllSystemUsers(skip, this.itemsPerPage, searchTerm, roleFilter).subscribe({
      next: (response: any) => {
        this.users = response.data || [];
        this.totalCount = response.totalCount || 0;
        this.totalPages = response.totalPages || 0;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading system users:', error);
        this.isLoading = false;
        this.toasterService.error('خطأ', 'حدث خطأ أثناء تحميل المستخدمين');
      }
    });
  }

  get filteredUsers() {
    // Filter by role if not 'all'
    if (this.activeTab === 'all') return this.users;
    // The backend already filters by role, so we just return all users
    return this.users;
  }

  get paginatedUser() {
    return this.filteredUsers;
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

  getRoleDisplayName(user: SystemUserAdminDto): string {
    return this.languageService.getLanguage() === 'ar' 
      ? (user.roleArabicName || user.roleName || 'غير محدد')
      : (user.roleName || 'غير محدد');
  }

  goBack(): void {
    window.history.back();
  }

  changePage(page: number) {
    this.currentPage = page - 1;
    this.loadUsers();
  }

  setActiveTab(tab: string) {
    this.activeTab = tab;
    this.currentPage = 0;
    this.loadUsers();
  }

  toggleDropdown(index: number) {
    this.openDropdownId = this.openDropdownId === index ? null : index;
  }

  deleteUser(userId: number) {
    this.confirmationTitle = 'تأكيد الحذف';
    this.confirmationMessage = 'هل أنت متأكد من حذف هذا المستخدم؟';
    this.pendingAction = () => this.performDelete(userId);
    this.showConfirmModal = true;
    this.openDropdownId = null;
  }

  private performDelete(userId: number): void {
    this.sub.sink = this.systemUserClient.deleteSystemUser(userId).subscribe({
      next: () => {
        this.toasterService.success('تم الحذف بنجاح', 'تم حذف المستخدم بنجاح');
        this.loadUsers();
      },
      error: (error) => {
        this.toasterService.error('خطأ', error?.message || 'حدث خطأ أثناء حذف المستخدم');
      }
    });
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

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    const target = event.target as HTMLElement;
    if (!target.closest('.custom-card')) {
      this.openDropdownId = null;
    }
  }
}
