import { DatePipe, NgClass, NgFor, NgIf } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { DeliveryManActiveHistoryResponseDto } from 'src/app/Core/services/NaqlahClient';

@Component({
  selector: 'app-captain-active-history-modal',
  standalone: true,
  imports: [NgIf, NgFor, NgClass, DatePipe],
  templateUrl: './captain-active-history-modal.component.html',
  styleUrl: './captain-active-history-modal.component.css'
})
export class CaptainActiveHistoryModalComponent {
  @Input() show = false;
  @Input() loading = false;
  @Input() isExporting = false;
  @Input() data: DeliveryManActiveHistoryResponseDto | null = null;
  @Output() closed = new EventEmitter<void>();
  @Output() exportRequested = new EventEmitter<void>();

  close(): void {
    this.closed.emit();
  }

  onExport(): void {
    this.exportRequested.emit();
  }

  trackHistory(_index: number, item: { id: number }): number {
    return item.id;
  }
}
