// header.component.ts
import { CommonModule, Location, NgClass } from '@angular/common';
import { Component, EventEmitter, Output, Renderer2, HostListener, ElementRef } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { LanguageService } from 'src/app/Core/services/language.service';
import { AuthService } from 'src/app/shared/services/auth.service';
import { AdminUserClient } from 'src/app/Core/services/NaqlahClient';
import { NotificationsComponent } from '../notifications/notifications.component';
import { trigger, state, style, transition, animate } from '@angular/animations';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, TranslateModule, RouterModule, NotificationsComponent],
  providers: [AdminUserClient], // تغيير من OrderAdminClient إلى AdminUserClient
  templateUrl: './header.component.html',
  animations: [
    // الأنيميشن الموحد لكل الـ dropdowns
    trigger('dropdownAnimation', [
      transition(':enter', [
        style({
          opacity: 0,
          transform: 'translateY(-10px) scale(0.95)',
          transformOrigin: 'top right'
        }),
        animate('200ms cubic-bezier(0.4, 0.0, 0.2, 1)', style({
          opacity: 1,
          transform: 'translateY(0) scale(1)'
        }))
      ]),
      transition(':leave', [
        animate('150ms cubic-bezier(0.4, 0.0, 1, 1)', style({
          opacity: 0,
          transform: 'translateY(-5px) scale(0.95)'
        }))
      ])
    ]),
    // أنيميشن خاص للـ settings icon
    trigger('settingsRotation', [
      state('closed', style({
        transform: 'rotate(0deg)'
      })),
      state('open', style({
        transform: 'rotate(90deg)'
      })),
      transition('closed <=> open', animate('300ms cubic-bezier(0.4, 0.0, 0.2, 1)'))
    ])
  ],
  styleUrl: './header.component.css'
})
export class HeaderComponent {
  // Language Properties
  activeLanguage: string = 'en';
  direction: string = 'ltr';

  // UI State Properties
  isScrolled: boolean = false;
  isMenuOpen: boolean = false;
  activePath: string = '/admin';
  isLogging: boolean = false;
  isLoggingOut: boolean = false; // إضافة loading state لتسجيل الخروج
  appearSideBarNav: boolean = false;

  // User Properties
  userEmail: string = '';
  userRole: string = '';

  // Dropdown States
  dropdownOpen: boolean = false;           // User profile dropdown
  languageDropdownOpen: boolean = false;   // Language dropdown
  settingsDropdownOpen: boolean = false;   // Settings dropdown

  profileItems = [
    {
      label: 'ADMIN.HEADER.PROFILE',  // مفتاح الترجمة بدلاً من النص المباشر
      icon: `<svg fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M10 9a3 3 0 100-6 3 3 0 000 6zm-7 9a7 7 0 1114 0H3z" clip-rule="evenodd"></path></svg>`,
      action: 'profile'
    },
    {
      label: 'ADMIN.HEADER.CHANGE_PASSWORD',
      icon: `<svg fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M5 9V7a5 5 0 0110 0v2a2 2 0 012 2v5a2 2 0 01-2 2H5a2 2 0 01-2-2v-5a2 2 0 012-2zm8-2v2H7V7a3 3 0 016 0z" clip-rule="evenodd"></path></svg>`,
      action: 'change_password'
    },
    {
      label: 'ADMIN.HEADER.ACCOUNT_INFO',
      icon: `<svg fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clip-rule="evenodd"></path></svg>`,
      action: 'account_info'
    }
  ];

  preferenceItems = [
    {
      label: 'ADMIN.HEADER.DARK_MODE',
      icon: `<svg fill="currentColor" viewBox="0 0 20 20"><path d="M17.293 13.293A8 8 0 016.707 2.707a8.001 8.001 0 1010.586 10.586z"></path></svg>`,
      action: 'dark_mode',
      hasToggle: true,
      isActive: false
    },
    {
      label: 'ADMIN.HEADER.LANGUAGE',
      icon: `<svg fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M7 2a1 1 0 011 1v1h3a1 1 0 110 2H9.578a18.87 18.87 0 01-1.724 4.78c.29.354.596.696.914 1.026a1 1 0 11-1.44 1.389c-.188-.196-.373-.396-.554-.6a19.098 19.098 0 01-3.107 3.567 1 1 0 01-1.334-1.49 17.087 17.087 0 003.13-3.733 18.992 18.992 0 01-1.487-2.494 1 1 0 111.79-.89c.234.47.489.928.764 1.372.417-.934.752-1.913.997-2.927H3a1 1 0 110-2h3V3a1 1 0 011-1zm6 6a1 1 0 01.894.553l2.991 5.982a.869.869 0 01.02.037l.99 1.98a1 1 0 11-1.79.895L15.383 16h-4.764l-.724 1.447a1 1 0 11-1.788-.894l.99-1.98.019-.038 2.99-5.982A1 1 0 0113 8zm-1.382 6h2.764L13 11.236 11.618 14z" clip-rule="evenodd"></path></svg>`,
      action: 'language'
    },
    {
      label: 'ADMIN.HEADER.NOTIFICATIONS_SETTING',
      icon: `<svg fill="currentColor" viewBox="0 0 20 20"><path d="M10 2a6 6 0 00-6 6v3.586l-.707.707A1 1 0 004 14h12a1 1 0 00.707-1.707L16 11.586V8a6 6 0 00-6-6zM10 18a3 3 0 01-3-3h6a3 3 0 01-3 3z"></path></svg>`,
      action: 'notifications',
      hasToggle: true,
      isActive: true
    },
    {
      label: 'ADMIN.HEADER.PRIVACY',
      icon: `<svg fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M2.166 4.999A11.954 11.954 0 0010 1.944 11.954 11.954 0 0017.834 5c.11.65.166 1.32.166 2.001 0 5.225-3.34 9.67-8 11.317C5.34 16.67 2 12.225 2 7c0-.682.057-1.35.166-2.001zm11.541 3.708a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clip-rule="evenodd"></path></svg>`,
      action: 'privacy'
    }
  ];

