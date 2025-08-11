import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SreviceRequestChatComponent } from './srevice-request-chat.component';

describe('SreviceRequestChatComponent', () => {
  let component: SreviceRequestChatComponent;
  let fixture: ComponentFixture<SreviceRequestChatComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SreviceRequestChatComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SreviceRequestChatComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
