import { NgClass, NgFor, NgIf } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { AdminCustomerListItemDto, CustomerAdminClient } from 'src/app/Core/services/NaqlahClient';
import { ToasterService } from 'src/app/Core/services/toaster.service';
import { PageHeaderComponent } from 'src/app/shared/components/page-header/page-header.component';
import { SubSink } from 'subsink';

@Component({
  selector: 'app-admin-customers',
  standalone: true,
  imports: [NgFor, NgClass, NgIf, TranslateModule, ReactiveFormsModule, RouterLink, PageHeaderComponent],
  providers: [CustomerAdminClient],
  templateUrl: './admin-customers.component.html',
  styleUrl: './admin-customers.component.css'
})
export class AdminCustomersComponent implements OnInit, OnDestroy {
  isLoading = false;
  searchControl = new FormControl('');
  currentPage = 0;
  itemsPerPage = 10;
  totalCount = 0;
  totalPages = 0;
  rows: AdminCustomerListItemDto[] = [];

  private sub = new SubSink();

  constructor(
    private customerAdminClient: CustomerAdminClient,
    private toaster: ToasterService,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.loadCustomers();
    this.sub.sink = this.searchControl.valueChanges
      .pipe(debounceTime(400), distinctUntilChanged())
      .subscribe(() => {
        this.currentPage = 0;
        this.loadCustomers();
      });
  }

  ngOnDestroy(): void {
    this.sub.unsubscribe();
  }

  loadCustomers(): void {
    this.isLoading = true;
    const skip = this.currentPage * this.itemsPerPage;
    const searchTerm = this.searchControl.value?.trim() || '';

    this.sub.sink = this.customerAdminClient
      .getAllCustomers(skip, this.itemsPerPage, searchTerm || null)
      .subscribe({
        next: (response) => {
          this.rows = response.data ?? [];
          this.totalCount = response.totalCount ?? 0;
          this.totalPages = response.totalPages ?? 0;
          this.isLoading = false;
        },
        error: (err) => {
          console.error(err);
          this.isLoading = false;
          const apiMsg = this.resolveApiError(err);
          this.toaster.error(
            this.translate.instant('COMMON.ERROR'),
            apiMsg || this.translate.instant('ADMIN.CUSTOMERS.ERR_GENERIC')
          );
        }
      });
  }

  resolveApiError(err: unknown): string {
    if (err && typeof err === 'object' && 'errorMessage' in err) {
      const msg = (err as { errorMessage?: string }).errorMessage;
      if (msg) return msg;
    }
    if (err instanceof Error && err.message) return err.message;
    return '';
  }

  get paginatedRows(): AdminCustomerListItemDto[] {
    return this.rows;
  }

  get displayCurrentPage(): number {
    return this.currentPage + 1;
  }

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

  changePage(page: number): void {
    this.currentPage = page - 1;
    this.loadCustomers();
  }
}
