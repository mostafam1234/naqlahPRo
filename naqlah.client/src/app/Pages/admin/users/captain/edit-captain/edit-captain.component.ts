import { NgClass, NgFor, NgIf } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ImageService } from 'src/app/Core/services/image.service';

@Component({
  selector: 'app-edit-captain',
  standalone: true,
  imports: [NgIf, RouterModule, ReactiveFormsModule],
  templateUrl: './edit-captain.component.html',
  styleUrl: './edit-captain.component.css'
})
export class EditCaptainComponent {
  captainForm: FormGroup;
  imagePreview: string | ArrayBuffer | null = null;

  constructor(private fb: FormBuilder, private imageService: ImageService) {
    this.captainForm = this.fb.group({
      nameAr: ['', Validators.required],
      nameEn: ['', Validators.required],
      phone: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      type: ['', Validators.required],
      image: [null]
    });
  }

  async onImageSelected(event: Event) {
    const result = await this.imageService.handleImageUpload(event);
    if (result?.success) {
      this.captainForm.patchValue({ image: result.base64 });
      this.captainForm.get('image')?.updateValueAndValidity();
      this.imagePreview = result.preview || null;
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
