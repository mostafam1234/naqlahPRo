import { NgClass, NgFor, NgIf } from '@angular/common';
import { Component, HostListener } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { LanguageService } from 'src/app/Core/services/language.service';

@Component({
  selector: 'app-types-control',
  standalone: true,
  imports: [NgIf, TranslateModule, RouterModule,],
  templateUrl: './types-control.component.html',
  styleUrl: './types-control.component.css'
})
export class TypesControlComponent {
  isModalOpen = false;
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
      name: 'أحمد يوسف',
      email: 'ahmed.youssef@example.com',
      location: 'القاهرة، مصر',
      phone: '+20 100 123 4567',
      avatar: 'https://i.pravatar.cc/150?img=1',
      type: 'individual'
    },
    {
      id: 2,
      name: 'منى خليل',
      email: 'mona.khalil@example.com',
      location: 'الرياض، السعودية',
      phone: '+966 50 234 5678',
      avatar: 'https://i.pravatar.cc/150?img=2',
      type: 'institution'
    },
    {
      id: 3,
      name: 'سارة علاء',
      email: 'sara.alaa@example.com',
      location: 'دبي، الإمارات',
      phone: '+971 55 987 6543',
      avatar: 'https://i.pravatar.cc/150?img=3',
      type: 'individual'
    },
    {
      id: 4,
      name: 'علي فهد',
      email: 'ali.fahd@example.com',
      location: 'الكويت',
      phone: '+965 600 11223',
      avatar: 'https://i.pravatar.cc/150?img=4',
      type: 'institution'
    },
    {
      id: 5,
      name: 'نورا سالم',
      email: 'nora.salem@example.com',
      location: 'بيروت، لبنان',
      phone: '+961 3 556677',
      avatar: 'https://i.pravatar.cc/150?img=5',
      type: 'individual'
    },
    {
      id: 6,
      name: 'كريم منصور',
      email: 'karim.mansour@example.com',
      location: 'عمان، الأردن',
      phone: '+962 79 332211',
      avatar: 'https://i.pravatar.cc/150?img=6',
      type:'individual'
    },
    {
      id: 7,
      name: 'ليلى عمر',
      email: 'laila.omar@example.com',
      location: 'تونس',
      phone: '+216 98 123 456',
      avatar: 'https://i.pravatar.cc/150?img=7',
      type: 'institution'
    },
    {
      id: 8,
      name: 'عبدالرحيم عارف',
      email: 'abdo.aref@example.com',
      location: 'مصر - الغردقة',
      phone: '+212 6 789 01234',
      avatar: 'https://i.pravatar.cc/150?img=8',
      type: 'institution'
    },
    {
      id: 9,
      name: 'معتز أحمد عادل',
      email: 'moataz.ahmed@example.com',
      location: 'السعودية - جدة',
      phone: '+212 6 789 01234',
      avatar: 'https://i.pravatar.cc/150?img=11',
      type: 'individual'
    },
    {
      id: 10,
      name: 'معتز أحمد عادل',
      email: 'moataz.ahmed@example.com',
      location: 'السعودية - جدة',
      phone: '+212 6 789 01234',
      avatar: 'https://i.pravatar.cc/150?img=12',
      type: 'individual'
    },
    {
      id: 11,
      name: 'معتز أحمد عادل',
      email: 'moataz.ahmed@example.com',
      location: 'السعودية - جدة',
      phone: '+212 6 789 01234',
      avatar: 'https://i.pravatar.cc/150?img=13',
      type: 'individual'
    },
    {
      id: 12,
      name: 'معتز أحمد عادل',
      email: 'moataz.ahmed@example.com',
      location: 'السعودية - جدة',
      phone: '+212 6 789 01234',
      avatar: 'https://i.pravatar.cc/150?img=14',
      type: 'individual'
    },
    {
      id: 13,
      name: 'معتز أحمد عادل',
      email: 'moataz.ahmed@example.com',
      location: 'السعودية - جدة',
      phone: '+212 6 789 01234',
      avatar: 'https://i.pravatar.cc/150?img=15',
      type: 'individual'
    },
    {
      id: 14,
      name: 'معتز أحمد عادل',
      email: 'moataz.ahmed@example.com',
      location: 'السعودية - جدة',
      phone: '+212 6 789 01234',
      avatar: 'https://i.pravatar.cc/150?img=16',
      type: 'individual'
    },
    {
      id: 15,
      name: 'معتز أحمد عادل',
      email: 'moataz.ahmed@example.com',
      location: 'السعودية - جدة',
      phone: '+212 6 789 01234',
      avatar: 'https://i.pravatar.cc/150?img=17',
      type: 'individual'
    },
    {
      id: 16,
      name: 'معتز أحمد عادل',
      email: 'moataz.ahmed@example.com',
      location: 'السعودية - جدة',
      phone: '+212 6 789 01234',
      avatar: 'https://i.pravatar.cc/150?img=18',
      type: 'individual'
    },
    {
      id: 17,
      name: 'معتز أحمد عادل',
      email: 'moataz.ahmed@example.com',
      location: 'السعودية - جدة',
      phone: '+212 6 789 01234',
      avatar: 'https://i.pravatar.cc/150?img=19',
      type: 'individual'
    },
    {
      id: 18,
      name: 'معتز أحمد عادل',
      email: 'moataz.ahmed@example.com',
      location: 'السعودية - جدة',
      phone: '+212 6 789 01234',
      avatar: 'https://i.pravatar.cc/150?img=20',
      type: 'individual'
    },
    {
      id: 19,
      name: 'معتز أحمد عادل',
      email: 'moataz.ahmed@example.com',
      location: 'السعودية - جدة',
      phone: '+212 6 789 01234',
      avatar: 'https://i.pravatar.cc/150?img=21',
      type: 'individual'
    },
    {
      id: 20,
      name: 'معتز أحمد عادل',
      email: 'moataz.ahmed@example.com',
      location: 'السعودية - جدة',
      phone: '+212 6 789 01234',
      avatar: 'https://i.pravatar.cc/150?img=21',
      type: 'individual'
    },
    {
      id: 21,
      name: 'معتز أحمد عادل',
      email: 'moataz.ahmed@example.com',
      location: 'السعودية - جدة',
      phone: '+212 6 789 01234',
      avatar: 'https://i.pravatar.cc/150?img=22',
      type: 'individual'
    },
    {
      id: 22,
      name: 'معتز أحمد عادل',
      email: 'moataz.ahmed@example.com',
      location: 'السعودية - جدة',
      phone: '+212 6 789 01234',
      avatar: 'https://i.pravatar.cc/150?img=23',
      type: 'individual'
    },
    {
      id: 23,
      name: 'معتز أحمد عادل',
      email: 'moataz.ahmed@example.com',
      location: 'السعودية - جدة',
      phone: '+212 6 789 01234',
      avatar: 'https://i.pravatar.cc/150?img=24',
      type: 'individual'
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
    this.openDropdownId = null;
  }

 @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    const target = event.target as HTMLElement;
    if (!target.closest('.custom-card')) {
      this.openDropdownId = null;
    }
  }

  openModal() {
    this.isModalOpen = true;
  }

  closeModal() {
    this.isModalOpen = false;
  }

  saveAction() {
    console.log('تم الحفظ');
    this.closeModal();
  }
}
