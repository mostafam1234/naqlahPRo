import { DeliveryManAdminClient, GetAllDeliveryMenRequestsDto } from './../../../../Core/services/NaqlahClient';
import { NgClass, NgFor, NgIf } from '@angular/common';
import { Component, HostListener } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { LanguageService } from 'src/app/Core/services/language.service';
import { SubSink } from 'subsink';
import { PageHeaderComponent } from 'src/app/shared/components/page-header/page-header.component';
import { DeliveryType } from 'src/app/Core/enums/delivery.enums';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';

@Component({
  selector: 'app-new-captain',
  standalone: true,
  imports: [NgFor, NgClass, NgIf, TranslateModule, RouterModule, PageHeaderComponent, ReactiveFormsModule],
  providers: [],
  templateUrl: './new-captain.component.html',
  styleUrl: './new-captain.component.css'
})
export class NewCaptainComponent {
  lang: string = 'ar';
  viewMode: 'cards' | 'table' = 'table';
  allDeliveryMenRequests: GetAllDeliveryMenRequestsDto[] = [];
  isLoading = false;
  totalCount: number = 0;
  totalPages: number = 0;
  currentPage: number = 0;
  itemsPerPage: number = 10;
  
  // Search functionality
  searchControl = new FormControl('');
  searchTerm: string = '';

  private sub = new SubSink();

  constructor(
    private languageService: LanguageService,
    private translateService: TranslateService,
    private router: Router,
    private deliveryManClient: DeliveryManAdminClient
  ) {}

  activeTab: DeliveryType = DeliveryType.All;
  openDropdownId: number = null;

  // Expose DeliveryType enum to template
  DeliveryType = DeliveryType;

  get language(){
    return this.languageService.getLanguage();
  }

  ngOnInit(){
    this.GetAllDeliveryMenRequests();
    
    // Setup search with debounce
    this.sub.sink = this.searchControl.valueChanges
      .pipe(
        debounceTime(500), // انتظار 500ms بعد آخر كتابة
        distinctUntilChanged() // عدم البحث إذا كانت القيمة نفسها
      )
      .subscribe(value => {
        this.searchTerm = value || '';
        this.currentPage = 0; // إعادة تعيين الصفحة للصفحة الأولى
        this.GetAllDeliveryMenRequests();
      });
  }

  GetAllDeliveryMenRequests(){
    this.isLoading = true;
    const skip = this.currentPage * this.itemsPerPage;

    this.sub.sink = this.deliveryManClient
      .getAllDeliveryMenRequests(skip, this.itemsPerPage, this.activeTab, this.searchTerm)
      .subscribe({
        next: (response: any) => {
          if (response.data && response.totalCount !== undefined) {
            this.allDeliveryMenRequests = response.data;
            this.totalCount = response.totalCount;
            this.totalPages = response.totalPages || Math.ceil(this.totalCount / this.itemsPerPage);
          } else {
            this.allDeliveryMenRequests = response;
            this.totalCount = response.length;
            this.totalPages = Math.ceil(this.totalCount / this.itemsPerPage);
          }
          this.isLoading = false;
        },
        error: (error) => {
          console.error('خطأ في جلب البيانات:', error);
          this.isLoading = false;
        }
      });
  }

  changePage(page: number) {
    const backendPage = page - 1;
    if (backendPage >= 0 && backendPage < this.totalPages) {
      this.currentPage = backendPage;
      this.GetAllDeliveryMenRequests();
    }
  }

  setActiveTab(tab: DeliveryType) {
    this.activeTab = tab;
    this.currentPage = 0; // Reset to first page when changing filter
    this.GetAllDeliveryMenRequests(); // Call API with new filter
  }

  get displayCurrentPage(): number {
    return this.currentPage + 1;
  }

  get visiblePages(): number[] {
    const current = this.displayCurrentPage;
    const total = this.totalPages;
    const pages: number[] = [];

    if (total <= 7) {
      for (let i = 1; i <= total; i++) {
        pages.push(i);
      }
    } else {
      pages.push(1);

      if (current <= 4) {
        for (let i = 2; i <= 5; i++) {
          pages.push(i);
        }
        pages.push(-1);
        pages.push(total);
      } else if (current >= total - 3) {
        pages.push(-1);
        for (let i = total - 4; i <= total; i++) {
          pages.push(i);
        }
      } else {

        pages.push(-1);
        for (let i = current - 1; i <= current + 1; i++) {
          pages.push(i);
        }
        pages.push(-1);
        pages.push(total);
      }
    }

    return pages;
  }

  toggleDropdown(index: number) {
    this.openDropdownId = this.openDropdownId === index ? null : index;
  }

  editCaptain(captain: any) {
    console.log('تعديل الكابتن:', captain);
    this.openDropdownId = null;
  }

  deleteCaptain(captain: any) {
    console.log('حذف الكابتن:', captain);
    this.openDropdownId = null;
  }

  previewCaptain(captain: any) {
    console.log('معاينة الكابتن:', captain);
    this.router.navigate(['/admin/newCaptain/action', captain.deliveryManId]);
    this.openDropdownId = null;
  }

  viewCaptainDetails(deliveryManId: number) {
    this.router.navigate(['/admin/newCaptain/action', deliveryManId]);
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    const target = event.target as HTMLElement;
    if (!target.closest('.custom-card')) {
      this.openDropdownId = null;
    }
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

  getRowNumber(index: number): number {
    return (this.currentPage * this.itemsPerPage) + index + 1;
  }

  getAbsoluteDifference(a: number, b: number): number {
    return Math.abs(a - b);
  }

  goBack(): void {
    window.history.back();
  }

  ngOnDestroy() {
    this.sub.unsubscribe();
  }
}
