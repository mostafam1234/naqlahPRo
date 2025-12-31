import { NgClass, NgFor, NgIf, DecimalPipe } from '@angular/common';
import { Component, AfterViewInit, OnInit, OnDestroy } from '@angular/core';
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
import { DashboardAdminClient, DashboardStatisticsDto, CategoryOrderCountDto, MonthlyCategoryDataDto, CityOrderCountDto, TodayOrderDto, OrderStatus } from 'src/app/Core/services/NaqlahClient';
import { catchError, finalize } from 'rxjs/operators';
import { of } from 'rxjs';
import { SubSink } from 'subsink';
import { TranslateModule } from '@ngx-translate/core';

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
  imports: [NgChartsModule, NgFor, NgClass, NgIf, DecimalPipe, TranslateModule],
  templateUrl: './admin-home.component.html',
  styleUrl: './admin-home.component.css'
})
export class AdminHomeComponent implements AfterViewInit, OnInit, OnDestroy {
  activeTab = 'all';
  currentPage = 1;
  itemsPerPage = 9;
  openDropdownId: number = null;
  lang: string = 'ar';
  viewMode: 'cards' | 'table' = 'table';

  // Dashboard Statistics
  statistics: DashboardStatisticsDto | null = null;
  isLoadingStatistics = false;

  // Chart Data
  topCategories: CategoryOrderCountDto[] = [];
  monthlyCategories: MonthlyCategoryDataDto[] = [];
  isLoadingCharts = false;

  // Cities Data
  topCities: CityOrderCountDto[] = [];
  isLoadingCities = false;
  worldMapChart: Chart | null = null;

  // Today Orders Data
  todayOrders: TodayOrderDto[] = [];
  isLoadingTodayOrders = false;

  // Category colors - consistent palette
  private categoryColors: string[] = [
    '#06B6D4', // cyan-500
    '#10B981', // emerald-500
    '#F59E0B', // amber-500
    '#EF4444', // red-500
    '#8B5CF6', // violet-500
    '#EC4899', // pink-500
    '#14B8A6', // teal-500
    '#F97316'  // orange-500
  ];

  private subs = new SubSink();

  constructor(
    private languageService: LanguageService,
    private dashboardClient: DashboardAdminClient
  ) {}

  ngOnInit(): void {
    this.loadStatistics();
    this.loadChartData();
    this.loadTopCities();
    this.loadTodayOrders();
  }

  ngOnDestroy(): void {
    this.subs.unsubscribe();
  }

  loadStatistics(): void {
    this.isLoadingStatistics = true;
    this.subs.sink = this.dashboardClient.getDashboardStatistics()
      .pipe(
        catchError(error => {
          console.error('Error loading dashboard statistics:', error);
          return of(null);
        }),
        finalize(() => {
          this.isLoadingStatistics = false;
        })
      )
      .subscribe(data => {
        if (data) {
          this.statistics = data;
        }
      });
  }

  loadChartData(): void {
    this.isLoadingCharts = true;
    let loadedCount = 0;
    const totalCharts = 2;
    
    // Load top categories for doughnut chart
    this.subs.add(
      this.dashboardClient.getTopCategoriesByOrderCount()
        .pipe(
          catchError(error => {
            console.error('Error loading top categories:', error);
            return of([]);
          }),
          finalize(() => {
            loadedCount++;
            if (loadedCount >= totalCharts) {
              this.isLoadingCharts = false;
            }
          })
        )
        .subscribe(data => {
          console.log('Top categories loaded:', data);
          this.topCategories = data || [];
          this.updateDoughnutChart();
        })
    );

    // Load monthly categories for bar chart
    this.subs.add(
      this.dashboardClient.getMonthlyTopCategories()
        .pipe(
          catchError(error => {
            console.error('Error loading monthly categories:', error);
            return of([]);
          }),
          finalize(() => {
            loadedCount++;
            if (loadedCount >= totalCharts) {
              this.isLoadingCharts = false;
            }
          })
        )
        .subscribe(data => {
          console.log('Monthly categories loaded:', data);
          this.monthlyCategories = data || [];
          this.updateBarChart();
        })
    );
  }

  getCategoryColor(index: number): string {
    return this.categoryColors[index % this.categoryColors.length];
  }
  // نوع الرسم البياني
  barChartType: ChartType = 'bar';

