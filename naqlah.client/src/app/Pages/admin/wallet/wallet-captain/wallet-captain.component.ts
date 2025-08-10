import { NgClass, NgFor, NgIf } from '@angular/common';
import { Component, HostListener } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { LanguageService } from 'src/app/Core/services/language.service';
@Component({
  selector: 'app-wallet-captain',
  standalone: true,
  imports: [NgFor, NgClass, NgIf, TranslateModule, RouterModule,],
  templateUrl: './wallet-captain.component.html',
  styleUrl: './wallet-captain.component.css'
})
export class WalletCaptainComponent {
  lang: string = 'ar';
  viewMode: 'cards' | 'table' = 'table';
  constructor(
    private languageService: LanguageService,
    private translateService: TranslateService,
    private router: Router) {}

  activeTab = 'all';
  currentPage = 1;
  itemsPerPage = 9;
  openDropdownId: number = null;
  get language(){
    return this.languageService.getLanguage();
  }
  captains = [
    {
      id:1,
      name:"سيد سامح",
      date: '02/08/2025',
      type2: 'مقيم',
      accountNumber: '5078034511352536',
      avatar: 'https://i.pravatar.cc/150?img=7',
      type: 'individual'

    },
    {
      id: 2,
      name: 'منى خليل',
      date: '02/08/2025',
      type2: 'مقيم',
      accountNumber: '5078034511352536',
      avatar: 'https://i.pravatar.cc/150?img=6',
      type: 'institution'
    },
    {
      id: 3,
      name: 'سارة علاء',
      date: '02/08/2025',
      type2: 'مقيم',
      accountNumber: '5078034511352536',
      avatar: 'https://i.pravatar.cc/150?img=5',
      type: 'institution'
    },
    {
      id: 4,
      name: 'علي فهد',
      date: '02/08/2025',
      type2: 'مقيم',
      accountNumber: '5078034511352536',
      avatar: 'https://i.pravatar.cc/150?img=4',
      type: 'individual'
    },
    {
      id: 5,
      name: 'نورا سالم',
      date: '02/08/2025',
      type2: 'مواطن',
      accountNumber: '5078034511352536',
      avatar: 'https://i.pravatar.cc/150?img=3',
      type: 'institution'
    },
    {
      id: 6,
      name: 'كريم منصور',
      date: '02/08/2025',
      type2: 'مواطن',
      accountNumber: '5078034511352536',
      avatar: 'https://i.pravatar.cc/150?img=2',
      type: 'institution'
    },
    {
      id: 7,
      name: 'ليلى عمر',
      date: '02/08/2025',
      type2: 'مقيم',
      accountNumber: '5078034511352536',
      avatar: 'https://i.pravatar.cc/150?img=2',
      type: 'individual'
    },
    {
      id: 8,
      name: 'عبدالرحيم عارف',
      date: '02/08/2025',
      type2: 'مقيم',
      accountNumber: '5078034511352536',
      avatar: 'https://i.pravatar.cc/150?img=2',
      type: 'institution'
    },
    {
      id: 9,
      name: 'معتز أحمد عادل',
      date: '02/08/2025',
      type2: 'مواطن',
      accountNumber: '5078034511352536',
      avatar: 'https://i.pravatar.cc/150?img=2',
      type: 'institution'
    },
    {
      id: 10,
      name: 'معتز أحمد عادل',
      date: '02/08/2025',
      type2: 'مواطن',
      accountNumber: '5078034511352536',
      avatar: 'https://i.pravatar.cc/150?img=2',
      type: 'institution'
    },
    {
      id: 11,
      name: 'معتز أحمد عادل',
      date: '02/08/2025',
      type2: 'مواطن',
      accountNumber: '5078034511352536',
      avatar: 'https://i.pravatar.cc/150?img=2',
      type: 'institution'
    },
    {
      id: 12,
      name: 'معتز أحمد عادل',
      date: '02/08/2025',
      type2: 'مواطن',
      accountNumber: '5078034511352536',
      avatar: 'https://i.pravatar.cc/150?img=2',
      type: 'institution'
    },
    {
      id: 13,
      name: 'معتز أحمد عادل',
      date: '02/08/2025',
      type2: 'مواطن',
      accountNumber: '5078034511352536',
      avatar: 'https://i.pravatar.cc/150?img=2',
      type: 'institution'
    },
    {
      id: 14,
      name: 'معتز أحمد عادل',
      date: '02/08/2025',
      type2: 'مقيم',
      accountNumber: '5078034511352536',
      avatar: 'https://i.pravatar.cc/150?img=2',
      type: 'institution'
    },
   ];

  get filteredCaptains() {
    if (this.activeTab === 'all') return this.captains;
    return this.captains.filter(c => c.type === this.activeTab);
  }

    get paginatedCaptains() {
    const start = (this.currentPage - 1) * this.itemsPerPage;
    return this.filteredCaptains.slice(start, start + this.itemsPerPage);
  }

  get totalPages() {
    return Math.ceil(this.filteredCaptains.length / this.itemsPerPage);
  }

  changePage(page: number) {
    this.currentPage = page;
  }

  setActiveTab(tab: string) {
    this.activeTab = tab;
    this.currentPage = 1;
  }
}
