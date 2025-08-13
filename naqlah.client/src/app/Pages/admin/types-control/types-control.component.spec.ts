import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TypesControlComponent } from './types-control.component';

describe('TypesControlComponent', () => {
  let component: TypesControlComponent;
  let fixture: ComponentFixture<TypesControlComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TypesControlComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TypesControlComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
