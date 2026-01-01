import { NgClass, NgFor, NgIf } from '@angular/common';
import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule, ReactiveFormsModule, FormControl } from '@angular/forms';
import { TranslateService, TranslateModule } from '@ngx-translate/core';
import { LanguageService } from 'src/app/Core/services/language.service';
import {
  OrderAdminClient,
  GetAllOrdersDto,
  OrderStatus,
  OrderType,
  CustomerType,
  PagedResultOfGetAllOrdersDto
} from 'src/app/Core/services/NaqlahClient';
import { PageHeaderComponent } from 'src/app/shared/components/page-header/page-header.component';
import { catchError, finalize, debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { of } from 'rxjs';
import { SubSink } from 'subsink';

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [NgClass, NgFor, NgIf, FormsModule, ReactiveFormsModule, PageHeaderComponent, TranslateModule],
  templateUrl: './orders.component.html',
  styleUrl: './orders.component.css'
})
export class OrdersComponent implements OnInit, OnDestroy {

  lang: string = 'ar';
  activeProgressTab: string = 'all';
  activeTab = 'all';
  currentPage = 1;
  itemsPerPage = 9; // Changed from 4 to 9 for better grid layout

  // Real data properties
  orders: GetAllOrdersDto[] = [];
  totalCount = 0;
  totalPages = 0;
  isLoading = false;
  searchControl = new FormControl('');
  
  private sub = new SubSink();

  // Status filter properties
  statusFilter?: OrderStatus;
  customerTypeFilter?: CustomerType;

  constructor(
    private languageService: LanguageService,
    private translateService: TranslateService,
    private router: Router,
    private orderClient: OrderAdminClient
  ) {}

