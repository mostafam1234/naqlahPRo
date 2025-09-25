import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-vehicle',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule],
  templateUrl: './vehicle.component.html',
  styleUrls: ['./vehicle.component.css']
})
export class VehicleComponent {
  menu: VehicleMenuItem[] = [
    { route: 'brands', title: 'ADMIN.VEHICLESMENUE.BRANDS', desc: 'ADMIN.VEHICLESMENUE.BRANDS_DESC', icon: 'bi bi-bookmark-star' },
    { route: 'types', title: 'ADMIN.VEHICLESMENUE.TYPES', desc: 'ADMIN.VEHICLESMENUE.TYPES_DESC', icon: 'bi bi-list-ul' }
  ];

}
export interface VehicleMenuItem { route: string; title: string; desc: string; icon: string }

// Add menu data for the template to read
VehicleComponent.prototype.menu = [
  { route: 'brands', title: 'ADMIN.VEHICLE.MENU.BRANDS', desc: 'ADMIN.VEHICLE.MENU.BRANDS_DESC', icon: 'bi bi-bookmark-star' },
  { route: 'types', title: 'ADMIN.VEHICLE.MENU.TYPES', desc: 'ADMIN.VEHICLE.MENU.TYPES_DESC', icon: 'bi bi-list-ul' }
];
