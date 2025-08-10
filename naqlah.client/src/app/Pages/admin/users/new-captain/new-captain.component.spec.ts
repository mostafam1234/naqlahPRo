import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NewCaptainComponent } from './new-captain.component';

describe('NewCaptainComponent', () => {
  let component: NewCaptainComponent;
  let fixture: ComponentFixture<NewCaptainComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NewCaptainComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(NewCaptainComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
