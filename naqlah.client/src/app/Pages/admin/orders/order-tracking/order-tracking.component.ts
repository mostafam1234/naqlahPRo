import { NgClass, NgFor, NgIf, DecimalPipe } from '@angular/common';
import { Component, OnInit, OnDestroy, AfterViewInit } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule, ReactiveFormsModule, FormControl } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import * as L from 'leaflet';
import {
  DeliveryManAdminClient,
  GetAllOrdersDto,
  OrderStatus,
  PagedResultOfGetAllOrdersDto
} from 'src/app/Core/services/NaqlahClient';
import { PageHeaderComponent } from 'src/app/shared/components/page-header/page-header.component';
import { catchError, finalize, debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { of } from 'rxjs';
import { SubSink } from 'subsink';

@Component({
  selector: 'app-order-tracking',
  standalone: true,
  imports: [NgIf, NgFor, NgClass, RouterLink, FormsModule, ReactiveFormsModule, PageHeaderComponent, TranslateModule, DecimalPipe],
  templateUrl: './order-tracking.component.html',
  styleUrl: './order-tracking.component.css'
})
export class OrderTrackingComponent implements OnInit, AfterViewInit, OnDestroy {
  deliveryManId: number | null = null;
  deliveryManName: string = '';
  deliveryManPhone: string = '';
  
  // Orders Data
  orders: GetAllOrdersDto[] = [];
  totalCount = 0;
  totalPages = 0;
  isLoading = false;
  searchControl = new FormControl('');
  statusFilter?: OrderStatus;
  
  currentPage = 1;
  itemsPerPage = 9;
  activeProgressTab: string = 'all';
  
  map: L.Map | null = null;
  private sub = new SubSink();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private deliveryManClient: DeliveryManAdminClient
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if (id) {
        this.deliveryManId = +id;
        this.loadDeliveryManOrders();
        this.setupSearch();
      }
    });
  }

  ngAfterViewInit(): void {
    // Map will be initialized when we have waypoints data
  }

  ngOnDestroy(): void {
    this.sub.unsubscribe();
    if (this.map) {
      this.map.remove();
    }
  }

  setupSearch(): void {
    this.sub.sink = this.searchControl.valueChanges
      .pipe(
        debounceTime(500),
        distinctUntilChanged()
      )
      .subscribe(() => {
        this.currentPage = 1;
        this.loadDeliveryManOrders();
      });
  }

  loadDeliveryManOrders(): void {
    if (!this.deliveryManId) return;

    this.isLoading = true;
    const skip = (this.currentPage - 1) * this.itemsPerPage;

    this.deliveryManClient.getOrdersByDeliveryManId(
      this.deliveryManId,
      skip,
      this.itemsPerPage,
      this.searchControl.value || undefined,
      this.statusFilter
    ).pipe(
      catchError(error => {
        console.error('Error loading delivery man orders:', error);
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
        
        // Get delivery man name from first order if available
        if (this.orders.length > 0) {
          // We don't have delivery man name in orders, so we'll use a placeholder
          this.deliveryManName = 'مندوب التوصيل';
        }
      } else {
        this.orders = [];
        this.totalCount = 0;
        this.totalPages = 0;
      }
    });
  }

  clearSearch(): void {
    this.searchControl.setValue('', { emitEvent: false });
    this.currentPage = 1;
    this.loadDeliveryManOrders();
  }

  setActiveProgressTab(tab: string): void {
    this.activeProgressTab = tab;
    this.currentPage = 1;

    switch (tab) {
      case 'all':
        this.statusFilter = undefined;
        break;
      case 'pending':
        this.statusFilter = OrderStatus.Pending;
        break;
      case 'assigned':
        this.statusFilter = OrderStatus.Assigned;
        break;
      case 'completed':
        this.statusFilter = OrderStatus.Completed;
        break;
      case 'cancelled':
        this.statusFilter = OrderStatus.Cancelled;
        break;
      default:
        this.statusFilter = undefined;
    }

    this.loadDeliveryManOrders();
  }

  changePage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.loadDeliveryManOrders();
    }
  }

  viewOrderDetails(orderId: number): void {
    this.router.navigate(['/admin/requests/details', orderId]);
  }

  getStatusText(status: OrderStatus): string {
    switch (status) {
      case OrderStatus.Pending:
        return 'معلق';
      case OrderStatus.Assigned:
        return 'مخصص';
      case OrderStatus.Completed:
        return 'مكتمل';
      case OrderStatus.Cancelled:
        return 'ملغي';
      default:
        return 'غير محدد';
    }
  }

  getStatusClass(status: OrderStatus): string {
    switch (status) {
      case OrderStatus.Pending:
        return 'bg-blue-100 text-blue-800';
      case OrderStatus.Assigned:
        return 'bg-purple-100 text-purple-800';
      case OrderStatus.Completed:
        return 'bg-green-100 text-green-800';
      case OrderStatus.Cancelled:
        return 'bg-red-100 text-red-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  }

  getOriginWayPoint(wayPoints: any[]): any {
    return wayPoints?.find(wp => wp.isOrigin);
  }

  getDestinationWayPoint(wayPoints: any[]): any {
    return wayPoints?.find(wp => wp.isDestination);
  }

  get displayStartCount(): number {
    if (this.totalCount === 0) return 0;
    return (this.currentPage - 1) * this.itemsPerPage + 1;
  }

  get displayEndCount(): number {
    const end = this.currentPage * this.itemsPerPage;
    return end > this.totalCount ? this.totalCount : end;
  }

  get visiblePages(): (number | string)[] {
    const pages: (number | string)[] = [];
    const maxVisible = 5;
    const total = this.totalPages;
    const current = this.currentPage;
    
    if (total <= maxVisible) {
      for (let i = 1; i <= total; i++) {
        pages.push(i);
      }
    } else {
      if (current <= 3) {
        for (let i = 1; i <= 4; i++) {
          pages.push(i);
        }
        pages.push('ellipsis');
        pages.push(total);
      } else if (current >= total - 2) {
        pages.push(1);
        pages.push('ellipsis');
        for (let i = total - 3; i <= total; i++) {
          pages.push(i);
        }
      } else {
        pages.push(1);
        pages.push('ellipsis');
        for (let i = current - 1; i <= current + 1; i++) {
          pages.push(i);
        }
        pages.push('ellipsis');
        pages.push(total);
      }
    }
    
    return pages;
  }

  isPageNumber(page: number | string): boolean {
    return typeof page === 'number';
  }

  isPageEllipsis(page: number | string): boolean {
    return page === 'ellipsis';
  }
}
