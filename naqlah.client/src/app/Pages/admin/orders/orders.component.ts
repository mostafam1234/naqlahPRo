import { DatePipe, DecimalPipe, NgClass, NgFor, NgIf } from '@angular/common';

import { Component, OnDestroy, OnInit } from '@angular/core';

import { Router } from '@angular/router';

import { FormsModule } from '@angular/forms';

import { TranslateModule } from '@ngx-translate/core';

import { ToasterService } from 'src/app/Core/services/toaster.service';

import {

  DeliveryManAdminClient,

  GetAllOrdersDto,

  OrderAdminClient,

  OrderStatisticsDto,

  OrderStatus,

  OrderStatusCountDto,

  PagedResultOfGetAllOrdersDto

} from 'src/app/Core/services/NaqlahClient';

import { PageHeaderComponent } from 'src/app/shared/components/page-header/page-header.component';

import { FormalSelectComponent } from 'src/app/shared/components/formal-select/formal-select.component';

import { FormalMultiSelectComponent } from 'src/app/shared/components/formal-multi-select/formal-multi-select.component';

import { PermissionService } from 'src/app/shared/services/permission.service';

import { of, Subject } from 'rxjs';

import { catchError, debounceTime, finalize, map, switchMap } from 'rxjs/operators';

import { SubSink } from 'subsink';

import {

  buildVisiblePages,

  CaptainOrdersFilterForm,

  cloneCaptainOrdersFilter,

  CUSTOMER_TYPE_OPTIONS,

  EMPTY_CAPTAIN_ORDERS_FILTER,

  getOrderStatCardClass,

  getOrderStatCardLabel,

  getOrderStatusBadgeClass,

  isActiveOrdersStatKey,

  mapDeliveryManToSelectOption,

  mapFileResponse,

  orderStatusToStatKey,

  OrderTrackingStatKey,

  parseDateFilter,

  resolveCustomerTypeFilter,

  resolveDeliveryManFilters,

  resolveStatusFilter,

  SelectOption,

  triggerFileDownload

} from './captain-orders.helpers';



@Component({

  selector: 'app-orders',

  standalone: true,

  imports: [

    NgClass, NgFor, NgIf, FormsModule, PageHeaderComponent, TranslateModule,

    DecimalPipe, DatePipe, FormalSelectComponent, FormalMultiSelectComponent

  ],

  templateUrl: './orders.component.html',

  styleUrl: './orders.component.css'

})

export class OrdersComponent implements OnInit, OnDestroy {

  orderStatistics: OrderStatisticsDto | null = null;

  isLoadingStatistics = false;



  orders: GetAllOrdersDto[] = [];

  filterDraft: CaptainOrdersFilterForm = { ...EMPTY_CAPTAIN_ORDERS_FILTER };

  filterApplied: CaptainOrdersFilterForm = { ...EMPTY_CAPTAIN_ORDERS_FILTER };

  selectedStatKey: OrderTrackingStatKey = 'all';



  totalCount = 0;

  totalPages = 0;

  isLoading = false;

  isExporting = false;

  isExportingSummary = false;

  hasSearched = false;

  currentPage = 1;

  readonly itemsPerPage = 10;



  deliveryManLookupOptions: SelectOption[] = [];

  selectedDeliveryMenCache: SelectOption[] = [];

  isLoadingDeliveryMenLookup = false;

  readonly deliveryManSearchPlaceholder = 'ابحث بالاسم أو الهاتف...';

  private deliveryManLookupSearchTerm = '';



  readonly customerTypeOptions = CUSTOMER_TYPE_OPTIONS;

  private readonly deliveryManSearch$ = new Subject<string>();

  private sub = new SubSink();



  constructor(

    private router: Router,

    private orderAdminClient: OrderAdminClient,

    private deliveryManClient: DeliveryManAdminClient,

    private permissionService: PermissionService,

    private toasterService: ToasterService

  ) {}



  ngOnInit(): void {

    this.permissionService.getPermissions().subscribe(() => {});

    this.loadStatistics();



    this.sub.sink = this.deliveryManSearch$

      .pipe(

        debounceTime(300),

        switchMap((term) => this.fetchDeliveryManLookup(term))

      )

      .subscribe((items) => {

        this.deliveryManLookupOptions = items.map((dm) => mapDeliveryManToSelectOption(dm));

      });

  }



