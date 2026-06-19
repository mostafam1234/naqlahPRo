import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class FormalDropdownCoordinatorService {
  private counter = 0;
  private activeId: string | null = null;
  private readonly activeChange = new Subject<string | null>();

  readonly activeChange$ = this.activeChange.asObservable();

  createInstanceId(): string {
    this.counter += 1;
    return `formal-dd-${this.counter}`;
  }

  notifyOpen(instanceId: string): void {
    this.activeId = instanceId;
    this.activeChange.next(instanceId);
  }

  notifyClose(instanceId: string): void {
    if (this.activeId === instanceId) {
      this.activeId = null;
      this.activeChange.next(null);
    }
  }
}
