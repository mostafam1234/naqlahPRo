import { NgClass, NgFor, NgIf, DecimalPipe } from '@angular/common';
import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormsModule, ReactiveFormsModule, FormControl } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { LanguageService } from 'src/app/Core/services/language.service';
import {
  DeliveryManAdminClient,
  GetAllDeliveryMenDto,
  DeliveryManStatisticsDto,
  PagedResultOfGetAllDeliveryMenDto
} from 'src/app/Core/services/NaqlahClient';
import { PageHeaderComponent } from 'src/app/shared/components/page-header/page-header.component';
import { catchError, finalize, debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { of } from 'rxjs';
import { SubSink } from 'subsink';

@Component({
  selector: 'app-control-captain-orders',
  standalone: true,
  imports: [NgFor, NgClass, NgIf, FormsModule, ReactiveFormsModule, RouterLink, PageHeaderComponent, TranslateModule, DecimalPipe],
  templateUrl: './control-captain-orders.component.html',
  styleUrl: './control-captain-orders.component.css'
})
export class ControlCaptainOrdersComponent implements OnInit, OnDestroy {
  lang: string = 'ar';
  
  // Statistics cards
  statistics: DeliveryManStatisticsDto | null = null;
  isLoadingStatistics = false;

  // Delivery Men Data
  deliveryMen: GetAllDeliveryMenDto[] = [];
  totalCount = 0;
  totalPages = 0;
  isLoading = false;
  searchControl = new FormControl('');

  currentPage = 1;
  itemsPerPage = 9;
  
  private sub = new SubSink();

  constructor(
    private languageService: LanguageService,
    private router: Router,
    private deliveryManClient: DeliveryManAdminClient
  ) {}

  get language() {
    return this.languageService.getLanguage();
  }

  ngOnInit(): void {
    this.loadStatistics();
    this.loadDeliveryMen();
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
        this.loadDeliveryMen();
      });
  }

  loadStatistics(): void {
    this.isLoadingStatistics = true;
    this.deliveryManClient.getDeliveryManStatistics()
      .pipe(
        catchError(error => {
          console.error('Error loading delivery man statistics:', error);
          return of(null);
        }),
        finalize(() => {
          this.isLoadingStatistics = false;
        })
      )
      .subscribe(data => {
        if (data) {
          this.statistics = data;
        }
      });
  }

  loadDeliveryMen(): void {
    this.isLoading = true;
    const skip = (this.currentPage - 1) * this.itemsPerPage;

    this.deliveryManClient.getAllDeliveryMen(
      skip,
      this.itemsPerPage,
      this.searchControl.value || undefined
    ).pipe(
      catchError(error => {
        console.error('Error loading delivery men:', error);
        const emptyResult = new PagedResultOfGetAllDeliveryMenDto();
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
        this.deliveryMen = response.data;
        this.totalCount = response.totalCount;
        this.totalPages = response.totalPages;
      } else {
        this.deliveryMen = [];
        this.totalCount = 0;
        this.totalPages = 0;
      }
    });
  }

  clearSearch(): void {
    this.searchControl.setValue('', { emitEvent: false });
    this.currentPage = 1;
    this.loadDeliveryMen();
  }

  changePage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.loadDeliveryMen();
    }
  }

  viewDeliveryManDetails(deliveryManId: number): void {
    this.router.navigate(['/admin/requests/controlCaptainRequest/requestTracking', deliveryManId]);
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
    
    if (this.totalPages <= maxVisible) {
      for (let i = 1; i <= this.totalPages; i++) {
        pages.push(i);
      }
    } else {
      if (this.currentPage <= 3) {
        for (let i = 1; i <= 4; i++) {
          pages.push(i);
        }
        pages.push('ellipsis');
        pages.push(this.totalPages);
      } else if (this.currentPage >= this.totalPages - 2) {
        pages.push(1);
        pages.push('ellipsis');
        for (let i = this.totalPages - 3; i <= this.totalPages; i++) {
          pages.push(i);
        }
      } else {
        pages.push(1);
        pages.push('ellipsis');
        for (let i = this.currentPage - 1; i <= this.currentPage + 1; i++) {
          pages.push(i);
        }
        pages.push('ellipsis');
        pages.push(this.totalPages);
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

  getImageUrl(imagePath: string): string {
    if (!imagePath) return 'assets/images/default-avatar.png';
    if (imagePath.startsWith('http')) return imagePath;
    return imagePath; // Adjust based on your image serving strategy
  }
}
