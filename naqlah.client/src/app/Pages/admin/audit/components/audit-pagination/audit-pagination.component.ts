import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-audit-pagination',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  templateUrl: './audit-pagination.component.html',
  styleUrls: ['./audit-pagination.component.css'],
})
export class AuditPaginationComponent {
  @Input() currentPage = 0;
  @Input() totalPages = 0;
  @Input() totalCount = 0;
  @Input() loading = false;

  @Output() pageChange = new EventEmitter<number>();

  goToPage(page: number): void {
    if (page < 0 || page >= this.totalPages) return;
    this.pageChange.emit(page);
  }

  get hasPrev(): boolean {
    return this.currentPage > 0;
  }

  get hasNext(): boolean {
    return this.currentPage < this.totalPages - 1 && this.totalPages > 0;
  }
}
