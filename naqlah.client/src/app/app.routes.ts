import { provideRouter, Routes } from '@angular/router';
import { AdminLayoutComponent } from './Layouts/admin-layout/admin-layout.component';
import { LandingPageLayoutComponent } from './Layouts/landing-page-layout/landing-page-layout.component';
import { authGuard } from './shared/services/auth.guard';
import { roleGuardGuard } from './shared/services/role-guard.guard';

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
    //canActivate: [authGuard, roleGuardGuard],
    //data: { expectedRole: 'Admin' },
    children: [
      {
        path: 'home',
        loadComponent: () =>
          import('./Pages/admin/admin-home/admin-home.component').then(
            (m) => m.AdminHomeComponent
          ),
      },
      {
        path: 'users/captains',
        loadComponent: () =>
          import('./Pages/admin/users/captain/captain.component').then(
            (m) => m.CaptainComponent
          ),
      },
      {
        path: 'users/captains/add-captain',
        loadComponent: () =>
          import('./Pages/admin/users/captain/add-captain/add-captain.component').then(
            (m) => m.AddCaptainComponent
          ),
      },
      {
        path: 'users/captains/edit-captain/:id',
        loadComponent: () =>
          import('./Pages/admin/users/captain/edit-captain/edit-captain.component').then(
            (m) => m.EditCaptainComponent
          ),
      },
      {
        path: 'users/captains/details/:id',
        loadComponent: () =>
          import('./Pages/admin/users/captain/captain-details/captain-details.component').then(
            (m) => m.CaptainDetailsComponent
          ),
      },
      {
        path: 'users/systemUsers',
        loadComponent: () =>
          import('./Pages/admin/users/systme-users/systme-users.component').then(
            (m) => m.SystmeUsersComponent
          ),
      },
      {
        path: 'users/systemUsers/add-employee',
        loadComponent: () =>
          import('./Pages/admin/users/systme-users/add-employee/add-employee.component').then(
            (m) => m.AddEmployeeComponent
          ),
      },
      {
        path: 'users/systemUsers/edit-employee/:id',
        loadComponent: () =>
          import('./Pages/admin/users/systme-users/edit-employee/edit-employee.component').then(
            (m) => m.EditEmployeeComponent
          ),
      },
      {
        path: 'users/systemUsers/details/:id',
        loadComponent: () =>
          import('./Pages/admin/users/systme-users/employee-details/employee-details.component').then(
            (m) => m.EmployeeDetailsComponent
          ),
      },
            {
              path: 'vehicles',
              loadComponent: () =>
                import('./Pages/admin/vehicle/vehicles/vehicles.component').then(
                  (m) => m.VehiclesComponent
                ),
              // children: [
              //   {
              //     path: 'brands',
              //     loadComponent: () => import('./Pages/admin/vehicle/vehicle-brands/vehicle-brands.component').then(m => m.VehicleBrandsComponent)
              //   },
              //   {
              //     path: 'types',
              //     loadComponent: () => import('./Pages/admin/vehicle/vehicle-types/vehicle-types.component').then(m => m.VehicleTypesComponent)
              //   }
              // ]
            },
      {
        path: 'newCaptain',
        loadComponent: () =>
          import('./Pages/admin/users/new-captain/new-captain.component').then(
            (m) => m.NewCaptainComponent
          ),
      },
      {
        path: 'newCaptain/action/:id',
        loadComponent: () =>
          import('./Pages/admin/users/new-captain/captain-action/captain-action.component').then(
            (m) => m.CaptainActionComponent
          ),
      },
      {
        path: 'main-categories',
        loadComponent: () =>
          import('./Pages/admin/main-category/main-category.component').then(
            (m) => m.MainCategoryComponent
          ),
      },
      {
        path: 'wallet/captain',
        loadComponent: () =>
          import('./Pages/admin/wallet/wallet-captain/wallet-captain.component').then(
            (m) => m.WalletCaptainComponent
          ),
      },
      {
        path: 'wallet/captain/details/:id',
        loadComponent: () =>
          import('./Pages/admin/wallet/wallet-captain/wallet-captain-details/wallet-captain-details.component').then(
            (m) => m.WalletCaptainDetailsComponent
          ),
      },
      {
        path: 'requests',
        loadComponent: () =>
          import('./Pages/admin/orders/orders.component').then(
            (m) => m.OrdersComponent
          ),
      },
      {
        path: 'requests/details/:id',
        loadComponent: () =>
          import('./Pages/admin/orders/order-details/order-details.component').then(
            (m) => m.OrderDetailsComponent
          ),
      },
      {
        path: 'requests/controlCaptainRequest',
        loadComponent: () =>
          import('./Pages/admin/orders/control-captain-orders/control-captain-orders.component').then(
            (m) => m.ControlCaptainOrdersComponent
          ),
      },
      {
        path: 'requests/controlCaptainRequest/requestTracking/:id',
        loadComponent: () =>
          import('./Pages/admin/orders/order-tracking/order-tracking.component').then(
            (m) => m.OrderTrackingComponent
          ),
      },
      {
        path: 'categoriesControl',
        loadComponent: () =>
          import('./Pages/admin/types-control/types-control.component').then(
            (m) => m.TypesControlComponent
          ),
      },
      {
        path: 'chat/serviceRequest',
        loadComponent: () =>
          import('./Pages/admin/chat/srevice-request-chat/srevice-request-chat.component').then(
            (m) => m.SreviceRequestChatComponent
          ),
      },
      {
        path: 'chat/review',
        loadComponent: () =>
          import('./Pages/admin/chat/chat-review/chat-review.component').then(
            (m) => m.ChatReviewComponent
          ),
      }
    ],
  }
];
export const appRouterProviders = [provideRouter(routes)];

