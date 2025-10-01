import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from './auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  
  console.log('🌐 HTTP Request:', req.method, req.url);
  
  // استثناء endpoints معينة من التوكن (مثل login)
  const excludedUrls = ['/LoginAdmin', '/login', '/register'];
  const isExcluded = excludedUrls.some(url => req.url.includes(url));
  
  if (isExcluded) {
    console.log('🔓 طلب مستثنى من التوكن:', req.url);
    return next(req);
  }

  // الحصول على التوكن
  const token = authService.getAccessToken();

  // إذا لم يكن هناك توكن، أرسل الطلب كما هو
  if (!token) {
    console.log('🔓 لا يوجد توكن - إرسال الطلب بدون Authorization header');
    return next(req);
  }

  // إذا كان التوكن منتهي الصلاحية، لا نضيفه
  if (authService.isTokenExpired()) {
    console.log('⏰ التوكن منتهي الصلاحية - إرسال الطلب بدون Authorization header');
    return next(req);
  }

  // نسخ الطلب وإضافة التوكن
  const authReq = req.clone({
    headers: req.headers.set('Authorization', `Bearer ${token}`)
  });

  console.log('🔐 تم إضافة التوكن للطلب:', req.url);
  console.log('🎫 التوكن (أول 50 حرف):', token.substring(0, 50) + '...');
  console.log('📋 Headers:', authReq.headers.get('Authorization') ? 'Authorization header موجود' : 'Authorization header مفقود');

  return next(authReq);
};
