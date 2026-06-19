import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule, DecimalPipe, NgClass } from '@angular/common';
import { ReactiveFormsModule, FormControl, FormsModule } from '@angular/forms';
import { ActivatedRoute, NavigationEnd, Router } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { PageHeaderComponent } from 'src/app/shared/components/page-header/page-header.component';
import {
  ActiveCategoryDto,
  DeliveryManVehicleDto,
  MainCategoryVehicleCountDto,
  VehicleAdminClient,
  VehicleTypeStatisticsDto
} from 'src/app/Core/services/NaqlahClient';
import { SubSink } from 'subsink';
import { catchError, debounceTime, distinctUntilChanged, filter, finalize, map, switchMap } from 'rxjs/operators';
import { forkJoin, of } from 'rxjs';
import { ToasterService } from 'src/app/Core/services/toaster.service';
import { ConfirmationModalComponent } from 'src/app/shared/components/confirmation-modal/confirmation-modal.component';
import { PermissionService } from 'src/app/shared/services/permission.service';
import {
  mapFileResponse,
  triggerFileDownload
} from '../../orders/captain-orders.helpers';

type VehicleStatFilterKey = 'all' | number;

@Component({
  selector: 'app-vehicles',
  standalone: true,
  imports: [
    CommonModule,
    NgClass,
    ReactiveFormsModule,
    FormsModule,
    TranslateModule,
    PageHeaderComponent,
    ConfirmationModalComponent,
    DecimalPipe
  ],
  templateUrl: './vehicles.component.html',
  styleUrls: ['./vehicles.component.css']
})
export class VehiclesComponent implements OnInit, OnDestroy {
  activeTab: 'brands' | 'types' = 'brands';
  isLoading = false;
  searchControl = new FormControl('');
  items: DeliveryManVehicleDto[] = [];

  vehicleStatistics: VehicleTypeStatisticsDto | null = null;
  isLoadingStatistics = false;
  isExportingStatistics = false;
  selectedStatFilter: VehicleStatFilterKey = 'all';

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
    private vehicleClient: VehicleAdminClient,
    private toasterService: ToasterService,
    private permissionService: PermissionService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.permissionService.getPermissions().subscribe(() => {});

    const tab = this.route.snapshot.queryParamMap.get('tab');
    if (tab === 'types' || tab === 'brands') {
      this.activeTab = tab;
    }

    this.loadItems();
    this.setupSearch();

    if (this.activeTab === 'types') {
      this.loadVehicleStatistics();
    }