  updateBarChart(): void {
    if (this.monthlyCategories && this.monthlyCategories.length > 0) {
      // Get month labels
      const labels = this.monthlyCategories.map(m => m.monthName);

      // Get all unique categories from all months (top 4)
      const allCategoryIds = new Set<number>();
      this.monthlyCategories.forEach(month => {
        if (month.categories && month.categories.length > 0) {
          month.categories.forEach(cat => allCategoryIds.add(cat.mainCategoryId));
        }
      });

      const categoryIds = Array.from(allCategoryIds).slice(0, 4);

      if (categoryIds.length === 0) {
        console.warn('No categories found in monthly data');
        return;
      }

      // Create datasets for each category
      const datasets = categoryIds.map((categoryId, index) => {
        const category = this.monthlyCategories
          .flatMap(m => m.categories || [])
          .find(c => c.mainCategoryId === categoryId);
        
        const categoryName = category?.categoryName || '';
        const color = this.getCategoryColor(index);

        // Get data for this category across all months
        const data = this.monthlyCategories.map(month => {
          const catInMonth = (month.categories || []).find(c => c.mainCategoryId === categoryId);
          return catInMonth?.orderCount || 0;
        });

        return {
          label: categoryName,
          data: [...data],
          backgroundColor: color,
        };
      });

      // Create new object reference to trigger Chart.js update
      this.barChartData = {
        labels: [...labels],
        datasets: [...datasets],
      };
      console.log('Bar chart updated:', this.barChartData);
    } else {
      console.warn('No monthly categories data available for bar chart');
    }
  }

  // البيانات كاملة كـ ChartData
  barChartData: ChartData<'bar'> = {
    labels: [],
    datasets: [],
  };

  barChartOptions: ChartOptions<'bar'> = {
    responsive: true,
    plugins: {
      legend: {
        display: true,
        position: 'top',
      },
    },
    scales: {
      y: {
        beginAtZero: true,
      },
    },
  };

  // الدونات تشارت
  doughnutChartData: ChartData<'doughnut'> = {
    labels: [],
    datasets: [
      {
        data: [],
        backgroundColor: [],
        hoverOffset: 10,
      },
    ],
  };

  doughnutChartOptions: ChartOptions<'doughnut'> = {
    responsive: true,
    plugins: {
      legend: {
        display: true,
        position: 'bottom',
      },
    },
  };

  doughnutChartType: ChartType = 'doughnut';

  updateDoughnutChart(): void {
    if (this.topCategories && this.topCategories.length > 0) {
      const labels = this.topCategories.map(c => c.categoryName);
      const data = this.topCategories.map(c => c.orderCount);
      const colors = this.topCategories.map((_, index) => this.getCategoryColor(index));

      // Create new object reference to trigger Chart.js update
      this.doughnutChartData = {
        labels: [...labels],
        datasets: [
          {
            data: [...data],
            backgroundColor: [...colors],
            hoverOffset: 10,
          },
        ],
      };
      console.log('Doughnut chart updated:', this.doughnutChartData);
    } else {
      console.warn('No top categories data available for doughnut chart');
    }
  }

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

  loadTopCities(): void {
    this.isLoadingCities = true;
    this.subs.sink = this.dashboardClient.getTopCitiesByOrderCount()
      .pipe(
        catchError(error => {
          console.error('Error loading top cities:', error);
          return of([]);
        }),
        finalize(() => {
          this.isLoadingCities = false;
        })
      )
      .subscribe(data => {
        this.topCities = data || [];
        this.updateWorldMapChart();
      });
  }
  async ngAfterViewInit() {
    await this.initializeWorldMap();
  }

  async initializeWorldMap(): Promise<void> {
    const ctx = document.getElementById('worldMapChart') as HTMLCanvasElement;
    if (!ctx) return;

    const world = await fetch('https://unpkg.com/world-atlas@2/countries-50m.json').then(res => res.json());
    const countries = topojson.feature(world, world.objects.countries) as any;

    if (this.worldMapChart) {
      this.worldMapChart.destroy();
    }

    this.worldMapChart = new Chart(ctx, {
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
        maintainAspectRatio: false,
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

    // Update map if cities are already loaded
    if (this.topCities.length > 0) {
      this.updateWorldMapChart();
    }
  }

  updateWorldMapChart(): void {
    // This will be enhanced to show city data on the map
    // For now, we'll keep the existing world map visualization
    // Future enhancement: could show city markers or regional heat map
  }

  loadTodayOrders(): void {
    this.isLoadingTodayOrders = true;
    this.subs.sink = this.dashboardClient.getTodayOrders()
      .pipe(
        catchError(error => {
          console.error('Error loading today orders:', error);
          return of([]);
        }),
        finalize(() => {
          this.isLoadingTodayOrders = false;
        })
      )
      .subscribe(data => {
        this.todayOrders = data || [];
      });
  }

  getOrderStatusClass(status: OrderStatus): string {
    switch (status) {
      case OrderStatus.Pending:
        return 'bg-yellow-100 text-yellow-800';
      case OrderStatus.Assigned:
        return 'bg-blue-100 text-blue-700';
      case OrderStatus.Completed:
        return 'bg-green-100 text-green-700';
      case OrderStatus.Cancelled:
        return 'bg-red-100 text-red-700';
      default:
        return 'bg-gray-100 text-gray-700';
    }
  }

  get language(){
    return this.languageService.getLanguage();
  }

  get paginatedTodayOrders() {
    const start = (this.currentPage - 1) * this.itemsPerPage;
    return this.todayOrders.slice(start, start + this.itemsPerPage);
  }

  get totalPages() {
    return Math.ceil(this.todayOrders.length / this.itemsPerPage);
  }

  changePage(page: number) {
    this.currentPage = page;
  }
}
