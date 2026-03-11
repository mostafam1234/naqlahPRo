import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { PageHeaderComponent } from 'src/app/shared/components/page-header/page-header.component';
import { ToasterService } from 'src/app/Core/services/toaster.service';
import {
  CreateRoleRequest,
  PermissionDefinitionDto,
  RoleLookupDto,
  RolePermissionsAdminClient,
  UpdateRolePermissionsRequest,
} from 'src/app/Core/services/NaqlahClient';

@Component({
  selector: 'app-role-permissions',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule, PageHeaderComponent],
  templateUrl: './role-permissions.component.html',
  styleUrl: './role-permissions.component.css',
})
export class RolePermissionsComponent implements OnInit {
  roles: RoleLookupDto[] = [];
  permissions: PermissionDefinitionDto[] = [];
  selectedRoleId: number | null = null;
  selectedPermissions = new Set<string>();
  loading = false;
  saving = false;
  error = '';
  groupedByModule: { module: string; items: PermissionDefinitionDto[] }[] = [];

  newRoleName = '';
  newRoleArabicName = '';
  creatingRole = false;
  showAddRoleForm = false;
  expandedModules = new Set<string>();

  get selectedRole(): RoleLookupDto | undefined {
    return this.roles.find((r) => r.id === this.selectedRoleId);
  }

  get totalPermissionsCount(): number {
    return this.permissions.length;
  }

  get selectedPermissionsCount(): number {
    return this.selectedPermissions.size;
  }

  getPermissionLabel(p: PermissionDefinitionDto): string {
    const key = 'ADMIN.ROLE_PERMISSIONS.PERMISSION_NAMES.' + p.name;
    const translated = this.translate.instant(key);
    return translated !== key ? translated : (p.description || p.name);
  }

  constructor(
    private rolePermissionsClient: RolePermissionsAdminClient,
    private toasterService: ToasterService,
    private translate: TranslateService,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.loadRoles();
    this.loadPermissionDefinitions();
  }

  loadRoles(): void {
    this.rolePermissionsClient.getRolesLookup().subscribe({
      next: (roles) => {
        this.roles = roles;
        if (this.roles.length > 0 && !this.selectedRoleId) {
          this.selectedRoleId = this.roles[0].id;
          this.loadRolePermissions();
        }
      },
      error: () => {
        this.error = this.translate.instant('ADMIN.ROLE_PERMISSIONS.ERROR_LOAD_ROLES') || 'Failed to load roles';
        this.toasterService.error(this.translate.instant('COMMON.ERROR') || 'خطأ', this.error);
      },
    });
  }

  loadPermissionDefinitions(): void {
    this.rolePermissionsClient.getAllPermissionDefinitions().subscribe({
      next: (list) => {
        this.permissions = list;
        const byModule = new Map<string, PermissionDefinitionDto[]>();
        list.forEach((p) => {
          const arr = byModule.get(p.module) ?? [];
          arr.push(p);
          byModule.set(p.module, arr);
        });
        this.groupedByModule = Array.from(byModule.entries()).map(
          ([module, items]) => ({ module, items })
        );
        this.expandedModules = new Set(this.groupedByModule.map(g => g.module));
      },
      error: () => {
        this.error = this.translate.instant('ADMIN.ROLE_PERMISSIONS.ERROR_LOAD_PERMISSIONS') || 'Failed to load permissions';
        this.toasterService.error(this.translate.instant('COMMON.ERROR') || 'خطأ', this.error);
      },
    });
  }

  onRoleChange(): void {
    if (this.selectedRoleId != null) {
      this.loadRolePermissions();
    } else {
      this.selectedPermissions.clear();
    }
  }

  loadRolePermissions(): void {
    if (this.selectedRoleId == null) return;
    this.loading = true;
    this.error = '';
    this.rolePermissionsClient.getRolePermissions(this.selectedRoleId).subscribe({
      next: (names) => {
        this.selectedPermissions = new Set(names);
        this.expandedModules = new Set(this.groupedByModule.map(g => g.module));
        this.loading = false;
      },
      error: () => {
        this.error = this.translate.instant('ADMIN.ROLE_PERMISSIONS.ERROR_LOAD_ROLE_PERMISSIONS') || 'Failed to load role permissions';
        this.toasterService.error(this.translate.instant('COMMON.ERROR') || 'خطأ', this.error);
        this.loading = false;
      },
    });
  }

  togglePermission(name: string): void {
    if (this.selectedPermissions.has(name)) {
      this.selectedPermissions.delete(name);
    } else {
      this.selectedPermissions.add(name);
    }
    this.selectedPermissions = new Set(this.selectedPermissions);
    this.cdr.markForCheck();
  }

