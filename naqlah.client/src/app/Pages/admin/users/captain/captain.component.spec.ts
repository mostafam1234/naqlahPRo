import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CaptainComponent } from './captain.component';

describe('CaptainComponent', () => {
  let component: CaptainComponent;
  let fixture: ComponentFixture<CaptainComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CaptainComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CaptainComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
