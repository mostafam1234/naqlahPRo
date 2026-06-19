import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../shared/services/auth.service';
import { LoginAdminCommand } from '../../Core/services/NaqlahClient';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {
  loginFormGroup!: FormGroup;
  isLoading = false;
  isSubmitting = false;
  errorMessage = '';
  showPassword = false;
  readonly currentYear = new Date().getFullYear();

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loginFormGroup = this.fb.group({
      userName: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(4)]]
    });
  }

  // دالة تسجيل الدخول مع debugging والـ redirect
  LogIn(formValue: any): void {
    if (this.loginFormGroup.invalid) {
      this.loginFormGroup.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    this.isLoading = true;
    this.errorMessage = '';

    const credentials = new LoginAdminCommand();
    credentials.userName = formValue.userName;
    credentials.password = formValue.password;

    console.log('🔄 بدء تسجيل الدخول...');
    console.log('📧 البريد الإلكتروني:', credentials.userName);

    this.authService.Login(credentials).subscribe({
      next: (response) => {
        console.log('✅ تم تسجيل الدخول بنجاح!');
        console.log('📦 الاستجابة:', response);
        
        // انتظار قصير لضمان حفظ التوكن
        setTimeout(() => {
          // فحص التوكن والدور
          const token = this.authService.getAccessToken();
          const userRole = this.authService.GetUserRole();
          const isAuthenticated = this.authService.isAuthenticated();
          
          console.log('=== تحليل نتائج تسجيل الدخول ===');
          console.log('🔐 يوجد توكن:', !!token);
          console.log('👤 دور المستخدم:', userRole);
          console.log('🔒 حالة التوثيق:', isAuthenticated);
          console.log('=====================================');

          // إعادة التوجيه لـ Admin Panel بعد تسجيل الدخول بنجاح (أي دور)
          if (token && isAuthenticated) {
            this.router.navigate(['/admin/home']).then(() => {
              console.log('🚀 تم إعادة التوجيه بنجاح');
            });
          } else {
            console.error('❌ مشكلة في التوكن أو التوثيق');
            this.errorMessage = 'مشكلة في حفظ بيانات تسجيل الدخول';
          }
          
          this.isSubmitting = false;
          this.isLoading = false;
        }, 100); // انتظار 100ms
      },
      error: (error) => {
        console.error('❌ فشل تسجيل الدخول:', error);
        console.error('📋 تفاصيل الخطأ الكاملة:', JSON.stringify(error, null, 2));
        
        // استخراج رسالة الخطأ
        let errorMsg = 'فشل في تسجيل الدخول';
        
        if (error.error?.error) {
          errorMsg = error.error.error;
        } else if (error.error?.message) {
          errorMsg = error.error.message;
        } else if (error.message) {
          errorMsg = error.message;
        } else if (error.errorMessage) {
          errorMsg = error.errorMessage;
        }
        
        this.errorMessage = errorMsg;
        this.isSubmitting = false;
        this.isLoading = false;
      }
    });
  }

  // دالة إظهار/إخفاء كلمة المرور
  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  // Helper methods للـ template
  hasFieldError(fieldName: string): boolean {
    const field = this.loginFormGroup.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  getFieldError(fieldName: string): string {
    const field = this.loginFormGroup.get(fieldName);
    if (field?.errors?.['required']) {
      return 'هذا الحقل مطلوب';
    }
    if (field?.errors?.['email']) {
      return 'يرجى إدخال بريد إلكتروني صحيح';
    }
    if (field?.errors?.['minlength']) {
      return 'كلمة المرور يجب أن تكون 4 أحرف على الأقل';
    }
    return '';
  }

  // Helper methods for template (compatibility)
  isFieldInvalid(fieldName: string): boolean {
    return this.hasFieldError(fieldName);
  }

  // دالة تسجيل الدخول القديمة (للتوافق)
  onSubmit(): void {
    this.LogIn(this.loginFormGroup.value);
  }

  // دالة للاختبار - يمكن حذفها لاحقاً
  testCurrentToken(): void {
    const token = this.authService.getAccessToken();
    const isAuth = this.authService.isAuthenticated();
    const userRole = this.authService.GetUserRole();
    
    console.log('=== اختبار التوكن الحالي ===');
    console.log('🔐 التوكن موجود:', !!token);
    console.log('🔒 مُوثق:', isAuth);
    console.log('👤 الدور:', userRole);
    console.log('===========================');
    
    if (token) {
      const payload = this.authService['parseJwt'](token);
      console.log('📋 محتوى التوكن:', payload);
    }
  }
}
