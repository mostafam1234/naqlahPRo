import { DatePipe, DecimalPipe, NgClass, NgFor, NgIf } from '@angular/common';

import { Component, OnDestroy, OnInit } from '@angular/core';

import { ActivatedRoute, Router } from '@angular/router';

import { FormsModule } from '@angular/forms';

import { TranslateModule } from '@ngx-translate/core';

import { ToasterService } from 'src/app/Core/services/toaster.service';

import {

  DeliveryManAdminClient,

  DeliveryManOrderStatusCountDto,

  DeliveryManSummaryDto,

  GetAllOrdersDto,

  OrderStatus,

  PagedResultOfGetAllOrdersDto

} from 'src/app/Core/services/NaqlahClient';

import { FormalSelectComponent } from 'src/app/shared/components/formal-select/formal-select.component';

import { PageHeaderComponent } from 'src/app/shared/components/page-header/page-header.component';

import { PermissionService } from 'src/app/shared/services/permission.service';

import { of } from 'rxjs';

import { catchError, finalize, map } from 'rxjs/operators';

import { SubSink } from 'subsink';

import {

  buildVisiblePages,

  CaptainOrdersFilterForm,

  CUSTOMER_TYPE_OPTIONS,

  EMPTY_CAPTAIN_ORDERS_FILTER,

  getOrderStatCardClass,

  getOrderStatusBadgeClass,

  isActiveOrdersStatKey,

  mapFileResponse,

  orderStatusToStatKey,

  OrderTrackingStatKey,

  parseDateFilter,

  resolveCustomerTypeFilter,

  resolveStatusFilter,

  triggerFileDownload

} from '../captain-orders.helpers';



@Component({

  selector: 'app-order-tracking',

  standalone: true,

  imports: [NgIf, NgFor, NgClass, FormsModule, PageHeaderComponent, TranslateModule, DecimalPipe, DatePipe, FormalSelectComponent],

  templateUrl: './order-tracking.component.html',

  styleUrl: './order-tracking.component.css'

})

export class OrderTrackingComponent implements OnInit, OnDestroy {

  deliveryManId: number | null = null;

  deliveryManSummary: DeliveryManSummaryDto | null = null;

  isLoadingSummary = false;



  orders: GetAllOrdersDto[] = [];

  totalCount = 0;

  totalPages = 0;

  isLoading = false;

  isExporting = false;
  isExportingSummary = false;
  hasSearched = false;



  filterDraft: CaptainOrdersFilterForm = { ...EMPTY_CAPTAIN_ORDERS_FILTER };

  filterApplied: CaptainOrdersFilterForm = { ...EMPTY_CAPTAIN_ORDERS_FILTER };

  selectedStatKey: OrderTrackingStatKey = 'all';



  currentPage = 1;

  readonly itemsPerPage = 10;

  readonly customerTypeOptions = CUSTOMER_TYPE_OPTIONS;



  private sub = new SubSink();



  constructor(

    private route: ActivatedRoute,

    private router: Router,

    private deliveryManClient: DeliveryManAdminClient,

    private permissionService: PermissionService,

    private toasterService: ToasterService

  ) {}



