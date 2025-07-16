import { NgClass, NgIf } from '@angular/common';
import { Component, EventEmitter, Output } from '@angular/core';
import { RouterModule } from '@angular/router';
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

  constructor(){}

  DisAppearSideBar() {
    this.appearSideBarNav = false;
    this.dataEmitter.emit(this.appearSideBarNav);
  }

  toggleDropdown(menu: string): void {
    this.openDropdown = this.openDropdown === menu ? null : menu;
  }
}
