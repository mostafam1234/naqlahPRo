import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { catchError, map, take, timeout } from 'rxjs/operators';
import { of } from 'rxjs';
import { AuthService } from './auth.service';
import { FULL_CONTROL_PERMISSION, PermissionService } from './permission.service';

/** Max wait for permissions API before treating as failure (avoids infinite loader when API hangs). */
const PERMISSION_CHECK_TIMEOUT_MS = 15000;

export const permissionGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const permissionService = inject(PermissionService);
  const router = inject(Router);

  if (!authService.IsLoggedIn()) {
    router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
    return false;
  }

  const requiredPermission = route.data['requiredPermission'] as string | undefined;
  if (!requiredPermission) {
    return true;
  }

  return permissionService.getPermissions().pipe(
    timeout(PERMISSION_CHECK_TIMEOUT_MS),
    take(1),
    map(permissions => {
      const hasPermission = permissions.includes(FULL_CONTROL_PERMISSION) || permissions.includes(requiredPermission);
      if (!hasPermission) {
        router.navigate(['/admin/home']);
        return false;
      }
      return true;
    }),
    catchError(() => {
      router.navigate(['/admin/home']);
      return of(false);
    })
  );
};
