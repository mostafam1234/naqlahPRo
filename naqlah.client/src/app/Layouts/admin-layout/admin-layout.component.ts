import { CommonModule, NgClass, NgIf } from '@angular/common';
import { Component, HostListener } from '@angular/core';
import { NavigationEnd, NavigationError, NavigationStart, Router, RouterModule, RouterOutlet } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { Subscription } from 'rxjs';
import { LanguageService } from 'src/app/Core/services/language.service';
import { LoaderService } from 'src/app/Core/services/loader.service';
import { HeaderComponent } from 'src/app/Pages/admin/header/header.component';
import { SideBarComponent } from 'src/app/Pages/admin/side-bar/side-bar.component';
import { PublicFooterComponent } from 'src/app/Pages/landing-page/public-footer/public-footer.component';

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [
    RouterModule,
    TranslateModule,
    SideBarComponent,
    RouterOutlet,
    HeaderComponent,
    CommonModule,
    NgIf,
    NgClass
  ],
  templateUrl: './admin-layout.component.html',
  styleUrl: './admin-layout.component.css'
})
export class AdminLayoutComponent {
  screenWidth: number = window.innerWidth;
  isSmallScreen: boolean = false;
  language: string = 'rtl';
  fadeState: string = 'in';
  slide: string = "";
  loading: boolean = false;
  routerSubscription!: Subscription;
  appearSideBar: boolean = true; // Default to open on large screens
  constructor(
    private languageService: LanguageService,
    private router: Router,
    private loaderService: LoaderService) {}
  ngOnInit() {
    this.getLanuage();
    this.onResize();
    // Set initial sidebar state based on screen size
    this.appearSideBar = !this.isSmallScreen;
    this.loaderService.loading$.subscribe((loadingState) => {
      this.loading = loadingState;
    });
    this.routerSubscription = this.router.events.subscribe((event) => {
      if (event instanceof NavigationStart) {
        this.loaderService.showLoader();
      } else if (event instanceof NavigationEnd || event instanceof NavigationError) {
        this.loaderService.hideLoader();
      }
    });
  }
  @HostListener('window:resize', ['$event'])
  onResize(): void {
    this.checkScreenSize();
  }

  private checkScreenSize(): void {
    this.isSmallScreen = window.innerWidth < 992;
    // Don't auto-close sidebar on screen size change - let user control it
  }

  handleSideBarVisibility(data: boolean){
    this.appearSideBar = data;
  }

  toggleSidebar() {
    this.appearSideBar = !this.appearSideBar;
  }

  closeSidebar() {
    this.appearSideBar = false;
  }
  getLanuage() {
    this.language = this.languageService.getLanguage();
  }

  ngOnDestroy() {
    if (this.routerSubscription) {
      this.routerSubscription.unsubscribe();
    }
  }
}
