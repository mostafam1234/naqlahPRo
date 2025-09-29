import { NgClass, NgIf } from '@angular/common';
import { Component, EventEmitter, Output, inject } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-side-bar',
  standalone: true,
  imports: [TranslateModule, RouterModule, NgClass, NgIf],
  templateUrl: './side-bar.component.html',
  styleUrl: './side-bar.component.css'
})
export class SideBarComponent {
  
  appearSideBarNav: boolean = false;
  @Output() dataEmitter: EventEmitter<any> = new EventEmitter();
  openDropdown: string | null = null;

  constructor(private router: Router){}

  DisAppearSideBar() {
    this.appearSideBarNav = false;
    this.dataEmitter.emit(this.appearSideBarNav);
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