  systemItems = [
    {
      label: 'ADMIN.HEADER.HELP_SUPPORT',
      icon: `<svg fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-8-3a1 1 0 00-.867.5 1 1 0 11-1.731-1A3 3 0 0113 8a3.001 3.001 0 01-2 2.83V11a1 1 0 11-2 0v-1a1 1 0 011-1 1 1 0 100-2zm0 8a1 1 0 100-2 1 1 0 000 2z" clip-rule="evenodd"></path></svg>`,
      action: 'help'
    },
    {
      label: 'ADMIN.HEADER.ABOUT_APP',
      icon: `<svg fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clip-rule="evenodd"></path></svg>`,
      action: 'about'
    },
    {
      label: 'ADMIN.HEADER.LOGOUT',
      icon: `<svg fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M3 3a1 1 0 00-1 1v12a1 1 0 102 0V4a1 1 0 00-1-1zm10.293 9.293a1 1 0 001.414 1.414l3-3a1 1 0 000-1.414l-3-3a1 1 0 10-1.414 1.414L14.586 9H7a1 1 0 100 2h7.586l-1.293 1.293z" clip-rule="evenodd"></path></svg>`,
      action: 'logout',
      isDangerous: true
    }
  ];

  @Output() dataEmitter: EventEmitter<any> = new EventEmitter();

  constructor(
    private renderer: Renderer2,
    private languageService: LanguageService,
    private translateService: TranslateService,
    private router: Router,
    private location: Location,
    private elementRef: ElementRef,
    private authService: AuthService,
    private adminUserClient: AdminUserClient // تغيير من orderAdminClient إلى adminUserClient
  ) {}

  ngOnInit() {
    this.isMenuOpen = false;
    this.activePath = this.location.path();

    // Initialize language from service
    const currentLang = this.languageService.getLanguage();
    this.activeLanguage = currentLang;
    this.direction = currentLang === 'ar' ? 'rtl' : 'ltr';

    // Set initial direction
    this.setDirection(this.direction);

    // Load user information
    this.loadUserInfo();

    // Subscribe to router events
    this.router.events.subscribe(() => {
      this.activePath = this.location.path();
    });
  }

  // Load user email and role
  loadUserInfo(): void {
    this.userEmail = this.authService.getUserEmail();
    this.userRole = this.authService.GetUserRole() || '';
  }

  // Get first letter of email for avatar
  getUserInitial(): string {
    if (!this.userEmail) {
      return 'U';
    }
    return this.userEmail.charAt(0).toUpperCase();
  }


  // ============== Language Methods ==============
  isActive(path: string): boolean {
    return this.activePath === path;
  }

  setDirection(direction: string) {
    const rootElement = document.documentElement;
    this.renderer.setAttribute(rootElement, 'dir', direction);
    this.renderer.setAttribute(rootElement, 'lang', direction === 'rtl' ? 'ar' : 'en');

    const language = direction === 'rtl' ? 'ar' : 'en';
    this.translateService.use(language);
    this.languageService.setLanguage(language);
    this.activeLanguage = language;
    this.direction = direction;

    // Add body classes for styling
    if (direction === 'rtl') {
      document.body.classList.add('rtl');
      document.body.classList.remove('ltr');
    } else {
      document.body.classList.add('ltr');
      document.body.classList.remove('rtl');
    }
  }

  ChangeLanguage(direction: string) {
    const newLang = direction === 'rtl' ? 'ar' : 'en';

    if (this.activeLanguage === newLang) {
      this.closeLanguageDropdown();
      return;
    }

    this.setDirection(direction);
    this.closeLanguageDropdown();
    location.reload();
  }