  ngOnInit(): void {

    this.permissionService.getPermissions().subscribe(() => {});

    this.sub.sink = this.route.paramMap.subscribe((params) => {

      const id = params.get('id');

      if (id) {

        this.deliveryManId = +id;

        this.loadDeliveryManSummary();

      }

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



  search(): void {

    this.filterApplied = { ...this.filterDraft };

    this.selectedStatKey = this.toStatKey(this.filterApplied.statusKey);

    this.currentPage = 1;

    this.hasSearched = true;

    this.loadDeliveryManOrders();

  }



  resetFilters(): void {

    this.filterDraft = { ...EMPTY_CAPTAIN_ORDERS_FILTER };

    this.selectedStatKey = 'all';

    this.search();

  }



  isStatCardSelected(key: OrderTrackingStatKey): boolean {

    return this.selectedStatKey === key;

  }



  getStatCardClass(key: OrderTrackingStatKey): string {

    return getOrderStatCardClass(key);

  }



  getStatCount(key: OrderTrackingStatKey): number {

    if (!this.deliveryManSummary) return 0;

    switch (key) {

      case 'all': return this.deliveryManSummary.totalOrders ?? 0;

      case 'active': return this.deliveryManSummary.activeOrders ?? 0;

      case 'pending': return this.deliveryManSummary.pendingOrders ?? 0;

      case 'assigned': return this.deliveryManSummary.assignedOrders ?? 0;

      case 'confirmed': return this.deliveryManSummary.confirmedGoingToPickupOrders ?? 0;

      case 'pickedup': return this.deliveryManSummary.pickedUpOrders ?? 0;

      case 'completed': return this.deliveryManSummary.completedOrders ?? 0;

      case 'cancelled': return this.deliveryManSummary.cancelledOrders ?? 0;

      default: return 0;

    }

  }



  get statusStatCards(): DeliveryManOrderStatusCountDto[] {
    const hiddenStatuses = new Set([OrderStatus.Pending, OrderStatus.Assigned]);
    return (this.deliveryManSummary?.ordersByStatus ?? []).filter(
      (item) => !hiddenStatuses.has(item.status)
    );
  }

  getStatCardLabel(item: DeliveryManOrderStatusCountDto): string {
    if (item.status === OrderStatus.PickedUpFromDeliveryMan) {
      return 'شحنات تم تسليمها للعميل';
    }
    return item.statusName;
  }



  getStatusStatKey(item: DeliveryManOrderStatusCountDto): OrderTrackingStatKey {

    return orderStatusToStatKey(item.status);

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



  private getAppliedCustomerTypeFilter() {

    return resolveCustomerTypeFilter(this.filterApplied.customerTypeKey);

  }



  loadDeliveryManSummary(): void {

    if (!this.deliveryManId) return;

    this.isLoadingSummary = true;

    this.sub.sink = this.deliveryManClient.getDeliveryManSummary(this.deliveryManId)

      .pipe(

        catchError(() => of(null)),

        finalize(() => { this.isLoadingSummary = false; })

      )

      .subscribe((summary) => {

        this.deliveryManSummary = summary;

        if (summary && !this.hasSearched) {

          this.search();

        }

      });

  }



  loadDeliveryManOrders(): void {

    if (!this.deliveryManId) return;

    this.isLoading = true;

    const skip = (this.currentPage - 1) * this.itemsPerPage;

    const term = this.filterApplied.searchTerm?.trim() || undefined;



    this.sub.sink = this.deliveryManClient.getOrdersByDeliveryManId(

      this.deliveryManId,

      skip,

      this.itemsPerPage,

      term ?? null,

      this.getAppliedStatusFilter() ?? null,

      this.isActiveOrdersFilter() ? true : null,

      this.getAppliedCustomerTypeFilter() ?? null,

      parseDateFilter(this.filterApplied.fromDate) ?? null,

      parseDateFilter(this.filterApplied.toDate) ?? null

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



  exportSummaryStats(): void {
    if (!this.deliveryManId) return;

    this.isExportingSummary = true;
    this.sub.sink = this.deliveryManClient.exportDeliveryManSummary(this.deliveryManId).pipe(
      map((file) => mapFileResponse(file, `CaptainShipmentStats_${this.deliveryManId}.xlsx`)),
      catchError(() => {
        this.toasterService.error('خطأ', 'تعذر تصدير الإحصائيات');
        return of(null);
      }),
      finalize(() => { this.isExportingSummary = false; })
    ).subscribe((result) => {
      if (!result) return;
      triggerFileDownload(result.blob, result.fileName);
      this.toasterService.success('تم', 'تم تصدير إحصائيات الشحنات بنجاح');
    });
  }

  exportOrders(): void {
    if (!this.deliveryManId) return;
    if (!this.hasSearched) this.search();

    this.isExporting = true;
    const term = this.filterApplied.searchTerm?.trim() || undefined;



    this.sub.sink = this.deliveryManClient.exportOrdersByDeliveryManId(

      this.deliveryManId,

      term ?? null,

      this.getAppliedStatusFilter() ?? null,

      this.isActiveOrdersFilter() ? true : null,

      this.getAppliedCustomerTypeFilter() ?? null,

      parseDateFilter(this.filterApplied.fromDate) ?? null,

      parseDateFilter(this.filterApplied.toDate) ?? null

    ).pipe(
      map((file) => mapFileResponse(file, `CaptainOrders_${this.deliveryManId}.xlsx`)),

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



  changePage(page: number): void {

    if (page >= 1 && page <= this.totalPages) {

      this.currentPage = page;

      this.loadDeliveryManOrders();

    }

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



  getOriginAddress(order: GetAllOrdersDto): string {

    return order.wayPoints?.find((w) => w.isOrigin)?.address || '—';

  }



  getDestinationAddress(order: GetAllOrdersDto): string {

    return order.wayPoints?.find((w) => w.isDestination)?.address || '—';

  }



  isPageNumber(page: number | string): boolean {

    return typeof page === 'number';

  }



  isPageEllipsis(page: number | string): boolean {

    return page === 'ellipsis';

  }

}


