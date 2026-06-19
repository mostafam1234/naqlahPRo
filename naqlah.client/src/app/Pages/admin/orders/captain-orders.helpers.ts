import { CustomerType, DeliveryManLookupDto, DeliveryRequesState, OrderStatus } from 'src/app/Core/services/NaqlahClient';
import { SelectOption } from 'src/app/shared/models/select-option.model';
import { getDefaultSearchDateRange } from 'src/app/shared/utils/date-filter.helpers';

const defaultSearchDates = getDefaultSearchDateRange();

export type { SelectOption };

export interface CaptainsListFilterForm {
  searchTerm: string;
  deliveryManIds: string[];
  deliveryManActiveKey: string;
  fromDate: string;
  toDate: string;
}

export const EMPTY_CAPTAINS_LIST_FILTER: CaptainsListFilterForm = {
  searchTerm: '',
  deliveryManIds: [],
  deliveryManActiveKey: 'all',
  fromDate: defaultSearchDates.fromDate,
  toDate: defaultSearchDates.toDate
};

export interface CaptainOrdersFilterForm {
  searchTerm: string;
  deliveryManIds: string[];
  deliveryManActiveKey: string;
  customerTypeKey: string;
  statusKey: string;
  fromDate: string;
  toDate: string;
}

export const EMPTY_CAPTAIN_ORDERS_FILTER: CaptainOrdersFilterForm = {
  searchTerm: '',
  deliveryManIds: [],
  deliveryManActiveKey: 'all',
  customerTypeKey: 'all',
  statusKey: 'all',
  fromDate: defaultSearchDates.fromDate,
  toDate: defaultSearchDates.toDate
};

export const DELIVERY_MAN_ACTIVE_OPTIONS: SelectOption[] = [
  { value: 'all', label: 'الكل' },
  { value: 'active', label: 'نشط' },
  { value: 'inactive', label: 'غير نشط' }
];

export const CUSTOMER_TYPE_OPTIONS: SelectOption[] = [
  { value: 'all', label: 'الكل' },
  { value: 'individual', label: 'طلبات الأفراد' },
  { value: 'establishment', label: 'طلبات الشركات والمؤسسات' }
];

export const ORDER_STATUS_OPTIONS: SelectOption[] = [
  { value: 'all', label: 'الكل' },
  { value: 'pending', label: 'معلقة' },
  { value: 'assigned', label: 'منسوبة الى مندوب' },
  { value: 'confirmed', label: 'تم تأكيد الذهاب لالتقاط الشحنة' },
  { value: 'pickedup', label: 'التقاط الطلب من المندوب' },
  { value: 'cancelled', label: 'ملغية' },
  { value: 'completed', label: 'مكتملة' }
];

export function resolveStatusFilter(key: string): OrderStatus | undefined {
  switch (key) {
    case 'pending': return OrderStatus.Pending;
    case 'assigned': return OrderStatus.Assigned;
    case 'confirmed': return OrderStatus.ConfirmedGoingToPickup;
    case 'pickedup': return OrderStatus.PickedUpFromDeliveryMan;
    case 'cancelled': return OrderStatus.Cancelled;
    case 'completed': return OrderStatus.Completed;
    default: return undefined;
  }
}

export type OrderTrackingStatKey =
  | 'all'
  | 'active'
  | 'pending'
  | 'assigned'
  | 'confirmed'
  | 'pickedup'
  | 'completed'
  | 'cancelled';

export function isActiveOrdersStatKey(key: string): boolean {
  return key === 'active';
}

export function orderStatusToStatKey(status: OrderStatus): OrderTrackingStatKey {
  switch (status) {
    case OrderStatus.Pending: return 'pending';
    case OrderStatus.Assigned: return 'assigned';
    case OrderStatus.ConfirmedGoingToPickup: return 'confirmed';
    case OrderStatus.PickedUpFromDeliveryMan: return 'pickedup';
    case OrderStatus.Cancelled: return 'cancelled';
    case OrderStatus.Completed: return 'completed';
    default: return 'all';
  }
}

export function getOrderStatCardLabel(status: OrderStatus, defaultName: string): string {
  if (status === OrderStatus.PickedUpFromDeliveryMan) {
    return 'شحنات تم تسليمها للعميل';
  }
  if (status === OrderStatus.Completed) {
    return 'الطلبات المكتملة';
  }
  if (status === OrderStatus.Cancelled) {
    return 'الطلبات الملغية';
  }
  return defaultName;
}

export const HIDDEN_ORDER_STAT_KEYS = new Set<OrderTrackingStatKey>(['pending', 'assigned']);

export function getOrderStatCardClass(key: OrderTrackingStatKey): string {
  switch (key) {
    case 'all': return 'na-stat-card--total';
    case 'active': return 'na-stat-card--active';
    case 'completed': return 'na-stat-card--completed';
    case 'cancelled': return 'na-stat-card--inactive';
    case 'pending': return 'na-stat-card--pending';
    case 'assigned': return 'na-stat-card--assigned';
    case 'confirmed': return 'na-stat-card--confirmed';
    case 'pickedup': return 'na-stat-card--pickedup';
    default: return 'na-stat-card--total';
  }
}

export function resolveCustomerTypeFilter(key: string): CustomerType | undefined {
  switch (key) {
    case 'individual': return CustomerType.Individual;
    case 'establishment': return CustomerType.Establishment;
    default: return undefined;
  }
}