  selectAllInModule(items: PermissionDefinitionDto[]): void {
    items.forEach(p => this.selectedPermissions.add(p.name));
    this.selectedPermissions = new Set(this.selectedPermissions);
    this.cdr.markForCheck();
  }

  deselectAllInModule(items: PermissionDefinitionDto[]): void {
    items.forEach(p => this.selectedPermissions.delete(p.name));
    this.selectedPermissions = new Set(this.selectedPermissions);
    this.cdr.markForCheck();
  }

  selectAllPermissions(): void {
    this.permissions.forEach(p => this.selectedPermissions.add(p.name));
    this.selectedPermissions = new Set(this.selectedPermissions);
    this.cdr.markForCheck();
  }

  deselectAllPermissions(): void {
    this.selectedPermissions.clear();
    this.selectedPermissions = new Set(this.selectedPermissions);
    this.cdr.markForCheck();
  }

  getSelectedCountInModule(items: PermissionDefinitionDto[]): number {
    return items.filter(p => this.selectedPermissions.has(p.name)).length;
  }

  toggleAddRoleForm(): void {
    this.showAddRoleForm = !this.showAddRoleForm;
    if (!this.showAddRoleForm) {
      this.newRoleName = '';
      this.newRoleArabicName = '';
    }
    this.cdr.markForCheck();
  }

  isModuleExpanded(module: string): boolean {
    return this.expandedModules.has(module);
  }

  toggleModule(module: string): void {
    if (this.expandedModules.has(module)) {
      this.expandedModules.delete(module);
    } else {
      this.expandedModules.add(module);
    }
    this.expandedModules = new Set(this.expandedModules);
    this.cdr.markForCheck();
  }

  expandAllModules(): void {
    this.groupedByModule.forEach(g => this.expandedModules.add(g.module));
    this.expandedModules = new Set(this.expandedModules);
    this.cdr.markForCheck();
  }

  collapseAllModules(): void {
    this.expandedModules.clear();
    this.expandedModules = new Set(this.expandedModules);
    this.cdr.markForCheck();
  }

  save(): void {
    if (this.selectedRoleId == null) return;
    this.saving = true;
    this.error = '';
    const request = new UpdateRolePermissionsRequest();
    request.roleId = this.selectedRoleId;
    request.permissionNames = Array.from(this.selectedPermissions);
    this.rolePermissionsClient.updateRolePermissions(request).subscribe({
      next: () => {
        this.saving = false;
        this.error = '';
        this.toasterService.success(
          this.translate.instant('COMMON.SUCCESS'),
          this.translate.instant('ADMIN.ROLE_PERMISSIONS.SAVED_SUCCESS'),
        );
      },
      error: () => {
        this.saving = false;
        const msg = this.translate.instant('ADMIN.ROLE_PERMISSIONS.SAVED_ERROR');
        this.error = msg;
        this.toasterService.error(
          this.translate.instant('COMMON.ERROR'),
          msg,
        );
      },
    });
  }

  addRole(): void {
    const name = this.newRoleName?.trim();
    if (!name) {
      const msg = this.translate.instant('ADMIN.ROLE_PERMISSIONS.ROLE_NAME_REQUIRED') || 'Role name is required.';
      this.toasterService.error(this.translate.instant('COMMON.ERROR'), msg);
      return;
    }
    this.creatingRole = true;
    this.error = '';
    const request = new CreateRoleRequest();
    request.name = name;
    request.arabicName = this.newRoleArabicName?.trim() || null;
    this.rolePermissionsClient.createRole(request).subscribe({
      next: (res) => {
        this.newRoleName = '';
        this.newRoleArabicName = '';
        this.showAddRoleForm = false;
        this.toasterService.success(
          this.translate.instant('COMMON.SUCCESS'),
          this.translate.instant('ADMIN.ROLE_PERMISSIONS.ROLE_CREATED'),
        );
        this.rolePermissionsClient.getRolesLookup().subscribe({
          next: (roles) => {
            this.roles = roles;
            this.selectedRoleId = res.id;
            this.loadRolePermissions();
            this.creatingRole = false;
          },
          error: () => {
            this.selectedRoleId = res.id;
            this.creatingRole = false;
          },
        });
      },
      error: (err) => {
        this.creatingRole = false;
        const errorMessage = err?.error?.errorMessage || err?.message || this.translate.instant('ADMIN.ROLE_PERMISSIONS.ADD_ROLE_ERROR');
        this.error = errorMessage;
        this.toasterService.error(
          this.translate.instant('COMMON.ERROR'),
          errorMessage,
        );
      },
    });
  }
}
