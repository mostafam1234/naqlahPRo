import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Router } from '@angular/router';
import { SubSink } from 'subsink';
import { AdminUserClient, UpdateCurrentUserProfileCommand, UserProfileDto } from 'src/app/Core/services/NaqlahClient';
import { ToasterService } from 'src/app/Core/services/toaster.service';
import { PageHeaderComponent } from 'src/app/shared/components/page-header/page-header.component';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule, PageHeaderComponent],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css']
})
export class ProfileComponent implements OnInit, OnDestroy {
  profileForm!: FormGroup;
  isLoading = false;
  isSaving = false;
  profileData: UserProfileDto | null = null;
  isEditMode = false;
  private sub = new SubSink();

  constructor(
    private fb: FormBuilder,
    private adminUserClient: AdminUserClient,
    private toasterService: ToasterService,
    private translate: TranslateService,
    private router: Router
  ) {
    this.initializeForm();
  }

  ngOnInit(): void {
    this.loadProfile();
  }

  ngOnDestroy(): void {
    this.sub.unsubscribe();
  }

  initializeForm(): void {
    this.profileForm = this.fb.group({
      userName: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(50)]],
      email: ['', [Validators.required, Validators.email]],
      phoneNumber: ['', [Validators.required, Validators.pattern(/^[0-9+\-\s()]+$/)]],
      fullName: ['', [Validators.maxLength(100)]],
      newPassword: ['', [Validators.minLength(6)]],
      confirmPassword: ['']
    }, { validators: this.passwordMatchValidator });
  }

  passwordMatchValidator(formGroup: FormGroup) {
    const newPassword = formGroup.get('newPassword')?.value;
    const confirmPassword = formGroup.get('confirmPassword')?.value;

    if (newPassword && confirmPassword && newPassword !== confirmPassword) {
      formGroup.get('confirmPassword')?.setErrors({ passwordMismatch: true });
      return { passwordMismatch: true };
    }

    if (confirmPassword && !newPassword) {
      formGroup.get('confirmPassword')?.setErrors({ passwordRequired: true });
      return { passwordRequired: true };
    }

    formGroup.get('confirmPassword')?.setErrors(null);
    return null;
  }

  loadProfile(): void {
    this.isLoading = true;
    this.sub.sink = this.adminUserClient.getCurrentUserProfile().subscribe({
      next: (response) => {
        this.profileData = response;
        this.populateForm(response);
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading profile:', error);
        this.toasterService.error('خطأ', 'حدث خطأ أثناء تحميل الملف الشخصي');
        this.isLoading = false;
      }
    });
  }

  populateForm(data: UserProfileDto): void {
    this.profileForm.patchValue({
      userName: data.userName || '',
      email: data.email || '',
      phoneNumber: data.phoneNumber || '',
      fullName: data.fullName || '',
      newPassword: '',
      confirmPassword: ''
    });
    this.profileForm.disable();
    this.isEditMode = false;
  }

  toggleEditMode(): void {
    if (this.isEditMode) {
      // Cancel editing - reload original data
      if (this.profileData) {
        this.populateForm(this.profileData);
      }
    } else {
      // Enable editing
      this.profileForm.enable();
      // Password fields are optional, so they can be enabled
      this.isEditMode = true;
    }
  }

  saveProfile(): void {
    // Check password fields if password is being changed
    const newPassword = this.profileForm.get('newPassword')?.value;
    const confirmPassword = this.profileForm.get('confirmPassword')?.value;

    if (newPassword || confirmPassword) {
      if (!newPassword) {
        this.toasterService.error('خطأ', 'يرجى إدخال كلمة المرور الجديدة');
        return;
      }
      if (newPassword !== confirmPassword) {
        this.toasterService.error('خطأ', 'كلمة المرور وتأكيد كلمة المرور غير متطابقين');
        return;
      }
      if (newPassword.length < 6) {
        this.toasterService.error('خطأ', 'كلمة المرور يجب أن تكون على الأقل 6 أحرف');
        return;
      }
    }

    if (this.profileForm.invalid) {
      this.markFormGroupTouched(this.profileForm);
      this.toasterService.error('خطأ', 'يرجى إكمال جميع الحقول المطلوبة بشكل صحيح');
      return;
    }

    this.isSaving = true;
    const formValue = this.profileForm.value;

    const command = new UpdateCurrentUserProfileCommand();
    command.userName = formValue.userName;
    command.email = formValue.email;
    command.phoneNumber = formValue.phoneNumber;
    command.fullName = formValue.fullName || '';
    command.newPassword = newPassword || undefined;

    this.sub.sink = this.adminUserClient.updateCurrentUserProfile(command).subscribe({
      next: () => {
        this.toasterService.success('نجاح', 'تم تحديث الملف الشخصي بنجاح');
        this.isEditMode = false;
        this.profileForm.disable();
        this.loadProfile(); // Reload to get updated data
        this.isSaving = false;
      },
      error: (error) => {
        console.error('Error updating profile:', error);
        const errorMessage = error?.error?.detail || error?.message || 'حدث خطأ أثناء تحديث الملف الشخصي';
        this.toasterService.error('خطأ', errorMessage);
        this.isSaving = false;
      }
    });
  }

  markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();
      if (control instanceof FormGroup) {
        this.markFormGroupTouched(control);
      }
    });
  }

  getFieldError(fieldName: string): string {
    const control = this.profileForm.get(fieldName);
    if (control?.errors && control.touched) {
      if (control.errors['required']) {
        return 'هذا الحقل مطلوب';
      }
      if (control.errors['email']) {
        return 'البريد الإلكتروني غير صحيح';
      }
      if (control.errors['minlength']) {
        return `الحد الأدنى ${control.errors['minlength'].requiredLength} أحرف`;
      }
      if (control.errors['maxlength']) {
        return `الحد الأقصى ${control.errors['maxlength'].requiredLength} أحرف`;
      }
      if (control.errors['pattern']) {
        return 'تنسيق غير صحيح';
      }
      if (control.errors['passwordMismatch']) {
        return 'كلمة المرور وتأكيد كلمة المرور غير متطابقين';
      }
      if (control.errors['passwordRequired']) {
        return 'يرجى إدخال كلمة المرور الجديدة أولاً';
      }
    }

    // Check form-level errors for password mismatch
    if (fieldName === 'confirmPassword' && this.profileForm.errors?.['passwordMismatch'] && control?.touched) {
      return 'كلمة المرور وتأكيد كلمة المرور غير متطابقين';
    }

    return '';
  }

  getInitials(): string {
    if (this.profileData?.fullName) {
      const names = this.profileData.fullName.split(' ');
      if (names.length >= 2) {
        return (names[0][0] + names[1][0]).toUpperCase();
      }
      return this.profileData.fullName.substring(0, 2).toUpperCase();
    }
    if (this.profileData?.userName) {
      return this.profileData.userName.substring(0, 2).toUpperCase();
    }
    return 'U';
  }
}

