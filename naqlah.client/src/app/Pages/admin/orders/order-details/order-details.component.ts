import { Component, OnInit, OnDestroy, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subscription } from 'rxjs';
import * as L from 'leaflet';
import { PageHeaderComponent } from 'src/app/shared/components/page-header/page-header.component';
import { ToasterService } from 'src/app/Core/services/toaster.service';
import { ConfirmationModalComponent } from 'src/app/shared/components/confirmation-modal/confirmation-modal.component';
import {
  OrderAdminClient,
  GetOrderDetailsForAdminDto,
  OrderStatus,
  CustomerType,
  OrderType,
  AvailableDeliveryManDto,
  AssignOrderToDeliveryManRequest
} from 'src/app/Core/services/NaqlahClient';
import { catchError, finalize } from 'rxjs/operators';
import { of } from 'rxjs';

interface TimelineEvent {
  title: string;
  description: string;
  timestamp: Date;
  status: 'completed' | 'current' | 'pending';
}

@Component({
  selector: 'app-order-details',
  standalone: true,
  imports: [CommonModule, TranslateModule, PageHeaderComponent, ConfirmationModalComponent],
  templateUrl: './order-details.component.html',
  styleUrls: ['./order-details.component.css']
})
export class OrderDetailsComponent implements OnInit, OnDestroy, AfterViewInit {
  orderDetails: GetOrderDetailsForAdminDto | null = null;
  orderTimeline: TimelineEvent[] = [];
  isLoading = true;
  orderId: number = 0;
  private map: L.Map | null = null;

  // Confirmation modal properties
  showConfirmation = false;
  confirmationTitle = '';
  confirmationMessage = '';
  private pendingAction?: () => void;

  // Assignment modal properties
  showAssignModal = false;
  availableDeliveryMen: AvailableDeliveryManDto[] = [];
  selectedDeliveryManId: number | null = null;
  isLoadingDeliveryMen = false;

