import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export type ThemeMode = 'light' | 'dark';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly storageKey = 'naqlah-theme';
  private dark = false;

  readonly isDark$ = new BehaviorSubject<boolean>(false);

  constructor() {
    this.init();
  }

  init(): void {
    const saved = localStorage.getItem(this.storageKey) as ThemeMode | null;
    this.apply(saved === 'dark', false);
  }

  get isDarkMode(): boolean {
    return this.dark;
  }

  get mode(): ThemeMode {
    return this.dark ? 'dark' : 'light';
  }

  toggle(): void {
    this.apply(!this.dark, true);
  }

  setTheme(mode: ThemeMode): void {
    this.apply(mode === 'dark', true);
  }

  private apply(dark: boolean, persist: boolean): void {
    this.dark = dark;
    const root = document.documentElement;
    root.classList.toggle('dark', dark);
    if (persist) {
      localStorage.setItem(this.storageKey, dark ? 'dark' : 'light');
    }
    this.isDark$.next(dark);
  }
}
