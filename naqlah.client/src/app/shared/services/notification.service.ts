import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { NotificationDto, NotificationAdminClient } from 'src/app/Core/services/NaqlahClient';
import { LanguageService } from 'src/app/Core/services/language.service';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private notificationsSubject = new BehaviorSubject<NotificationDto[]>([]);
  public notifications$ = this.notificationsSubject.asObservable();

  private unreadCountSubject = new BehaviorSubject<number>(0);
  public unreadCount$ = this.unreadCountSubject.asObservable();

  constructor(
    private notificationAdminClient: NotificationAdminClient,
    private languageService: LanguageService
  ) {
    // Don't load notifications here - let components decide when to load
  }

  loadNotifications(): void {
    this.notificationAdminClient.getNotifications(0, 20, false).subscribe({
      next: (result) => {
        // API already returns Title/Message based on language, so we can use them directly
        this.notificationsSubject.next(result.data);
      },
      error: (error) => {
        console.error('Error loading notifications:', error);
      }
    });
  }

  loadUnreadCount(): void {
    this.notificationAdminClient.getUnreadNotificationsCount().subscribe({
      next: (count) => {
        this.unreadCountSubject.next(count);
      },
      error: (error) => {
        console.error('Error loading unread count:', error);
      }
    });
  }

  addNotification(notificationData: any): void {
    console.log('📥 Adding notification from SignalR:', notificationData);
    const currentNotifications = this.notificationsSubject.value;
    console.log('📥 Current notifications count:', currentNotifications.length);
    const isArabic = this.languageService.getLanguage() === 'ar';
    
    // Map notification based on language - Backend sends ArabicTitle/EnglishTitle (capital letters)
    const mappedData: any = { ...notificationData };
    
    // Handle both camelCase and PascalCase property names
    const arabicTitle = notificationData.ArabicTitle || notificationData.arabicTitle || notificationData.title || '';
    const englishTitle = notificationData.EnglishTitle || notificationData.englishTitle || notificationData.title || '';
    const arabicMessage = notificationData.ArabicMessage || notificationData.arabicMessage || notificationData.message || '';
    const englishMessage = notificationData.EnglishMessage || notificationData.englishMessage || notificationData.message || '';
    
    mappedData.title = isArabic ? arabicTitle : englishTitle;
    mappedData.message = isArabic ? arabicMessage : englishMessage;
    mappedData.id = notificationData.Id || notificationData.id || 0;
    mappedData.orderId = notificationData.OrderId !== undefined ? notificationData.OrderId : (notificationData.orderId !== undefined ? notificationData.orderId : null);
    mappedData.notificationType = notificationData.NotificationType || notificationData.notificationType || 1;
    // Parse creation date - handle both string and Date object
    // Backend sends dates as ISO strings which JavaScript's Date constructor handles correctly
    const creationDateValue = notificationData.CreationDate || notificationData.creationDate;
    if (creationDateValue) {
      // If it's already a Date object, use it; otherwise parse the string
      if (creationDateValue instanceof Date) {
        mappedData.creationDate = creationDateValue;
      } else {
        // Parse the date string - JavaScript Date handles ISO strings correctly
        mappedData.creationDate = new Date(creationDateValue);
      }
    } else {
      // Default to current time if no date provided
      mappedData.creationDate = new Date();
    }
    mappedData.isRead = notificationData.IsRead !== undefined ? notificationData.IsRead : (notificationData.isRead !== undefined ? notificationData.isRead : false);
    
    const mappedNotification = new NotificationDto();
    mappedNotification.init(mappedData);
    
    console.log('✅ Mapped notification:', mappedNotification);
    console.log('✅ Notification title:', mappedNotification.title);
    console.log('✅ Notification message:', mappedNotification.message);
    
    // Play notification sound
    this.playNotificationSound();
    
    // Add to the beginning of the array
    const updatedNotifications = [mappedNotification, ...currentNotifications];
    console.log('✅ Updated notifications count:', updatedNotifications.length);
    this.notificationsSubject.next(updatedNotifications);
    console.log('✅ Notifications subject updated');
    
    // Update unread count
    if (!mappedNotification.isRead) {
      const newCount = this.unreadCountSubject.value + 1;
      console.log('✅ Updating unread count to:', newCount);
      this.unreadCountSubject.next(newCount);
    }
  }

  private playNotificationSound(): void {
    try {
      // Create audio element and play notification sound
      const audio = new Audio('assets/sounds/notification.wav');
      audio.volume = 1; // Set volume to 50%
      audio.play().catch(error => {
        console.warn('Could not play notification sound:', error);
        // If the sound file doesn't exist, try playing a beep sound using Web Audio API
        this.playBeepSound();
      });
    } catch (error) {
      console.warn('Error playing notification sound:', error);
      this.playBeepSound();
    }
  }

  private playBeepSound(): void {
    try {
      // Fallback: Generate a simple beep sound using Web Audio API
      const audioContext = new (window.AudioContext || (window as any).webkitAudioContext)();
      const oscillator = audioContext.createOscillator();
      const gainNode = audioContext.createGain();
      
      oscillator.connect(gainNode);
      gainNode.connect(audioContext.destination);
      
      oscillator.frequency.value = 800; // Frequency in Hz (higher pitch)
      oscillator.type = 'sine';
      
      gainNode.gain.setValueAtTime(0.3, audioContext.currentTime);
      gainNode.gain.exponentialRampToValueAtTime(0.01, audioContext.currentTime + 0.2);
      
      oscillator.start(audioContext.currentTime);
      oscillator.stop(audioContext.currentTime + 0.2);
    } catch (error) {
      console.warn('Could not play beep sound:', error);
    }
  }

  markAsRead(notificationId: number): void {
    this.notificationAdminClient.markNotificationAsRead(notificationId).subscribe({
      next: () => {
        const currentNotifications = this.notificationsSubject.value;
        const updatedNotifications = currentNotifications.map(n => {
          if (n.id === notificationId) {
            const updated = new NotificationDto();
            updated.init({ ...(n as any), isRead: true });
            return updated;
          }
          return n;
        });
        this.notificationsSubject.next(updatedNotifications);
        this.loadUnreadCount();
      },
      error: (error) => {
        console.error('Error marking notification as read:', error);
      }
    });
  }

  markAllAsRead(): void {
    this.notificationAdminClient.markAllNotificationsAsRead().subscribe({
      next: () => {
        const currentNotifications = this.notificationsSubject.value;
        const updatedNotifications = currentNotifications.map(n => {
          const updated = new NotificationDto();
          updated.init({ ...(n as any), isRead: true });
          return updated;
        });
        this.notificationsSubject.next(updatedNotifications);
        this.unreadCountSubject.next(0);
      },
      error: (error) => {
        console.error('Error marking all notifications as read:', error);
      }
    });
  }

  getUnreadCount(): number {
    return this.unreadCountSubject.value;
  }

  getNotifications(): NotificationDto[] {
    return this.notificationsSubject.value;
  }
}
