import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LandingPageLayoutComponent } from './landing-page-layout.component';

describe('LandingPageLayoutComponent', () => {
  let component: LandingPageLayoutComponent;
  let fixture: ComponentFixture<LandingPageLayoutComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LandingPageLayoutComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(LandingPageLayoutComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
