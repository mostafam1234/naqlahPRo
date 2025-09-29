import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';
import { inject } from '@angular/core';

export const roleGuardGuard: CanActivateFn = (route, state) => {
  
  const router = inject(Router);
  const authService = inject(AuthService)
  const allowedUser = authService.GetUserRole();
  const expectedRole = route.data['expectedRole'] as string; 
  const result = expectedRole.split(',');
  if (result.includes(allowedUser)) {
    return true; 
  } 
  console.warn('🚫 Access denied! Required role:', expectedRole, 'Current role:', allowedUser);
  router.navigateByUrl('/login');
  return false;
};
