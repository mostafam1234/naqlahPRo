import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl, FormsModule } from '@angular/forms';
import { HttpClient, HttpParams } from '@angular/common/http';
import { TranslateModule } from '@ngx-translate/core';
import { SubSink } from 'subsink';
import { AppConfigService } from 'src/app/shared/services/AppConfigService';

interface OrderRatingAdminDto {
  id: number;
  orderId: number;
  orderNumber: string;
  customerName: string;
  deliveryManName: string | null;
  rating: number;
  comment: string | null;
  createdDate: string;
}

interface PagedResult<T> {
  data: T[];
  totalCount: number;
  totalPages: number;
}

@Component({
  selector: 'app-order-ratings',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, TranslateModule],
  templateUrl: './order-ratings.component.html',
  styleUrls: ['./order-ratings.component.css']
})
export class OrderRatingsComponent implements OnInit, OnDestroy {
  isLoading = false;
  items: OrderRatingAdminDto[] = [];

  totalCount = 0;
  totalPages = 0;
  currentPage = 0;
  itemsPerPage = 10;

  fromDate = '';
  toDate = '';
  selectedRating: number | null = null;

  private sub = new SubSink();
  private baseUrl = '';

  constructor(
    private http: HttpClient,
    private appConfig: AppConfigService
  ) {}

  ngOnInit(): void {
    this.baseUrl = this.appConfig.Config?.apiBaseUrl || '';
    this.loadItems();
  }

  ngOnDestroy(): void {
    this.sub.unsubscribe();
  }

  loadItems(): void {
    this.isLoading = true;
    const skip = this.currentPage * this.itemsPerPage;

    let params = new HttpParams()
      .set('skip', skip.toString())
      .set('take', this.itemsPerPage.toString());

    if (this.fromDate) params = params.set('fromDate', this.fromDate);
    if (this.toDate)   params = params.set('toDate', this.toDate);
    if (this.selectedRating !== null) {
      params = params.set('minRating', this.selectedRating.toString());
      params = params.set('maxRating', this.selectedRating.toString());
    }

    this.sub.sink = this.http
      .get<PagedResult<OrderRatingAdminDto>>(
        `${this.baseUrl}/api/OrderAdmin/GetOrderRatings`,
        { params }
      )
      .subscribe({
        next: (res) => {
          this.items = res.data;
          this.totalCount = res.totalCount;
          this.totalPages = res.totalPages;
          this.isLoading = false;
        },
        error: () => {
          this.isLoading = false;
        }
      });
  }

  setRatingFilter(value: number | null): void {
    this.selectedRating = value;
    this.currentPage = 0;
    this.loadItems();
  }

  applyDateFilter(): void {
    this.currentPage = 0;
    this.loadItems();
  }

  clearFilters(): void {
    this.fromDate = '';
    this.toDate = '';
    this.selectedRating = null;
    this.currentPage = 0;
    this.loadItems();
  }

  changePage(page: number): void {
    const backendPage = page - 1;
    if (backendPage >= 0 && backendPage < this.totalPages) {
      this.currentPage = backendPage;
      this.loadItems();
    }
  }

  get displayCurrentPage(): number { return this.currentPage + 1; }

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

  starsArray(rating: number): number[] {
    return Array.from({ length: 5 }, (_, i) => i + 1);
  }

  ratingLabel(rating: number): string {
    const labels: Record<number, string> = {
      1: 'سيئ جداً',
      2: 'سيئ',
      3: 'مقبول',
      4: 'جيد',
      5: 'ممتاز'
    };
    return labels[rating] ?? '';
  }

  ratingBadgeClass(rating: number): string {
    if (rating >= 5) return 'bg-green-100 text-green-700';
    if (rating >= 4) return 'bg-teal-100 text-teal-700';
    if (rating >= 3) return 'bg-yellow-100 text-yellow-700';
    if (rating >= 2) return 'bg-orange-100 text-orange-700';
    return 'bg-red-100 text-red-700';
  }

  readonly ratingOptions = [1, 2, 3, 4, 5];
}
