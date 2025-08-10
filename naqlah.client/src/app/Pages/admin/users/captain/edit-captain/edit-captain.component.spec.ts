import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EditCaptainComponent } from './edit-captain.component';

describe('EditCaptainComponent', () => {
  let component: EditCaptainComponent;
  let fixture: ComponentFixture<EditCaptainComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EditCaptainComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EditCaptainComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
