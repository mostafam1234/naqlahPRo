import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { AdminUserClient } from 'src/app/Core/services/NaqlahClient';

/** Matches backend: user with this permission bypasses all permission checks. */
export const FULL_CONTROL_PERMISSION = 'FullControl';

@Injectable({
  providedIn: 'root'
})
export class PermissionService {
  private cachedPermissions: string[] | null = null;

  constructor(private adminUserClient: AdminUserClient) {}

  getPermissions(): Observable<string[]> {
    if (this.cachedPermissions !== null) {
      return of(this.cachedPermissions);
    }
    return this.adminUserClient.getCurrentUserPermissions().pipe(
      tap(permissions => { this.cachedPermissions = permissions; }),
      catchError(() => { this.cachedPermissions = []; return of([]); })
    );
  }

  /** Returns true if user has the given permission or has FullControl (bypass). */
  hasPermission(permission: string): boolean {
    if (!this.cachedPermissions) return false;
    return this.cachedPermissions.includes(FULL_CONTROL_PERMISSION) || this.cachedPermissions.includes(permission);
  }

  clearCache(): void {
    this.cachedPermissions = null;
  }

  loadAndGetPermissions(): Observable<string[]> {
    this.clearCache();
    return this.getPermissions();
  }
}
