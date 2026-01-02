import { HttpClient } from '@angular/common/http';
import { Injectable, Injector } from '@angular/core';
import { BehaviorSubject, catchError, EMPTY } from 'rxjs';
import { SystemConfiguration } from '../Models/SystemConfiguration';

@Injectable({
  providedIn: 'root',
})
export class AppConfigService {
  private envUrl = 'assets/appSettings.json';
  private config: SystemConfiguration;
  loaded$ = new BehaviorSubject<boolean>(false);
  loaded: boolean = false;
  constructor(protected inej: Injector) {
    this.config = new SystemConfiguration();
    console.log('🔧 AppConfigService constructor called');
    console.log('🔧 Starting to load config...');
    this.loadConfig().catch(err => {
      console.error('❌ Failed to load config:', err);
    });
  }
  loadConfig(): Promise<any> {
    return new Promise((resolve, reject) => {
      const http = this.inej.get(HttpClient);
      console.log('📂 Loading config from:', this.envUrl);
      http
        .get(this.envUrl)
        .pipe(
          catchError((err) => {
            console.error('❌ Error loading appSettings.json:', err);
            console.error('❌ URL attempted:', this.envUrl);
            return EMPTY;
          })
        )
        .subscribe(
          (response: any) => {
            console.log('✅ Config loaded successfully:', response);
            this.config = response;
            this.loaded = true;
            if (!this.loaded$.value) {
              this.loaded$.next(true);
            }
            resolve(true);
          },
          (err) => {
            console.error('❌ Error in config subscription:', err);
            reject(err);
          }
        );
    });
  }

  getConfig(): SystemConfiguration {
    console.log('📋 getConfig() called, loaded:', this.loaded, 'config:', this.config);
    return this.config;
  }

  get Config(): SystemConfiguration {
    return this.config;
  }
}
