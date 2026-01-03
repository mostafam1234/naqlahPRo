import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormControl, FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { SubSink } from 'subsink';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { ToasterService } from 'src/app/Core/services/toaster.service';
import { ConfirmationModalComponent } from 'src/app/shared/components/confirmation-modal/confirmation-modal.component';
import { AreasSettingsAdminClient, CityLookupDto, NeighborhoodAdminDto, AddNeighborhoodCommand, UpdateNeighborhoodCommand } from 'src/app/Core/services/NaqlahClient';


@Component({
  selector: 'app-neighborhoods',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, TranslateModule, ConfirmationModalComponent],
  providers: [AreasSettingsAdminClient],
  templateUrl: './neighborhoods.component.html',
  styleUrls: ['./neighborhoods.component.css']
})
export class NeighborhoodsComponent implements OnInit, OnDestroy {
  showModal = false;
  showUpdateModal = false;
  editingItem: NeighborhoodAdminDto | null = null;
  isLoading = false;

  itemForm: FormGroup;
  searchControl = new FormControl('');
  cityFilterControl = new FormControl<number | null>(null);

  items: NeighborhoodAdminDto[] = [];
  cities: CityLookupDto[] = [];

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
      cityId: [null, [Validators.required]]
    });
  }

  ngOnInit(): void {
    this.loadCities();
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

    this.sub.sink = this.cityFilterControl.valueChanges.subscribe(() => {
      this.currentPage = 0;
      this.loadItems();
    });
  }

  loadCities(): void {
    this.sub.sink = this.areasSettingsClient.getAllCitiesLookup().subscribe({
      next: (response) => {
        this.cities = response;
      },
      error: (error) => {
        console.error('Error loading cities:', error);
      }
    });
  }

  loadItems(): void {
    this.isLoading = true;
    const skip = this.currentPage * this.itemsPerPage;
    const searchTerm = this.searchControl.value || '';
    const cityId = this.cityFilterControl.value;
    this.sub.sink = this.areasSettingsClient.getAllNeighborhoods(skip, this.itemsPerPage, searchTerm, cityId).subscribe({
      next: (response: any) => {
        this.items = response.data;
        this.totalCount = response.totalCount;
        this.totalPages = response.totalPages;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading neighborhoods:', error);
        this.isLoading = false;
      }
    });
  }

  openAdd(): void {
    this.editingItem = null;
    this.itemForm.reset();
    this.showModal = true;
  }

  openEdit(item: NeighborhoodAdminDto): void {
    this.editingItem = item;
    this.itemForm.patchValue({
      arabicName: item.arabicName,
      englishName: item.englishName,
      cityId: item.cityId
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
    const command = new AddNeighborhoodCommand();
    command.arabicName = value.arabicName;
    command.englishName = value.englishName;
    command.cityId = value.cityId;
    this.sub.sink = this.areasSettingsClient.addNeighborhood(command).subscribe({
      next: () => {
        this.closeModal();
        this.toasterService.success('تمت الإضافة بنجاح', 'تمت إضافة الحي بنجاح');
        this.loadItems();
      },
      error: (error) => {
        this.toasterService.error('خطأ', error?.message || 'حدث خطأ أثناء إضافة الحي');
      }
    });
  }

  update(): void {
    if (this.itemForm.invalid) return;

    const itemId = this.editingItem?.id;
    const value = this.itemForm.value;
    const command = new UpdateNeighborhoodCommand();
    command.id = itemId;
    command.arabicName = value.arabicName;
    command.englishName = value.englishName;
    command.cityId = value.cityId;
    this.sub.sink = this.areasSettingsClient.updateNeighborhood(command).subscribe({
      next: () => {
        this.closeModal();
        this.toasterService.success('تم التحديث بنجاح', 'تم تحديث الحي بنجاح');
        this.loadItems();
      },
      error: (error) => {
        this.toasterService.error('خطأ', error?.message || 'حدث خطأ أثناء تحديث الحي');
      }
    });
  }

  confirmDelete(itemId: number): void {
    this.confirmationTitle = 'تأكيد الحذف';
    this.confirmationMessage = 'هل أنت متأكد من حذف هذا الحي؟';
    this.pendingAction = () => this.performDelete(itemId);
    this.showConfirmModal = true;
  }

  private performDelete(itemId: number): void {
    this.sub.sink = this.areasSettingsClient.deleteNeighborhood(itemId).subscribe({
      next: () => {
        this.toasterService.success('تم الحذف بنجاح', 'تم حذف الحي بنجاح');
        this.loadItems();
      },
      error: (error) => {
        this.toasterService.error('خطأ', error?.message || 'حدث خطأ أثناء حذف الحي');
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


