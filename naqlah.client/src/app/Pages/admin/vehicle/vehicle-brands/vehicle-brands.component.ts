import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-vehicle-brands',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule],
  templateUrl: './vehicle-brands.component.html',
  styleUrls: ['./vehicle-brands.component.css']
})
export class VehicleBrandsComponent implements OnInit {
  brandForm: FormGroup;
  showModal = false;
  editingBrand: any = null;

  // displayed list (mock/sample). Backend wiring is handled elsewhere per your request.
  brands: Array<{ id: number; arabicName: string; englishName: string }> = [];

  // sample data for design preview
  private sampleBrands = [
    { id: 1, arabicName: 'تويوتا', englishName: 'Toyota' },
    { id: 2, arabicName: 'هيونداي', englishName: 'Hyundai' },
    { id: 3, arabicName: 'كيا', englishName: 'Kia' }
  ];

  constructor(private fb: FormBuilder) {
    this.brandForm = this.fb.group({
      arabicName: ['', [Validators.required, Validators.maxLength(100)]],
      englishName: ['', [Validators.required, Validators.maxLength(100)]]
    });
  }

  ngOnInit(): void {
    // use local sample data so UI shows immediately
    this.brands = [...this.sampleBrands];
  }

  openAdd(): void {
    this.editingBrand = null;
    this.brandForm.reset();
    this.showModal = true;
  }

  openEdit(row: any): void {
    this.editingBrand = row;
    this.brandForm.patchValue({ arabicName: row.arabicName, englishName: row.englishName });
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.editingBrand = null;
    this.brandForm.reset();
  }

  deleteLocal(row: any): void {
    // design-only local delete with confirmation
    if (!confirm('Delete this brand?')) return;
    this.brands = this.brands.filter(b => b.id !== row.id);
  }

  submit(): void {
    if (this.brandForm.invalid) return;
    const value = this.brandForm.value;

    if (this.editingBrand) {
      // update local item
      this.brands = this.brands.map(b => (b.id === this.editingBrand.id ? { ...b, ...value } : b));
    } else {
      // add locally with a simple id
      const newId = (this.brands.length ? Math.max(...this.brands.map(b => b.id)) : 0) + 1;
      this.brands = [{ id: newId, ...value }, ...this.brands];
    }

    this.closeModal();
  }
}
