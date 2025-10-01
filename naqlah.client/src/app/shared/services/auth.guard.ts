import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // Check if user is logged in
  if (authService.IsLoggedIn()) {
    return true;
  }

  const returnUrl = state.url;

  router.navigate(['/login'], {
    queryParams: { returnUrl: returnUrl }
  });

  return false;
};
