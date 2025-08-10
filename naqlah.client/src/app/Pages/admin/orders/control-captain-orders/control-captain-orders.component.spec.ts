import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ControlCaptainOrdersComponent } from './control-captain-orders.component';

describe('ControlCaptainOrdersComponent', () => {
  let component: ControlCaptainOrdersComponent;
  let fixture: ComponentFixture<ControlCaptainOrdersComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ControlCaptainOrdersComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ControlCaptainOrdersComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
