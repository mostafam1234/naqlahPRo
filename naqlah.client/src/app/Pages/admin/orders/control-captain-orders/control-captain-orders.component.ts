import { DatePipe, DecimalPipe, NgClass, NgFor, NgIf } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { ToasterService } from 'src/app/Core/services/toaster.service';
import {
  DeliveryManAdminClient,
  DeliveryManStatisticsDto,
  GetAllDeliveryMenDto,
  PagedResultOfGetAllDeliveryMenDto
} from 'src/app/Core/services/NaqlahClient';
import { PageHeaderComponent } from 'src/app/shared/components/page-header/page-header.component';
import { PermissionService } from 'src/app/shared/services/permission.service';
import { of, Subject } from 'rxjs';
import { catchError, debounceTime, finalize, map, switchMap } from 'rxjs/operators';
import { SubSink } from 'subsink';
import { FormalSelectComponent } from 'src/app/shared/components/formal-select/formal-select.component';
import { FormalMultiSelectComponent } from 'src/app/shared/components/formal-multi-select/formal-multi-select.component';
import {
  buildVisiblePages,
  CaptainsListFilterForm,
  cloneCaptainsListFilter,
  DELIVERY_MAN_ACTIVE_OPTIONS,
  EMPTY_CAPTAINS_LIST_FILTER,
  mapDeliveryManToSelectOption,
  mapFileResponse,
  parseDateFilter,
  resolveDeliveryManActiveFilter,
  resolveDeliveryManFilters,
  SelectOption,
  triggerFileDownload
} from '../captain-orders.helpers';

type CaptainActiveStatKey = 'all' | 'active' | 'inactive';

@Component({
  selector: 'app-control-captain-orders',
  standalone: true,
  imports: [
    NgFor, NgClass, NgIf, FormsModule, PageHeaderComponent, TranslateModule,
    DecimalPipe, DatePipe, FormalSelectComponent, FormalMultiSelectComponent
  ],
  templateUrl: './control-captain-orders.component.html',
  styleUrl: './control-captain-orders.component.css'
})
export class ControlCaptainOrdersComponent implements OnInit, OnDestroy {
  statistics: DeliveryManStatisticsDto | null = null;
  isLoadingStatistics = false;

  deliveryMen: GetAllDeliveryMenDto[] = [];
  filterDraft: CaptainsListFilterForm = { ...EMPTY_CAPTAINS_LIST_FILTER };
  filterApplied: CaptainsListFilterForm = { ...EMPTY_CAPTAINS_LIST_FILTER };
  selectedStatKey: CaptainActiveStatKey = 'all';

  captainTotalCount = 0;
  captainTotalPages = 0;
  isLoadingCaptains = false;
  captainPage = 1;
  hasSearchedCaptains = false;
  isExporting = false;

  deliveryManLookupOptions: SelectOption[] = [];
  selectedDeliveryMenCache: SelectOption[] = [];
  isLoadingDeliveryMenLookup = false;
  readonly deliveryManSearchPlaceholder = 'ابحث بالاسم أو الهاتف...';
  private deliveryManLookupSearchTerm = '';

  readonly deliveryManActiveOptions = DELIVERY_MAN_ACTIVE_OPTIONS;
  readonly itemsPerPage = 10;

  private readonly deliveryManSearch$ = new Subject<string>();
  private sub = new SubSink();

