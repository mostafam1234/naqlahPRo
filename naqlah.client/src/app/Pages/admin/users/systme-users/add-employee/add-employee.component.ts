import { Component } from '@angular/core';
import { NgIf } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
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

  constructor(private fb: FormBuilder) {
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

  onImageSelected(event: Event) {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (file) {
      this.empForm.patchValue({ image: file });
      this.empForm.get('image')?.updateValueAndValidity();

      const reader = new FileReader();
      reader.onload = () => {
        this.imagePreview = reader.result;
      };
      reader.readAsDataURL(file);
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
