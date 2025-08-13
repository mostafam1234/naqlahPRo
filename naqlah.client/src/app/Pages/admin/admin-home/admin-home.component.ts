import { NgClass, NgFor, NgIf } from '@angular/common';
import { Component, AfterViewInit  } from '@angular/core';
import { Chart, ChartData, ChartOptions, ChartType, registerables } from 'chart.js';
import {
  ChoroplethController,
  GeoFeature,
  ColorScale,
  ProjectionScale,
  topojson,
} from 'chartjs-chart-geo';
import { NgChartsModule } from 'ng2-charts';
import { LanguageService } from 'src/app/Core/services/language.service';

Chart.register(
  ...registerables,
  ChoroplethController,
  GeoFeature,
  ColorScale,
  ProjectionScale
);

@Component({
  selector: 'app-admin-home',
  standalone: true,
  imports: [NgChartsModule, NgFor, NgClass, NgIf ],
  templateUrl: './admin-home.component.html',
  styleUrl: './admin-home.component.css'
})
export class AdminHomeComponent implements AfterViewInit  {
  activeTab = 'all';
  currentPage = 1;
  itemsPerPage = 9;
  openDropdownId: number = null;
  lang: string = 'ar';
  viewMode: 'cards' | 'table' = 'table';

  constructor(private languageService: LanguageService,){

  }
  // الخيارات
  barChartOptions: ChartOptions = {
    responsive: true,
  };

  // نوع الرسم البياني
  barChartType: ChartType = 'bar';

  // البيانات كاملة كـ ChartData
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

  // الدونات تشارت
  doughnutChartData: ChartData<'doughnut'> = {
    labels: ['حمـال ثقيلة', 'ملابس', 'أدوية'],
    datasets: [
      {
        data: [40, 30, 30],
        backgroundColor: ['#F6C667', '#B9E2C2', '#A1B9F1'],
        hoverOffset: 10,
      },
    ],
  };

  doughnutChartType: ChartType = 'doughnut';

  // complains chart
  lineChartData: ChartData<'line'> = {
  labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
  datasets: [
    {
      label: 'Total Customer',
      data: [140, 110, 160, 130, 120, 179, 90, 100, 150, 120, 100, 130],
      fill: true,
      borderColor: '#3B82F6',
      backgroundColor: 'rgba(59, 130, 246, 0.2)',
      tension: 0.4,
      pointBackgroundColor: '#3B82F6',
    }
  ]
};

lineChartOptions: ChartOptions<'line'> = {
  responsive: true,
  plugins: {
    tooltip: {
      callbacks: {
        label: function (context) {
          return `Total Customer: ${context.parsed.y}`;
        }
      }
    },
    legend: {
      display: false
    }
  },
  scales: {
    y: {
      beginAtZero: true
    }
  }
};

lineChartType: ChartType = 'line';

barChartDataForComplains: ChartData<'bar'> = {
  labels: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'],
  datasets: [
    {
      label: 'الشكاوي',
      data: [100, 120, 130, 110, 90, 170, 60],
      backgroundColor: ['#B9E2C2', '#A1B9F1', '#F6C667', '#F9B572', '#B0D9E8', '#6DA9E4', '#7FB685'],
      borderRadius: 10,
      barPercentage: 0.6,
    }
  ]
};

barChartOptionsForComplains: ChartOptions<'bar'> = {
  responsive: true,
  plugins: {
    legend: {
      display: false
    },
  },
  scales: {
    y: {
      beginAtZero: true
    }
  }
};

barChartTypeForComplains: ChartType = 'bar';

cities = [
    { name: 'مدينة الدمام', percentage: 20 },
    { name: 'مدينة جدة', percentage: 60 },
    { name: 'مدينة مكة', percentage: 40 },
  ];
  async ngAfterViewInit() {
  const world = await fetch('https://unpkg.com/world-atlas@2/countries-50m.json').then(res => res.json());
  const countries = topojson.feature(world, world.objects.countries) as any;

  const ctx = document.getElementById('worldMapChart') as HTMLCanvasElement;

  new Chart(ctx, {
    type: 'choropleth',
    data: {
      labels: countries.features.map((d: any) => d.properties.name),
      datasets: [{
        label: 'طلبات حسب الدولة',
        data: countries.features.map((d: any) => ({
          feature: d,
          value: Math.floor(Math.random() * 100) + 1
        })),
        borderColor: '#ffffff',
        borderWidth: 0.5,
        backgroundColor: (ctx) => {
          const val = (ctx.raw as { value: number })?.value || 0;
          return val > 50 ? '#3b82f6' : '#9ca3af';
        }
      }]
    },
    options: {
      responsive: true,
      plugins: {
        legend: {
          display: false
        }
      },
      scales: {
        projection: {
          axis: 'x',
          projection: 'equalEarth'
        },
        color: {
          display: false
        }
      }
    }
  });
}

  get language(){
    return this.languageService.getLanguage();
  }
  shipping = [
    {
      id:1,
      type: 'نقل أدوية',
      destination: 'متعددة الواجهات',
      price: '12.00$',
      state: 'جارى العمل على نقل الشحنة',
      stateId:1
    },
    {
      id:2,
      type: 'نقل أدوية',
      destination: 'متعددة الواجهات',
      price: '12.00$',
      state: 'فى الطريق',
      stateId:2
    },
    {
      id:1,
      type: 'نقل أدوية',
      destination: 'متعددة الواجهات',
      price: '12.00$',
      state: 'تم التوصيل',
      stateId:3
    },
    {
      id:1,
      type: 'نقل أدوية',
      destination: 'متعددة الواجهات',
      price: '12.00$',
      state: 'تم الغاء الشحنة',
      stateId:4
    },
    {
      id:1,
      type: 'نقل أدوية',
      destination: 'متعددة الواجهات',
      price: '12.00$',
      state: 'تم التوصيل',
      stateId:3
    },
    {
      id:1,
      type: 'نقل أدوية',
      destination: 'متعددة الواجهات',
      price: '12.00$',
      state: 'فى الطريق',
      stateId:2
    },
    {
      id:1,
      type: 'نقل أدوية',
      destination: 'متعددة الواجهات',
      price: '12.00$',
      state: 'تم التوصيل',
      stateId:3
    },
        {
      id:1,
      type: 'نقل أدوية',
      destination: 'متعددة الواجهات',
      price: '12.00$',
      state: 'تم التوصيل',
      stateId:3
    },
        {
      id:1,
      type: 'نقل أدوية',
      destination: 'متعددة الواجهات',
      price: '12.00$',
      state: 'فى الطريق',
      stateId:2
    },
        {
      id:1,
      type: 'نقل أدوية',
      destination: 'متعددة الواجهات',
      price: '12.00$',
      state: 'تم التوصيل',
      stateId:3
    },
        {
      id:1,
      type: 'نقل أدوية',
      destination: 'متعددة الواجهات',
      price: '12.00$',
      state: 'فى الطريق',
      stateId:2
    },
    {
      id:1,
      type: 'نقل أدوية',
      destination: 'متعددة الواجهات',
      price: '12.00$',
      state: 'فى الطريق',
      stateId:2
    },

  ];

  get filteredshipping() {
    if (this.activeTab === 'all') return this.shipping;
    return this.shipping.filter(c => c.type === this.activeTab);
  }

    get paginatedshipping() {
    const start = (this.currentPage - 1) * this.itemsPerPage;
    return this.filteredshipping.slice(start, start + this.itemsPerPage);
  }

  get totalPages() {
    return Math.ceil(this.filteredshipping.length / this.itemsPerPage);
  }

  changePage(page: number) {
    this.currentPage = page;
  }
}