  constructor(
    private router: Router,
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
        switchMap((term) => this.fetchDeliveryManLookup(term, this.filterDraft.deliveryManActiveKey))
      )
      .subscribe((items) => {
        this.deliveryManLookupOptions = items.map((dm) => mapDeliveryManToSelectOption(dm));
      });
  }

  ngOnDestroy(): void {
    this.sub.unsubscribe();
  }

  onDeliveryManPanelOpen(): void {
    this.deliveryManLookupSearchTerm = '';
    this.deliveryManSearch$.next('');
  }

  onDeliveryManSearch(term: string): void {
    this.deliveryManLookupSearchTerm = term;
    this.deliveryManSearch$.next(term);
  }

  onDeliveryManActiveFilterChange(key: string): void {
    this.filterDraft.deliveryManActiveKey = key;
    this.filterDraft.deliveryManIds = [];
    this.selectedDeliveryMenCache = [];
    this.selectedStatKey = this.toStatKey(key);
    this.refreshDeliveryManLookupNow();
  }

  onStatCardClick(key: CaptainActiveStatKey): void {
    this.filterDraft.deliveryManActiveKey = key;
    this.selectedStatKey = key;
    this.searchCaptains();
  }

  private refreshDeliveryManLookupNow(): void {
    this.sub.sink = this.fetchDeliveryManLookup(
      this.deliveryManLookupSearchTerm,
      this.filterDraft.deliveryManActiveKey
    ).subscribe((items) => {
      this.deliveryManLookupOptions = items.map((dm) => mapDeliveryManToSelectOption(dm));
    });
  }

  private fetchDeliveryManLookup(term: string, activeKey: string) {
    this.isLoadingDeliveryMenLookup = true;
    const searchTerm = term?.trim() || undefined;
    const activeFilter = resolveDeliveryManActiveFilter(activeKey);

    return this.deliveryManClient.getAvailableDeliveryMenLookup(searchTerm ?? null, activeFilter ?? null).pipe(
      catchError(() => {
        this.toasterService.error('خطأ', 'تعذر تحميل قائمة المناديب');
        return of([]);
      }),
      finalize(() => { this.isLoadingDeliveryMenLookup = false; })
    );
  }

  loadStatistics(): void {
    this.isLoadingStatistics = true;
    this.sub.sink = this.deliveryManClient.getDeliveryManStatistics()
      .pipe(catchError(() => of(null)), finalize(() => { this.isLoadingStatistics = false; }))
      .subscribe((data) => { this.statistics = data; });
  }

  searchCaptains(): void {
    this.filterApplied = cloneCaptainsListFilter(this.filterDraft);
    this.selectedStatKey = this.toStatKey(this.filterApplied.deliveryManActiveKey);
    this.captainPage = 1;
    this.hasSearchedCaptains = true;
    this.loadDeliveryMen();
  }

  resetCaptainSearch(): void {
    this.filterDraft = { ...EMPTY_CAPTAINS_LIST_FILTER };
    this.selectedDeliveryMenCache = [];
    this.deliveryManLookupOptions = [];
    this.selectedStatKey = 'all';
    this.searchCaptains();
  }

  loadDeliveryMen(): void {
    this.isLoadingCaptains = true;
    const skip = (this.captainPage - 1) * this.itemsPerPage;
    const term = this.filterApplied.searchTerm?.trim() || undefined;

    this.sub.sink = this.deliveryManClient.getAllDeliveryMen(
      resolveDeliveryManFilters(this.filterApplied.deliveryManIds) ?? null,
      skip,
      this.itemsPerPage,
      term ?? null,
      resolveDeliveryManActiveFilter(this.filterApplied.deliveryManActiveKey) ?? null,
      parseDateFilter(this.filterApplied.fromDate) ?? null,
      parseDateFilter(this.filterApplied.toDate) ?? null
    ).pipe(
      catchError(() => {
        const empty = new PagedResultOfGetAllDeliveryMenDto();
        empty.data = [];
        empty.totalCount = 0;
        empty.totalPages = 0;
        return of(empty);
      }),
      finalize(() => { this.isLoadingCaptains = false; })
    ).subscribe((response) => {
      this.deliveryMen = response?.data ?? [];
      this.captainTotalCount = response?.totalCount ?? 0;
      this.captainTotalPages = response?.totalPages ?? 0;
    });
  }

  exportCaptains(): void {
    if (!this.hasSearchedCaptains) {
      this.searchCaptains();
    }

    this.isExporting = true;
    const term = this.filterApplied.searchTerm?.trim() || undefined;

    this.sub.sink = this.deliveryManClient.exportAllDeliveryMen(
      resolveDeliveryManFilters(this.filterApplied.deliveryManIds) ?? null,
      term ?? null,
      resolveDeliveryManActiveFilter(this.filterApplied.deliveryManActiveKey) ?? null,
      parseDateFilter(this.filterApplied.fromDate) ?? null,
      parseDateFilter(this.filterApplied.toDate) ?? null
    ).pipe(
      map((file) => mapFileResponse(file, `DeliveryMen_${Date.now()}.xlsx`)),
      catchError(() => {
        this.toasterService.error('خطأ', 'تعذر تصدير البيانات');
        return of(null);
      }),
      finalize(() => { this.isExporting = false; })
    ).subscribe((result) => {
      if (!result) return;
      triggerFileDownload(result.blob, result.fileName);
      this.toasterService.success('تم', 'تم تصدير المناديب بنجاح');
    });
  }

  changeCaptainPage(page: number): void {
    if (page >= 1 && page <= this.captainTotalPages) {
      this.captainPage = page;
      this.loadDeliveryMen();
    }
  }

  viewDeliveryManDetails(deliveryManId: number): void {
    this.router.navigate(['/admin/requests/controlCaptainRequest/requestTracking', deliveryManId]);
  }

  isStatCardSelected(key: CaptainActiveStatKey): boolean {
    return this.selectedStatKey === key;
  }

  private toStatKey(key: string): CaptainActiveStatKey {
    if (key === 'active' || key === 'inactive') return key;
    return 'all';
  }

  get captainVisiblePages(): (number | string)[] {
    return buildVisiblePages(this.captainPage, this.captainTotalPages);
  }

  get captainDisplayStart(): number {
    return this.captainTotalCount === 0 ? 0 : (this.captainPage - 1) * this.itemsPerPage + 1;
  }

  get captainDisplayEnd(): number {
    return Math.min(this.captainPage * this.itemsPerPage, this.captainTotalCount);
  }

  isPageNumber(page: number | string): boolean {
    return typeof page === 'number';
  }

  isPageEllipsis(page: number | string): boolean {
    return page === 'ellipsis';
  }
}
