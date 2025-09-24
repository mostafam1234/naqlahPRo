import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-vehicle-types',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule],
  templateUrl: './vehicle-types.component.html',
  styleUrls: ['./vehicle-types.component.css']
})
export class VehicleTypesComponent implements OnInit {
  typeForm: FormGroup;
  showModal = false;
  editingType: any = null;

  types: Array<{ id: number; arabicName: string; englishName: string }> = [];
  private sampleTypes = [
    { id: 1, arabicName: 'دراجة نارية', englishName: 'Motorbike' },
    { id: 2, arabicName: 'سيارة صغيرة', englishName: 'Small Car' }
  ];

  constructor(private fb: FormBuilder) {
    this.typeForm = this.fb.group({
      arabicName: ['', [Validators.required, Validators.maxLength(100)]],
      englishName: ['', [Validators.required, Validators.maxLength(100)]]
    });
  }

  ngOnInit(): void {
    this.types = [...this.sampleTypes];
  }

  openAdd(): void { this.editingType = null; this.typeForm.reset(); this.showModal = true; }
  openEdit(t: any){ this.editingType = t; this.typeForm.patchValue({ arabicName: t.arabicName, englishName: t.englishName }); this.showModal = true; }
  closeModal(){ this.showModal = false; this.editingType = null; this.typeForm.reset(); }
  deleteLocal(t: any){ if(!confirm('Delete this type?')) return; this.types = this.types.filter(x => x.id !== t.id); }
  submit(){ if(this.typeForm.invalid) return; const v = this.typeForm.value; if(this.editingType){ this.types = this.types.map(x => x.id === this.editingType.id ? { ...x, ...v } : x); } else { const id = (this.types.length ? Math.max(...this.types.map(x=>x.id)) : 0) + 1; this.types = [{ id, ...v }, ...this.types]; } this.closeModal(); }
}
