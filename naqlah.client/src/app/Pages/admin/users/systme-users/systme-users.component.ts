import { NgClass, NgFor, NgIf } from '@angular/common';
import { Component, HostListener } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { LanguageService } from 'src/app/Core/services/language.service';
@Component({
  selector: 'app-systme-users',
  standalone: true,
  imports: [NgFor, NgClass, NgIf, TranslateModule, RouterModule,],
  templateUrl: './systme-users.component.html',
  styleUrl: './systme-users.component.css'
})
export class SystmeUsersComponent {
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
  users = [
    {
      id:1,
      name: 'أحمد يوسف',
      age: 35,
      avatar: 'https://i.pravatar.cc/150?img=1',
      job:'hr',
      nationality:'مصرى',
      phone: '+20 100 123 4567',
      jobType:'منسق',
    },
    {
      id: 2,
      name: 'أحمد عمر',
      age: 47,
      avatar: 'https://i.pravatar.cc/150?img=2',
      job:'finance',
      nationality:'مصرى',
      phone: '+20 1091305668',
      jobType:'رئيس قسم',
    },
    {
      id: 3,
      name: 'محمد سعد ',
      age: 25,
      avatar: 'https://i.pravatar.cc/150?img=3',
      job:'accountant',
      nationality:'مصرى',
      phone: '+20 100 007 2164',
      jobType:'محاسب مالى',
    },
    {
      id:4,
      name: 'محمد سيد',
      age: 27,
      job:'customerService',
      avatar: 'https://i.pravatar.cc/150?img=4',
      nationality:'مصرى',
      phone: '+20 100 123 4567',
      jobType:'منسق',
    },
    {
      id:5,
      name: 'أحمد يوسف',
      age: 25,
      avatar: 'https://i.pravatar.cc/150?img=5',
      job:'reviewer',
      nationality:'مصرى',
      phone: '+20 100 123 4567',
      jobType:'منسق',
    },
    {
      id:6,
      name: 'أحمد يوسف',
      age: 29,
      avatar: 'https://i.pravatar.cc/150?img=6',
      job:'accountant',
      nationality:'مصرى',
      phone: '+20 100 123 4567',
      jobType:'منسق',
    },
    {
      id:7,
      name: 'أحمد يوسف',
      age: 32,
      avatar: 'https://i.pravatar.cc/150?img=7',
      job:'reviewer',
      nationality:'مصرى',
      phone: '+20 100 123 4567',
      jobType:'منسق',
    },
    {
      id:8,
      name: 'أحمد يوسف',
      age: 39,
      avatar: 'https://i.pravatar.cc/150?img=8',
      job:'hr',
      nationality:'مصرى',
      phone: '+20 100 123 4567',
      jobType:'منسق',
    },
    {
      id:9,
      name: 'أحمد يوسف',
      age: 40,
      job:'customerService',
      avatar: 'https://i.pravatar.cc/150?img=9',
      nationality:'مصرى',
      phone: '+20 100 123 4567',
      jobType:'منسق',
    },
    {
      id: 10,
      name: 'أحمد رامى',
      age: 23,
      avatar: 'https://i.pravatar.cc/150?img=10',
      job:'hr',
      nationality:'مصرى',
      phone: '+20 100 123 4567',
      jobType:'منسق',
    },
  ];

  get filteredUsers() {
    if (this.activeTab === 'all') return this.users;
    return this.users.filter(c => c.job === this.activeTab);
  }

  get paginatedUser() {
    const start = (this.currentPage - 1) * this.itemsPerPage;
    return this.filteredUsers.slice(start, start + this.itemsPerPage);
  }

  get totalPages() {
    return Math.ceil(this.filteredUsers.length / this.itemsPerPage);
  }

  changePage(page: number) {
    this.currentPage = page;
  }

  setActiveTab(tab: string) {
    this.activeTab = tab;
    this.currentPage = 1;
  }

  toggleDropdown(index: number) {
    this.openDropdownId = this.openDropdownId === index ? null : index;
  }

  deleteUser(captain: any) {
    console.log('حذف المستخدم:', captain);
    this.openDropdownId = null;
  }

 @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    const target = event.target as HTMLElement;
    if (!target.closest('.custom-card')) {
      this.openDropdownId = null;
    }
  }
}
