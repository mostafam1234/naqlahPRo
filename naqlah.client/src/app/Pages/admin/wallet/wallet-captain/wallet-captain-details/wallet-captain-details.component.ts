import { NgFor, NgIf } from '@angular/common';
import { AfterViewInit, Component, ElementRef, ViewChild } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { Chart, ChartData, ChartOptions, ChartType, registerables } from 'chart.js';
import { ChoroplethController, ColorScale, GeoFeature, ProjectionScale } from 'chartjs-chart-geo';
import { NgChartsModule } from 'ng2-charts';

Chart.register(
  ...registerables,
  ChoroplethController,
  GeoFeature,
  ColorScale,
  ProjectionScale
);

@Component({
  selector: 'app-wallet-captain-details',
  standalone: true,
  imports: [NgChartsModule, NgIf, TranslateModule, NgFor, RouterLink],
  templateUrl: './wallet-captain-details.component.html',
  styleUrl: './wallet-captain-details.component.css'
})
export class WalletCaptainDetailsComponent {
  currentPage = 1;
  itemsPerPage = 9;
  captains = [
    {
      id:1,
      name: 'أحمد يوسف',
      code:'8531245',
      processNumber: '123456',
      date: '05/07/2025',
      requestNumber: '12452',
      totalPrice: 1245
    },
    {
      id:1,
      name: 'أحمد يوسف',
      code:'8531245',
      processNumber: '123456',
      date: '05/07/2025',
      requestNumber: '12452',
      totalPrice: 1245
    },
    {
      id:1,
      name: 'أحمد يوسف',
      code:'8531245',
      processNumber: '123456',
      date: '05/07/2025',
      requestNumber: '12452',
      totalPrice: 1245
    },
    {
      id:1,
      name: 'أحمد يوسف',
      code:'8531245',
      processNumber: '123456',
      date: '05/07/2025',
      requestNumber: '12452',
      totalPrice: 1245
    },
    {
      id:1,
      name: 'أحمد يوسف',
      code:'8531245',
      processNumber: '123456',
      date: '05/07/2025',
      requestNumber: '12452',
      totalPrice: 1245
    },
    {
      id:1,
      name: 'أحمد يوسف',
      code:'8531245',
      processNumber: '123456',
      date: '05/07/2025',
      requestNumber: '12452',
      totalPrice: 1245
    }
  ];

  get paginatedCaptains() {
    const start = (this.currentPage - 1) * this.itemsPerPage;
    return this.captains.slice(start, start + this.itemsPerPage);
  }

  get totalPages() {
    return Math.ceil(this.captains.length / this.itemsPerPage);
  }

  changePage(page: number) {
    this.currentPage = page;
  }

  barChartOptions: ChartOptions = {
    responsive: true,
  };

  barChartType: ChartType = 'bar';

  barChartData: ChartData<'bar'> = {
    labels: ['Sept 1-6', 'Sept 7-12', 'Sept 13-18', 'Sept 19-24', 'Sept 25-30'],
    datasets: [
      {
        label: 'أدوية',
        data: [30, 30, 32, 33, 30],
        backgroundColor: '#7DBAC8',
      },
      {
        label: 'ملابس',
        data: [20, 22, 23, 24, 10],
        backgroundColor: '#F6C667',
      },
      {
        label: 'حمـال ثقيلة',
        data: [45, 42, 46, 50, 12],
        backgroundColor: '#5BA4F0',
      },
    ],
  };
}