  ngOnDestroy(): void {

    this.sub.unsubscribe();

  }



  hasPermission(permission: string): boolean {

    return this.permissionService.hasPermission(permission);

  }



  onStatCardClick(key: OrderTrackingStatKey): void {

    this.filterDraft.statusKey = key;

    this.selectedStatKey = key;

    this.search();

  }



  isStatCardSelected(key: OrderTrackingStatKey): boolean {

    return this.selectedStatKey === key;

  }



  getStatCardClass(key: OrderTrackingStatKey): string {

    return getOrderStatCardClass(key);

  }



  getStatCount(key: OrderTrackingStatKey): number {

    if (!this.orderStatistics) return 0;

    switch (key) {

      case 'all': return this.orderStatistics.totalOrders ?? 0;

      case 'active': return this.orderStatistics.activeOrders ?? 0;

      case 'confirmed': return this.orderStatistics.confirmedGoingToPickupOrders ?? 0;

      case 'pickedup': return this.orderStatistics.pickedUpOrders ?? 0;

      case 'completed': return this.orderStatistics.completedOrders ?? 0;

      case 'cancelled': return this.orderStatistics.cancelledOrders ?? 0;

      default: return 0;

    }

  }



  get statusStatCards(): OrderStatusCountDto[] {

    const hiddenStatuses = new Set([OrderStatus.Pending, OrderStatus.Assigned]);

    return (this.orderStatistics?.ordersByStatus ?? []).filter(

      (item) => !hiddenStatuses.has(item.status)

    );

  }



  getStatusStatKey(item: OrderStatusCountDto): OrderTrackingStatKey {

    return orderStatusToStatKey(item.status);

  }



  getStatCardLabel(item: OrderStatusCountDto): string {

    return getOrderStatCardLabel(item.status, item.statusName);

  }



  onDeliveryManPanelOpen(): void {

    this.deliveryManLookupSearchTerm = '';

    this.deliveryManSearch$.next('');

  }



  onDeliveryManSearch(term: string): void {
    this.deliveryManLookupSearchTerm = term;
    this.deliveryManSearch$.next(term);
  }

  private fetchDeliveryManLookup(term: string) {
    this.isLoadingDeliveryMenLookup = true;
    const searchTerm = term?.trim() || undefined;

    return this.deliveryManClient.getAvailableDeliveryMenLookup(searchTerm ?? null, null).pipe(

      catchError(() => {

        this.toasterService.error('خطأ', 'تعذر تحميل قائمة المندوبين');

        return of([]);

      }),

      finalize(() => { this.isLoadingDeliveryMenLookup = false; })

    );

  }



  private toStatKey(key: string): OrderTrackingStatKey {

    const valid: OrderTrackingStatKey[] = [

      'all', 'active', 'pending', 'assigned', 'confirmed', 'pickedup', 'completed', 'cancelled'

    ];

    return valid.includes(key as OrderTrackingStatKey) ? key as OrderTrackingStatKey : 'all';

  }



  private getAppliedStatusFilter(): OrderStatus | undefined {

    if (isActiveOrdersStatKey(this.filterApplied.statusKey)) return undefined;

    return resolveStatusFilter(this.filterApplied.statusKey);

  }



  private isActiveOrdersFilter(): boolean {

    return isActiveOrdersStatKey(this.filterApplied.statusKey);

  }



  loadStatistics(): void {

    this.isLoadingStatistics = true;

    this.sub.sink = this.orderAdminClient.getOrderStatistics()

      .pipe(

        catchError(() => of(null)),

        finalize(() => { this.isLoadingStatistics = false; })

      )

      .subscribe((stats) => {

        this.orderStatistics = stats;

        if (stats && !this.hasSearched) {

          this.search();

        }

      });

  }



  search(): void {

    this.filterApplied = cloneCaptainOrdersFilter(this.filterDraft);

    this.selectedStatKey = this.toStatKey(this.filterApplied.statusKey);

    this.currentPage = 1;

    this.hasSearched = true;

    this.loadOrders();

  }



  resetFilters(): void {

    this.filterDraft = { ...EMPTY_CAPTAIN_ORDERS_FILTER };

    this.selectedDeliveryMenCache = [];

    this.deliveryManLookupOptions = [];

    this.selectedStatKey = 'all';

    this.search();

  }



