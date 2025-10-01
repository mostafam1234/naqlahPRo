import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { EMPTY, Observable, catchError, map, throwError } from 'rxjs';
import { LoginResponse } from '../Models/login_response';
import { AppConfigService } from './AppConfigService';
import { AccessTokenResponse, AdminResponse, AdminUserClient, Client, LoginRequest, LoginRquestDto, RefreshRequest, LoginAdminCommand } from 'src/app/Core/services/NaqlahClient';
import { SubSink } from 'subsink';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  subs = new SubSink();

  constructor(
    private httpClient: HttpClient,
    private appConfigService: AppConfigService,
    private adminUser: AdminUserClient,
    private client: Client,
    private router: Router
  ) { }

  // Login Method
  Login(credential: LoginAdminCommand): Observable<AdminResponse> {
    return this.adminUser.loginAdmin(credential).pipe(
      map((response: AdminResponse) => {
        console.log('📦 استجابة تسجيل الدخول الكاملة:', response);
        this.storeAdminTokens(response);
        
        // فحص فوري بعد الحفظ
        const savedToken = this.getAccessToken();
        console.log('💾 التوكن المحفوظ (أول 100 حرف):', savedToken?.substring(0, 100) + '...');
        
        // فحص محتوى التوكن فوراً
        if (savedToken) {
          const payload = this.parseJwt(savedToken);
          console.log('🔍 محتوى JWT Token الكامل:', payload);
          
          // البحث عن الـ Role
          const role = this.GetUserRole();
          console.log('👤 الدور المستخرج:', role);
        }
        
        return response;
      }),
      catchError((error) => {
        console.error('❌ خطأ في تسجيل الدخول:', error);
        return throwError(() => error);
      })
    );
  }

  // Refresh Token Method
  refreshToken(): Observable<AccessTokenResponse> {
    const refreshToken = this.getRefreshToken();

    if (!refreshToken) {
      this.logout();
      return throwError(() => new Error('No refresh token available'));
    }

    const refreshRequest = new RefreshRequest();
    refreshRequest.refreshToken = refreshToken;

    return this.client.postRefresh(refreshRequest).pipe(
      map((response: AccessTokenResponse) => {
        this.storeAccessTokens(response);
        return response;
      }),
      catchError((error) => {
        console.error('Refresh token error:', error);
        this.logout();
        return throwError(() => error);
      })
    );
  }

  // Store tokens from AdminResponse (Login)
  private storeAdminTokens(response: AdminResponse): void {
    if (response?.tokenResponse) {
      console.log('💾 بدء حفظ التوكنات...');
      console.log('🔐 Access Token (أول 50 حرف):', response.tokenResponse.accessToken.substring(0, 50) + '...');
      console.log('⏰ انتهاء الصلاحية:', response.tokenResponse.expiresIn, 'ثانية');
      
      localStorage.setItem("accessToken", response.tokenResponse.accessToken);
      localStorage.setItem("refreshToken", response.tokenResponse.refreshToken);
      localStorage.setItem("role", response.tokenResponse.role);
      const expirationTime = new Date().getTime() + response.tokenResponse.expiresIn * 1000;
      localStorage.setItem('expirationTime', expirationTime.toString());
      
      // تأكيد الحفظ
      const stored = localStorage.getItem("accessToken");
      console.log('✅ تأكيد الحفظ:', stored ? 'تم بنجاح' : 'فشل');
    } else {
      console.error('❌ لا يوجد tokenResponse في الاستجابة!');
      console.log('📋 الاستجابة المتوفرة:', response);
    }
  }

  // Store tokens from AccessTokenResponse (Refresh)
  private storeAccessTokens(response: AccessTokenResponse): void {
    if (response) {
      localStorage.setItem("accessToken", response.accessToken);
      localStorage.setItem("refreshToken", response.refreshToken);
      const expirationTime = new Date().getTime() + response.expiresIn * 1000;
      localStorage.setItem('expirationTime', expirationTime.toString());
    }
  }

  // Get Access Token
  getAccessToken(): string | null {
    return localStorage.getItem('accessToken');
  }
  getRole(): string | null {
    return localStorage.getItem('role');
  }
  // Get Refresh Token
  getRefreshToken(): string | null {
    return localStorage.getItem('refreshToken');
  }

  // استخراج الدور من JWT Token مع debugging مفصل
  GetUserRole(): string {
    const token = this.getAccessToken();
    if (!token) {
      console.log('🔓 لا يوجد توكن');
      return '';
    }

    try {
      const foundRole = this.getRole();
           
      if (!foundRole) {
        console.warn('⚠️ لم يتم العثور على الدور في التوكن');
      }
      
      return foundRole;
    } catch (error) {
      console.error('❌ خطأ في تحليل JWT Token:', error);
      return '';
    }
  }

  // دالة لتحليل JWT Token
  private parseJwt(token: string): any {
    try {
      const base64Url = token.split('.')[1];
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
      const jsonPayload = decodeURIComponent(atob(base64).split('').map(function(c) {
        return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
      }).join(''));
      return JSON.parse(jsonPayload);
    } catch (error) {
      console.error('❌ فشل في تحليل JWT:', error);
      return {};
    }
  }

  // Check if token is expired
  isTokenExpired(): boolean {
    const expirationTime = localStorage.getItem('expirationTime');
    if (!expirationTime) {
      return true;
    }
    const expired = new Date().getTime() > parseInt(expirationTime);
    if (expired) {
      console.log('⏰ التوكن منتهي الصلاحية');
    }
    return expired;
  }

  // Check if user is authenticated
  isAuthenticated(): boolean {
    const hasToken = !!this.getAccessToken();
    const notExpired = !this.isTokenExpired();
    const isAuth = hasToken && notExpired;
    
    console.log('🔒 فحص التوثيق:', {
      hasToken,
      notExpired,
      isAuthenticated: isAuth
    });
    
    return isAuth;
  }

  IsLoggedIn(): boolean {
    const token = this.getAccessToken();

    if (!token) {
      return false;
    }

    if (this.isTokenExpired()) {
      this.clearTokens();
      return false;
    }

    return true;
  }

  // التحقق من دور الأدمن
  isAdmin(): boolean {
    const userRole = this.GetUserRole();
    return userRole === 'Admin';
  }

  // Clear tokens without navigation (used internally)
  private clearTokens(): void {
    console.log('🗑️ مسح التوكنات من localStorage');
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('expirationTime');
  }

  // Logout
  logout(): void {
    this.clearTokens();
    this.router.navigate(['/login']);
  }

  ngOnDestroy(): void {
    this.subs.unsubscribe();
  }
}
