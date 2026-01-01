import { DatePipe, NgClass, NgFor, NgIf } from '@angular/common';
import { Component, HostListener, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { NotificationDto, NotificationType } from 'src/app/Core/services/NaqlahClient';
import { NotificationService } from 'src/app/shared/services/notification.service';
import { SignalRService } from 'src/app/shared/services/SignalRService';
import { AuthService } from 'src/app/shared/services/auth.service';
import { LanguageService } from 'src/app/Core/services/language.service';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-notifications',
  standalone: true,
  imports: [NgClass, NgIf, NgFor, DatePipe, TranslateModule],
  templateUrl: './notifications.component.html',
  styleUrl: './notifications.component.css'
})
export class NotificationsComponent implements OnInit, OnDestroy {
  isOpen = false;
  notifications: NotificationDto[] = [];
  unreadCount: number = 0;
  private subscriptions = new Subscription();

  constructor(
    private notificationService: NotificationService,
    private signalRService: SignalRService,
    private authService: AuthService,
    private router: Router,
    private languageService: LanguageService
  ) {}

  ngOnInit(): void {
    // Load initial notifications
    this.notificationService.loadNotifications();
    this.notificationService.loadUnreadCount();

    this.subscriptions.add(
      this.notificationService.notifications$.subscribe(notifications => {
        console.log('📋 Notifications updated in component, count:', notifications.length);
        this.notifications = notifications;
        console.log('📋 Component notifications array updated');
      })
    );

    // Subscribe to unread count
    this.subscriptions.add(
      this.notificationService.unreadCount$.subscribe(count => {
        console.log('🔢 Unread count updated in component:', count);
        this.unreadCount = count;
      })
    );

    // IMPORTANT: Subscribe to SignalR notifications BEFORE starting connection
    // This ensures we don't miss any notifications
    this.subscriptions.add(
      this.signalRService.ListenForNotifications().subscribe(notification => {
        console.log('🔔 Notification received in component from SignalR:', notification);
        console.log('🔔 Calling addNotification...');
        this.notificationService.addNotification(notification);
        console.log('🔔 addNotification called');
      })
    );

    // Start SignalR connection for notifications (after subscription is set up)
    const token = this.authService.getAccessToken();
    if (token) {
      console.log('Starting SignalR notification connection...');
      this.signalRService.StartNotificationConnection(token);
    } else {
      console.warn('No access token available for SignalR connection');
    }

    // Reload notifications when language changes
    this.subscriptions.add(
      this.languageService.language$.subscribe(() => {
        this.notificationService.loadNotifications();
        this.notificationService.loadUnreadCount();
      })
    );
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  get unreadCountDisplay(): number {
    return this.unreadCount;
  }

  toggleDropdown(): void {
    this.isOpen = !this.isOpen;
  }

  closeDropdown(): void {
    this.isOpen = false;
  }

  markAsRead(notification: NotificationDto): void {
    if (!notification.isRead) {
      this.notificationService.markAsRead(notification.id);
    }
  }

  markAllAsRead(): void {
    this.notificationService.markAllAsRead();
  }

  viewAllNotifications(): void {
    console.log('Navigate to all notifications');
    this.closeDropdown();
  }

  getNotificationTypeClass(notificationType: NotificationType): string {
    switch (notificationType) {
      case NotificationType.NewOrder:
        return 'info';
      case NotificationType.OrderStatusChanged:
        return 'success';
      default:
        return 'info';
    }
  }

  formatTime(date: Date | string): string {
    if (!date) {
      return '';
    }
    
    const now = new Date();
    // Ensure we're working with a Date object
    const notificationDate = date instanceof Date ? date : new Date(date);
    
    // Check if date is valid
    if (isNaN(notificationDate.getTime())) {
      return '';
    }
    
    const diffInSeconds = Math.floor((now.getTime() - notificationDate.getTime()) / 1000);

    const isArabic = this.languageService.getLanguage() === 'ar';

    if (diffInSeconds < 60) {
      return isArabic ? 'الآن' : 'Just now';
    } else if (diffInSeconds < 3600) {
      const minutes = Math.floor(diffInSeconds / 60);
      return isArabic ? `منذ ${minutes} دقيقة` : `${minutes} minutes ago`;
    } else if (diffInSeconds < 86400) {
      const hours = Math.floor(diffInSeconds / 3600);
      return isArabic ? `منذ ${hours} ساعة` : `${hours} hours ago`;
    } else {
      const days = Math.floor(diffInSeconds / 86400);
      return isArabic ? `منذ ${days} يوم` : `${days} days ago`;
    }
  }

  onNotificationClick(notification: NotificationDto): void {
    this.markAsRead(notification);
    if (notification.orderId) {
      this.router.navigate(['/admin/requests/details', notification.orderId]);
      this.closeDropdown();
    }
  }

  trackByNotificationId(index: number, notification: NotificationDto): number {
    return notification.id;
  }

  @HostListener('document:keydown.escape', ['$event'])
  onEscapeKey(event: KeyboardEvent): void {
    if (this.isOpen) {
      this.closeDropdown();
    }
  }
}