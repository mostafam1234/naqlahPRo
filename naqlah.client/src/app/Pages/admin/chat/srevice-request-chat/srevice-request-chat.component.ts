import { CommonModule, NgFor, NgIf } from '@angular/common';
import { Component, ViewChild, ElementRef } from '@angular/core';
import { FormsModule } from '@angular/forms';


interface Contact {
  id: number;
  name: string;
  avatar: string;
  lastMessage: string;
  time: string;
  unread?: number;
  isArchived?: boolean;
  isOnline?: boolean;
}

interface Message {
  id: number;
  text: string;
  time: string;
  isSent: boolean;
  image?: string;
  type?: 'text' | 'image';
}

interface Conversation {
  contactId: number;
  messages: Message[];
}

type FilterType = 'all' | 'unread' | 'read' | 'archived';

@Component({
  selector: 'app-srevice-request-chat',
  standalone: true,
  imports: [NgIf, NgFor, CommonModule, FormsModule],
  templateUrl: './srevice-request-chat.component.html',
  styleUrl: './srevice-request-chat.component.css'
})
export class SreviceRequestChatComponent {
 selectedContact: Contact | null = null;
  newMessage = '';
  isMobile = false;
  isMenuVisible = false;
  selectedFile: File | null = null;
  previewImage: string | null = null;
  imagePreviewUrl: string | null = null;
  searchQuery = '';
  currentFilter: FilterType = 'all';
  activeTab = 'all';

  // Store conversations for each contact
  conversations: Conversation[] = [];

  // Current messages (computed from selected contact)
  get messages(): Message[] {
    if (!this.selectedContact) return [];

    const conversation = this.conversations.find(c => c.contactId === this.selectedContact!.id);
    return conversation ? conversation.messages : [];
  }

  // Store filtered contacts result
  private _filteredContacts: Contact[] = [];

  // Update filtered contacts when search or filter changes
  updateFilteredContacts() {
    let filtered = [...this.contacts];

    // Apply search filter
    if (this.searchQuery.trim()) {
      const query = this.searchQuery.toLowerCase().trim();
      filtered = filtered.filter(contact =>
        contact.name.toLowerCase().includes(query) ||
        contact.lastMessage.toLowerCase().includes(query)
      );
    }

    // Apply category filter
    switch (this.currentFilter) {
      case 'unread':
        filtered = filtered.filter(contact => (contact.unread || 0) > 0);
        break;
      case 'read':
        filtered = filtered.filter(contact => (contact.unread || 0) === 0);
        break;
      case 'archived':
        filtered = filtered.filter(contact => contact.isArchived);
        break;
      case 'all':
      default:
        filtered = filtered.filter(contact => !contact.isArchived);
        break;
    }

    this._filteredContacts = filtered;
  }

  // Getter for filtered contacts
  get filteredContacts(): Contact[] {
    return this._filteredContacts;
  }

  setActiveTab(tab: string) {
    this.activeTab = tab;
  }
  contacts: Contact[] = [
    {
      id: 1,
      name: 'أحمد محمد',
      avatar: 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=64&h=64&fit=crop&crop=face&auto=format',
      lastMessage: 'مرحباً، كيف حالك اليوم؟',
      time: '10:30',
      unread: 2,
      isOnline: true
    },
    {
      id: 2,
      name: 'فاطمة علي',
      avatar: 'https://images.unsplash.com/photo-1438761681033-6461ffad8d80?w=64&h=64&fit=crop&crop=face&auto=format',
      lastMessage: 'شكراً لك على المساعدة',
      time: '09:15',
      unread: 0,
      isOnline: false
    },
    {
      id: 3,
      name: 'محمد حسن',
      avatar: 'https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=64&h=64&fit=crop&crop=face&auto=format',
      lastMessage: 'سأكون متأخراً قليلاً',
      time: 'أمس',
      unread: 1,
      isOnline: true
    },
    {
      id: 4,
      name: 'نورا أحمد',
      avatar: 'https://images.unsplash.com/photo-1438761681033-6461ffad8d80?w=64&h=64&fit=crop&crop=face&auto=format',
      lastMessage: 'تمام، سأراك غداً',
      time: 'أمس',
      unread: 0,
      isOnline: false
    },
    {
      id: 5,
      name: 'عمر سعد',
      avatar: 'https://images.unsplash.com/photo-1500648767791-00dcc994a43e?w=64&h=64&fit=crop&crop=face&auto=format',
      lastMessage: 'هل انتهيت من المشروع؟',
      time: 'الثلاثاء',
      unread: 0,
      isArchived: true,
      isOnline: false
    }
  ];



