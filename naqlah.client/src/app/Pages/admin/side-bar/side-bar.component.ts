import { NgClass, NgIf } from '@angular/common';
import { Component, EventEmitter, Output, Input, inject, OnDestroy } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { Subscription } from 'rxjs';
import { AuditLogNotificationService } from 'src/app/shared/services/audit-log-notification.service';
import { PermissionService } from 'src/app/shared/services/permission.service';

@Component({
  selector: 'app-side-bar',
  standalone: true,
  imports: [TranslateModule, RouterModule, NgClass, NgIf],
  templateUrl: './side-bar.component.html',
  styleUrl: './side-bar.component.css'
})
export class SideBarComponent implements OnDestroy {
  appearSideBarNav: boolean = false;
  @Input() isMobile: boolean = false;
  @Input() isOpen: boolean = false;
  @Input() isRtl: boolean = false;
  @Output() dataEmitter: EventEmitter<any> = new EventEmitter();
  @Output() closeSidebar: EventEmitter<void> = new EventEmitter<void>();
  openDropdown: string | null = null;
  permissions: string[] = [];
  auditLogNotificationCount: number = 0;
  private sub = new Subscription();

  private router = inject(Router);
  private permissionService = inject(PermissionService);
  auditLogNotificationService = inject(AuditLogNotificationService);

  ngOnInit(): void {
    this.permissionService.getPermissions().subscribe(p => this.permissions = p);
    this.sub.add(this.auditLogNotificationService.count$.subscribe(c => this.auditLogNotificationCount = c));
  }

  ngOnDestroy(): void {
    this.sub.unsubscribe();
  }

  onAuditMenuClick(): void {
    this.auditLogNotificationService.resetCount();
  }

  hasPermission(name: string): boolean {
    return this.permissionService.hasPermission(name);
  }

  DisAppearSideBar() {
    this.appearSideBarNav = false;
    this.dataEmitter.emit(this.appearSideBarNav);
  }

  closeSidebarMobile() {
    this.closeSidebar.emit();
  }

  toggleDropdown(menu: string): void {
    this.openDropdown = this.openDropdown === menu ? null : menu;
    if (menu === 'requests') {
      this.router.navigate(['/admin/requests']);
    } else if (menu == 'category') {
      this.router.navigate(['/admin/categoriesControl']);
    }
  }

  logout(): void {
    this.router.navigate(['/login']);
  }
}
