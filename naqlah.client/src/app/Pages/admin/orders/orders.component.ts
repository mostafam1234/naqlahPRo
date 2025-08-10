import { NgClass, NgFor, NgIf } from '@angular/common';
import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { LanguageService } from 'src/app/Core/services/language.service';
import * as L from 'leaflet';
@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [NgClass, NgFor, NgIf],
  templateUrl: './orders.component.html',
  styleUrl: './orders.component.css'
})
export class OrdersComponent {

  lang: string = 'ar';
  activeProgressTab: string = 'all';
    constructor(
      private languageService: LanguageService,
      private translateService: TranslateService,
      private router: Router) {}

    activeTab = 'all';
    currentPage = 1;
    itemsPerPage = 4;
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
    orders = [
    {
      id:1,
      name: 'أحمد يوسف',
      email: 'ahmed.youssef@example.com',
      location: 'القاهرة، مصر',
      phone: '+20 100 123 4567',
      avatar: 'https://i.pravatar.cc/150?img=1',
      type: 'individual'
    },
  ];

  setActiveTab(tab: string) {
    this.activeTab = tab;
    this.currentPage = 1;
  }

  setActiveProgressTab(tab: string) {
    this.activeProgressTab = tab;
    this.currentPage = 1;
  }

  shipmentCards = [
    {
      number: 45654,
      from:'2972 Westheimer',
      fromAddress:'Rd. Santa Ana, Illinois 85486',
      to:'2972 Westheimer',
      toAddress: 'Rd. Santa Ana, Illinois 85486',
      customer: 'احمد السيد'
    },
    {
      number: 45654,
      from:'2972 Westheimer',
      fromAddress:'Rd. Santa Ana, Illinois 85486',
      to:'2972 Westheimer',
      toAddress: 'Rd. Santa Ana, Illinois 85486',
      customer: 'احمد السيد'
    },
    {
      number: 45654,
      from:'2972 Westheimer',
      fromAddress:'Rd. Santa Ana, Illinois 85486',
      to:'2972 Westheimer',
      toAddress: 'Rd. Santa Ana, Illinois 85486',
      customer: 'احمد السيد'
    },
    {
      number: 45654,
      from:'2972 Westheimer',
      fromAddress:'Rd. Santa Ana, Illinois 85486',
      to:'2972 Westheimer',
      toAddress: 'Rd. Santa Ana, Illinois 85486',
      customer: 'احمد السيد'
    },
        {
      number: 45654,
      from:'2972 Westheimer',
      fromAddress:'Rd. Santa Ana, Illinois 85486',
      to:'2972 Westheimer',
      toAddress: 'Rd. Santa Ana, Illinois 85486',
      customer: 'احمد السيد'
    },
        {
      number: 45654,
      from:'2972 Westheimer',
      fromAddress:'Rd. Santa Ana, Illinois 85486',
      to:'2972 Westheimer',
      toAddress: 'Rd. Santa Ana, Illinois 85486',
      customer: 'احمد السيد'
    },
        {
      number: 45654,
      from:'2972 Westheimer',
      fromAddress:'Rd. Santa Ana, Illinois 85486',
      to:'2972 Westheimer',
      toAddress: 'Rd. Santa Ana, Illinois 85486',
      customer: 'احمد السيد'
    },
        {
      number: 45654,
      from:'2972 Westheimer',
      fromAddress:'Rd. Santa Ana, Illinois 85486',
      to:'2972 Westheimer',
      toAddress: 'Rd. Santa Ana, Illinois 85486',
      customer: 'احمد السيد'
    },
        {
      number: 45654,
      from:'2972 Westheimer',
      fromAddress:'Rd. Santa Ana, Illinois 85486',
      to:'2972 Westheimer',
      toAddress: 'Rd. Santa Ana, Illinois 85486',
      customer: 'احمد السيد'
    },
        {
      number: 45654,
      from:'2972 Westheimer',
      fromAddress:'Rd. Santa Ana, Illinois 85486',
      to:'2972 Westheimer',
      toAddress: 'Rd. Santa Ana, Illinois 85486',
      customer: 'احمد السيد'
    },
        {
      number: 45654,
      from:'2972 Westheimer',
      fromAddress:'Rd. Santa Ana, Illinois 85486',
      to:'2972 Westheimer',
      toAddress: 'Rd. Santa Ana, Illinois 85486',
      customer: 'احمد السيد'
    },
        {
      number: 45654,
      from:'2972 Westheimer',
      fromAddress:'Rd. Santa Ana, Illinois 85486',
      to:'2972 Westheimer',
      toAddress: 'Rd. Santa Ana, Illinois 85486',
      customer: 'احمد السيد'
    },
        {
      number: 45654,
      from:'2972 Westheimer',
      fromAddress:'Rd. Santa Ana, Illinois 85486',
      to:'2972 Westheimer',
      toAddress: 'Rd. Santa Ana, Illinois 85486',
      customer: 'احمد السيد'
    },

  ];

  get paginatedOrders() {
    const start = (this.currentPage - 1) * this.itemsPerPage;
    return this.shipmentCards.slice(start, start + this.itemsPerPage);
  }
  get totalPages() {
    return Math.ceil(this.shipmentCards.length / this.itemsPerPage);
  }

  changePage(page: number) {
    this.currentPage = page;
  }

}
