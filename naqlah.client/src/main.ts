import { bootstrapApplication } from '@angular/platform-browser';
import { AppComponent } from './app/app.component';
import { appRouterProviders } from './app/app.routes';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { importProvidersFrom } from '@angular/core';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { TranslateLoader, TranslateModule } from '@ngx-translate/core';
import { TranslateHttpLoader } from '@ngx-translate/http-loader';
import { provideToastr } from 'ngx-toastr';
import { authInterceptor } from './app/shared/services/auth.interceptor';
import { API_BASE_URL, RolePermissionsAdminClient } from './app/Core/services/NaqlahClient';
import { AppConfigService } from './app/shared/services/AppConfigService';

export function HttpLoaderFactory(httpClient: HttpClient) {
  return new TranslateHttpLoader(httpClient);
}

function resolveApiBaseUrl(config: AppConfigService): string {
  const configured = config.getConfig().apiBaseUrl?.replace(/\/$/, '') ?? '';
  if (typeof window === 'undefined') {
    return configured;
  }

  const host = window.location.hostname;
  if (host === 'localhost' || host === '127.0.0.1') {
    return '';
  }

  return configured;
}

bootstrapApplication(AppComponent, {
  providers: [
    // إضافة HTTP Client مع الـ interceptors
    provideHttpClient(
      withInterceptors([authInterceptor])
    ),
    
    importProvidersFrom(BrowserAnimationsModule),
    provideToastr({
      timeOut: 10000,
      positionClass: 'toast-top-left',
      preventDuplicates: true,
    }),
    appRouterProviders,
    RolePermissionsAdminClient,
    { provide: API_BASE_URL, useFactory: (config: AppConfigService) => resolveApiBaseUrl(config), deps: [AppConfigService] },
    importProvidersFrom(
      TranslateModule.forRoot({
        loader: {
          provide: TranslateLoader,
          useFactory: HttpLoaderFactory,
          deps: [HttpClient],
        },
      })
    )
  ],
})
