import { Component, OnInit, OnDestroy, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subscription } from 'rxjs';
import Swal from 'sweetalert2';
import { PageHeaderComponent } from 'src/app/shared/components/page-header/page-header.component';
import * as L from 'leaflet';

interface OrderDetails {
  number: string;
  status: 'pending' | 'assigned' | 'in-progress' | 'completed' | 'cancelled';
  orderType: 'delivery' | 'pickup' | 'round-trip';
  createdAt: Date;
  from: string;
  fromAddress: string;
  fromCoordinates: { lat: number; lng: number; };
  to: string;
  toAddress: string;
  toCoordinates: { lat: number; lng: number; };
  distance: number;
  pickupTime: Date;
  deliveryTime: Date;
  customerName: string;
  customerPhone: string;
  customerType: 'individual' | 'institution';
  totalAmount: number;
  
  // Order Package Information
  orderPackage: {
    arabicDescription: string;
    englishDescription: string;
    minWeightInKg: number;
    maxWeightInKg: number;
  };
  
  // Order Categories/Items
  orderItems: Array<{
    id: number;
    arabicCategoryName: string;
    englishCategoryName: string;
  }>;
  
  // Order Services
  orderServices: Array<{
    id: number;
    arabicName: string;
    englishName: string;
    price: number;
  }>;
  
  // Payment Information
  paymentInfo: {
    paymentMethodId: number;
    paymentMethodName: string;
    amount: number;
    paymentStatus: 'pending' | 'paid' | 'failed' | 'refunded';
  };
  
  captain: {
    name: string;
    phone: string;
    vehicleType: string;
    vehiclePlate: string;
  };
  beforeLoadingPhoto?: string;
  afterDeliveryPhoto?: string;
  lastLocationUpdate?: Date;
}

interface StatusHistoryEvent {
  status: 'pending' | 'assigned' | 'in-progress' | 'completed' | 'cancelled';
  timestamp: Date;
  description: string;
}

interface TimelineEvent {
  title: string;
  description: string;
  timestamp: Date;
  status: 'completed' | 'current' | 'pending';
}

@Component({
  selector: 'app-order-details',
  standalone: true,
  imports: [CommonModule, TranslateModule, PageHeaderComponent],
  templateUrl: './order-details.component.html',
  styleUrls: ['./order-details.component.css']
})
export class OrderDetailsComponent implements OnInit, OnDestroy, AfterViewInit {
  orderDetails: OrderDetails | null = null;
  orderTimeline: TimelineEvent[] = [];
  isLoading = true;
  orderId: string = '';
  private map?: L.Map;
  
