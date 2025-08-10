import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CaptainActionComponent } from './captain-action.component';

describe('CaptainActionComponent', () => {
  let component: CaptainActionComponent;
  let fixture: ComponentFixture<CaptainActionComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CaptainActionComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CaptainActionComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
