import { NgIf } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-add-captain',
  standalone: true,
  imports: [ReactiveFormsModule, NgIf, RouterModule],
  templateUrl: './add-captain.component.html',
  styleUrl: './add-captain.component.css'
})
export class AddCaptainComponent {
  captainForm: FormGroup;
  imagePreview: string | ArrayBuffer | null = null;

  constructor(private fb: FormBuilder) {
    this.captainForm = this.fb.group({
      nameAr: ['', Validators.required],
      nameEn: ['', Validators.required],
      phone: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      type: ['', Validators.required],
      image: [null]
    });
  }

  onImageSelected(event: Event) {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (file) {
      this.captainForm.patchValue({ image: file });
      this.captainForm.get('image')?.updateValueAndValidity();

      const reader = new FileReader();
      reader.onload = () => {
        this.imagePreview = reader.result;
      };
      reader.readAsDataURL(file);
    }
  }

  submitForm() {
    if (this.captainForm.valid) {
      console.log( this.captainForm.value);
    } else {
      this.captainForm.markAllAsTouched();
    }
  }
}