export function getOrderStatusBadgeClass(status: OrderStatus): string {
  switch (status) {
    case OrderStatus.Pending: return 'na-status na-status--pending';
    case OrderStatus.Assigned: return 'na-status na-status--assigned';
    case OrderStatus.ConfirmedGoingToPickup: return 'na-status na-status--confirmed';
    case OrderStatus.PickedUpFromDeliveryMan: return 'na-status na-status--pickedup';
    case OrderStatus.Completed: return 'na-status na-status--completed';
    case OrderStatus.Cancelled: return 'na-status na-status--cancelled';
    default: return 'na-status na-status--neutral';
  }
}

export function parseDateFilter(value: string | null | undefined): Date | undefined {
  if (!value) return undefined;
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? undefined : date;
}

export function triggerFileDownload(blob: Blob, fileName: string): void {
  const url = URL.createObjectURL(blob);
  const anchor = document.createElement('a');
  anchor.href = url;
  anchor.download = fileName;
  anchor.click();
  URL.revokeObjectURL(url);
}

export function mapFileResponse(
  file: { data: Blob; fileName?: string | null },
  fallbackFileName: string
): { blob: Blob; fileName: string } {
  return {
    blob: file.data,
    fileName: file.fileName || fallbackFileName
  };
}

export function buildVisiblePages(currentPage: number, totalPages: number, maxVisible = 5): (number | string)[] {
  const pages: (number | string)[] = [];
  if (totalPages <= maxVisible) {
    for (let i = 1; i <= totalPages; i++) pages.push(i);
    return pages;
  }
  if (currentPage <= 3) {
    for (let i = 1; i <= 4; i++) pages.push(i);
    pages.push('ellipsis', totalPages);
  } else if (currentPage >= totalPages - 2) {
    pages.push(1, 'ellipsis');
    for (let i = totalPages - 3; i <= totalPages; i++) pages.push(i);
  } else {
    pages.push(1, 'ellipsis', currentPage - 1, currentPage, currentPage + 1, 'ellipsis', totalPages);
  }
  return pages;
}

export function resolveDeliveryManActiveFilter(key: string): boolean | undefined {
  switch (key) {
    case 'active': return true;
    case 'inactive': return false;
    default: return undefined;
  }
}

export function resolveDeliveryManFilters(keys: string[]): number[] | undefined {
  const ids = keys
    .map((key) => Number(key))
    .filter((id) => Number.isFinite(id) && id > 0);
  return ids.length > 0 ? ids : undefined;
}

export function getDeliveryManActiveBadgeClass(active: boolean): string {
  return active ? 'na-lookup-badge na-lookup-badge--ok' : 'na-lookup-badge na-lookup-badge--off';
}

export function getDeliveryManStateBadgeClass(state: DeliveryRequesState): string {
  switch (state) {
    case DeliveryRequesState.Approved: return 'na-lookup-badge na-lookup-badge--ok';
    case DeliveryRequesState.New: return 'na-lookup-badge na-lookup-badge--pending';
    case DeliveryRequesState.Rejected: return 'na-lookup-badge na-lookup-badge--danger';
    case DeliveryRequesState.Blocked: return 'na-lookup-badge na-lookup-badge--danger';
    case DeliveryRequesState.Suspended: return 'na-lookup-badge na-lookup-badge--warn';
    default: return 'na-lookup-badge na-lookup-badge--neutral';
  }
}

export function mapDeliveryManToSelectOption(dm: Pick<
  DeliveryManLookupDto,
  'id' | 'fullName' | 'phoneNumber' | 'active' | 'activeStatusName' | 'deliveryState' | 'deliveryStateName'
>): SelectOption {
  const meta = `${dm.fullName} ${dm.phoneNumber} ${dm.activeStatusName} ${dm.deliveryStateName}`.toLowerCase();

  return {
    value: String(dm.id),
    label: dm.fullName,
    hint: dm.phoneNumber,
    meta,
    activeLabel: dm.activeStatusName,
    activeBadgeClass: getDeliveryManActiveBadgeClass(dm.active),
    stateLabel: dm.deliveryStateName,
    stateBadgeClass: getDeliveryManStateBadgeClass(dm.deliveryState)
  };
}

export function cloneCaptainsListFilter(filter: CaptainsListFilterForm): CaptainsListFilterForm {
  return {
    ...filter,
    deliveryManIds: [...filter.deliveryManIds]
  };
}

export function hasActiveCaptainsFilters(filter: CaptainsListFilterForm): boolean {
  return !!(
    filter.searchTerm?.trim() ||
    filter.deliveryManIds.length > 0 ||
    filter.deliveryManActiveKey !== 'all' ||
    filter.fromDate ||
    filter.toDate
  );
}

export function cloneCaptainOrdersFilter(filter: CaptainOrdersFilterForm): CaptainOrdersFilterForm {
  return {
    ...filter,
    deliveryManIds: [...filter.deliveryManIds]
  };
}

export function hasActiveFilters(filter: CaptainOrdersFilterForm): boolean {
  return !!(
    filter.searchTerm?.trim() ||
    filter.deliveryManIds.length > 0 ||
    filter.deliveryManActiveKey !== 'all' ||
    filter.customerTypeKey !== 'all' ||
    filter.statusKey !== 'all' ||
    filter.fromDate ||
    filter.toDate
  );
}
