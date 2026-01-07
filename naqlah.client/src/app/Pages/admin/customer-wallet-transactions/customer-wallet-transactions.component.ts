import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormControl } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { SubSink } from 'subsink';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { ToasterService } from 'src/app/Core/services/toaster.service';
import { WalletTransactionAdminDto, CustomerLookupDto, AddWalletTransactionCommand, WalletTransactionsAdminClient } from 'src/app/Core/services/NaqlahClient';
import { PageHeaderComponent } from 'src/app/shared/components/page-header/page-header.component';

@Component({
  selector: 'app-customer-wallet-transactions',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule, PageHeaderComponent],
  templateUrl: './customer-wallet-transactions.component.html',
  styleUrls: ['./customer-wallet-transactions.component.css'],
  providers: [WalletTransactionsAdminClient]
})
export class CustomerWalletTransactionsComponent implements OnInit, OnDestroy {
  showModal = false;
  showCustomerModal = false;
  isLoading = false;
  isLoadingCustomers = false;
  isLoadingBalance = false;

  transactionForm: FormGroup;
  filterForm: FormGroup;
  customerSearchControl = new FormControl('');

  selectedCustomer: CustomerLookupDto | null = null;
  availableCustomers: CustomerLookupDto[] = [];
  filteredCustomers: CustomerLookupDto[] = [];

  transactions: WalletTransactionAdminDto[] = [];
  walletBalance = 0;
  totalCount = 0;
  totalPages = 0;
  currentPage = 0;
  itemsPerPage = 10;

  private sub = new SubSink();

  constructor(
    private fb: FormBuilder,
    private toasterService: ToasterService,
    private walletTransactionsClient: WalletTransactionsAdminClient
  ) {
    this.transactionForm = this.fb.group({
      customerId: [null, [Validators.required]],
      arabicDescription: ['', [Validators.required, Validators.maxLength(500)]],
      englishDescription: ['', [Validators.required, Validators.maxLength(500)]],
      amount: [0, [Validators.required, Validators.min(0.01)]],
      withdraw: [false],
      orderId: [null]
    });

    this.filterForm = this.fb.group({
      searchTerm: [''],
      fromDate: [null],
      toDate: [null],
      withdraw: [null]
    });
  }

  ngOnInit(): void {
    this.setupCustomerSearch();
    this.setupFilterChanges();
  }

  ngOnDestroy(): void {
    this.sub.unsubscribe();
  }

  setupCustomerSearch(): void {
    this.sub.sink = this.customerSearchControl.valueChanges
      .pipe(
        debounceTime(400),
        distinctUntilChanged()
      )
      .subscribe((searchTerm) => {
        // Only load customers if search term has at least 2 characters
        if (searchTerm && searchTerm.trim().length >= 2) {
          this.loadCustomers(searchTerm.trim());
        } else {
          // Clear customers list if search is too short or empty
          this.availableCustomers = [];
          this.filteredCustomers = [];
        }
      });
  }

  setupFilterChanges(): void {
    // Removed auto-triggering - filters will be applied via search button
  }

  applyFilters(): void {
    if (!this.selectedCustomer) {
      this.toasterService.warning('تحذير', 'يرجى اختيار عميل أولاً');
      return;
    }
    this.currentPage = 0;
    this.loadTransactions();
    this.calculateWalletBalance();
  }

  resetFilters(): void {
    this.filterForm.reset({
      searchTerm: '',
      fromDate: null,
      toDate: null,
      withdraw: null
    });
    if (this.selectedCustomer) {
      this.currentPage = 0;
      this.loadTransactions();
      this.calculateWalletBalance();
    }
  }

  loadCustomers(searchTerm?: string): void {
    if (!searchTerm || searchTerm.trim().length < 2) {
      this.availableCustomers = [];
      this.filteredCustomers = [];
      this.isLoadingCustomers = false;
      return;
    }

    this.isLoadingCustomers = true;
    this.sub.sink = this.walletTransactionsClient.getCustomersLookup(searchTerm || undefined).subscribe({
      next: (response) => {
        this.availableCustomers = response;
        this.filteredCustomers = response;
        this.isLoadingCustomers = false;
      },
      error: (error) => {
        console.error('Error loading customers:', error);
        this.availableCustomers = [];
        this.filteredCustomers = [];
        this.isLoadingCustomers = false;
      }
    });
  }

  get hasSearchTerm(): boolean {
    const value = this.customerSearchControl.value;
    return value && value.trim().length >= 2;
  }

  loadTransactions(): void {
    debugger;
    if (!this.selectedCustomer) {
      this.transactions = [];
      this.totalCount = 0;
      this.totalPages = 0;
      this.walletBalance = 0;
      return;
    }

    this.isLoading = true;
    const skip = this.currentPage * this.itemsPerPage;
    const filters = this.filterForm.value;

    // Simple date handling - date input returns YYYY-MM-DD string
    let fromDate: Date | null = null;
    let toDate: Date | null = null;

    if (filters.fromDate) {
      const dateStr = String(filters.fromDate).split('T')[0]; // Get YYYY-MM-DD part
      fromDate = new Date(dateStr + 'T00:00:00');
    }

    if (filters.toDate) {
      const dateStr = String(filters.toDate).split('T')[0]; // Get YYYY-MM-DD part
      toDate = new Date(dateStr + 'T23:59:59');
    }

    // Simple withdraw filter handling
    let withdrawFilter: boolean | null = null;
    if (filters.withdraw !== null && filters.withdraw !== undefined && filters.withdraw !== 'null') {
      withdrawFilter = filters.withdraw === true || filters.withdraw === 'true';
    }

    this.sub.sink = this.walletTransactionsClient.getAllWalletTransactions(
      skip,
      this.itemsPerPage,
      filters.searchTerm || null,
      this.selectedCustomer.id,
      fromDate,
      toDate,
      withdrawFilter
    ).subscribe({
      next: (response) => {
        this.transactions = response.data || [];
        this.totalCount = response.totalCount || 0;
        this.totalPages = response.totalPages || 0;
        this.isLoading = false;
        this.calculateWalletBalance();
      },
      error: (error) => {
        console.error('Error loading transactions:', error);
        this.isLoading = false;
        this.toasterService.error('خطأ', 'حدث خطأ أثناء تحميل المعاملات');
      }
    });
  }

