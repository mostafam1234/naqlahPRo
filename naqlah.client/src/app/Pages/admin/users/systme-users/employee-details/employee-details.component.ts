import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule, NgClass } from '@angular/common';
import { Router, RouterModule, ActivatedRoute } from '@angular/router';
import { SystemUserAdminClient, SystemUserAdminDto } from 'src/app/Core/services/NaqlahClient';
import { SubSink } from 'subsink';
import { ToasterService } from 'src/app/Core/services/toaster.service';
import { PageHeaderComponent } from 'src/app/shared/components/page-header/page-header.component';
import { TranslateModule } from '@ngx-translate/core';
import { LanguageService } from 'src/app/Core/services/language.service';

@Component({
  selector: 'app-employee-details',
  standalone: true,
  imports: [CommonModule, NgClass, RouterModule, PageHeaderComponent, TranslateModule],
  providers: [SystemUserAdminClient],
  templateUrl: './employee-details.component.html',
  styleUrl: './employee-details.component.css'
})
export class EmployeeDetailsComponent implements OnInit, OnDestroy {
  user: SystemUserAdminDto | null = null;
  isLoading = false;
  userId: number | null = null;
  private sub = new SubSink();

  constructor(
    private systemUserClient: SystemUserAdminClient,
    private toasterService: ToasterService,
    private router: Router,
    private route: ActivatedRoute,
    private languageService: LanguageService
  ) {}

  getRoleDisplayName(): string {
    if (!this.user) return 'غير محدد';
    return this.languageService.getLanguage() === 'ar' 
      ? (this.user.roleArabicName || this.user.roleName || 'غير محدد')
      : (this.user.roleName || 'غير محدد');
  }

  ngOnInit(): void {
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

  loadUser(): void {
    if (!this.userId) return;
    
    this.isLoading = true;
    this.sub.sink = this.systemUserClient.getSystemUserById(this.userId).subscribe({
      next: (user) => {
        this.user = user;
        this.isLoading = false;
      },
      error: (error) => {
        this.isLoading = false;
        this.toasterService.error('خطأ', 'حدث خطأ أثناء تحميل بيانات المستخدم');
        this.router.navigate(['/admin/users/systemUsers']);
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/admin/users/systemUsers']);
  }
}
