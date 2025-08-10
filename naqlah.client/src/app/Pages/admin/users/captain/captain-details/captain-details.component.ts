import { NgClass, NgFor, NgIf } from '@angular/common';
import { Component, HostListener } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { LanguageService } from 'src/app/Core/services/language.service';

@Component({
  selector: 'app-captain-details',
  standalone: true,
  imports: [NgFor, NgClass, NgIf, TranslateModule, RouterModule,],
  templateUrl: './captain-details.component.html',
  styleUrl: './captain-details.component.css'
})
export class CaptainDetailsComponent {
  activeTab = 'all';
  currentPage = 1;
  itemsPerPage = 9;
  openDropdownId: number = null;
  lang: string = 'ar';
  viewMode: 'cards' | 'table' = 'table';
  constructor(
    private languageService: LanguageService,
    private translateService: TranslateService,
    private router: Router) {}
  get language(){
    return this.languageService.getLanguage();
  }
  shipping = [
    {
      id:1,
      type: 'نقل أدوية',
      destination: 'متعددة الواجهات',
      price: '12.00$',
      state: 'جارى العمل على نقل الشحنة',
      stateId:1
    },
    {
      id:2,
      type: 'نقل أدوية',
      destination: 'متعددة الواجهات',
      price: '12.00$',
      state: 'فى الطريق',
      stateId:2
    },
    {
      id:1,
      type: 'نقل أدوية',
      destination: 'متعددة الواجهات',
      price: '12.00$',
      state: 'تم التوصيل',
      stateId:3
    },
    {
      id:1,
      type: 'نقل أدوية',
      destination: 'متعددة الواجهات',
      price: '12.00$',
      state: 'تم الغاء الشحنة',
      stateId:4
    },
    {
      id:1,
      type: 'نقل أدوية',
      destination: 'متعددة الواجهات',
      price: '12.00$',
      state: 'تم التوصيل',
      stateId:3
    },
    {
      id:1,
      type: 'نقل أدوية',
      destination: 'متعددة الواجهات',
      price: '12.00$',
      state: 'فى الطريق',
      stateId:2
    },
    {
      id:1,
      type: 'نقل أدوية',
      destination: 'متعددة الواجهات',
      price: '12.00$',
      state: 'تم التوصيل',
      stateId:3
    },
        {
      id:1,
      type: 'نقل أدوية',
      destination: 'متعددة الواجهات',
      price: '12.00$',
      state: 'تم التوصيل',
      stateId:3
    },
        {
      id:1,
      type: 'نقل أدوية',
      destination: 'متعددة الواجهات',
      price: '12.00$',
      state: 'فى الطريق',
      stateId:2
    },
        {
      id:1,
      type: 'نقل أدوية',
      destination: 'متعددة الواجهات',
      price: '12.00$',
      state: 'تم التوصيل',
      stateId:3
    },
        {
      id:1,
      type: 'نقل أدوية',
      destination: 'متعددة الواجهات',
      price: '12.00$',
      state: 'فى الطريق',
      stateId:2
    },
    {
      id:1,
      type: 'نقل أدوية',
      destination: 'متعددة الواجهات',
      price: '12.00$',
      state: 'فى الطريق',
      stateId:2
    },

  ];

  get filteredshipping() {
    if (this.activeTab === 'all') return this.shipping;
    return this.shipping.filter(c => c.type === this.activeTab);
  }

    get paginatedshipping() {
    const start = (this.currentPage - 1) * this.itemsPerPage;
    return this.filteredshipping.slice(start, start + this.itemsPerPage);
  }

  get totalPages() {
    return Math.ceil(this.filteredshipping.length / this.itemsPerPage);
  }

  changePage(page: number) {
    this.currentPage = page;
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
