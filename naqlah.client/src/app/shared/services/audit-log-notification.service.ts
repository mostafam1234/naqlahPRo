import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { PermissionService } from './permission.service';
import { SignalRService } from './SignalRService';

@Injectable({
  providedIn: 'root'
})
export class AuditLogNotificationService {
  private countSubject = new BehaviorSubject<number>(0);
  public count$: Observable<number> = this.countSubject.asObservable();

  constructor(
    private signalRService: SignalRService,
    private permissionService: PermissionService
  ) {
    this.signalRService.ListenForNewAuditLog().subscribe(() => {
      if (this.permissionService.hasPermission('CanViewAuditLog')) {
        this.countSubject.next(this.countSubject.value + 1);
        this.playNotificationSound();
      }
    });
  }

  getCount(): number {
    return this.countSubject.value;
  }

  resetCount(): void {
    this.countSubject.next(0);
  }

  private playNotificationSound(): void {
    try {
      const audio = new Audio('assets/sounds/notification.wav');
      audio.volume = 1;
      audio.play().catch(() => this.playBeepSound());
    } catch {
      this.playBeepSound();
    }
  }

  private playBeepSound(): void {
    try {
      const audioContext = new (window.AudioContext || (window as any).webkitAudioContext)();
      const oscillator = audioContext.createOscillator();
      const gainNode = audioContext.createGain();
      oscillator.connect(gainNode);
      gainNode.connect(audioContext.destination);
      oscillator.frequency.value = 800;
      oscillator.type = 'sine';
      gainNode.gain.setValueAtTime(0.3, audioContext.currentTime);
      gainNode.gain.exponentialRampToValueAtTime(0.01, audioContext.currentTime + 0.2);
      oscillator.start(audioContext.currentTime);
      oscillator.stop(audioContext.currentTime + 0.2);
    } catch {}
  }
}
