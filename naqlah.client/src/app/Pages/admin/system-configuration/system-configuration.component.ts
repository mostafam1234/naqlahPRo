import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { Subject, tap, takeUntil, catchError, finalize, of } from 'rxjs';
import { SystemConfigurationClient, SystemConfigurationDto, UpdateSystemConfigurationCommand } from 'src/app/Core/services/NaqlahClient';
import { ToasterService } from 'src/app/Core/services/toaster.service';
import { PageHeaderComponent } from 'src/app/shared/components/page-header/page-header.component';
import { PermissionService } from 'src/app/shared/services/permission.service';

@Component({
  selector: 'app-system-configuration',
  templateUrl: './system-configuration.component.html',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    TranslateModule,
    PageHeaderComponent
  ],
  providers: [SystemConfigurationClient,ToasterService]
})
export class SystemConfigurationComponent implements OnInit, OnDestroy {

  configForm!: FormGroup;

  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private permissionService: PermissionService,
    private client: SystemConfigurationClient,
    private toasterService: ToasterService
  ) {}

  ngOnInit(): void {
    this.buildForm();
    this.loadConfiguration();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  hasPermission(name: string): boolean {
    return this.permissionService.hasPermission(name);
  }

  private buildForm(): void {
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
  }

  private loadConfiguration(): void {
    this.client
      .getSystemConfiguration()
      .pipe(
        tap(config => this.patchForm(config)),
        takeUntil(this.destroy$)
      )
      .subscribe();
  }

   patchForm(config: SystemConfigurationDto): void {
    this.configForm.patchValue({
      baseKm: config.baseKm,
      baseKmRate: config.baseKmRate,
      extraKmRate: config.extraKmRate,
      baseHours: config.baseHours,
      baseHourRate: config.baseHourRate,
      extraHourRate: config.extraHourRate,
      vatRate: config.vatRate,
      serviceFees: config.serviceFess 
    });
  }

   mapFormToCommand(): UpdateSystemConfigurationCommand {
  const formValue = this.configForm.value;

  var request=new UpdateSystemConfigurationCommand();
  request.baseKm=formValue.baseKm;
  request.baseKmRate=formValue.baseKmRate;
  request.extraKmRate=formValue.extraKmRate;
  request.baseHours=formValue.baseHours;
  request.baseHourRate=formValue.baseHourRate;
  request.extraHourRate=formValue.extraHourRate;
  request.vatRate=formValue.vatRate;
  request.serviceFess=formValue.serviceFees;
  request.id=1;

  return request;
}


  save(): void {
    debugger;
     
    if (this.configForm.invalid) return;
          var request=this.mapFormToCommand();
     this.client.update(request)
          .pipe(
            catchError(error => {
              console.error('Error loading order details:', error);
              this.toasterService.error('خطأ', 'حدث خطأ في حفظ الاعدادات');
              return of(null);
            }),
            finalize(() => {
             
            })
          )
          .subscribe(response => {
           this.toasterService.success("حفظ الاعدادات","لقد تم حفظ الاعدادات بنجاح")
          });

   
    
    
  }

  goBack(): void {
    history.back();
  }
}
