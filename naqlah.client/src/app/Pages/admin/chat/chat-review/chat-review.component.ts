import { CommonModule, NgFor, NgIf } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';

interface User {
  id: number;
  name: string;
  avatar: string;
  lastMessage: string;
  time: string;
  unread: number;
  status: 'online' | 'offline';
}

interface Message {
  id: number;
  userId: number;
  content: string;
  time: string;
  type: 'sent' | 'received';
  status: 'pending' | 'approved' | 'rejected';
}

@Component({
  selector: 'app-chat-review',
  standalone: true,
  imports: [NgIf, NgFor, CommonModule, FormsModule],
  templateUrl: './chat-review.component.html',
  styleUrl: './chat-review.component.css'
})
export class ChatReviewComponent {
  searchTerm: string = '';
  selectedUser: User | null = null;
  users: User[] = [
    {
      id: 1,
      name: 'أحمد محمد',
      avatar: 'https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?ixlib=rb-1.2.1&ixid=eyJhcHBfaWQiOjEyMDd9&auto=format&fit=facearea&facepad=2&w=256&h=256&q=80',
      lastMessage: 'شكراً لتعاونكم معنا',
      time: '10:30',
      unread: 3,
      status: 'online'
    },
    {
      id: 2,
      name: 'فاطمة علي',
      avatar: 'https://images.unsplash.com/photo-1494790108755-2616b612b790?ixlib=rb-1.2.1&ixid=eyJhcHBfaWQiOjEyMDd9&auto=format&fit=facearea&facepad=2&w=256&h=256&q=80',
      lastMessage: 'متى يمكننا اللقاء؟',
      time: '09:45',
      unread: 1,
      status: 'offline'
    },
    {
      id: 3,
      name: 'عمر حسن',
      avatar: 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?ixlib=rb-1.2.1&ixid=eyJhcHBfaWQiOjEyMDd9&auto=format&fit=facearea&facepad=2&w=256&h=256&q=80',
      lastMessage: 'تم إرسال الملفات المطلوبة',
      time: '08:20',
      unread: 0,
      status: 'online'
    }
  ];

  filteredUsers: User[] = [...this.users];

  messages: Message[] = [
    { id: 1, userId: 1, content: 'السلام عليكم ورحمة الله', time: '10:25', type: 'received', status: 'approved' },
    { id: 2, userId: 1, content: 'وعليكم السلام ورحمة الله وبركاته', time: '10:26', type: 'sent', status: 'approved' },
    { id: 3, userId: 1, content: 'أريد الاستفسار عن الخدمة', time: '10:28', type: 'received', status: 'pending' },
    { id: 4, userId: 1, content: 'شكراً لتعاونكم معنا', time: '10:30', type: 'received', status: 'pending' },

    { id: 5, userId: 2, content: 'مرحباً', time: '09:40', type: 'received', status: 'approved' },
    { id: 6, userId: 2, content: 'أهلاً وسهلاً بك', time: '09:42', type: 'sent', status: 'approved' },
    { id: 7, userId: 2, content: 'متى يمكننا اللقاء؟', time: '09:45', type: 'received', status: 'pending' },

    { id: 8, userId: 3, content: 'تم إرسال الملفات المطلوبة', time: '08:20', type: 'received', status: 'approved' }
  ];

  filterUsers() {
    if (!this.searchTerm) {
      this.filteredUsers = [...this.users];
    } else {
      this.filteredUsers = this.users.filter(user =>
        user.name.includes(this.searchTerm)
      );
    }
  }

  selectUser(user: User) {
    this.selectedUser = user;
    // تصفير عدد الرسائل غير المقروءة
    user.unread = 0;
  }

  getSelectedUserMessages(): Message[] {
    if (!this.selectedUser) return [];
    return this.messages.filter(message => message.userId === this.selectedUser!.id);
  }

  approveMessage(messageId: number) {
    const message = this.messages.find(m => m.id === messageId);
    if (message) {
      message.status = 'approved';
    }
  }

  rejectMessage(messageId: number) {
    const message = this.messages.find(m => m.id === messageId);
    if (message) {
      message.status = 'rejected';
    }
  }

  approveAllMessages() {
    if (!this.selectedUser) return;
    this.messages
      .filter(m => m.userId === this.selectedUser!.id && m.status === 'pending')
      .forEach(m => m.status = 'approved');
  }

  rejectAllMessages() {
    if (!this.selectedUser) return;
    this.messages
      .filter(m => m.userId === this.selectedUser!.id && m.status === 'pending')
      .forEach(m => m.status = 'rejected');
  }

  getStatusText(status: string): string {
    switch (status) {
      case 'pending': return 'بانتظار المراجعة';
      case 'approved': return 'مُوافق عليها';
      case 'rejected': return 'مرفوضة';
      default: return '';
    }
  }
}
