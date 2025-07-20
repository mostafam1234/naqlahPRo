import { provideRouter, Routes } from '@angular/router';
import { AdminLayoutComponent } from './Layouts/admin-layout/admin-layout.component';
import { LandingPageLayoutComponent } from './Layouts/landing-page-layout/landing-page-layout.component';

const routes: Routes = [
  { path:'', redirectTo:'/home', pathMatch:'full' },
  {
    path: 'login',
    loadComponent: () =>
      import('./Pages/login/login.component').then(
        (m) => m.LoginComponent
      ),
  },
  {
    path:'',
    component: LandingPageLayoutComponent,
    children: [
      {
        path: 'home',
        loadComponent: () =>
          import('./Pages/landing-page/home-page/home-page.component').then(
            (m) => m.HomePageComponent
          ),
      },
    ],
  },
  { path:'admin', redirectTo:'admin/home', pathMatch:'full' },
  {
    path: 'admin',
    component: AdminLayoutComponent,
    children: [
      {
        path: 'home',
        loadComponent: () =>
          import('./Pages/admin/admin-home/admin-home.component').then(
            (m) => m.AdminHomeComponent
          ),
      },
    ],
  }
];
export const appRouterProviders = [provideRouter(routes)];

