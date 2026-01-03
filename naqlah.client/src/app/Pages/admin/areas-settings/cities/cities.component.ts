import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormControl, FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { SubSink } from 'subsink';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { ToasterService } from 'src/app/Core/services/toaster.service';
import { ConfirmationModalComponent } from 'src/app/shared/components/confirmation-modal/confirmation-modal.component';
import { AreasSettingsAdminClient, CityAdminDto, RegionLookupDto, AddCityCommand, UpdateCityCommand } from 'src/app/Core/services/NaqlahClient';


@Component({
  selector: 'app-cities',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, TranslateModule, ConfirmationModalComponent],
  providers: [AreasSettingsAdminClient],
  templateUrl: './cities.component.html',
  styleUrls: ['./cities.component.css']
})
export class CitiesComponent implements OnInit, OnDestroy {
  showModal = false;
  showUpdateModal = false;
  editingItem: CityAdminDto | null = null;
  isLoading = false;

  itemForm: FormGroup;
  searchControl = new FormControl('');
  regionFilterControl = new FormControl<number | null>(null);

  items: CityAdminDto[] = [];
  regions: RegionLookupDto[] = [];

  totalCount = 0;
  totalPages = 0;
  currentPage = 0;
  itemsPerPage = 10;

  showConfirmModal = false;
  confirmationTitle = '';
  confirmationMessage = '';
  private pendingAction?: () => void;

  private sub = new SubSink();

  constructor(
    private fb: FormBuilder,
    private toasterService: ToasterService,
    private areasSettingsClient: AreasSettingsAdminClient
  ) {
    this.itemForm = this.fb.group({
      arabicName: ['', [Validators.required, Validators.maxLength(100)]],
      englishName: ['', [Validators.required, Validators.maxLength(100)]],
      regionId: [null, [Validators.required]]
    });
  }

  ngOnInit(): void {
    this.loadRegions();
    this.loadItems();
    this.setupSearch();
  }

  ngOnDestroy(): void {
    this.sub.unsubscribe();
  }

  setupSearch(): void {
    this.sub.sink = this.searchControl.valueChanges
      .pipe(
        debounceTime(500),
        distinctUntilChanged()
      )
      .subscribe(() => {
        this.currentPage = 0;
        this.loadItems();
      });

    this.sub.sink = this.regionFilterControl.valueChanges.subscribe(() => {
      this.currentPage = 0;
      this.loadItems();
    });
  }

  loadRegions(): void {
    this.sub.sink = this.areasSettingsClient.getAllRegionsLookup().subscribe({
      next: (response) => {
        this.regions = response;
      },
      error: (error) => {
        console.error('Error loading regions:', error);
      }
    });
  }

  loadItems(): void {
    this.isLoading = true;
    const skip = this.currentPage * this.itemsPerPage;
    const searchTerm = this.searchControl.value || '';
    const regionId = this.regionFilterControl.value;
    this.sub.sink = this.areasSettingsClient.getAllCities(skip, this.itemsPerPage, searchTerm, regionId).subscribe({
      next: (response: any) => {
        this.items = response.data;
        this.totalCount = response.totalCount;
        this.totalPages = response.totalPages;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading cities:', error);
        this.isLoading = false;
      }
    });
  }

  openAdd(): void {
    this.editingItem = null;
    this.itemForm.reset();
    this.showModal = true;
  }

  openEdit(item: CityAdminDto): void {
    this.editingItem = item;
    this.itemForm.patchValue({
      arabicName: item.arabicName,
      englishName: item.englishName,
      regionId: item.regionId
    });
    this.showUpdateModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.showUpdateModal = false;
    this.editingItem = null;
    this.itemForm.reset();
  }

  submit(): void {
    if (this.itemForm.invalid) return;
    this.performAdd();
  }

  private performAdd(): void {
    const value = this.itemForm.value;
    const command = new AddCityCommand();
    command.arabicName = value.arabicName;
    command.englishName = value.englishName;
    command.regionId = value.regionId;
    this.sub.sink = this.areasSettingsClient.addCity(command).subscribe({
      next: () => {
        this.closeModal();
        this.toasterService.success('تمت الإضافة بنجاح', 'تمت إضافة المدينة بنجاح');
        this.loadItems();
      },
      error: (error) => {
        this.toasterService.error('خطأ', error?.message || 'حدث خطأ أثناء إضافة المدينة');
      }
    });
  }

  update(): void {
    if (this.itemForm.invalid) return;

    const itemId = this.editingItem?.id;
    const value = this.itemForm.value;
    const command = new UpdateCityCommand();
    command.id = itemId;
    command.arabicName = value.arabicName;
    command.englishName = value.englishName;
    command.regionId = value.regionId;
    this.sub.sink = this.areasSettingsClient.updateCity(command).subscribe({
      next: () => {
        this.closeModal();
        this.toasterService.success('تم التحديث بنجاح', 'تم تحديث المدينة بنجاح');
        this.loadItems();
      },
      error: (error) => {
        this.toasterService.error('خطأ', error?.message || 'حدث خطأ أثناء تحديث المدينة');
      }
    });
  }

  confirmDelete(itemId: number): void {
    this.confirmationTitle = 'تأكيد الحذف';
    this.confirmationMessage = 'هل أنت متأكد من حذف هذه المدينة؟';
    this.pendingAction = () => this.performDelete(itemId);
    this.showConfirmModal = true;
  }

  private performDelete(itemId: number): void {
    this.sub.sink = this.areasSettingsClient.deleteCity(itemId).subscribe({
      next: () => {
        this.toasterService.success('تم الحذف بنجاح', 'تم حذف المدينة بنجاح');
        this.loadItems();
      },
      error: (error) => {
        this.toasterService.error('خطأ', error?.message || 'حدث خطأ أثناء حذف المدينة');
      }
    });
  }

  changePage(page: number): void {
    const backendPage = page - 1;
    if (backendPage >= 0 && backendPage < this.totalPages) {
      this.currentPage = backendPage;
      this.loadItems();
    }
  }

  get displayCurrentPage(): number {
    return this.currentPage + 1;
  }

  get visiblePages(): number[] {
    const current = this.displayCurrentPage;
    const total = this.totalPages;
    const pages: number[] = [];

    if (total <= 7) {
      for (let i = 1; i <= total; i++) {
        pages.push(i);
      }
    } else {
      pages.push(1);
      if (current <= 4) {
        for (let i = 2; i <= 5; i++) {
          pages.push(i);
        }
        pages.push(-1);
        pages.push(total);
      } else if (current >= total - 3) {
        pages.push(-1);
        for (let i = total - 4; i <= total; i++) {
          pages.push(i);
        }
      } else {
        pages.push(-1);
        for (let i = current - 1; i <= current + 1; i++) {
          pages.push(i);
        }
        pages.push(-1);
        pages.push(total);
      }
    }
    return pages;
  }

  get displayStartCount(): number {
    if (this.totalCount === 0) return 0;
    return (this.currentPage * this.itemsPerPage) + 1;
  }

  get displayEndCount(): number {
    if (this.totalCount === 0) return 0;
    const endCount = (this.currentPage + 1) * this.itemsPerPage;
    return Math.min(endCount, this.totalCount);
  }

  onConfirmationConfirmed(): void {
    this.showConfirmModal = false;
    if (this.pendingAction) {
      this.pendingAction();
      this.pendingAction = undefined;
    }
  }

  onConfirmationCancelled(): void {
    this.showConfirmModal = false;
    this.pendingAction = undefined;
  }
}


