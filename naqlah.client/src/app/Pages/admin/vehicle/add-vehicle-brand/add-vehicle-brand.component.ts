import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { Router } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { SubSink } from 'subsink';
import { finalize } from 'rxjs/operators';
import { PageHeaderComponent } from 'src/app/shared/components/page-header/page-header.component';
import { AddVehicleBrandCommand, VehicleAdminClient } from 'src/app/Core/services/NaqlahClient';
import { ToasterService } from 'src/app/Core/services/toaster.service';
import { buildBrandForm } from '../vehicle-form.helpers';

@Component({
  selector: 'app-add-vehicle-brand',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule, PageHeaderComponent],
  templateUrl: './add-vehicle-brand.component.html',
  styleUrl: './add-vehicle-brand.component.css'
})
export class AddVehicleBrandComponent implements OnInit, OnDestroy {
  form: FormGroup;
  isSubmitting = false;
  private sub = new SubSink();

  constructor(
    private fb: FormBuilder,
    private vehicleClient: VehicleAdminClient,
    private toasterService: ToasterService,
    private router: Router
  ) {
    this.form = buildBrandForm(this.fb);
  }

  ngOnInit(): void {}

  ngOnDestroy(): void {
    this.sub.unsubscribe();
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
    const command = new AddVehicleBrandCommand();
    command.arabicName = value.arabicName;
    command.englishName = value.englishName;

    this.isSubmitting = true;
    this.sub.sink = this.vehicleClient
      .addVehicleBrand(command)
      .pipe(finalize(() => (this.isSubmitting = false)))
      .subscribe({
        next: () => {
          this.toasterService.success('تمت الإضافة', 'تمت إضافة الماركة بنجاح');
          this.onCancel();
        },
        error: (error) => {
          this.toasterService.error('خطأ', error?.message || 'تعذر إضافة الماركة');
        }
      });
  }
}