  loadOrders(): void {

    this.isLoading = true;

    const skip = (this.currentPage - 1) * this.itemsPerPage;

    const term = this.filterApplied.searchTerm?.trim() || undefined;



    this.sub.sink = this.orderAdminClient.getAllOrders(

      skip,

      this.itemsPerPage,

      term ?? null,

      this.getAppliedStatusFilter() ?? null,

      this.isActiveOrdersFilter() ? true : null,

      resolveCustomerTypeFilter(this.filterApplied.customerTypeKey) ?? null,

      parseDateFilter(this.filterApplied.fromDate) ?? null,

      parseDateFilter(this.filterApplied.toDate) ?? null,

      resolveDeliveryManFilters(this.filterApplied.deliveryManIds) ?? null

    ).pipe(

      catchError(() => {

        const empty = new PagedResultOfGetAllOrdersDto();

        empty.data = [];

        empty.totalCount = 0;

        empty.totalPages = 0;

        return of(empty);

      }),

      finalize(() => { this.isLoading = false; })

    ).subscribe((response) => {

      this.orders = response?.data ?? [];

      this.totalCount = response?.totalCount ?? 0;

      this.totalPages = response?.totalPages ?? 0;

    });

  }



  changePage(page: number): void {

    if (page >= 1 && page <= this.totalPages) {

      this.currentPage = page;

      this.loadOrders();

    }

  }



  exportSummaryStats(): void {

    this.isExportingSummary = true;

    this.sub.sink = this.orderAdminClient.exportOrderStatistics().pipe(
      map((file) => mapFileResponse(file, `OrderStatistics_${Date.now()}.xlsx`)),

      catchError(() => {

        this.toasterService.error('خطأ', 'تعذر تصدير الإحصائيات');

        return of(null);

      }),

      finalize(() => { this.isExportingSummary = false; })

    ).subscribe((result) => {

      if (!result) return;

      triggerFileDownload(result.blob, result.fileName);

      this.toasterService.success('تم', 'تم تصدير إحصائيات الطلبات بنجاح');

    });

  }



  exportOrders(): void {

    if (!this.hasSearched) this.search();



    this.isExporting = true;

    const term = this.filterApplied.searchTerm?.trim() || undefined;



    this.sub.sink = this.orderAdminClient.exportAllOrders(

      term ?? null,

      this.getAppliedStatusFilter() ?? null,

      this.isActiveOrdersFilter() ? true : null,

      resolveCustomerTypeFilter(this.filterApplied.customerTypeKey) ?? null,

      parseDateFilter(this.filterApplied.fromDate) ?? null,

      parseDateFilter(this.filterApplied.toDate) ?? null,

      resolveDeliveryManFilters(this.filterApplied.deliveryManIds) ?? null

    ).pipe(
      map((file) => mapFileResponse(file, `Orders_${Date.now()}.xlsx`)),

      catchError(() => {

        this.toasterService.error('خطأ', 'تعذر تصدير البيانات');

        return of(null);

      }),

      finalize(() => { this.isExporting = false; })

    ).subscribe((result) => {

      if (!result) return;

      triggerFileDownload(result.blob, result.fileName);

      this.toasterService.success('تم', 'تم تصدير الطلبات بنجاح');

    });

  }



  viewOrderDetails(orderId: number): void {

    this.router.navigate(['/admin/requests/details', orderId]);

  }



  getStatusClass(status: OrderStatus): string {

    return getOrderStatusBadgeClass(status);

  }



  getOrderStatusLabel(order: GetAllOrdersDto): string {

    if (order.status === OrderStatus.PickedUpFromDeliveryMan) {

      return 'تم تسليمها للعميل';

    }

    return order.statusName;

  }



  get visiblePages(): (number | string)[] {

    return buildVisiblePages(this.currentPage, this.totalPages);

  }



  get displayStartCount(): number {

    return this.totalCount === 0 ? 0 : (this.currentPage - 1) * this.itemsPerPage + 1;

  }



  get displayEndCount(): number {

    return Math.min(this.currentPage * this.itemsPerPage, this.totalCount);

  }



  isPageNumber(page: number | string): boolean {

    return typeof page === 'number';

  }



  isPageEllipsis(page: number | string): boolean {

    return page === 'ellipsis';

  }

}


