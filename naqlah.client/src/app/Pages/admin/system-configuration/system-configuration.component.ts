import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { PageHeaderComponent } from 'src/app/shared/components/page-header/page-header.component';

@Component({
  selector: 'app-system-configuration',
  templateUrl: './system-configuration.component.html',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, TranslateModule, PageHeaderComponent]
})
export class SystemConfigurationComponent implements OnInit {

  configForm!: FormGroup;

  constructor(private fb: FormBuilder) {}

  ngOnInit(): void {
    this.configForm = this.fb.group({
      baseKm: [0, Validators.required],
      baseKmRate: [0, Validators.required],
      extraKmRate: [0, Validators.required],
      baseHours: [0, Validators.required],
      baseHourRate: [0, Validators.required],
      extraHourRate: [0, Validators.required],
      vatRate: [0, Validators.required],
      serviceFees: [0, Validators.required]
    });

    // TODO: load configuration from API
  }

  save(): void {
    if (this.configForm.invalid) return;

    const model = this.configForm.value;
    console.log('Saving system configuration', model);

    // TODO: call API
  }

  goBack(): void {
    history.back();
  }
}