  private routeSubscription?: Subscription;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.routeSubscription = this.route.params.subscribe(params => {
      this.orderId = params['id'];
      this.loadOrderDetails();
    });
  }

  ngOnDestroy(): void {
    if (this.routeSubscription) {
      this.routeSubscription.unsubscribe();
    }
  }

  private loadOrderDetails(): void {
    this.isLoading = true;
    
    // Simulate API call - replace with actual service call
    setTimeout(() => {
      this.orderDetails = {
        number: this.orderId,
        status: 'assigned',
        orderType: 'delivery',
        createdAt: new Date('2024-01-15T10:30:00'),
        from: 'الرياض',
        fromAddress: 'شارع الملك فهد، حي العليا، الرياض',
        fromCoordinates: { lat: 24.7136, lng: 46.6753 }, // Riyadh coordinates
        to: 'جدة',
        toAddress: 'شارع الأمير سلطان، حي الشرفية، جدة',
        toCoordinates: { lat: 21.5433, lng: 39.1728 }, // Jeddah coordinates
        distance: 950,
        pickupTime: new Date('2024-01-15T14:00:00'),
        deliveryTime: new Date('2024-01-16T08:00:00'),
        customerName: 'أحمد محمد السالم',
        customerPhone: '+966501234567',
        customerType: 'individual',
        totalAmount: 1250,
        
        // Order Package Information
        orderPackage: {
          arabicDescription: 'حزمة التوصيل السريع - حتى 50 كيلوجرام',
          englishDescription: 'Express Delivery Package - Up to 50 KG',
          minWeightInKg: 1,
          maxWeightInKg: 50
        },
        
        // Order Categories/Items
        orderItems: [
          {
            id: 1,
            arabicCategoryName: 'إلكترونيات',
            englishCategoryName: 'Electronics'
          },
          {
            id: 2,
            arabicCategoryName: 'ملابس',
            englishCategoryName: 'Clothing'
          }
        ],
        
        // Order Services
        orderServices: [
          {
            id: 1,
            arabicName: 'توصيل سريع',
            englishName: 'Express Delivery',
            price: 50
          },
          {
            id: 2,
            arabicName: 'تأمين الشحنة',
            englishName: 'Cargo Insurance',
            price: 25
          },
          {
            id: 3,
            arabicName: 'التعامل مع العناصر الهشة',
            englishName: 'Fragile Item Handling',
            price: 30
          }
        ],
        
        // Payment Information
        paymentInfo: {
          paymentMethodId: 1,
          paymentMethodName: 'بطاقة ائتمان',
          amount: 1250,
          paymentStatus: 'paid'
        },
        
        captain: {
          name: 'محمد أحمد العلي',
          phone: '+966507654321',
          vehicleType: 'شاحنة صغيرة',
          vehiclePlate: 'أ ب ج 1234'
        },
        beforeLoadingPhoto: './assets/images/sample-before-loading.jpg',
        lastLocationUpdate: new Date()
      };

      this.generateTimeline();
      this.isLoading = false;
      this.initializeMap();
    }, 1000);
  }

  private generateTimeline(): void {
    if (!this.orderDetails) return;

    this.orderTimeline = [
      {
        title: this.translate.instant('ADMIN.PAGES.ORDERS.DETAILS.TIMELINE.ORDER_CREATED'),
        description: this.translate.instant('ADMIN.PAGES.ORDERS.DETAILS.TIMELINE.ORDER_CREATED_DESC'),
        timestamp: this.orderDetails.createdAt,
        status: 'completed'
      },
      {
        title: this.translate.instant('ADMIN.PAGES.ORDERS.DETAILS.TIMELINE.CAPTAIN_ASSIGNED'),
        description: this.translate.instant('ADMIN.PAGES.ORDERS.DETAILS.TIMELINE.CAPTAIN_ASSIGNED_DESC', { name: this.orderDetails.captain.name }),
        timestamp: new Date(this.orderDetails.createdAt.getTime() + 15 * 60 * 1000), // 15 minutes later
        status: 'completed'
      },
      {
        title: this.translate.instant('ADMIN.PAGES.ORDERS.DETAILS.TIMELINE.EN_ROUTE_PICKUP'),
        description: this.translate.instant('ADMIN.PAGES.ORDERS.DETAILS.TIMELINE.EN_ROUTE_PICKUP_DESC'),
        timestamp: new Date(this.orderDetails.createdAt.getTime() + 30 * 60 * 1000), // 30 minutes later
        status: this.orderDetails.status === 'in-progress' ? 'current' : 'completed'
      },
      {
        title: this.translate.instant('ADMIN.PAGES.ORDERS.DETAILS.TIMELINE.PICKUP_COMPLETED'),
        description: this.translate.instant('ADMIN.PAGES.ORDERS.DETAILS.TIMELINE.PICKUP_COMPLETED_DESC'),
        timestamp: this.orderDetails.pickupTime,
        status: this.orderDetails.status === 'completed' ? 'completed' : 'pending'
      },
      {
        title: this.translate.instant('ADMIN.PAGES.ORDERS.DETAILS.TIMELINE.DELIVERY_COMPLETED'),
        description: this.translate.instant('ADMIN.PAGES.ORDERS.DETAILS.TIMELINE.DELIVERY_COMPLETED_DESC'),
        timestamp: this.orderDetails.deliveryTime,
        status: this.orderDetails.status === 'completed' ? 'completed' : 'pending'
      }
    ];
  }

  getStatusText(status?: string): string {
    if (!status) return '';
    
    const statusMap: { [key: string]: string } = {
      'pending': this.translate.instant('ADMIN.PAGES.ORDERS.TABS.PENDING'),
      'in-progress': this.translate.instant('ADMIN.PAGES.ORDERS.TABS.IN_PROGRESS'),
      'completed': this.translate.instant('ADMIN.PAGES.ORDERS.TABS.COMPLETED'),
      'cancelled': this.translate.instant('ADMIN.PAGES.ORDERS.TABS.CANCELLED')
    };
    
    return statusMap[status] || status;
  }

  getCustomerTypeText(type?: string): string {
    if (!type) return '';
    
    const typeMap: { [key: string]: string } = {
      'individual': this.translate.instant('ADMIN.PAGES.ORDERS.DETAILS.CUSTOMER_TYPE.INDIVIDUAL'),
      'institution': this.translate.instant('ADMIN.PAGES.ORDERS.DETAILS.CUSTOMER_TYPE.INSTITUTION')
    };
    
    return typeMap[type] || type;
  }

  getPaymentStatusText(status?: string): string {
    if (!status) return '';
    
    const statusMap: { [key: string]: string } = {
      'pending': this.translate.instant('ADMIN.PAGES.ORDERS.DETAILS.PAYMENT_STATUS.PENDING'),
      'paid': this.translate.instant('ADMIN.PAGES.ORDERS.DETAILS.PAYMENT_STATUS.PAID'),
      'failed': this.translate.instant('ADMIN.PAGES.ORDERS.DETAILS.PAYMENT_STATUS.FAILED'),
      'refunded': this.translate.instant('ADMIN.PAGES.ORDERS.DETAILS.PAYMENT_STATUS.REFUNDED')
    };
    
    return statusMap[status] || status;
  }

  canCancelOrder(): boolean {
    return this.orderDetails?.status === 'pending' || this.orderDetails?.status === 'in-progress';
  }

  async cancelOrder(): Promise<void> {
    if (!this.canCancelOrder()) return;

    const result = await Swal.fire({
      title: this.translate.instant('ADMIN.PAGES.ORDERS.DETAILS.CANCEL_CONFIRM.TITLE'),
      text: this.translate.instant('ADMIN.PAGES.ORDERS.DETAILS.CANCEL_CONFIRM.TEXT'),
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#dc2626',
      cancelButtonColor: '#6b7280',
      confirmButtonText: this.translate.instant('ADMIN.PAGES.ORDERS.DETAILS.CANCEL_CONFIRM.CONFIRM'),
      cancelButtonText: this.translate.instant('ADMIN.PAGES.ORDERS.DETAILS.CANCEL_CONFIRM.CANCEL'),
      reverseButtons: true
    });

    if (result.isConfirmed) {
      try {
        // Simulate API call to cancel order
        // await this.orderService.cancelOrder(this.orderId);
        
        // Show success message
        await Swal.fire({
          title: this.translate.instant('ADMIN.PAGES.ORDERS.DETAILS.CANCEL_SUCCESS.TITLE'),
          text: this.translate.instant('ADMIN.PAGES.ORDERS.DETAILS.CANCEL_SUCCESS.TEXT'),
          icon: 'success',
          confirmButtonText: this.translate.instant('ADMIN.PAGES.ORDERS.DETAILS.CANCEL_SUCCESS.OK')
        });

        // Update order status
        if (this.orderDetails) {
          this.orderDetails.status = 'cancelled';
        }

        // Navigate back to orders list
        this.router.navigate(['/admin/orders']);
        
      } catch (error) {
        // Show error message
        await Swal.fire({
          title: this.translate.instant('ADMIN.PAGES.ORDERS.DETAILS.CANCEL_ERROR.TITLE'),
          text: this.translate.instant('ADMIN.PAGES.ORDERS.DETAILS.CANCEL_ERROR.TEXT'),
          icon: 'error',
          confirmButtonText: this.translate.instant('ADMIN.PAGES.ORDERS.DETAILS.CANCEL_ERROR.OK')
        });
      }
    }
  }

  viewOrderDetails(orderNumber: string): void {
    this.router.navigate(['/admin/orders/details', orderNumber]);
  }

  goBack(): void {
    this.router.navigate(['/admin/orders']);
  }

  ngAfterViewInit(): void {
    // Map will be initialized after data is loaded in loadOrderDetails
  }

  private initializeMap(): void {
    if (!this.orderDetails) return;

    // Initialize Leaflet map
    this.map = L.map('orderMap', {
      center: [
        (this.orderDetails.fromCoordinates.lat + this.orderDetails.toCoordinates.lat) / 2,
        (this.orderDetails.fromCoordinates.lng + this.orderDetails.toCoordinates.lng) / 2
      ],
      zoom: 6,
      attributionControl: true
    });

    // Add tile layer (OpenStreetMap)
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '© OpenStreetMap contributors'
    }).addTo(this.map);

    // Custom pickup icon (green)
    const pickupIcon = L.divIcon({
      className: 'custom-pickup-marker',
      html: `<div class="bg-green-500 text-white rounded-full w-8 h-8 flex items-center justify-center text-sm font-bold shadow-lg">
               <i class="fas fa-arrow-up"></i>
             </div>`,
      iconSize: [32, 32],
      iconAnchor: [16, 16]
    });

    // Custom delivery icon (red)
    const deliveryIcon = L.divIcon({
      className: 'custom-delivery-marker',
      html: `<div class="bg-red-500 text-white rounded-full w-8 h-8 flex items-center justify-center text-sm font-bold shadow-lg">
               <i class="fas fa-arrow-down"></i>
             </div>`,
      iconSize: [32, 32],
      iconAnchor: [16, 16]
    });

    // Add pickup marker
    const pickupMarker = L.marker(
      [this.orderDetails.fromCoordinates.lat, this.orderDetails.fromCoordinates.lng],
      { icon: pickupIcon }
    ).addTo(this.map);

    // Add delivery marker
    const deliveryMarker = L.marker(
      [this.orderDetails.toCoordinates.lat, this.orderDetails.toCoordinates.lng],
      { icon: deliveryIcon }
    ).addTo(this.map);

    // Add popups with location info
    pickupMarker.bindPopup(`
      <div class="text-sm">
        <div class="font-bold text-green-600 mb-1">${this.translate.instant('ADMIN.PAGES.ORDERS.DETAILS.MAP.PICKUP_LOCATION')}</div>
        <div class="font-semibold">${this.orderDetails.from}</div>
        <div class="text-gray-600 text-xs">${this.orderDetails.fromAddress}</div>
      </div>
    `);

    deliveryMarker.bindPopup(`
      <div class="text-sm">
        <div class="font-bold text-red-600 mb-1">${this.translate.instant('ADMIN.PAGES.ORDERS.DETAILS.MAP.DELIVERY_LOCATION')}</div>
        <div class="font-semibold">${this.orderDetails.to}</div>
        <div class="text-gray-600 text-xs">${this.orderDetails.toAddress}</div>
      </div>
    `);

    // Add route line between pickup and delivery
    const routeLine = L.polyline([
      [this.orderDetails.fromCoordinates.lat, this.orderDetails.fromCoordinates.lng],
      [this.orderDetails.toCoordinates.lat, this.orderDetails.toCoordinates.lng]
    ], {
      color: '#3b82f6',
      weight: 4,
      opacity: 0.8,
      dashArray: '10,10'
    }).addTo(this.map);

    // Add route popup with distance info
    const midpoint = [
      (this.orderDetails.fromCoordinates.lat + this.orderDetails.toCoordinates.lat) / 2,
      (this.orderDetails.fromCoordinates.lng + this.orderDetails.toCoordinates.lng) / 2
    ];

    L.circleMarker(midpoint as [number, number], {
      radius: 6,
      color: '#3b82f6',
      fillColor: '#dbeafe',
      fillOpacity: 0.8,
      weight: 2
    }).addTo(this.map).bindPopup(`
      <div class="text-sm text-center">
        <div class="font-bold text-blue-600">${this.translate.instant('ADMIN.PAGES.ORDERS.DETAILS.MAP.ROUTE_INFO')}</div>
        <div class="text-gray-600">${this.orderDetails.distance} ${this.translate.instant('ADMIN.PAGES.ORDERS.DETAILS.MAP.KILOMETERS')}</div>
      </div>
    `);

    // Fit map bounds to show both markers
    const group = new L.FeatureGroup([pickupMarker, deliveryMarker]);
    this.map.fitBounds(group.getBounds().pad(0.1));
  }
}