  ngOnInit(): void {
    this.loadOrders();
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
        this.currentPage = 1;
        this.loadOrders();
      });
  }

  loadOrders(): void {
    this.isLoading = true;

    const skip = (this.currentPage - 1) * this.itemsPerPage;

    this.orderClient.getAllOrders(
      skip,
      this.itemsPerPage,
      this.searchControl.value || undefined,
      this.statusFilter,
      this.customerTypeFilter
    ).pipe(
      catchError(error => {
        console.error('Error loading orders:', error);
        // Return empty PagedResult on error
        const emptyResult = new PagedResultOfGetAllOrdersDto();
        emptyResult.data = [];
        emptyResult.totalCount = 0;
        emptyResult.totalPages = 0;
        return of(emptyResult);
      }),
      finalize(() => {
        this.isLoading = false;
      })
    ).subscribe(response => {
      if (response && response.data) {
        this.orders = response.data;
        this.totalCount = response.totalCount;
        this.totalPages = response.totalPages;
      } else {
        this.orders = [];
        this.totalCount = 0;
        this.totalPages = 0;
        console.error('Failed to load orders: Invalid response structure');
      }
    });
  }

  setActiveTab(tab: string): void {
    this.activeTab = tab;

    // Map tab to customer type filter
    switch (tab) {
      case 'individual':
        this.customerTypeFilter = CustomerType.Individual;
        break;
      case 'institution':
        this.customerTypeFilter = CustomerType.Establishment;
        break;
      default:
        this.customerTypeFilter = undefined;
        break;
    }

    this.currentPage = 1;
    this.loadOrders();
  }

  setActiveProgressTab(tab: string): void {
    this.activeProgressTab = tab;

    // Map tab to status filter
    switch (tab) {
      case 'assigned':
        this.statusFilter = OrderStatus.Assigned;
        break;
      case 'pending':
        this.statusFilter = OrderStatus.Pending;
        break;
      case 'expired':
        this.statusFilter = OrderStatus.Cancelled;
        break;
      case 'completed':
        this.statusFilter = OrderStatus.Completed;
        break;
      default:
        this.statusFilter = undefined;
        break;
    }

    this.currentPage = 1;
    this.loadOrders();
  }

  clearSearch(): void {
    this.searchControl.setValue('', { emitEvent: false });
    this.currentPage = 1;
    this.loadOrders();
  }

  changePage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.loadOrders();
    }
  }

  viewOrderDetails(orderId: number): void {
    this.router.navigate(['/admin/requests/details', orderId]);
  }

  getStatusClass(status: OrderStatus): string {
    switch (status) {
      case OrderStatus.Pending:
        return 'bg-yellow-100 text-yellow-800';
      case OrderStatus.Assigned:
        return 'bg-blue-100 text-blue-800';
      case OrderStatus.Completed:
        return 'bg-green-100 text-green-800';
      case OrderStatus.Cancelled:
        return 'bg-red-100 text-red-800';
      default:
        return 'bg-neutral-100 text-neutral-800';
    }
  }

  get paginatedOrders(): GetAllOrdersDto[] {
    return this.orders; // Orders are already paginated from the backend
  }

  // Generate visible pages with smart pagination logic (like captain component)
  get visiblePages(): number[] {
    const current = this.currentPage;
    const total = this.totalPages;
    const pages: number[] = [];

    if (total <= 7) {
      // إذا كان المجموع أقل من أو يساوي 7، اعرض كل الصفحات
      for (let i = 1; i <= total; i++) {
        pages.push(i);
      }
    } else {
      // دائماً اعرض الصفحة الأولى
      pages.push(1);

      if (current <= 4) {
        // إذا كانت الصفحة الحالية في البداية
        for (let i = 2; i <= 5; i++) {
          pages.push(i);
        }
        pages.push(-1); // نقاط للفصل
        pages.push(total);
      } else if (current >= total - 3) {
        // إذا كانت الصفحة الحالية في النهاية
        pages.push(-1);
        for (let i = total - 4; i <= total; i++) {
          pages.push(i);
        }
      } else {
        // إذا كانت الصفحة الحالية في المنتصف
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

  // Helper method for counting display
  get displayStartCount(): number {
    if (this.totalCount === 0) return 0;
    return ((this.currentPage - 1) * this.itemsPerPage) + 1;
  }

  get displayEndCount(): number {
    if (this.totalCount === 0) return 0;
    const endCount = this.currentPage * this.itemsPerPage;
    return Math.min(endCount, this.totalCount);
  }

  // Helper methods for template
  getOriginWayPoint(wayPoints: any[]): any {
    if (!wayPoints || wayPoints.length === 0) return null;
    return wayPoints.find(wp => wp.isOrigin) || wayPoints[0];
  }

  getDestinationWayPoint(wayPoints: any[]): any {
    if (!wayPoints || wayPoints.length === 0) return null;
    return wayPoints.find(wp => wp.isDestination) || wayPoints[wayPoints.length - 1];
  }

  getIntermediateWayPoints(wayPoints: any[]): any[] {
    if (!wayPoints || wayPoints.length <= 2) return [];
    return wayPoints.filter(wp => !wp.isOrigin && !wp.isDestination);
  }

  getIntermediateWayPointsCount(wayPoints: any[]): number {
    if (!wayPoints || wayPoints.length <= 2) return 0;
    return wayPoints.filter(wp => !wp.isOrigin && !wp.isDestination).length;
  }

  getStatusText(status: OrderStatus): string {
    switch (status) {
      case OrderStatus.Pending:
        return 'معلق';
      case OrderStatus.Assigned:
        return 'مُعين';
      case OrderStatus.Completed:
        return 'مكتمل';
      case OrderStatus.Cancelled:
        return 'ملغي';
      default:
        return 'غير معروف';
    }
  }

  // Helper method to check if page is a number (for template)
  isPageNumber(page: number | string): page is number {
    return typeof page === 'number';
  }

  // Helper method to check if page is ellipsis (for template)
  isPageEllipsis(page: number | string): page is string {
    return typeof page === 'string';
  }
}
