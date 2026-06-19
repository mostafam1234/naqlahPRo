import { NgIf } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import {
  AdminCustomerDetailDto,
  AdminResetCustomerPasswordRequest,
  CustomerAdminClient,
  CustomerType
} from 'src/app/Core/services/NaqlahClient';
import { ToasterService } from 'src/app/Core/services/toaster.service';
import { ConfirmationModalComponent } from 'src/app/shared/components/confirmation-modal/confirmation-modal.component';
import { PageHeaderComponent } from 'src/app/shared/components/page-header/page-header.component';
import { PermissionService } from 'src/app/shared/services/permission.service';
import { AppConfigService } from 'src/app/shared/services/AppConfigService';
import { SubSink } from 'subsink';

@Component({
  selector: 'app-admin-customer-detail',
  standalone: true,
  imports: [NgIf, TranslateModule, ReactiveFormsModule, RouterLink, ConfirmationModalComponent, PageHeaderComponent],
  providers: [CustomerAdminClient],
  templateUrl: './admin-customer-detail.component.html',
  styleUrl: './admin-customer-detail.component.css'
})
export class AdminCustomerDetailComponent implements OnInit, OnDestroy {
  CustomerType = CustomerType;

  customer: AdminCustomerDetailDto | null = null;
  isLoading = false;
  customerId: number | null = null;

  showConfirmModal = false;
  confirmationTitle = '';
  confirmationMessage = '';
  private pendingAction?: () => void;

  showPasswordModal = false;
  newPasswordControl = new FormControl('');

  previewImage: string | null = null;
  previewLabel = '';
  readonly defaultCustomerImage = 'assets/images/customerIcon.png';

  private sub = new SubSink();
  private baseUrl = '';

  constructor(
    private customerAdminClient: CustomerAdminClient,
    private route: ActivatedRoute,
    private router: Router,
    private toaster: ToasterService,
    private translate: TranslateService,
    private permissionService: PermissionService,
    private appConfig: AppConfigService
  ) {}

  ngOnInit(): void {
    this.baseUrl = this.appConfig.Config?.apiBaseUrl?.replace(/\/$/, '') || '';
    this.permissionService.getPermissions().subscribe(() => {});
    this.sub.sink = this.route.paramMap.subscribe((pm) => {
      const raw = pm.get('customerId');
      const id = raw ? Number.parseInt(raw, 10) : NaN;
      if (!Number.isFinite(id) || id <= 0) {
        this.toaster.error(this.translate.instant('COMMON.ERROR'), this.translate.instant('ADMIN.CUSTOMERS.DETAIL_ERR_INVALID'));
        void this.router.navigate(['/admin/users/customers']);
        return;
      }
      this.customerId = id;
      this.loadDetail();
    });
  }

  ngOnDestroy(): void {
    this.sub.unsubscribe();
  }

  hasPermission(name: string): boolean {
    return this.permissionService.hasPermission(name);
  }

  dash(s: string | null | undefined): string {
    const t = s?.trim();
    return t || this.translate.instant('ADMIN.CUSTOMERS.NO_EMAIL');
  }

  resolveImageUrl(path: string | null | undefined): string {
    const trimmed = path?.trim();
    if (!trimmed) return this.defaultCustomerImage;

    if (/^https?:\/\//i.test(trimmed)) {
      return trimmed;
    }

    if (trimmed.includes('/ImageBank/')) {
      return trimmed.startsWith('/') ? `${this.baseUrl}${trimmed}` : trimmed;
    }

    const fileName = trimmed.replace(/\\/g, '/').split('/').pop();
    if (!fileName) return this.defaultCustomerImage;

    return `${this.baseUrl}/ImageBank/Customer/${fileName}`;
  }

  openPreview(imageUrl: string, label: string): void {
    this.previewImage = imageUrl;
    this.previewLabel = label;
  }

  openPreviewByKey(imageUrl: string, labelKey: string): void {
    this.openPreview(imageUrl, this.translate.instant(labelKey));
  }

  closePreview(): void {
    this.previewImage = null;
    this.previewLabel = '';
  }

  loadDetail(): void {
    if (!this.customerId) return;
    this.isLoading = true;
    this.customer = null;
    this.sub.sink = this.customerAdminClient.getCustomerDetail(this.customerId).subscribe({
      next: (d) => {
        this.customer = d;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
        this.toaster.error(
          this.translate.instant('COMMON.ERROR'),
          this.translate.instant('ADMIN.CUSTOMERS.DETAIL_ERR_LOAD')
        );
        void this.router.navigate(['/admin/users/customers']);
      }
    });
  }

  resolveApiError(err: unknown): string {
    if (err && typeof err === 'object' && 'errorMessage' in err) {
      const msg = (err as { errorMessage?: string }).errorMessage;
      if (msg) return msg;
    }
    if (err instanceof Error && err.message) return err.message;
    return '';
  }