  isActiveLanguage(language: string): boolean {
    return this.activeLanguage === language;
  }

  getLanguageDirection(): string {
    return this.activeLanguage === 'ar' ? 'rtl' : 'ltr';
  }

  getCurrentLanguageDisplayName(): string {
    return this.activeLanguage === 'ar' ? 'العربية' : 'English';
  }

  // ============== Dropdown Control Methods ==============

  // User Profile Dropdown
  toggleDropdown() {
    this.dropdownOpen = !this.dropdownOpen;
    if (this.dropdownOpen) {
      this.closeOtherDropdowns('user');
    }
  }

  closeDropdown() {
    this.dropdownOpen = false;
  }

  // Language Dropdown
  toggleLanguageDropdown() {
    this.languageDropdownOpen = !this.languageDropdownOpen;
    if (this.languageDropdownOpen) {
      this.closeOtherDropdowns('language');
    }
  }

  closeLanguageDropdown() {
    this.languageDropdownOpen = false;
  }

  // Settings Dropdown
  toggleSettingsDropdown() {
    this.settingsDropdownOpen = !this.settingsDropdownOpen;
    if (this.settingsDropdownOpen) {
      this.closeOtherDropdowns('settings');
    }
  }

  closeSettingsDropdown() {
    this.settingsDropdownOpen = false;
  }

  // Utility method to close other dropdowns
  closeOtherDropdowns(except?: string) {
    if (except !== 'user') this.dropdownOpen = false;
    if (except !== 'language') this.languageDropdownOpen = false;
    if (except !== 'settings') this.settingsDropdownOpen = false;
  }

  closeAllDropdowns() {
    this.dropdownOpen = false;
    this.languageDropdownOpen = false;
    this.settingsDropdownOpen = false;
  }

  // ============== Settings Menu Methods ==============
  handleMenuClick(action: string) {
    console.log('Menu clicked:', action);

    // Handle toggle actions
    if (action === 'dark_mode') {
      const item = this.preferenceItems.find(i => i.action === 'dark_mode');
      if (item) item.isActive = !item.isActive;
    } else if (action === 'notifications') {
      const item = this.preferenceItems.find(i => i.action === 'notifications');
      if (item) item.isActive = !item.isActive;
    }

    // Navigation logic
    switch (action) {
      case 'profile':
        // Navigate to profile page
        this.router.navigate(['/profile']);
        break;
      case 'change_password':
        // Navigate to change password page
        this.router.navigate(['/change-password']);
        break;
      case 'language':
        // Open language dropdown
        this.toggleLanguageDropdown();
        break;
      case 'logout':
        this.logout();
        break;
    }

    // Close settings dropdown after action
    this.closeSettingsDropdown();
  }

  // ============== Other Methods ==============
  AppearSideBar() {
    // Toggle sidebar state
    this.appearSideBarNav = !this.appearSideBarNav;
    this.dataEmitter.emit(this.appearSideBarNav);
  }

  // تحديث دالة logout لاستخدام AdminUserClient بدلاً من OrderAdminClient
  logout() {
    console.log('🔄 بدء عملية تسجيل الخروج...');
    this.isLoggingOut = true;
    this.closeAllDropdowns();

    this.adminUserClient.logout().subscribe({
      next: (response) => {
        console.log('✅ تم تسجيل الخروج من الخادم:', response);
        this.completeLogout();
      },
      error: (error) => {
        console.warn('⚠️ خطأ في تسجيل الخروج من الخادم، لكن سنكمل العملية:', error);
        this.completeLogout();
      }
    });
  }

  private completeLogout() {
    this.authService.logout();
    this.isLoggingOut = false;
    
    console.log('✅ تم تسجيل الخروج بنجاح');
      }

  // Prevent dropdown close when clicking inside
  onDropdownClick(event: Event): void {
    event.stopPropagation();
  }

  // ============== Host Listeners ==============
  @HostListener('document:keydown.escape', ['$event'])
  onEscapeKey(event: KeyboardEvent): void {
    if (this.dropdownOpen || this.languageDropdownOpen || this.settingsDropdownOpen) {
      this.closeAllDropdowns();
    }
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: Event): void {
    const target = event.target as HTMLElement;

    // Close dropdowns when clicking outside
    const userDropdown = target.closest('.header-user_profile');
    const languageDropdown = target.closest('.language-dropdown-container');
    const settingsDropdown = target.closest('.settings-dropdown-container');

    if (!userDropdown && this.dropdownOpen) {
      this.closeDropdown();
    }

    if (!languageDropdown && this.languageDropdownOpen) {
      this.closeLanguageDropdown();
    }

    if (!settingsDropdown && this.settingsDropdownOpen) {
      this.closeSettingsDropdown();
    }
  }

  // ============== Getters for Animation States ==============
  get settingsIconState(): string {
    return this.settingsDropdownOpen ? 'open' : 'closed';
  }
}