  constructor() {
    this.checkScreenSize();
    this.initializeConversations();
    this.updateFilteredContacts(); // Initialize filtered contacts
    window.addEventListener('resize', () => this.checkScreenSize());
    // Close menu when clicking outside
    document.addEventListener('click', (event) => {
      const target = event.target as HTMLElement;
      if (!target.closest('.relative')) {
        this.isMenuVisible = false;
      }
    });
  }

  // Initialize conversations with sample data for each contact
  initializeConversations() {
    this.conversations = [
      {
        contactId: 1, // أحمد محمد
        messages: [
          {
            id: 1,
            text: 'مرحباً! كيف حالك؟',
            time: '10:00',
            isSent: false
          },
          {
            id: 2,
            text: 'أهلاً بك، أنا بخير والحمد لله',
            time: '10:01',
            isSent: true
          },
          {
            id: 3,
            text: 'مرحباً، كيف حالك اليوم؟',
            time: '10:30',
            isSent: false
          }
        ]
      },
      {
        contactId: 2, // فاطمة علي
        messages: [
          {
            id: 4,
            text: 'شكراً لك على المساعدة',
            time: '09:00',
            isSent: false
          },
          {
            id: 5,
            text: 'العفو، أي وقت',
            time: '09:15',
            isSent: true
          }
        ]
      },
      {
        contactId: 3, // محمد حسن
        messages: [
          {
            id: 6,
            text: 'سأكون متأخراً قليلاً',
            time: 'أمس',
            isSent: false
          },
          {
            id: 7,
            text: 'لا مشكلة، خذ وقتك',
            time: 'أمس',
            isSent: true
          }
        ]
      },
      {
        contactId: 4, // نورا أحمد
        messages: [
          {
            id: 8,
            text: 'تمام، سأراك غداً',
            time: 'أمس',
            isSent: false
          }
        ]
      },
      {
        contactId: 5, // عمر سعد
        messages: [
          {
            id: 9,
            text: 'هل انتهيت من المشروع؟',
            time: 'الثلاثاء',
            isSent: false
          },
          {
            id: 10,
            text: 'نعم، تقريباً انتهيت',
            time: 'الثلاثاء',
            isSent: true
          }
        ]
      }
    ];
  }

  checkScreenSize() {
    this.isMobile = window.innerWidth < 768;
  }

  toggleMenu() {
    this.isMenuVisible = !this.isMenuVisible;
  }

  // Helper methods for menu counts
  getTotalUnreadCount(): number {
    return this.contacts.reduce((total, contact) => total + (contact.unread || 0), 0);
  }

  getAllContactsCount(): number {
    return this.contacts.filter(c => !c.isArchived).length;
  }

  getReadContactsCount(): number {
    return this.contacts.filter(c => !c.unread && !c.isArchived).length;
  }

  getArchivedContactsCount(): number {
    return this.contacts.filter(c => c.isArchived).length;
  }

  // Filter methods
  setFilter(filter: FilterType) {
    this.currentFilter = filter;
    this.updateFilteredContacts();
    this.isMenuVisible = false;
  }

  getFilterLabel(filter: FilterType): string {
    switch (filter) {
      case 'unread': return 'غير المقروءة';
      case 'read': return 'المقروءة';
      case 'archived': return 'المؤرشفة';
      default: return 'الكل';
    }
  }

  // Search methods
  onSearchChange() {
    this.updateFilteredContacts();
  }

  clearSearch() {
    this.searchQuery = '';
    this.updateFilteredContacts();
    this.isMenuVisible = false;
  }

  // Action methods
  markAllAsRead() {
    this.contacts.forEach(contact => {
      contact.unread = 0;
    });
    this.updateFilteredContacts();
    this.isMenuVisible = false;
  }

  archiveContact(contactId: number) {
    const contact = this.contacts.find(c => c.id === contactId);
    if (contact) {
      contact.isArchived = true;
      this.updateFilteredContacts();
    }
  }

