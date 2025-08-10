import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddCaptainComponent } from './add-captain.component';

describe('AddCaptainComponent', () => {
  let component: AddCaptainComponent;
  let fixture: ComponentFixture<AddCaptainComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddCaptainComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AddCaptainComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