  calculateWalletBalance(): void {
    if (!this.selectedCustomer) {
      this.walletBalance = 0;
      return;
    }

    this.isLoadingBalance = true;
    const filters = this.filterForm.value;

    // Simple date handling
    let fromDate: Date | null = null;
    let toDate: Date | null = null;

    if (filters.fromDate) {
      const dateStr = String(filters.fromDate).split('T')[0];
      fromDate = new Date(dateStr + 'T00:00:00');
    }

    if (filters.toDate) {
      const dateStr = String(filters.toDate).split('T')[0];
      toDate = new Date(dateStr + 'T23:59:59');
    }

    // Simple withdraw filter
    let withdrawFilter: boolean | null = null;
    if (filters.withdraw !== null && filters.withdraw !== undefined && filters.withdraw !== 'null') {
      withdrawFilter = filters.withdraw === true || filters.withdraw === 'true';
    }

    this.sub.sink = this.walletTransactionsClient.getWalletBalance(
      this.selectedCustomer.id,
      fromDate,
      toDate,
      withdrawFilter
    ).subscribe({
      next: (balance) => {
        this.walletBalance = balance;
        this.isLoadingBalance = false;
      },
      error: (error) => {
        console.error('Error calculating wallet balance:', error);
        this.walletBalance = 0;
        this.isLoadingBalance = false;
      }
    });
  }

  openCustomerModal(): void {
    this.showCustomerModal = true;
    this.customerSearchControl.setValue('');
    this.availableCustomers = [];
    this.filteredCustomers = [];
  }

  closeCustomerModal(): void {
    this.showCustomerModal = false;
    this.customerSearchControl.setValue('');
  }

  selectCustomer(customer: CustomerLookupDto): void {
    this.selectedCustomer = customer;
    this.closeCustomerModal();
    this.loadTransactions();
  }

  removeCustomer(): void {
    this.selectedCustomer = null;
    this.transactions = [];
    this.walletBalance = 0;
    this.totalCount = 0;
    this.totalPages = 0;
    this.filterForm.reset();
  }

  saveCustomer(): void {
    if (this.selectedCustomer) {
      // Customer is already selected and stored, just close modal
      this.closeCustomerModal();
    }
  }

  openAdd(): void {
    if (!this.selectedCustomer) {
      this.toasterService.warning('تحذير', 'يرجى اختيار عميل أولاً');
      return;
    }
    this.transactionForm.patchValue({
      customerId: this.selectedCustomer.id
    });
    this.transactionForm.reset({
      customerId: this.selectedCustomer.id,
      arabicDescription: '',
      englishDescription: '',
      amount: 0,
      withdraw: false,
      orderId: null
    });
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    if (this.selectedCustomer) {
      this.transactionForm.patchValue({
        customerId: this.selectedCustomer.id
      });
    }
  }

  submit(): void {
    if (this.transactionForm.invalid) {
      this.toasterService.error('خطأ', 'يرجى ملء جميع الحقول المطلوبة');
      return;
    }

    const value = this.transactionForm.value;
    const command = new AddWalletTransactionCommand();
    command.customerId = value.customerId;
    command.arabicDescription = value.arabicDescription;
    command.englishDescription = value.englishDescription;
    command.amount = value.amount;
    command.withdraw = value.withdraw;
    command.orderId = value.orderId || undefined;

    this.sub.sink = this.walletTransactionsClient.addWalletTransaction(command).subscribe({
      next: () => {
        this.closeModal();
        this.toasterService.success('تمت الإضافة بنجاح', 'تمت إضافة المعاملة بنجاح');
        this.loadTransactions();
      },
      error: (error) => {
        this.toasterService.error('خطأ', error?.message || 'حدث خطأ أثناء إضافة المعاملة');
      }
    });
  }

  changePage(page: number): void {
    const backendPage = page - 1;
    if (backendPage >= 0 && backendPage < this.totalPages) {
      this.currentPage = backendPage;
      this.loadTransactions();
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

  formatAmount(amount: number, withdraw: boolean): string {
    const sign = withdraw ? '-' : '+';
    return `${sign} ${amount.toFixed(2)}`;
  }

  getAmountClass(withdraw: boolean): string {
    return withdraw
      ? 'text-red-600 font-semibold'
      : 'text-green-600 font-semibold';
  }

  formatBalance(balance: number): string {
    return balance.toFixed(2);
  }

  getBalanceClass(): string {
    return this.walletBalance >= 0
      ? 'text-green-600 font-bold'
      : 'text-red-600 font-bold';
  }

  formatDate(date: Date | string | null | undefined): string {
    if (!date) return '-';
    const d = typeof date === 'string' ? new Date(date) : date;
    if (isNaN(d.getTime())) return '-';

    // Format as normal date: 5/5/2025 (M/D/YYYY)
    const day = d.getDate();
    const month = d.getMonth() + 1;
    const year = d.getFullYear();
    const hours = d.getHours().toString().padStart(2, '0');
    const minutes = d.getMinutes().toString().padStart(2, '0');

    return `${day}/${month}/${year} ${hours}:${minutes}`;
  }
}