  unarchiveContact(contactId: number) {
    const contact = this.contacts.find(c => c.id === contactId);
    if (contact) {
      contact.isArchived = false;
      this.updateFilteredContacts();
    }
  }

  selectContact(contact: Contact) {
    this.selectedContact = contact;
    // Clear unread count when selecting contact
    contact.unread = 0;

    // Create conversation if doesn't exist
    if (!this.conversations.find(c => c.contactId === contact.id)) {
      this.conversations.push({
        contactId: contact.id,
        messages: []
      });
    }
  }

  goBack() {
    if (this.isMobile) {
      this.selectedContact = null;
    }
  }

  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file && file.type.startsWith('image/')) {
      this.selectedFile = file;

      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.previewImage = e.target.result;
      };
      reader.readAsDataURL(file);
    }
  }

  removeSelectedImage() {
    this.selectedFile = null;
    this.previewImage = null;

    // Clear file input
    const fileInput = document.querySelector('input[type="file"]') as HTMLInputElement;
    if (fileInput) {
      fileInput.value = '';
    }
  }

  openImagePreview(imageUrl: string) {
    this.imagePreviewUrl = imageUrl;
  }

  closeImagePreview() {
    this.imagePreviewUrl = null;
  }

  sendMessage() {
    if ((this.newMessage.trim() || this.previewImage) && this.selectedContact) {
      const message: Message = {
        id: Date.now(),
        text: this.newMessage,
        time: new Date().toLocaleTimeString('ar-EG', {
          hour: '2-digit',
          minute: '2-digit'
        }),
        isSent: true,
        image: this.previewImage || undefined,
        type: this.previewImage ? 'image' : 'text'
      };

      // Add message to the specific conversation
      let conversation = this.conversations.find(c => c.contactId === this.selectedContact!.id);
      if (!conversation) {
        conversation = {
          contactId: this.selectedContact.id,
          messages: []
        };
        this.conversations.push(conversation);
      }

      conversation.messages.push(message);

      // Update last message in contact
      this.selectedContact.lastMessage = this.previewImage ? '📷 صورة' : this.newMessage;
      this.selectedContact.time = message.time;

      // Clear inputs
      this.newMessage = '';
      this.removeSelectedImage();

      // Scroll to bottom after message is added
      setTimeout(() => {
        const messagesContainer = document.querySelector('.overflow-y-auto');
        if (messagesContainer) {
          messagesContainer.scrollTop = messagesContainer.scrollHeight;
        }
      }, 10);
    }
  }

  // Helper method to get conversation by contact ID (useful for backend integration)
  getConversationByContactId(contactId: number): Conversation | undefined {
    return this.conversations.find(c => c.contactId === contactId);
  }

  // Helper method to add message to specific conversation (useful for receiving messages)
  addMessageToConversation(contactId: number, message: Message) {
    let conversation = this.conversations.find(c => c.contactId === contactId);
    if (!conversation) {
      conversation = {
        contactId: contactId,
        messages: []
      };
      this.conversations.push(conversation);
    }

    conversation.messages.push(message);

    // Update contact's last message
    const contact = this.contacts.find(c => c.id === contactId);
    if (contact) {
      contact.lastMessage = message.image ? '📷 صورة' : message.text;
      contact.time = message.time;

      // Add unread count if not currently selected
      if (!this.selectedContact || this.selectedContact.id !== contactId) {
        contact.unread = (contact.unread || 0) + 1;
      }
    }

    // Scroll to bottom if this is the current conversation
    if (this.selectedContact && this.selectedContact.id === contactId) {
      setTimeout(() => {
        const messagesContainer = document.querySelector('.overflow-y-auto');
        if (messagesContainer) {
          messagesContainer.scrollTop = messagesContainer.scrollHeight;
        }
      }, 10);
    }
  }

  // Demo method to simulate receiving a message (for testing)
  simulateReceivedMessage() {
    if (this.selectedContact) {
      const demoMessage: Message = {
        id: Date.now(),
        text: 'هذه رسالة وهمية للاختبار',
        time: new Date().toLocaleTimeString('ar-EG', {
          hour: '2-digit',
          minute: '2-digit'
        }),
        isSent: false,
        type: 'text'
      };

      this.addMessageToConversation(this.selectedContact.id, demoMessage);
    }
  }

}