  promptDeactivate(): void {
    const c = this.customer;
    if (!c || c.isDeleted || !c.isActive) return;
    this.confirmationTitle = 'ADMIN.CUSTOMERS.MODAL_CONFIRM_TITLE';
    this.confirmationMessage = 'ADMIN.CUSTOMERS.CONFIRM_DEACTIVATE';
    this.pendingAction = () => {
      this.sub.sink = this.customerAdminClient.setCustomerUserActive(c.customerId, false).subscribe({
        next: () => {
          this.toaster.success(
            this.translate.instant('COMMON.SUCCESS'),
            this.translate.instant('ADMIN.CUSTOMERS.TOAST_DEACTIVATED')
          );
          this.loadDetail();
        },
        error: (err) =>
          this.toaster.error(
            this.translate.instant('COMMON.ERROR'),
            this.resolveApiError(err) || this.translate.instant('ADMIN.CUSTOMERS.ERR_GENERIC')
          )
      });
    };
    this.showConfirmModal = true;
  }

  activateCustomer(): void {
    const c = this.customer;
    if (!c || c.isDeleted) return;
    this.sub.sink = this.customerAdminClient.setCustomerUserActive(c.customerId, true).subscribe({
      next: () => {
        this.toaster.success(
          this.translate.instant('COMMON.SUCCESS'),
          this.translate.instant('ADMIN.CUSTOMERS.TOAST_ACTIVATED')
        );
        this.loadDetail();
      },
      error: (err) =>
        this.toaster.error(
          this.translate.instant('COMMON.ERROR'),
          this.resolveApiError(err) || this.translate.instant('ADMIN.CUSTOMERS.ERR_GENERIC')
        )
    });
  }

  promptDelete(): void {
    const c = this.customer;
    if (!c) return;
    this.confirmationTitle = 'ADMIN.CUSTOMERS.MODAL_CONFIRM_TITLE';
    this.confirmationMessage = 'ADMIN.CUSTOMERS.CONFIRM_DELETE';
    this.pendingAction = () => {
      this.sub.sink = this.customerAdminClient.deleteCustomerUser(c.customerId).subscribe({
        next: () => {
          this.toaster.success(this.translate.instant('COMMON.SUCCESS'), this.translate.instant('ADMIN.CUSTOMERS.TOAST_DELETED'));
          void this.router.navigate(['/admin/users/customers']);
        },
        error: (err) =>
          this.toaster.error(
            this.translate.instant('COMMON.ERROR'),
            this.resolveApiError(err) || this.translate.instant('ADMIN.CUSTOMERS.ERR_GENERIC')
          )
      });
    };
    this.showConfirmModal = true;
  }

  promptRestore(): void {
    const c = this.customer;
    if (!c || !c.isDeleted) return;
    this.confirmationTitle = 'ADMIN.CUSTOMERS.MODAL_CONFIRM_TITLE';
    this.confirmationMessage = 'ADMIN.CUSTOMERS.CONFIRM_RESTORE';
    this.pendingAction = () => {
      this.sub.sink = this.customerAdminClient.restoreCustomerUser(c.customerId).subscribe({
        next: () => {
          this.toaster.success(
            this.translate.instant('COMMON.SUCCESS'),
            this.translate.instant('ADMIN.CUSTOMERS.TOAST_RESTORED')
          );
          this.loadDetail();
        },
        error: (err) =>
          this.toaster.error(
            this.translate.instant('COMMON.ERROR'),
            this.resolveApiError(err) || this.translate.instant('ADMIN.CUSTOMERS.ERR_GENERIC')
          )
      });
    };
    this.showConfirmModal = true;
  }

  onConfirmationConfirmed(): void {
    this.showConfirmModal = false;
    const fn = this.pendingAction;
    this.pendingAction = undefined;
    if (fn) fn();
  }

  onConfirmationCancelled(): void {
    this.showConfirmModal = false;
    this.pendingAction = undefined;
  }

  openPasswordModal(): void {
    this.newPasswordControl.setValue('');
    this.showPasswordModal = true;
  }

  closePasswordModal(): void {
    this.showPasswordModal = false;
    this.newPasswordControl.setValue('');
  }

  submitPasswordReset(): void {
    const c = this.customer;
    const pwd = (this.newPasswordControl.value ?? '').trim();
    if (!c || !pwd) {
      this.toaster.error(this.translate.instant('COMMON.ERROR'), this.translate.instant('ADMIN.CUSTOMERS.PWD_REQUIRED'));
      return;
    }

    const body = AdminResetCustomerPasswordRequest.fromJS({ newPassword: pwd });
    this.sub.sink = this.customerAdminClient.resetCustomerPassword(body, c.customerId).subscribe({
      next: () => {
        this.toaster.success(this.translate.instant('COMMON.SUCCESS'), this.translate.instant('ADMIN.CUSTOMERS.TOAST_PWD_RESET'));
        this.closePasswordModal();
        this.loadDetail();
      },
      error: (err) =>
        this.toaster.error(
          this.translate.instant('COMMON.ERROR'),
          this.resolveApiError(err) || this.translate.instant('ADMIN.CUSTOMERS.ERR_GENERIC')
        )
    });
  }
}
