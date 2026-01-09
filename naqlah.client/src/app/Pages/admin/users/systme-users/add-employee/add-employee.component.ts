import { Component, OnInit, OnDestroy } from '@angular/core';
import { NgIf, NgFor, NgClass } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { ImageService } from 'src/app/Core/services/image.service';
import { SystemUserAdminClient, AddSystemUserCommand, RoleLookupDto } from 'src/app/Core/services/NaqlahClient';
import { SubSink } from 'subsink';
import { ToasterService } from 'src/app/Core/services/toaster.service';
import { LanguageService } from 'src/app/Core/services/language.service';
import { PageHeaderComponent } from 'src/app/shared/components/page-header/page-header.component';

@Component({
  selector: 'app-add-employee',
  standalone: true,
  imports: [ReactiveFormsModule, NgIf, NgFor, NgClass, RouterModule, PageHeaderComponent],
  providers: [SystemUserAdminClient],
  templateUrl: './add-employee.component.html',
  styleUrl: './add-employee.component.css'
})
export class AddEmployeeComponent implements OnInit, OnDestroy {
  empForm: FormGroup;
  imagePreview: string | ArrayBuffer | null = null;
  isLoading = false;
  roles: RoleLookupDto[] = [];
  isLoadingRoles = false;
  private sub = new SubSink();

  constructor(
    private fb: FormBuilder, 
    private imageService: ImageService,
    private systemUserClient: SystemUserAdminClient,
    private toasterService: ToasterService,
    public router: Router,
    private languageService: LanguageService
  ) {
    this.empForm = this.fb.group({
      userName: ['', [Validators.required, Validators.minLength(3)]],
      email: ['', [Validators.required, Validators.email]],
      phoneNumber: ['', [Validators.required, this.ksaPhoneValidator]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      fullName: ['', Validators.required],
      roleId: [null, Validators.required],
      isActive: [true]
    });
  }

  // Custom validator for KSA phone number (starts with 05, exactly 10 digits)
  ksaPhoneValidator(control: any) {
    if (!control.value) {
      return null;
    }
    const phone = control.value.trim().replace(/\s+/g, '').replace(/-/g, '');
    const ksaPhoneRegex = /^05\d{8}$/;
    if (!ksaPhoneRegex.test(phone)) {
      return { invalidKsaPhone: true };
    }
    return null;
  }

  getFieldError(fieldName: string): string {
    const control = this.empForm.get(fieldName);
    if (!control || !control.errors || !control.touched) {
      return '';
    }

    if (control.errors['required']) {
      return 'هذا الحقل مطلوب';
    }

    if (control.errors['minlength']) {
      const requiredLength = control.errors['minlength'].requiredLength;
      if (fieldName === 'userName') {
        return `اسم المستخدم يجب أن يكون ${requiredLength} أحرف على الأقل`;
      }
      if (fieldName === 'password') {
        return `كلمة المرور يجب أن تكون ${requiredLength} أحرف على الأقل`;
      }
    }

    if (control.errors['email']) {
      return 'البريد الإلكتروني غير صحيح';
    }

    if (control.errors['invalidKsaPhone']) {
      return 'رقم الهاتف غير صحيح. يجب أن يبدأ بـ 05 ويتكون من 10 أرقام (مثال: 0512345678)';
    }

    return 'قيمة غير صحيحة';
  }

  ngOnInit(): void {
    this.loadRoles();
  }

  ngOnDestroy(): void {
    this.sub.unsubscribe();
  }

  loadRoles(): void {
    this.isLoadingRoles = true;
    this.sub.sink = this.systemUserClient.getAllRolesLookup().subscribe({
      next: (roles) => {
        this.roles = roles;
        this.isLoadingRoles = false;
      },
      error: (error) => {
        console.error('Error loading roles:', error);
        this.isLoadingRoles = false;
        this.toasterService.error('خطأ', 'حدث خطأ أثناء تحميل الأدوار');
      }
    });
  }

  getRoleDisplayName(role: RoleLookupDto): string {
    return this.languageService.getLanguage() === 'ar' ? role.arabicName : role.name;
  }

  async onImageSelected(event: Event) {
    const result = await this.imageService.handleImageUpload(event);
    if (result?.success) {
      this.imagePreview = result.preview || null;
      // Note: Image is not stored in backend currently, but we keep it for future use
    }
  }

  onPhoneInput(event: Event) {
    const input = event.target as HTMLInputElement;
    let value = input.value.replace(/\D/g, ''); // Remove non-digits
    
    // Ensure it starts with 05
    if (value.length > 0 && !value.startsWith('05')) {
      value = '05' + value.replace(/^05/, '');
    }
    
    // Limit to 10 digits
    if (value.length > 10) {
      value = value.substring(0, 10);
    }
    
    // Update form control value
    this.empForm.patchValue({ phoneNumber: value }, { emitEvent: false });
  }

  submitForm() {
    // Mark all fields as touched to show validation errors
    this.empForm.markAllAsTouched();

    if (this.empForm.valid) {
      this.isLoading = true;
      const formValue = this.empForm.value;
      
      // Clean phone number (remove spaces and dashes)
      const cleanPhoneNumber = formValue.phoneNumber.trim().replace(/\s+/g, '').replace(/-/g, '');
      
      const command = new AddSystemUserCommand();
      command.userName = formValue.userName.trim();
      command.email = formValue.email.trim();
      command.phoneNumber = cleanPhoneNumber;
      command.password = formValue.password;
      command.fullName = formValue.fullName.trim();
      command.roleId = formValue.roleId;
      command.isActive = formValue.isActive ?? true;

      this.sub.sink = this.systemUserClient.addSystemUser(command).subscribe({
        next: (response) => {
          this.isLoading = false;
          this.toasterService.success('نجح', 'تمت إضافة المستخدم بنجاح');
          // Navigate after a short delay to show the success message
          setTimeout(() => {
            this.router.navigate(['/admin/users/systemUsers']);
          }, 500);
        },
        error: (error) => {
          this.isLoading = false;
          
          // Extract error message from backend response
          let errorMessage = 'حدث خطأ أثناء إضافة المستخدم';
          
          if (error?.error) {
            // Try to get error from ProblemDetail or direct error message
            if (error.error.detail) {
              errorMessage = error.error.detail;
            } else if (error.error.title) {
              errorMessage = error.error.title;
            } else if (typeof error.error === 'string') {
              errorMessage = error.error;
            } else if (error.error.message) {
              errorMessage = error.error.message;
            }
          } else if (error?.message) {
            errorMessage = error.message;
          }
          
          this.toasterService.error('خطأ', errorMessage);
        }
      });
    }
  }
}
