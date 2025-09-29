import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

export const authGuard: CanActivateFn = (route, state) => { 
  let msg = "";
  const authService = inject(AuthService);
  const router = inject(Router);
  const currentUser = authService.IsLoggedIn();
  if(currentUser){
    return true;
  } 
  router.navigateByUrl('/login');
  return false;
};
