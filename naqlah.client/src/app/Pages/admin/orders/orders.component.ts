import { NgClass } from '@angular/common';
import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { LanguageService } from 'src/app/Core/services/language.service';

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [NgClass],
  templateUrl: './orders.component.html',
  styleUrl: './orders.component.css'
})
export class OrdersComponent {
    lang: string = 'ar';
    constructor(
      private languageService: LanguageService,
      private translateService: TranslateService,
      private router: Router) {}

    activeTab = 'all';
    currentPage = 1;
    itemsPerPage = 9;
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
  get paginatedCaptains() {
    const start = (this.currentPage - 1) * this.itemsPerPage;
    return this.orders.slice(start, start + this.itemsPerPage);
  }
  get totalPages() {
    return Math.ceil(this.orders.length / this.itemsPerPage);
  }

  changePage(page: number) {
    this.currentPage = page;
  }

  setActiveTab(tab: string) {
    this.activeTab = tab;
    this.currentPage = 1;
  }
}
