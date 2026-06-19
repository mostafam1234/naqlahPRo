import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HttpClient, HttpParams } from '@angular/common/http';
import { SubSink } from 'subsink';
import { AppConfigService } from 'src/app/shared/services/AppConfigService';
import { ToasterService } from 'src/app/Core/services/toaster.service';
import { PageHeaderComponent } from 'src/app/shared/components/page-header/page-header.component';

interface BroadcastNotificationDto {
  id: number;
  arabicTitle: string;
  englishTitle: string;
  arabicMessage: string;
  englishMessage: string;
  notificationType: number;
  targetAll: boolean;
  targetCustomerType: number | null;
  isScheduled: boolean;
  scheduledDate: string | null;
  isProcessed: boolean;
  recipientCount: number;
  creationDate: string;
}

interface PagedResult<T> {
  data: T[];
  totalCount: number;
  totalPages: number;
}

@Component({
  selector: 'app-customer-notifications',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, PageHeaderComponent],
  templateUrl: './customer-notifications.component.html',
  styleUrls: ['./customer-notifications.component.css']
})
export class CustomerNotificationsComponent implements OnInit, OnDestroy {
  activeTab: 'compose' | 'history' = 'compose';

  // Compose form
  form: FormGroup;
  isSending = false;

  // History list
  isLoading = false;
  items: BroadcastNotificationDto[] = [];
  totalCount = 0;
  totalPages = 0;
  currentPage = 0;
  itemsPerPage = 10;
  scheduledOnlyFilter: boolean | null = null;

  private sub = new SubSink();
  private baseUrl = '';

  readonly notificationTypes = [
    { value: 12, label: 'عام' },
    { value: 11, label: 'رسالة مجدولة' },
    { value: 7, label: 'تعيين كابتن' },
    { value: 8, label: 'إلغاء طلب' },
    { value: 9, label: 'إكمال طلب' },
    { value: 10, label: 'تحديث حالة الطلب' },
  ];

  readonly customerTypes = [
    { value: null, label: 'الكل' },
    { value: 1, label: 'أفراد' },
    { value: 2, label: 'مؤسسات' },
  ];

  constructor(
    private fb: FormBuilder,
    private http: HttpClient,
    private appConfig: AppConfigService,
    private toaster: ToasterService
  ) {
    this.form = this.fb.group({
      arabicTitle: ['', [Validators.required, Validators.maxLength(200)]],
      englishTitle: ['', [Validators.required, Validators.maxLength(200)]],
      arabicMessage: ['', [Validators.required, Validators.maxLength(1000)]],
      englishMessage: ['', [Validators.required, Validators.maxLength(1000)]],
      notificationType: [12, Validators.required],
      targetAll: [true],
      targetCustomerType: [null],
      isScheduled: [false],
      scheduledDate: [null],
    });
  }

  ngOnInit(): void {
    this.baseUrl = this.appConfig.Config?.apiBaseUrl || '';
    this.loadHistory();
  }

  ngOnDestroy(): void {
    this.sub.unsubscribe();
  }

  switchTab(tab: 'compose' | 'history'): void {
    this.activeTab = tab;
    if (tab === 'history') this.loadHistory();
  }

  get isScheduled(): boolean {
    return this.form.get('isScheduled')?.value === true;
  }

  get targetAll(): boolean {
    return this.form.get('targetAll')?.value === true;
  }

  send(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const v = this.form.value;

    if (v.isScheduled && !v.scheduledDate) {
      this.toaster.error('خطأ', 'يرجى تحديد تاريخ الجدولة');
      return;
    }

    const body = {
      arabicTitle: v.arabicTitle,
      englishTitle: v.englishTitle,
      arabicMessage: v.arabicMessage,
      englishMessage: v.englishMessage,
      notificationType: Number(v.notificationType),
      targetAll: v.targetAll,
      targetCustomerType: v.targetAll ? null : v.targetCustomerType,
      customerIds: null,
      isScheduled: v.isScheduled,
      scheduledDate: v.isScheduled && v.scheduledDate ? new Date(v.scheduledDate).toISOString() : null,
    };

    this.isSending = true;
    this.sub.sink = this.http
      .post<number>(`${this.baseUrl}/api/NotificationAdmin/SendToCustomers`, body)
      .subscribe({
        next: () => {
          this.isSending = false;
          this.toaster.success('تم الإرسال', v.isScheduled ? 'تمت جدولة الإشعار بنجاح' : 'تم إرسال الإشعار بنجاح');
          this.form.reset({ targetAll: true, notificationType: 12, isScheduled: false });
          this.activeTab = 'history';
          this.loadHistory();
        },
        error: (err: any) => {
          this.isSending = false;
          this.toaster.error('خطأ', err?.error?.detail || err?.error || 'حدث خطأ أثناء الإرسال');
        }
      });
  }

  loadHistory(): void {
    this.isLoading = true;
    const skip = this.currentPage * this.itemsPerPage;
    let params = new HttpParams()
      .set('skip', skip.toString())
      .set('take', this.itemsPerPage.toString());

    if (this.scheduledOnlyFilter === true)
      params = params.set('scheduledOnly', 'true');

    this.sub.sink = this.http
      .get<PagedResult<BroadcastNotificationDto>>(
        `${this.baseUrl}/api/NotificationAdmin/GetBroadcastNotifications`,
        { params }
      )
      .subscribe({
        next: (res) => {
          this.items = res.data;
          this.totalCount = res.totalCount;
          this.totalPages = res.totalPages;
          this.isLoading = false;
        },
        error: () => { this.isLoading = false; }
      });
  }

  setScheduledFilter(value: boolean | null): void {
    this.scheduledOnlyFilter = value;
    this.currentPage = 0;
    this.loadHistory();
  }

  changePage(page: number): void {
    const backendPage = page - 1;
    if (backendPage >= 0 && backendPage < this.totalPages) {
      this.currentPage = backendPage;
      this.loadHistory();
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

  targetLabel(item: BroadcastNotificationDto): string {
    if (item.targetAll) {
      if (item.targetCustomerType === 1) return 'جميع الأفراد';
      if (item.targetCustomerType === 2) return 'جميع المؤسسات';
      return 'جميع العملاء';
    }
    return `${item.recipientCount} عميل محدد`;
  }

  statusBadge(item: BroadcastNotificationDto): { label: string; cls: string } {
    if (!item.isScheduled)
      return { label: 'تم الإرسال', cls: 'bg-green-100 text-green-700' };
    if (!item.isProcessed)
      return { label: 'مجدول', cls: 'bg-amber-100 text-amber-700' };
    return { label: 'أُرسل (مجدول)', cls: 'bg-teal-100 text-teal-700' };
  }

  typeName(type: number): string {
    return this.notificationTypes.find(t => t.value === type)?.label ?? type.toString();
  }

  hasError(field: string): boolean {
    const ctrl = this.form.get(field);
    return !!(ctrl && ctrl.invalid && ctrl.touched);
  }

  goBack(): void {
    window.history.back();
  }
}