  private routeSubscription?: Subscription;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private translate: TranslateService,
    private orderClient: OrderAdminClient,
    private toasterService: ToasterService
  ) {}

  ngOnInit(): void {
    this.routeSubscription = this.route.params.subscribe(params => {
      this.orderId = +params['id']; // Convert to number
      this.loadOrderDetails();
    });
  }

  ngOnDestroy(): void {
    if (this.routeSubscription) {
      this.routeSubscription.unsubscribe();
    }
  }

  private loadOrderDetails(): void {
    if (!this.orderId) {
      console.error('No order ID provided');
      this.router.navigate(['/admin/orders']);
      return;
    }

    this.isLoading = true;

    this.orderClient.getOrderDetails(this.orderId)
      .pipe(
        catchError(error => {
          console.error('Error loading order details:', error);
          this.toasterService.error('خطأ', 'حدث خطأ في تحميل تفاصيل الطلب');
          return of(null);
        }),
        finalize(() => {
          this.isLoading = false;
        })
      )
      .subscribe(response => {
        if (response) {
          this.orderDetails = response;
          setTimeout(() => this.initializeMapWithRealData(), 500);
        } else {
          this.toasterService.error('خطأ', 'لم يتم العثور على الطلب');
          this.router.navigate(['/admin/orders']);
        }
      });
  }


  getStatusText(status?: OrderStatus): string {
    if (!status) return '';

    switch (status) {
      case OrderStatus.Pending:
        return this.translate.instant('ADMIN.PAGES.ORDERS.TABS.PENDING');
      case OrderStatus.Assigned:
        return this.translate.instant('ADMIN.PAGES.ORDERS.TABS.ASSIGNED');
      case OrderStatus.Completed:
        return this.translate.instant('ADMIN.PAGES.ORDERS.TABS.COMPLETED');
      case OrderStatus.Cancelled:
        return this.translate.instant('ADMIN.PAGES.ORDERS.TABS.CANCELLED');
      default:
        return 'غير معروف';
    }
  }

  getStatusColorClass(status?: OrderStatus): string {
    if (!status) return 'bg-gray-100 text-gray-800';

    switch (status) {
      case OrderStatus.Pending:
        return 'bg-yellow-100 text-yellow-800';
      case OrderStatus.Assigned:
        return 'bg-blue-100 text-blue-800';
      case OrderStatus.Completed:
        return 'bg-green-100 text-green-800';
      case OrderStatus.Cancelled:
        return 'bg-red-100 text-red-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  }

  canCancelOrder(): boolean {
    // Can cancel only pending orders
    return this.orderDetails?.status === OrderStatus.Pending;
  }

  canAssignOrder(): boolean {
    // Can assign only pending orders
    return this.orderDetails?.status === OrderStatus.Pending;
  }

  canCompleteOrder(): boolean {
    // Can complete if assigned
    return this.orderDetails?.status === OrderStatus.Assigned;
  }

  cancelOrder(): void {
    this.confirmationTitle = 'تأكيد إلغاء الطلب';
    this.confirmationMessage = 'هل أنت متأكد من إلغاء هذا الطلب؟';
    this.pendingAction = () => this.cancelOrderFromAdmin();
    this.showConfirmation = true;
  }

  cancelOrderFromAdmin(): void {
    this.orderClient.cancelOrder(this.orderId).subscribe({
      next: (res) => {
        this.toasterService.success('تم إلغاء الطلب', 'تم إلغاء الطلب بنجاح');
        this.loadOrderDetails();
      },
      error: (err) => {
        console.log('🔍 Full Error Object:', err);
        console.log('🔍 err.error:', err?.error);
        console.log('🔍 err.message:', err?.message);
        console.log('🔍 err.status:', err?.status);
        
        const errorMessage = err?.error?.error || err?.error || 'يمكن إلغاء الطلبات المعلقة فقط';
        this.toasterService.error('خطأ', errorMessage);
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/admin/orders']);
  }

  ngAfterViewInit(): void {
    console.log('ngAfterViewInit called');
    // Wait for DOM to be fully rendered
    setTimeout(() => {
      console.log('Attempting to initialize map with real data');
      this.initializeMapWithRealData();
    }, 500);
  }

  private initializeMapWithRealData(): void {
    console.log('Initializing map with real waypoints data');

    // Check if we have waypoints data from the backend
    if (!this.orderDetails?.wayPoints || this.orderDetails.wayPoints.length === 0) {
      console.log('No waypoints data available, using default map');
      return;
    }

    console.log('WayPoints from backend:', this.orderDetails.wayPoints);

    const mapContainer = document.getElementById('orderMap');
    if (!mapContainer) {
      console.error('Map container not found!');
      return;
    }

    try {
      // If map already exists, clear it
      if (this.map) {
        this.map.remove();
      }

      // Configure Leaflet default icon path
      delete (L.Icon.Default.prototype as any)._getIconUrl;
      L.Icon.Default.mergeOptions({
        iconRetinaUrl: 'assets/leaflet/images/marker-icon-2x.png',
        iconUrl: 'assets/leaflet/images/marker-icon.png',
        shadowUrl: 'assets/leaflet/images/marker-shadow.png'
      });

      // Get origin and destination from real data
      const originWayPoint = this.orderDetails.wayPoints.find(wp => wp.isOrigin);
      const destinationWayPoint = this.orderDetails.wayPoints.find(wp => wp.isDestination);

      console.log('Origin WayPoint:', originWayPoint);
      console.log('Destination WayPoint:', destinationWayPoint);

      if (!originWayPoint || !destinationWayPoint) {
          console.log('Origin or destination waypoint not found');
          return;
      }

      // Validate coordinates
      if (!originWayPoint.latitude || !originWayPoint.longitude ||
          !destinationWayPoint.latitude || !destinationWayPoint.longitude) {
        console.log('Invalid coordinates in waypoints');
        return;
      }

      // Calculate center point between origin and destination using REAL coordinates
      const centerLat = (originWayPoint.latitude + destinationWayPoint.latitude) / 2;
      const centerLng = (originWayPoint.longitude + destinationWayPoint.longitude) / 2;

      console.log(`Using real coordinates - Center: ${centerLat}, ${centerLng}`);
      console.log(`Origin: ${originWayPoint.latitude}, ${originWayPoint.longitude}`);
      console.log(`Destination: ${destinationWayPoint.latitude}, ${destinationWayPoint.longitude}`);

      // Initialize map with REAL coordinates from backend
      this.map = L.map('orderMap', {
        zoomControl: true,
        scrollWheelZoom: true,
        doubleClickZoom: true,
        attributionControl: true
      }).setView([centerLat, centerLng], 12);

      // Use beautiful tile layer
      L.tileLayer('https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png', {
        attribution: '© OpenStreetMap © CartoDB',
        subdomains: 'abcd',
        maxZoom: 19
      }).addTo(this.map);

      // Create custom icons
      const startIcon = L.divIcon({
        html: `
          <div style="
            background: linear-gradient(135deg, #10b981, #059669);
            width: 40px;
            height: 40px;
            border-radius: 50% 50% 50% 0;
            border: 3px solid white;
            box-shadow: 0 4px 8px rgba(0,0,0,0.3);
            display: flex;
            align-items: center;
            justify-content: center;
            color: white;
            font-weight: bold;
            font-size: 18px;
            transform: rotate(-45deg);
          ">
            <span style="transform: rotate(45deg);">🚀</span>
          </div>
        `,
        className: 'custom-div-icon',
        iconSize: [40, 40],
        iconAnchor: [20, 40]
      });

      const endIcon = L.divIcon({
        html: `
          <div style="
            background: linear-gradient(135deg, #dc2626, #b91c1c);
            width: 40px;
            height: 40px;
            border-radius: 50% 50% 50% 0;
            border: 3px solid white;
            box-shadow: 0 4px 8px rgba(0,0,0,0.3);
            display: flex;
            align-items: center;
            justify-content: center;
            color: white;
            font-weight: bold;
            font-size: 18px;
            transform: rotate(-45deg);
          ">
            <span style="transform: rotate(45deg);">🎯</span>
          </div>
        `,
        className: 'custom-div-icon',
        iconSize: [40, 40],
        iconAnchor: [20, 40]
      });

      // Add markers with REAL coordinates and addresses from backend
      const startMarker = L.marker([originWayPoint.latitude, originWayPoint.longitude], { icon: startIcon }).addTo(this.map);
      startMarker.bindPopup(`
        <div style="text-align: center; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; direction: rtl;">
          <div style="background: linear-gradient(135deg, #10b981, #059669); color: white; padding: 10px; border-radius: 8px 8px 0 0; margin: -10px -10px 10px -10px;">
            <strong>🚀 نقطة الاستلام</strong>
          </div>
          <div style="padding: 5px;">
            <p style="margin: 5px 0; color: #374151;"><strong>العنوان:</strong> ${originWayPoint.address || 'غير محدد'}</p>
            <p style="margin: 5px 0; color: #6b7280; font-size: 12px;">إحداثيات: ${originWayPoint.latitude.toFixed(4)}, ${originWayPoint.longitude.toFixed(4)}</p>
            ${originWayPoint.regionName ? '<p style="margin: 5px 0; color: #6b7280; font-size: 12px;">المنطقة: ' + originWayPoint.regionName + '</p>' : ''}
            ${originWayPoint.pickedUpDate ? '<p style="margin: 5px 0; color: #10b981; font-size: 12px;">✓ تم الاستلام</p>' : '<p style="margin: 5px 0; color: #f59e0b; font-size: 12px;">⏳ في الانتظار</p>'}
          </div>
        </div>
      `, {
        maxWidth: 300,
        className: 'custom-popup'
      });

      const endMarker = L.marker([destinationWayPoint.latitude, destinationWayPoint.longitude], { icon: endIcon }).addTo(this.map);
      endMarker.bindPopup(`
        <div style="text-align: center; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; direction: rtl;">
          <div style="background: linear-gradient(135deg, #dc2626, #b91c1c); color: white; padding: 10px; border-radius: 8px 8px 0 0; margin: -10px -10px 10px -10px;">
            <strong>🎯 نقطة التسليم</strong>
          </div>
          <div style="padding: 5px;">
            <p style="margin: 5px 0; color: #374151;"><strong>العنوان:</strong> ${destinationWayPoint.address || 'غير محدد'}</p>
            <p style="margin: 5px 0; color: #6b7280; font-size: 12px;">إحداثيات: ${destinationWayPoint.latitude.toFixed(4)}, ${destinationWayPoint.longitude.toFixed(4)}</p>
            ${destinationWayPoint.regionName ? '<p style="margin: 5px 0; color: #6b7280; font-size: 12px;">المنطقة: ' + destinationWayPoint.regionName + '</p>' : ''}
            ${destinationWayPoint.status === 3 ? '<p style="margin: 5px 0; color: #10b981; font-size: 12px;">✓ تم التسليم</p>' : '<p style="margin: 5px 0; color: #f59e0b; font-size: 12px;">⏳ في الانتظار</p>'}
          </div>
        </div>
      `, {
        maxWidth: 300,
        className: 'custom-popup'
      });

      // Create route from all waypoints in order using REAL coordinates
      const routeCoordinates: [number, number][] = [];

      // Add origin first
      if (originWayPoint.latitude && originWayPoint.longitude) {
        routeCoordinates.push([originWayPoint.latitude, originWayPoint.longitude]);
      }

      // Add intermediate waypoints
      const intermediateWayPoints = this.orderDetails.wayPoints.filter(wp => !wp.isOrigin && !wp.isDestination);
      intermediateWayPoints.forEach(wp => {
        if (wp.latitude && wp.longitude) {
          routeCoordinates.push([wp.latitude, wp.longitude]);
        }
      });

      // Add destination last
      if (destinationWayPoint.latitude && destinationWayPoint.longitude) {
        routeCoordinates.push([destinationWayPoint.latitude, destinationWayPoint.longitude]);
      }

      console.log('Route coordinates from backend:', routeCoordinates);

      // Draw the route using REAL coordinates
      if (routeCoordinates.length > 1) {
        const mainRoute = L.polyline(routeCoordinates, {
          color: '#3b82f6',
          weight: 6,
          opacity: 0.8,
          lineCap: 'round',
          lineJoin: 'round'
        }).addTo(this.map);

        // Add shadow effect
        L.polyline(routeCoordinates, {
          color: '#1e40af',
          weight: 8,
          opacity: 0.3,
          lineCap: 'round',
          lineJoin: 'round'
        }).addTo(this.map);
      }

      // Add intermediate waypoints if any using REAL coordinates
      intermediateWayPoints.forEach((wp, index) => {
        if (wp.latitude && wp.longitude) {
          const intermediateIcon = L.divIcon({
            html: `
              <div style="
                background: linear-gradient(135deg, #f59e0b, #d97706);
                width: 32px;
                height: 32px;
                border-radius: 50%;
                border: 2px solid white;
                box-shadow: 0 2px 6px rgba(0,0,0,0.3);
                display: flex;
                align-items: center;
                justify-content: center;
                color: white;
                font-weight: bold;
                font-size: 14px;
              ">
                ${index + 1}
              </div>
            `,
            className: 'intermediate-waypoint',
            iconSize: [32, 32],
            iconAnchor: [16, 16]
          });

          const intermediateMarker = L.marker([wp.latitude, wp.longitude], { icon: intermediateIcon }).addTo(this.map);
          intermediateMarker.bindPopup(`
            <div style="text-align: center; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; direction: rtl;">
              <div style="background: linear-gradient(135deg, #f59e0b, #d97706); color: white; padding: 8px; border-radius: 6px 6px 0 0; margin: -8px -8px 8px -8px;">
                <strong>📍 نقطة وسطية ${index + 1}</strong>
              </div>
              <div style="padding: 5px;">
                <p style="margin: 5px 0; color: #374151;"><strong>العنوان:</strong> ${wp.address || 'غير محدد'}</p>
                <p style="margin: 5px 0; color: #6b7280; font-size: 12px;">إحداثيات: ${wp.latitude.toFixed(4)}, ${wp.longitude.toFixed(4)}</p>
                ${wp.regionName ? '<p style="margin: 5px 0; color: #6b7280; font-size: 12px;">المنطقة: ' + wp.regionName + '</p>' : ''}
              </div>
            </div>
          `, {
            maxWidth: 280,
            className: 'custom-popup'
          });
        }
      });

      // Fit map to show all markers using REAL coordinates
      const allMarkers = [startMarker, endMarker];
      const group = L.featureGroup(allMarkers);
      this.map.fitBounds(group.getBounds(), {
        padding: [30, 30],
        maxZoom: 15
      });

      // Add loaded class to remove loading animation
      setTimeout(() => {
        const mapContainer = document.querySelector('.map-container');
        if (mapContainer) {
          mapContainer.classList.add('loaded');
        }
      }, 100);

      console.log('Map initialized successfully with REAL DATA from backend');
    } catch (error) {
      console.error('Error initializing map with real data:', error);
      // Fallback to default map
    }
  }

  // Confirmation modal methods
  onConfirmationConfirmed(): void {
    this.showConfirmation = false;
    if (this.pendingAction) {
      this.pendingAction();
      this.pendingAction = undefined;
    }
  }

  onConfirmationCancelled(): void {
    this.showConfirmation = false;
    this.pendingAction = undefined;
  }

  openAssignModal(): void {
    this.showAssignModal = true;
    this.selectedDeliveryManId = null;
    this.loadAvailableDeliveryMen();
  }

  closeAssignModal(): void {
    this.showAssignModal = false;
    this.selectedDeliveryManId = null;
  }

  loadAvailableDeliveryMen(): void {
    this.isLoadingDeliveryMen = true;
    this.orderClient.getAvailableDeliveryMen().subscribe({
      next: (deliveryMen) => {
        this.availableDeliveryMen = deliveryMen;
        this.isLoadingDeliveryMen = false;
      },
      error: (err) => {
        this.isLoadingDeliveryMen = false;
        const errorMessage = err?.error?.error || err?.error || 'حدث خطأ أثناء تحميل المندوبين المتاحين';
        this.toasterService.error('خطأ', errorMessage);
      }
    });
  }

  assignOrder(): void {
    if (!this.selectedDeliveryManId) {
      this.toasterService.error('خطأ', 'يرجى اختيار مندوب التوصيل');
      return;
    }

    const request = new AssignOrderToDeliveryManRequest();
    request.orderId = this.orderId;
    request.deliveryManId = this.selectedDeliveryManId;

    this.orderClient.assignOrderToDeliveryMan(request).subscribe({
      next: (res) => {
        this.toasterService.success('تم تعيين المندوب', 'تم تعيين مندوب التوصيل بنجاح');
        this.closeAssignModal();
        this.loadOrderDetails();
      },
      error: (err) => {
        const errorMessage = err?.error?.error || err?.error || 'حدث خطأ أثناء تعيين المندوب';
        this.toasterService.error('خطأ', errorMessage);
      }
    });
  }

  completeOrder(): void {
    this.confirmationTitle = 'إكمال الطلب';
    this.confirmationMessage = 'هل أنت متأكد من إكمال هذا الطلب؟';
    this.pendingAction = () => this.completeOrderFromAdmin();
    this.showConfirmation = true;
  }

  completeOrderFromAdmin(): void {
    this.orderClient.completeOrder(this.orderId).subscribe({
      next: (res) => {
        this.toasterService.success('تم إكمال الطلب', 'تم إكمال الطلب بنجاح');
        this.loadOrderDetails();
      },
      error: (err) => {
        const errorMessage = err?.error?.error || err?.error || 'حدث خطأ أثناء إكمال الطلب';
        this.toasterService.error('خطأ', errorMessage);
      }
    });
  }
}
