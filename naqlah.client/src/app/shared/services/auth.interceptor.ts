import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from './auth.service';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);
    
  const excludedUrls = ['/LoginAdmin', '/login', '/register', '/refresh', '/Refresh'];
  const isExcluded = excludedUrls.some(url => req.url.includes(url));
  
  if (isExcluded) {
    return next(req);
  }

  let token = authService.getAccessToken();

  if (!token) {
    console.log('🔓 لا يوجد توكن - إعادة التوجيه إلى تسجيل الدخول');
    setTimeout(() => {
      if (!authService.getAccessToken()) {
        router.navigate(['/login']);
      }
    }, 0);
    return throwError(() => new Error('No access token available'));
  }

  // Get language from localStorage (default to 'ar' if not set)
  const language = localStorage.getItem('language') || 'ar';
  const nowDate = new Date();
  const timeOffsetInMinutes = (nowDate.getTimezoneOffset()) * (-1);

  if (authService.isTokenExpired()) {
    console.log('⏰ التوكن منتهي الصلاحية - محاولة التحديث...');
    const refreshToken = authService.getRefreshToken();
    
    if (!refreshToken) {
      console.log('❌ لا يوجد refresh token - إعادة التوجيه إلى تسجيل الدخول');
      authService.logout();
      return throwError(() => new Error('No refresh token available'));
    }

    return authService.refreshToken().pipe(
      switchMap((response) => {
        token = authService.getAccessToken();
        
        if (!token) {
          console.log('❌ فشل في الحصول على التوكن بعد التحديث');
          authService.logout();
          return throwError(() => new Error('Failed to get token after refresh'));
        }

        // إضافة التوكن المحدث واللغة إلى الطلب
        const authReq = req.clone({
          headers: req.headers
            .set('Authorization', `Bearer ${token}`)
            .set('Accept-Language', language)
            .set('DateTimeOffset', `${timeOffsetInMinutes}`)
        });

        return next(authReq);
      }),
      catchError((error) => {
        console.error('❌ فشل في تحديث التوكن:', error);
        authService.logout();
        return throwError(() => error);
      })
    );
  }

  // إضافة التوكن واللغة إلى الطلب
  const authReq = req.clone({
    headers: req.headers
      .set('Authorization', `Bearer ${token}`)
      .set('Accept-Language', language)
      .set('DateTimeOffset', `${timeOffsetInMinutes}`)
  });

  return next(authReq);
};
