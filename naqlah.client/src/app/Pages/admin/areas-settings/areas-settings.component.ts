import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PageHeaderComponent } from 'src/app/shared/components/page-header/page-header.component';
import { RegionsComponent } from './regions/regions.component';
import { CitiesComponent } from './cities/cities.component';
import { NeighborhoodsComponent } from './neighborhoods/neighborhoods.component';

@Component({
  selector: 'app-areas-settings',
  standalone: true,
  imports: [CommonModule, PageHeaderComponent, RegionsComponent, CitiesComponent, NeighborhoodsComponent],
  templateUrl: './areas-settings.component.html',
  styleUrls: ['./areas-settings.component.css']
})
export class AreasSettingsComponent implements OnInit {
  selectedView: 'regions' | 'cities' | 'neighborhoods' = 'regions';

  constructor() { }

  ngOnInit(): void {
  }

  selectView(view: 'regions' | 'cities' | 'neighborhoods'): void {
    this.selectedView = view;
  }

  getTabClass(view: 'regions' | 'cities' | 'neighborhoods'): string {
    const baseClasses = 'flex items-center px-6 py-3 rounded-lg font-medium text-sm transition-all duration-200 ease-in-out';
    const activeClasses = 'bg-primary-500 text-white shadow-md hover:bg-primary-600 transform hover:scale-105';
    const inactiveClasses = 'bg-neutral-100 text-neutral-700 hover:bg-neutral-200 hover:text-neutral-900';
    
    return this.selectedView === view 
      ? `${baseClasses} ${activeClasses}` 
      : `${baseClasses} ${inactiveClasses}`;
  }

  goBack(): void {
    window.history.back();
  }
}

