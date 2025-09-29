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
    this.loadConfig();
  }
  loadConfig(): Promise<any> {
    return new Promise((resolve, reject) => {
      const http = this.inej.get(HttpClient);
      http
        .get(this.envUrl)
        .pipe(
          catchError((err) => {
            console.log(err);
            return EMPTY;
          })
        )
        .subscribe(
          (response: any) => {
            this.config = response;
            this.loaded = true;
            if (!this.loaded$.value) {
              this.loaded$.next(true);
            }
            resolve(true);
          },
          (err) => {
            debugger;
          }
        );
    });
  }

  getConfig(): SystemConfiguration {
    return this.config;
  }

  get Config(): SystemConfiguration {
    return this.config;
  }
}
