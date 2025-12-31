import { Component, OnInit, OnDestroy } from '@angular/core';
import { NgIf, NgFor, NgClass } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule, ActivatedRoute } from '@angular/router';
import { ImageService } from 'src/app/Core/services/image.service';
import { SystemUserAdminClient, UpdateSystemUserCommand, RoleLookupDto } from 'src/app/Core/services/NaqlahClient';
import { SubSink } from 'subsink';
import { ToasterService } from 'src/app/Core/services/toaster.service';
import { LanguageService } from 'src/app/Core/services/language.service';
import { PageHeaderComponent } from 'src/app/shared/components/page-header/page-header.component';

@Component({
  selector: 'app-edit-employee',
  standalone: true,
  imports: [ReactiveFormsModule, NgIf, NgFor, NgClass, RouterModule, PageHeaderComponent],
  providers: [SystemUserAdminClient],
  templateUrl: './edit-employee.component.html',
  styleUrl: './edit-employee.component.css'
})
export class EditEmployeeComponent implements OnInit, OnDestroy {
  empForm: FormGroup;
  imagePreview: string | ArrayBuffer | null = null;
  isLoading = false;
  userId: number | null = null;
  roles: RoleLookupDto[] = [];
  isLoadingRoles = false;
  private sub = new SubSink();

  constructor(
    private fb: FormBuilder, 
    private imageService: ImageService,
    private systemUserClient: SystemUserAdminClient,
    private toasterService: ToasterService,
    public router: Router,
    private route: ActivatedRoute,
    private languageService: LanguageService
  ) {
    this.empForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      phoneNumber: ['', [Validators.required]],
      fullName: ['', Validators.required],
      roleId: [null, Validators.required],
      isActive: [true],
      newPassword: ['']
    });
  }

  ngOnInit(): void {
    this.loadRoles();
    this.route.params.subscribe(params => {
      this.userId = +params['id'];
      if (this.userId) {
        this.loadUser();
      }
    });
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

  loadUser(): void {
    if (!this.userId) return;
    
    this.isLoading = true;
    this.sub.sink = this.systemUserClient.getSystemUserById(this.userId).subscribe({
      next: (user) => {
        this.empForm.patchValue({
          email: user.email,
          phoneNumber: user.phoneNumber,
          fullName: user.fullName,
          roleId: user.roleId,
          isActive: user.isActive
        });
        this.isLoading = false;
      },
      error: (error) => {
        this.isLoading = false;
        this.toasterService.error('خطأ', 'حدث خطأ أثناء تحميل بيانات المستخدم');
        this.router.navigate(['/admin/users/systemUsers']);
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
    }
  }

  submitForm() {
    if (this.empForm.valid && this.userId) {
      this.isLoading = true;
      const formValue = this.empForm.value;
      
      const command = new UpdateSystemUserCommand();
      command.id = this.userId;
      command.email = formValue.email;
      command.phoneNumber = formValue.phoneNumber;
      command.fullName = formValue.fullName;
      command.roleId = formValue.roleId;
      command.isActive = formValue.isActive;
      command.newPassword = formValue.newPassword && formValue.newPassword.length > 0 ? formValue.newPassword : null;

      this.sub.sink = this.systemUserClient.updateSystemUser(command).subscribe({
        next: () => {
          this.toasterService.success('تم التحديث بنجاح', 'تم تحديث بيانات المستخدم بنجاح');
          this.router.navigate(['/admin/users/systemUsers']);
        },
        error: (error) => {
          this.isLoading = false;
          this.toasterService.error('خطأ', error?.message || 'حدث خطأ أثناء تحديث المستخدم');
        }
      });
    } else {
      this.empForm.markAllAsTouched();
    }
  }
}
