import { CommonModule, NgIf } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { Component } from '@angular/core';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { TranslateService } from '@ngx-translate/core/lib/translate.service';
import { catchError, EMPTY } from 'rxjs';
import { Client, LoginRequest } from 'src/app/Core/services/NaqlahClient';
import { AppConfigService } from 'src/app/shared/services/AppConfigService';
import { AuthService } from 'src/app/shared/services/auth.service';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-login',
  standalone: true,
    imports: [
    ReactiveFormsModule,
    CommonModule,
    FormsModule,
  ],
  providers: [AuthService, AppConfigService, Client],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  loginFormGroup: FormGroup;
  errorMessage: string = '';
  error: boolean = false;
  isSubmitting: boolean = false;
  showPassword: boolean = false;

  constructor(
    private authServices: AuthService,
    private userClinet: Client,
  ) {
    this.loginFormGroup = new FormGroup({
      userName: new FormControl('', [
        Validators.required,
        Validators.email
      ]),
      password: new FormControl('', [
        Validators.required,
        Validators.minLength(6)
      ]),
    });
  }

  // Toggle password visibility
  togglePasswordVisibility() {
    this.showPassword = !this.showPassword;
  }

  // Get field error message
  getFieldError(fieldName: string): string {
    const field = this.loginFormGroup.get(fieldName);
    if (field && field.errors && field.touched) {
      if (field.errors['required']) {
        return fieldName === 'userName' ? 'البريد الإلكتروني مطلوب' : 'كلمة المرور مطلوبة';
      }
      if (field.errors['email']) {
        return 'يرجى إدخال بريد إلكتروني صحيح';
      }
      if (field.errors['minlength']) {
        return 'كلمة المرور يجب أن تكون على الأقل 6 أحرف';
      }
    }
    return '';
  }

  // Check if field has error
  hasFieldError(fieldName: string): boolean {
    const field = this.loginFormGroup.get(fieldName);
    return !!(field && field.errors && field.touched);
  }

  LogIn(formValues: any) {
    // Check form validation before submitting
    if (this.loginFormGroup.invalid) {
      // Mark all fields as touched to show validation errors
      Object.keys(this.loginFormGroup.controls).forEach(key => {
        this.loginFormGroup.get(key)?.markAsTouched();
      });
      return;
    }

    // Clear previous errors and start loading
    this.error = false;
    this.errorMessage = '';
    this.isSubmitting = true;

    var email = formValues.userName;
    var password = formValues.password;
    var loginRequest = new LoginRequest();
    loginRequest.email = email;
    loginRequest.password = password;

    console.log('🔐 Attempting login:', { email, password: '***' });

    this.userClinet.postLogin(loginRequest).subscribe(
      (success) => {
        this.authServices
          .Login(loginRequest)
          .pipe(
            catchError((err) => {
              console.error('❌ Login failed:', err);
              this.errorMessage = 'البريد الإلكتروني أو كلمة المرور غير صحيحة';
              this.error = true;
              this.isSubmitting = false;
              return EMPTY;
            })
          )
          .subscribe((data) => {
            console.log('✅ Login successful, redirecting...');
            this.isSubmitting = false;
            window.location.href = '/admin';
          });
      },
      (error) => {
        console.error('Login error:', error);
        this.isSubmitting = false;
        this.error = true;
        this.errorMessage = 'البريد الإلكتروني أو كلمة المرور غير صحيحة';
        
        Swal.fire({
          title: 'خطأ في تسجيل الدخول',
          text: this.errorMessage,
          icon: 'error',
          confirmButtonText: 'حسناً',
          confirmButtonColor: '#ef4444'
        });
      }
    );
  }
}
