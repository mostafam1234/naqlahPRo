import { NgClass, NgFor, NgIf } from '@angular/common';
import { Component, HostListener, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { LanguageService } from 'src/app/Core/services/language.service';
import { PageHeaderComponent } from 'src/app/shared/components/page-header/page-header.component';
import { 
  DeliveryManAdminClient, 
  GetDeliveryManRequestDetailsDto,
  GetAllOrdersDto,
  PagedResultOfGetAllOrdersDto,
  OrderStatus
} from 'src/app/Core/services/NaqlahClient';
import { SubSink } from 'subsink';
import { DatePipe, DecimalPipe } from '@angular/common';

@Component({
  selector: 'app-captain-details',
  standalone: true,
  imports: [NgFor, NgClass, NgIf, TranslateModule, RouterModule, PageHeaderComponent, DatePipe, DecimalPipe],
  providers: [DeliveryManAdminClient, DatePipe, DecimalPipe],
  templateUrl: './captain-details.component.html',
  styleUrl: './captain-details.component.css'
})
export class CaptainDetailsComponent implements OnInit, OnDestroy {
  activeTab = 'all';
  currentPage = 1;
  itemsPerPage = 9;
  openDropdownId: number = null;
  lang: string = 'ar';
  viewMode: 'cards' | 'table' = 'table';
  
  // Captain data
  captainDetails: GetDeliveryManRequestDetailsDto | null = null;
  captainName: string = '';
  captainImage: string | null = null;
  deliveryType: string = '';
  deliveryManId: number = 0;
  
  // Orders data
  orders: GetAllOrdersDto[] = [];
  totalCount: number = 0;
  totalPages: number = 0;
  isLoading = false;
  isLoadingOrders = false;
  
  private subs = new SubSink();
  
  constructor(
    private languageService: LanguageService,
    private translateService: TranslateService,
    private router: Router,
    private route: ActivatedRoute,
    private deliveryManClient: DeliveryManAdminClient,
    private datePipe: DatePipe
  ) {}
  
  get language(){
    return this.languageService.getLanguage();
  }

  ngOnInit() {
    // Get deliveryManId from route params
    this.subs.sink = this.route.params.subscribe(params => {
      this.deliveryManId = +params['id'];
      if (this.deliveryManId) {
        this.loadCaptainDetails();
      }
    });

    // Initialize language
    this.lang = this.languageService.getLanguage();
  }

  ngOnDestroy() {
    this.subs.unsubscribe();
  }

  loadCaptainDetails() {
    if (!this.deliveryManId) return;
    
    this.isLoading = true;
    this.subs.sink = this.deliveryManClient
      .getDeliveryManDetails(this.deliveryManId)
      .subscribe({
        next: (response) => {
          this.captainDetails = response;
          this.captainName = response.fullName || 'غير متوفر';
          this.captainImage = response.personalImagePath || null;
          this.deliveryType = this.getDeliveryTypeLabel(response.deliveryType);
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Error loading captain details:', error);
          this.isLoading = false;
        }
      });
  }


  getDeliveryTypeLabel(deliveryType: string): string {
    if (!deliveryType) return 'غير متوفر';
    const type = deliveryType.toLowerCase();
    if (type.includes('resident') || type.includes('مقيم')) return 'مقيم';
    if (type.includes('citizen') || type.includes('مواطن')) return 'مواطن';
    return deliveryType;
  }

  getDeliveryLicenseTypeLabel(licenseType: string): string {
    if (!licenseType) return 'غير متوفر';
    const type = licenseType.toLowerCase();
    if (type.includes('public') || type.includes('عامة')) return 'رخصة عامة';
    if (type.includes('private') || type.includes('خاصة')) return 'رخصة خاصة';
    return licenseType;
  }

  formatDate(date: Date | string | null): string {
    if (!date) return 'غير متوفر';
    const dateObj = typeof date === 'string' ? new Date(date) : date;
    return this.datePipe.transform(dateObj, 'dd/MM/yyyy') || 'غير متوفر';
  }


  isIdentityValid(): boolean {
    if (!this.captainDetails || !this.captainDetails.identityExpirationDate) return false;
    const expirationDate = new Date(this.captainDetails.identityExpirationDate);
    return expirationDate > new Date();
  }

  isLicenseValid(): boolean {
    if (!this.captainDetails || !this.captainDetails.drivingLicenseExpirationDate) return false;
    const expirationDate = new Date(this.captainDetails.drivingLicenseExpirationDate);
    return expirationDate > new Date();
  }

  goBack(): void {
    window.history.back();
  }

  toggleDropdown(index: number) {
    this.openDropdownId = this.openDropdownId === index ? null : index;
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    const target = event.target as HTMLElement;
    if (!target.closest('.custom-card')) {
      this.openDropdownId = null;
    }
  }
}