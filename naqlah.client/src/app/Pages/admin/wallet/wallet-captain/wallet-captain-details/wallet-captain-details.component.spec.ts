import { ComponentFixture, TestBed } from '@angular/core/testing';

import { WalletCaptainDetailsComponent } from './wallet-captain-details.component';

describe('WalletCaptainDetailsComponent', () => {
  let component: WalletCaptainDetailsComponent;
  let fixture: ComponentFixture<WalletCaptainDetailsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [WalletCaptainDetailsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(WalletCaptainDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
