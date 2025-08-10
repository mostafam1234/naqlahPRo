import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SystmeUsersComponent } from './systme-users.component';

describe('SystmeUsersComponent', () => {
  let component: SystmeUsersComponent;
  let fixture: ComponentFixture<SystmeUsersComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SystmeUsersComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SystmeUsersComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
