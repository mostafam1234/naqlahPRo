import { NgClass, NgFor, NgIf } from '@angular/common';
import { Component } from '@angular/core';
import { RouterLink, RouterModule } from '@angular/router';
import * as L from 'leaflet';

@Component({
  selector: 'app-order-tracking',
  standalone: true,
  imports: [NgIf, NgFor, RouterModule, RouterLink],
  templateUrl: './order-tracking.component.html',
  styleUrl: './order-tracking.component.css'
})
export class OrderTrackingComponent {
  timelineSteps = [
    { title: 'تم استلام الطلب', time: '1:00 pm', description: ' المنطقة:- القاهرة',destination: 'الى المنطقة:- القاهرة', completed: true },
    { title: 'جارى عملية التحميل', time: '2:00 pm', description: ' المنطقة:- القاهرة', completed: true },
    { title: 'تم الوصول الى الموقع', time: '3:00 pm', description: ' المنطقة:- قنا', completed: true },
  ];

      ngAfterViewInit(): void {
        const map = L.map('map').setView([30.0444, 31.2357], 13); // القاهرة كمثال

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
          attribution: '© OpenStreetMap contributors'
        }).addTo(map);

        const startMarker = L.marker([30.0444, 31.2357]).addTo(map);
        startMarker.bindPopup('📍 نقطة البداية');

        const endMarker = L.marker([30.0500, 31.2400]).addTo(map);
        endMarker.bindPopup('🏁 نقطة النهاية');

        const route: [number, number][] = [
          [30.0444, 31.2357],
          [30.0465, 31.2370],
          [30.0480, 31.2385],
          [30.0500, 31.2400]
        ];

        L.polyline(route, {
          color: 'black',
          weight: 4
        }).addTo(map);
      }
}
