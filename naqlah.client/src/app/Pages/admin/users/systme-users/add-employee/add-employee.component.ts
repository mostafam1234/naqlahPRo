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
      phoneNumber: ['', [Validators.required]],
      password: ['', [Validators.required, Validators.minLength(4)]],
      fullName: ['', Validators.required],
      roleId: [null, Validators.required],
      isActive: [true]
    });
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

  submitForm() {
    if (this.empForm.valid) {
      this.isLoading = true;
      const formValue = this.empForm.value;
      
      const command = new AddSystemUserCommand();
      command.userName = formValue.userName;
      command.email = formValue.email;
      command.phoneNumber = formValue.phoneNumber;
      command.password = formValue.password;
      command.fullName = formValue.fullName;
      command.roleId = formValue.roleId;
      command.isActive = formValue.isActive ?? true;

      this.sub.sink = this.systemUserClient.addSystemUser(command).subscribe({
        next: () => {
          this.toasterService.success('تمت الإضافة بنجاح', 'تمت إضافة المستخدم بنجاح');
          this.router.navigate(['/admin/users/systemUsers']);
        },
        error: (error) => {
          this.isLoading = false;
          this.toasterService.error('خطأ', error?.message || 'حدث خطأ أثناء إضافة المستخدم');
        }
      });
    } else {
      this.empForm.markAllAsTouched();
    }
  }
}
