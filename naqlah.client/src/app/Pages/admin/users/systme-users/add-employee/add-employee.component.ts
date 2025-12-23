import { Component } from '@angular/core';
import { NgIf } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ImageService } from 'src/app/Core/services/image.service';
@Component({
  selector: 'app-add-employee',
  standalone: true,
  imports: [ReactiveFormsModule, NgIf, RouterModule],
  templateUrl: './add-employee.component.html',
  styleUrl: './add-employee.component.css'
})
export class AddEmployeeComponent {
  empForm: FormGroup;
  imagePreview: string | ArrayBuffer | null = null;

  constructor(private fb: FormBuilder, private imageService: ImageService) {
    this.empForm = this.fb.group({
      nameAr: ['', Validators.required],
      nameEn: ['', Validators.required],
      age:[null, Validators.required],
      phone: ['', Validators.required],
      nationality: ['', [Validators.required, Validators.email]],
      password: [null,Validators.required],
      job: ['', Validators.required],
      jobType: ['', Validators.required],
      image: [null]
    });
  }

  async onImageSelected(event: Event) {
    const result = await this.imageService.handleImageUpload(event);
    if (result?.success) {
      this.empForm.patchValue({ image: result.base64 });
      this.empForm.get('image')?.updateValueAndValidity();
      this.imagePreview = result.preview || null;
    }
  }

  submitForm() {
    if (this.empForm.valid) {
      console.log( this.empForm.value);
    } else {
      this.empForm.markAllAsTouched();
    }
  }
}
