import { CommonModule, Location } from '@angular/common';
import { Component, EventEmitter, Output, Renderer2 } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { LanguageService } from 'src/app/Core/services/language.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, TranslateModule, RouterModule],
  templateUrl: './header.component.html',
  styleUrl: './header.component.css'
})
export class HeaderComponent {
  activeLanguage: string = 'en';
  isScrolled: boolean = false;
  direction: string = 'ltr';
  isMenuOpen: boolean = false;
  activePath: string = '/admin';
  isLogging: boolean = false;
  appearSideBarNav: boolean = false;
  dropdownOpen: boolean = false;
  languageDropdownOpen: boolean = false;
  @Output() dataEmitter: EventEmitter<any> = new EventEmitter();
  constructor(
    private renderer: Renderer2,
    private languageService: LanguageService,
    private translateService: TranslateService,
    private router: Router,
    private location: Location,
  ) {}

  ngOnInit() {
    this.isMenuOpen = false;
    this.activePath = this.location.path();
    const savedDirection = (this.direction =
      this.languageService.getLanguage() === 'ar' ? 'rtl' : 'ltr');
    this.setDirection(savedDirection);
    this.router.events.subscribe(() => {
      this.activePath = this.location.path();
    });
  }

  isActive(path: string): boolean {
    return this.activePath === path;
  }

  setDirection(direction: string) {
    const rootElement = document.documentElement;
    this.renderer.setAttribute(rootElement, 'dir', direction);
    const language = direction === 'rtl' ? 'ar' : 'en';
    this.translateService.use(language);
    this.languageService.setLanguage(language);
    this.activeLanguage = language;
  }

  ChangeLanguage(direction: string) {
    this.setDirection(direction);
    location.reload();
  }

  AppearSideBar() {
    this.appearSideBarNav = true;
    this.dataEmitter.emit(this.appearSideBarNav);
  }

  isActiveLanguage(language: string): boolean {
    return this.activeLanguage === language;
  }

  getLanguageDirection(): string {
    return this.activeLanguage === 'ar' ? 'rtl' : 'ltr';
  }

  toggleDropdown() {
    this.dropdownOpen = !this.dropdownOpen;
  }

  toggleLanguageDropdown(){
    this.languageDropdownOpen = !this.languageDropdownOpen;
  }
  logout() {
    console.log('Logging out...');
    this.dropdownOpen = false;
  }
}
