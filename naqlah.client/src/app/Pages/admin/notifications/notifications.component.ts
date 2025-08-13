import { NgClass, NgFor, NgIf } from '@angular/common';
import { Component, HostListener } from '@angular/core';

interface Notification {
  id: number;
  title: string;
  message: string;
  time: string;
  isRead: boolean;
  type: 'info' | 'success' | 'warning' | 'error';
  avatar?: string;
}

@Component({
  selector: 'app-notifications',
  standalone: true,
  imports: [NgClass, NgIf, NgFor],
  templateUrl: './notifications.component.html',
  styleUrl: './notifications.component.css'
})
export class NotificationsComponent {
 isOpen = false;
  notifications: Notification[] = [];

  constructor() {}

  ngOnInit(): void {
    // Sample notifications data
    this.notifications = [
      {
        id: 1,
        title: 'رسالة جديدة من أحمد محمد',
        message: 'تم إرسال رسالة جديدة إليك من أحمد محمد. يرجى مراجعة الرسالة في أقرب وقت ممكن.',
        time: 'منذ دقيقتين',
        isRead: false,
        type: 'info'
      },
      {
        id: 2,
        title: 'تم تحديث الملف الشخصي بنجاح',
        message: 'تم حفظ التغييرات على ملفك الشخصي بنجاح.',
        time: 'منذ 10 دقائق',
        isRead: false,
        type: 'success'
      },
      {
        id: 3,
        title: 'تحذير: انتهاء صلاحية كلمة المرور قريباً',
        message: 'ستنتهي صلاحية كلمة المرور الخاصة بك خلال 3 أيام. يرجى تحديثها.',
        time: 'منذ ساعة',
        isRead: true,
        type: 'warning'
      },
      {
        id: 4,
        title: 'فشل في تحميل الملف',
        message: 'حدث خطأ أثناء تحميل الملف. يرجى المحاولة مرة أخرى.',
        time: 'منذ ساعتين',
        isRead: true,
        type: 'error'
      },
      {
        id: 5,
        title: 'طلب صداقة جديد',
        message: 'أرسل لك سارة أحمد طلب صداقة جديد.',
        time: 'أمس',
        isRead: false,
        type: 'info'
      }
    ];
  }

  get unreadCount(): number {
    return this.notifications.filter(n => !n.isRead).length;
  }

  toggleDropdown(): void {
    this.isOpen = !this.isOpen;
  }

  closeDropdown(): void {
    this.isOpen = false;
  }

  markAsRead(notification: Notification): void {
    notification.isRead = true;
  }

  markAllAsRead(): void {
    this.notifications.forEach(n => n.isRead = true);
  }

  viewAllNotifications(): void {
    console.log('Navigate to all notifications');
    this.closeDropdown();
  }

  trackByNotificationId(index: number, notification: Notification): number {
    return notification.id;
  }

  @HostListener('document:keydown.escape', ['$event'])
  onEscapeKey(event: KeyboardEvent): void {
    if (this.isOpen) {
      this.closeDropdown();
    }
  }
}
