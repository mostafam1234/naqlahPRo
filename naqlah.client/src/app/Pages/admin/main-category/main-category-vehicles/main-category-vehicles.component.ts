import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { SubSink } from 'subsink';
import { catchError, finalize, map, of } from 'rxjs';
import { PageHeaderComponent } from 'src/app/shared/components/page-header/page-header.component';
import {
  MainCategoryAdminClient,
  MainCategoryVehicleTypeDto
} from 'src/app/Core/services/NaqlahClient';
import { ToasterService } from 'src/app/Core/services/toaster.service';
import { AppConfigService } from 'src/app/shared/services/AppConfigService';
import { triggerFileDownload } from '../../orders/captain-orders.helpers';

@Component({
  selector: 'app-main-category-vehicles',
  standalone: true,
  imports: [CommonModule, TranslateModule, PageHeaderComponent],
  providers: [MainCategoryAdminClient],
  templateUrl: './main-category-vehicles.component.html',
  styleUrls: ['./main-category-vehicles.component.css']
})
export class MainCategoryVehiclesComponent implements OnInit, OnDestroy {
  isLoading = false;
  isExporting = false;
  mainCategoryId = 0;
  categoryArabicName = '';
  categoryEnglishName = '';
  vehicleTypes: MainCategoryVehicleTypeDto[] = [];

  private sub = new SubSink();
  private baseUrl = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private mainCategoryClient: MainCategoryAdminClient,
    private http: HttpClient,
    private appConfig: AppConfigService,
    private toasterService: ToasterService,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.baseUrl = this.appConfig.Config?.apiBaseUrl || '';
    this.sub.sink = this.route.paramMap.subscribe(params => {
      const id = Number(params.get('id'));
      if (!id) {
        this.goBack();
        return;
      }

      this.mainCategoryId = id;
      this.loadVehicleTypes();
    });
  }

  ngOnDestroy(): void {
    this.sub.unsubscribe();
  }

  loadVehicleTypes(): void {
    this.isLoading = true;

    this.sub.sink = this.mainCategoryClient
      .getVehicleTypesByMainCategoryId(this.mainCategoryId)
      .subscribe({
        next: (response) => {
          this.categoryArabicName = response.mainCategoryArabicName ?? '';
          this.categoryEnglishName = response.mainCategoryEnglishName ?? '';
          this.vehicleTypes = response.vehicleTypes ?? [];
          this.isLoading = false;
        },
        error: () => {
          this.categoryArabicName = '';
          this.categoryEnglishName = '';
          this.vehicleTypes = [];
          this.isLoading = false;
        }
      });
  }

  exportLinkedVehicles(): void {
    this.isExporting = true;
    const fallbackFileName = `MainCategoryVehicles_${this.mainCategoryId}_${Date.now()}.xlsx`;

    this.sub.sink = this.http
      .get(`${this.baseUrl}/api/MainCategoryAdmin/ExportMainCategoryVehicleTypes`, {
        params: { mainCategoryId: this.mainCategoryId.toString() },
        observe: 'response',
        responseType: 'blob'
      })
      .pipe(
        map((response) => ({
          blob: response.body as Blob,
          fileName: this.extractFileName(response.headers.get('content-disposition')) || fallbackFileName
        })),
        catchError(() => {
          this.toasterService.error(
            this.translate.instant('COMMON.ERROR'),
            this.translate.instant('ADMIN.SHIPMENT_CATEGORIES.EXPORT_ERROR')
          );
          return of(null);
        }),
        finalize(() => {
          this.isExporting = false;
        })
      )
      .subscribe((result) => {
        if (!result?.blob) return;
        triggerFileDownload(result.blob, result.fileName);
        this.toasterService.success(
          this.translate.instant('COMMON.SUCCESS'),
          this.translate.instant('ADMIN.SHIPMENT_CATEGORIES.EXPORT_SUCCESS')
        );
      });
  }

  private extractFileName(contentDisposition: string | null): string | null {
    if (!contentDisposition) return null;

    const utf8Match = /filename\*=UTF-8''([^;\n]+)/i.exec(contentDisposition);
    if (utf8Match?.[1]) {
      return decodeURIComponent(utf8Match[1]);
    }

    const basicMatch = /filename="?([^";\n]+)"?/i.exec(contentDisposition);
    return basicMatch?.[1] ?? null;
  }

  get pageSubtitle(): string {
    const name = this.categoryArabicName || this.categoryEnglishName;
    return name ? `${name}` : '';
  }

  getLoadCategoryLabel(item: MainCategoryVehicleTypeDto): string {
    const ar = item.loadCategoryArabicName?.trim();
    const en = item.loadCategoryEnglishName?.trim();
    if (ar && en) {
      return `${ar} / ${en}`;
    }
    return ar || en || '—';
  }

  goBack(): void {
    this.router.navigate(['/admin/main-categories']);
  }
}