    this.sub.sink = this.router.events
      .pipe(filter((event): event is NavigationEnd => event instanceof NavigationEnd))
      .subscribe((event) => {
        const url = event.urlAfterRedirects;
        if (!url.startsWith('/admin/vehicles')) return;
        if (url.includes('/brand/') || url.includes('/type/')) return;

        const tab = this.route.snapshot.queryParamMap.get('tab');
        if (tab === 'types' || tab === 'brands') {
          this.activeTab = tab;
        }

        this.loadItems();
        if (this.activeTab === 'types') {
          this.loadVehicleStatistics();
        }
      });
  }

  ngOnDestroy(): void {
    this.sub.unsubscribe();
  }

  get mainCategoryStatCards(): MainCategoryVehicleCountDto[] {
    return this.vehicleStatistics?.mainCategoryCounts ?? [];
  }

  get totalVehicleTypesCount(): number {
    return this.vehicleStatistics?.totalVehicleTypes ?? 0;
  }

  getSectionStatCount(item: MainCategoryVehicleCountDto): number {
    return item.vehicleTypeCount ?? 0;
  }

  getSectionStatLabel(item: MainCategoryVehicleCountDto): string {
    return item.name || item.arabicName || item.englishName || '';
  }

  hasPermission(permission: string): boolean {
    return this.permissionService.hasPermission(permission);
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
    const filterByCategory = this.activeTab === 'types' && this.selectedStatFilter !== 'all';
    const skip = filterByCategory ? 0 : this.currentPage * this.itemsPerPage;
    const take = filterByCategory ? 5000 : this.itemsPerPage;
    const searchTerm = this.searchControl.value || '';

    const apiCall = this.activeTab === 'brands'
      ? this.vehicleClient.getVehiclesBrands(skip, take, searchTerm)
      : this.vehicleClient.getVehiclesTypes(skip, take, searchTerm);

    this.sub.sink = apiCall.subscribe({
      next: (response) => {
        let data = response.data ?? [];
        let totalCount = response.totalCount ?? 0;
        let totalPages = response.totalPages ?? 0;

        if (filterByCategory) {
          const categoryId = this.selectedStatFilter as number;
          data = data.filter((item) =>
            item.mainCategories?.some((category) => category.id === categoryId)
          );

          totalCount = data.length;
          const start = this.currentPage * this.itemsPerPage;
          data = data.slice(start, start + this.itemsPerPage);
          totalPages = this.itemsPerPage > 0 ? Math.ceil(totalCount / this.itemsPerPage) : 0;
        }

        this.items = data;
        this.totalCount = totalCount;
        this.totalPages = totalPages;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
        this.toasterService.error('خطأ', 'تعذر تحميل البيانات');
      }
    });
  }

  loadVehicleStatistics(): void {
    this.isLoadingStatistics = true;

    this.sub.sink = this.vehicleClient
      .getVehicleTypeStatistics()
      .pipe(
        map((stats) => this.normalizeStatistics(stats)),
        catchError(() => of(null)),
        switchMap((stats) => {
          const normalized = stats;
          const needsFallback =
            !normalized ||
            (normalized.totalVehicleTypes ?? 0) === 0 ||
            (normalized.mainCategoryCounts?.length ?? 0) === 0;

          if (!needsFallback) {
            return of(normalized);
          }

          return this.buildStatisticsFallback$().pipe(
            map((fallback) => {
              if (!fallback) {
                return normalized;
              }
              if (!normalized || (normalized.totalVehicleTypes ?? 0) === 0) {
                return fallback;
              }
              normalized.mainCategoryCounts = fallback.mainCategoryCounts;
              return normalized;
            })
          );
        }),
        catchError(() => {
          this.toasterService.error('خطأ', 'تعذر تحميل إحصائيات أنواع المركبات');
          return of(null);
        }),
        finalize(() => {
          this.isLoadingStatistics = false;
        })
      )
      .subscribe((stats) => {
        this.vehicleStatistics = stats;
      });
  }

  private normalizeStatistics(stats: VehicleTypeStatisticsDto | null): VehicleTypeStatisticsDto | null {
    if (!stats) return null;

    const raw = stats as unknown as Record<string, unknown>;
    return VehicleTypeStatisticsDto.fromJS({
      totalVehicleTypes: stats.totalVehicleTypes ?? raw['TotalVehicleTypes'],
      totalRegisteredVehicles: stats.totalRegisteredVehicles ?? raw['TotalRegisteredVehicles'],
      loadCategoryCounts: stats.loadCategoryCounts ?? raw['LoadCategoryCounts'],
      mainCategoryCounts: stats.mainCategoryCounts ?? raw['MainCategoryCounts']
    });
  }

  private buildStatisticsFallback$() {
    return forkJoin({
      types: this.vehicleClient.getVehiclesTypes(0, 5000, ''),
      categories: this.vehicleClient.getMainCategoriesLookup().pipe(catchError(() => of([] as ActiveCategoryDto[])))
    }).pipe(
      map(({ types, categories }) =>
        this.buildStatisticsFromTypes(types.data ?? [], categories ?? [])
      ),
      catchError(() => of(null))
    );
  }

  private buildStatisticsFromTypes(
    types: DeliveryManVehicleDto[],
    categories: ActiveCategoryDto[]
  ): VehicleTypeStatisticsDto {
    const typeCountByCategory = new Map<number, number>();
    const categoryLabels = new Map<number, string>();

    for (const category of categories) {
      categoryLabels.set(category.id, category.name);
    }

    for (const type of types) {
      for (const category of type.mainCategories ?? []) {
        if (!categoryLabels.has(category.id)) {
          categoryLabels.set(
            category.id,
            category.name || category.arabicName || category.englishName || `#${category.id}`
          );
        }
        typeCountByCategory.set(category.id, (typeCountByCategory.get(category.id) ?? 0) + 1);
      }
    }

    const mainCategoryCounts: MainCategoryVehicleCountDto[] =
      categories.length > 0
        ? categories
            .slice()
            .sort((a, b) => (a.name || '').localeCompare(b.name || '', 'ar'))
            .map((category) =>
              MainCategoryVehicleCountDto.fromJS({
                mainCategoryId: category.id,
                name: category.name,
                arabicName: category.name,
                englishName: category.name,
                vehicleTypeCount: typeCountByCategory.get(category.id) ?? 0,
                registeredVehicleCount: 0
              })
            )
        : Array.from(categoryLabels.entries())
            .sort((a, b) => a[1].localeCompare(b[1], 'ar'))
            .map(([id, name]) =>
              MainCategoryVehicleCountDto.fromJS({
                mainCategoryId: id,
                name,
                arabicName: name,
                englishName: name,
                vehicleTypeCount: typeCountByCategory.get(id) ?? 0,
                registeredVehicleCount: 0
              })
            );

    return VehicleTypeStatisticsDto.fromJS({
      totalVehicleTypes: types.length,
      totalRegisteredVehicles: 0,
      loadCategoryCounts: [],
      mainCategoryCounts
    });
  }

  exportVehicleStatistics(): void {
    this.isExportingStatistics = true;

    this.sub.sink = this.vehicleClient
      .exportVehicleTypeStatistics()
      .pipe(
        map((file) => mapFileResponse(file, `VehicleTypeStatistics_${Date.now()}.xlsx`)),
        catchError(() => {
          this.toasterService.error('خطأ', 'تعذر تصدير الإحصائيات');
          return of(null);
        }),
        finalize(() => {
          this.isExportingStatistics = false;
        })
      )
      .subscribe((result) => {
        if (!result) return;
        triggerFileDownload(result.blob, result.fileName);
        this.toasterService.success('تم', 'تم تصدير إحصائيات أنواع المركبات بنجاح');
      });
  }

  onStatCardClick(key: VehicleStatFilterKey): void {
    this.selectedStatFilter = key;
    this.currentPage = 0;
    this.loadItems();
  }

  isStatCardSelected(key: VehicleStatFilterKey): boolean {
    return this.selectedStatFilter === key;
  }

  isSectionStatSelected(mainCategoryId: number): boolean {
    return this.selectedStatFilter === mainCategoryId;
  }

  onSectionStatClick(mainCategoryId: number): void {
    this.onStatCardClick(mainCategoryId);
  }

  getStatCardClass(index: number): string {
    const classes = [
      'na-stat-card--confirmed',
      'na-stat-card--pickedup',
      'na-stat-card--completed',
      'na-stat-card--inactive'
    ];
    return classes[index % classes.length];
  }

  setActiveTab(tab: 'brands' | 'types'): void {
    this.activeTab = tab;
    this.currentPage = 0;
    this.selectedStatFilter = 'all';
    this.searchControl.setValue('');

    if (tab === 'types') {
      this.loadVehicleStatistics();
    }

    this.loadItems();
  }

  openAdd(): void {
    if (this.activeTab === 'brands') {
      this.router.navigate(['/admin/vehicles/brand/add']);
      return;
    }
    this.router.navigate(['/admin/vehicles/type/add']);
  }

  openEdit(item: DeliveryManVehicleDto): void {
    if (this.activeTab === 'brands') {
      this.router.navigate(['/admin/vehicles/brand/edit', item.id], { state: { item } });
      return;
    }
    this.router.navigate(['/admin/vehicles/type/edit', item.id], { state: { item } });
  }

  confirmDelete(itemId: number): void {
    const itemType = this.activeTab === 'brands' ? 'الماركة' : 'نوع المركبة';
    this.confirmationTitle = 'تأكيد الحذف';
    this.confirmationMessage = `هل أنت متأكد من حذف ${itemType}؟`;
    this.pendingAction = () => this.performDelete(itemId);
    this.showConfirmModal = true;
  }

  private performDelete(itemId: number): void {
    const apiCall = this.activeTab === 'brands'
      ? this.vehicleClient.deleteVehicleBrand(itemId)
      : this.vehicleClient.deleteVehicleType(itemId);

    this.sub.sink = apiCall.subscribe({
      next: () => {
        this.toasterService.success('تم الحذف بنجاح', 'تم حذف العنصر بنجاح');
        this.loadItems();
        if (this.activeTab === 'types') {
          this.loadVehicleStatistics();
        }
      },
      error: (error) => {
        this.toasterService.error('خطأ', error?.message || 'حدث خطأ أثناء الحذف');
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
    return (this.currentPage * this.itemsPerPage) + 1;
  }

  get displayEndCount(): number {
    if (this.totalCount === 0) return 0;
    const endCount = (this.currentPage + 1) * this.itemsPerPage;
    return Math.min(endCount, this.totalCount);
  }

  goBack(): void {
    window.history.back();
  }

  get currentTabTitle(): string {
    return this.activeTab === 'brands' ? 'ADMIN.VEHICLESMENUE.BRANDS' : 'ADMIN.VEHICLESMENUE.TYPES';
  }

  get currentTabDescription(): string {
    return this.activeTab === 'brands'
      ? 'ADMIN.VEHICLESMENUE.BRANDS_DESC'
      : 'ADMIN.VEHICLESMENUE.TYPES_DESC';
  }

  get addButtonText(): string {
    return this.activeTab === 'brands'
      ? 'ADMIN.VEHICLESMENUE.ADD_BUTTON_BRAND'
      : 'ADMIN.VEHICLESMENUE.ADD_BUTTON';
  }

  get tableTitle(): string {
    return this.activeTab === 'brands'
      ? 'ADMIN.VEHICLESMENUE.TABLE_TITLE_BRAND'
      : 'ADMIN.VEHICLESMENUE.TABLE_TITLE';
  }

  get noDataText(): string {
    return this.activeTab === 'brands'
      ? 'ADMIN.VEHICLESMENUE.NO_DATA_BRAND'
      : 'ADMIN.VEHICLESMENUE.NO_DATA';
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
}
