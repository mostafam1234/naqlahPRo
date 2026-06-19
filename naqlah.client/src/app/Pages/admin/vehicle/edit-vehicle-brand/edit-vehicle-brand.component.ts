import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { SubSink } from 'subsink';
import { finalize } from 'rxjs/operators';
import { PageHeaderComponent } from 'src/app/shared/components/page-header/page-header.component';
import {
  DeliveryManVehicleDto,
  UpdateVehicleBrandCommand,
  VehicleAdminClient
} from 'src/app/Core/services/NaqlahClient';
import { ToasterService } from 'src/app/Core/services/toaster.service';
import { buildBrandForm, loadVehicleBrandById } from '../vehicle-form.helpers';

@Component({
  selector: 'app-edit-vehicle-brand',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule, PageHeaderComponent],
  templateUrl: './edit-vehicle-brand.component.html',
  styleUrl: './edit-vehicle-brand.component.css'
})
export class EditVehicleBrandComponent implements OnInit, OnDestroy {
  form: FormGroup;
  brandId = 0;
  isLoading = true;
  isSubmitting = false;
  private sub = new SubSink();

  constructor(
    private fb: FormBuilder,
    private vehicleClient: VehicleAdminClient,
    private toasterService: ToasterService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.form = buildBrandForm(this.fb);
  }

  ngOnInit(): void {
    this.brandId = Number(this.route.snapshot.paramMap.get('id'));
    const stateItem = history.state?.item as DeliveryManVehicleDto | undefined;

    if (stateItem?.id === this.brandId) {
      this.patchForm(stateItem);
      this.isLoading = false;
      return;
    }

    this.sub.sink = loadVehicleBrandById(this.vehicleClient, this.brandId)
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe((item) => {
        if (!item) {
          this.toasterService.error('خطأ', 'لم يتم العثور على الماركة');
          this.onCancel();
          return;
        }
        this.patchForm(item);
      });
  }

  ngOnDestroy(): void {
    this.sub.unsubscribe();
  }

  private patchForm(item: DeliveryManVehicleDto): void {
    this.form.patchValue({
      arabicName: item.arabicName,
      englishName: item.englishName
    });
  }

  onCancel(): void {
    this.router.navigate(['/admin/vehicles'], { queryParams: { tab: 'brands' } });
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.value;
    const command = new UpdateVehicleBrandCommand();
    command.vehicleBrandId = this.brandId;
    command.arabicName = value.arabicName;
    command.englishName = value.englishName;

    this.isSubmitting = true;
    this.sub.sink = this.vehicleClient
      .updateVehicleBrand(command)
      .pipe(finalize(() => (this.isSubmitting = false)))
      .subscribe({
        next: () => {
          this.toasterService.success('تم التحديث', 'تم تحديث الماركة بنجاح');
          this.onCancel();
        },
        error: (error) => {
          this.toasterService.error('خطأ', error?.message || 'تعذر تحديث الماركة');
        }
      });
  }
}
