import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ChatReviewComponent } from './chat-review.component';

describe('ChatReviewComponent', () => {
  let component: ChatReviewComponent;
  let fixture: ComponentFixture<ChatReviewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ChatReviewComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ChatReviewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
