# SignalR Customer Notifications Implementation Guide

## Overview

This document provides a comprehensive guide for implementing real-time notifications to admin users using SignalR. The system enables instant notification delivery to connected web clients (admin panel) when customer-related events occur.

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [SignalR Setup](#signalr-setup)
3. [Server-Side Implementation](#server-side-implementation)
4. [Client-Side Implementation](#client-side-implementation)
5. [Notification Flow](#notification-flow)
6. [When to Send Notifications](#when-to-send-notifications)
7. [Implementation Examples](#implementation-examples)
8. [Best Practices](#best-practices)
9. [Troubleshooting](#troubleshooting)

---

## Architecture Overview

The SignalR notification system consists of:

- **SignalR Hub**: Server-side hub that manages connections and broadcasts notifications
- **NotificationHubService**: Service layer that creates notifications and sends them via SignalR
- **NotificationService**: Service that persists notifications to the database
- **Client SignalR Service**: Angular service that manages SignalR connection
- **Admin UI Components**: Components that display and handle notifications

### Flow Diagram

```
Customer Action → Backend Event → NotificationHubService → Create Notification in DB
                                                              ↓
                                                      SignalR Hub → Broadcast to Admin Clients
                                                              ↓
                                                    Angular SignalR Service → Notification Component
```

---

## SignalR Setup

### 1. Prerequisites

- **Server**: ASP.NET Core with SignalR package
- **Client**: Angular with `@microsoft/signalr` package
- **Authentication**: JWT-based authentication for SignalR connections

### 2. Server-Side Setup

#### Install NuGet Package

```xml
<PackageReference Include="Microsoft.AspNetCore.SignalR" Version="1.1.0" />
```

#### Configure SignalR in Program.cs

```csharp
// Add SignalR service
builder.Services.AddSignalR();

// ... other configurations ...

// Map SignalR Hub
app.MapHub<Presentaion.Hubs.NotificationHub>("/NotificationHub");
```

#### Register NotificationHubService

```csharp
// In dependency injection configuration
services.AddScoped<NotificationHubService>();
```

### 3. Client-Side Setup

#### Install NPM Package

```bash
npm install @microsoft/signalr
```

#### Package.json

```json
{
  "dependencies": {
    "@microsoft/signalr": "^10.0.0"
  }
}
}
```

---

## Server-Side Implementation

### 1. SignalR Hub

The hub manages client connections and handles connection/disconnection events.

**File**: `Presentaion/Hubs/NotificationHub.cs`

```csharp
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace Presentaion.Hubs
{
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            System.Console.WriteLine($"[NotificationHub] Client connected: {Context.ConnectionId}");
            System.Console.WriteLine($"[NotificationHub] User: {Context.UserIdentifier}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            System.Console.WriteLine($"[NotificationHub] Client disconnected: {Context.ConnectionId}");
            if (exception != null)
            {
                System.Console.WriteLine($"[NotificationHub] Disconnect error: {exception.Message}");
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
```

**Key Points**:
- `OnConnectedAsync`: Called when a client connects
- `OnDisconnectedAsync`: Called when a client disconnects
- `Context.UserIdentifier`: Contains the user ID from JWT token (if configured)
- `Context.ConnectionId`: Unique connection identifier

### 2. NotificationHubService

This service creates notifications in the database and sends them via SignalR.

**File**: `Presentaion/Services/NotificationHubService.cs`

```csharp
using Application.Shared.Services;
using Domain.Enums;
using Domain.Models;
using Microsoft.AspNetCore.SignalR;
using Presentaion.Hubs;
using System.Linq;
using System.Threading.Tasks;

namespace Presentaion.Services
{
    public class NotificationHubService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly INotificationService _notificationService;

        public NotificationHubService(
            IHubContext<NotificationHub> hubContext, 
            INotificationService notificationService)
        {
            _hubContext = hubContext;
            _notificationService = notificationService;
        }

        public async Task SendNotificationAsync(
            string arabicTitle,
            string englishTitle,
            string arabicMessage,
            string englishMessage,
            NotificationType notificationType,
            int? orderId = null,
            int? userId = null)
        {
            // Step 1: Create notification in database
            var notification = await _notificationService.CreateNotificationAsync(
                arabicTitle,
                englishTitle,
                arabicMessage,
                englishMessage,
                notificationType,
                orderId,
                userId);

            // Step 2: Prepare notification DTO for SignalR
            var notificationDto = new
            {
                Id = notification.Id,
                ArabicTitle = notification.ArabicTitle,
                EnglishTitle = notification.EnglishTitle,
                ArabicMessage = notification.ArabicMessage,
                EnglishMessage = notification.EnglishMessage,
                OrderId = notification.OrderId,
                NotificationType = notification.NotificationType,
                CreationDate = notification.CreationDate,
                IsRead = notification.IsRead
            };

            // Step 3: Send notification via SignalR
            try
            {
                if (userId.HasValue)
                {
                    // Send to specific user
                    System.Console.WriteLine($"[NotificationHubService] Sending notification to user {userId.Value}");
                    await _hubContext.Clients.User(userId.Value.ToString())
                        .SendAsync("NewNotification", notificationDto);
                    System.Console.WriteLine($"[NotificationHubService] Notification sent to user {userId.Value}");
                }
                else
                {
                    // Send to all connected clients (admin users)
                    System.Console.WriteLine("[NotificationHubService] Sending notification to all connected clients");
                    await _hubContext.Clients.All.SendAsync("NewNotification", notificationDto);
                    System.Console.WriteLine("[NotificationHubService] Notification sent to all clients");
                }
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"[NotificationHubService] Error sending notification: {ex.Message}");
                System.Console.WriteLine($"[NotificationHubService] Stack trace: {ex.StackTrace}");
            }
        }
    }
}
```

**Key Points**:
- Creates notification in database first (for persistence)
- Sends notification via SignalR to connected clients
- Supports sending to specific user or all clients
- Uses `NewNotification` as the method name (client listens for this)

### 3. Notification Model

**File**: `Domain/Models/Notification.cs` (conceptual)

```csharp
public class Notification
{
    public int Id { get; set; }
    public string ArabicTitle { get; set; }
    public string EnglishTitle { get; set; }
    public string ArabicMessage { get; set; }
    public string EnglishMessage { get; set; }
    public int? OrderId { get; set; }
    public NotificationType NotificationType { get; set; }
    public DateTime CreationDate { get; set; }
    public bool IsRead { get; set; }
    public int? UserId { get; set; }
}
```

### 4. NotificationType Enum

```csharp
public enum NotificationType
{
    NewOrder = 1,
    OrderStatusChanged = 2,
    WaititngCustomerAction = 3,
    OrderCompleted = 4,
    OrderCancelled = 5
    // ... other types
}
```

---

## Client-Side Implementation

### 1. SignalR Service

Angular service that manages SignalR connection and handles incoming notifications.

**File**: `naqlah.client/src/app/shared/services/SignalRService.ts`

```typescript
import { Injectable, NgZone } from '@angular/core';
import { HttpTransportType, HubConnection, HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { BehaviorSubject, Observable } from 'rxjs';
import { NotificationService } from './notification.service';

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  connection: HubConnection | null = null;
  notificationConnection: HubConnection | null = null;
  isConnected: boolean = false;
  
  private notificationSubject = new BehaviorSubject<any>(null);
  public notification$: Observable<any> = this.notificationSubject.asObservable();

  constructor(
    private ngZone: NgZone,
    private notificationService: NotificationService
  ) {}

  // Start notification connection
  StartNotificationConnection(accessToken: string): void {
    const hubUrl = `${this.getBaseUrl()}/NotificationHub`;
    this.createNotificationConnection(hubUrl, accessToken);
  }

  private getBaseUrl(): string {
    // Return your API base URL
    return 'https://your-api-url.com';
  }

  private createNotificationConnection(hubUrl: string, accessToken: string): void {
    var connection = new HubConnectionBuilder()
      .configureLogging(LogLevel.Information)
      .withUrl(hubUrl, {
        accessTokenFactory: () => {
          const token = localStorage.getItem('accessToken') || accessToken;
          console.log('SignalR access token factory called, token exists:', !!token);
          if (!token) {
            console.error('❌ No access token available for SignalR connection!');
          }
          return Promise.resolve(token || '');
        },
        skipNegotiation: true,
        transport: HttpTransportType.WebSockets
      })
      .withAutomaticReconnect([0, 2000, 10000, 30000])
      .build();

    // Register event handler BEFORE starting connection
    connection.on('NewNotification', (notification: any) => {
      // Use NgZone to ensure Angular change detection runs
      this.ngZone.run(() => {
        // Emit notification to subscribers
        this.notificationSubject.next(notification);
      });
    });

    // Store connection
    this.notificationConnection = connection;

    // Start connection
    connection.start().then(() => {
      this.isConnected = true;
      console.log('✅ SignalR Connected!');
    }).catch((err) => {
      console.error('❌ Notification SignalR connection error:', err);
      this.isConnected = false;
      this.notificationConnection = null;
    });

    // Handle reconnection
    connection.onreconnecting(() => {
      console.log('Notification SignalR reconnecting...');
    });

    connection.onreconnected(() => {
      console.log('Notification SignalR reconnected!');
    });

    connection.onclose((error) => {
      console.log('🔴 Notification SignalR connection closed', error);
      this.isConnected = false;
      this.notificationConnection = null;
    });
  }

  // Listen for notifications
  ListenForNotifications(): Observable<any> {
    return this.notification$;
  }

  // Stop connection
  async StopNotificationConnection(): Promise<void> {
    if (this.notificationConnection) {
      try {
        await this.notificationConnection.stop();
        console.log('Notification SignalR connection stopped');
        this.notificationConnection = null;
        this.isConnected = false;
      } catch (err) {
        console.error('Error stopping Notification SignalR connection:', err);
      }
    }
  }
}
```

**Key Points**:
- Uses JWT token for authentication
- Registers `NewNotification` event handler
- Uses `NgZone` to ensure Angular change detection
- Implements automatic reconnection
- Exposes observable for components to subscribe

### 2. Notification Service (Angular)

Service that manages notification state and API calls.

**File**: `naqlah.client/src/app/shared/services/notification.service.ts`

```typescript
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { SignalRService } from './SignalRService';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private notificationsSubject = new BehaviorSubject<any[]>([]);
  public notifications$: Observable<any[]> = this.notificationsSubject.asObservable();

  private unreadCountSubject = new BehaviorSubject<number>(0);
  public unreadCount$: Observable<number> = this.unreadCountSubject.asObservable();

  constructor(private signalRService: SignalRService) {
    // Subscribe to SignalR notifications
    this.signalRService.ListenForNotifications().subscribe(notification => {
      if (notification) {
        console.log('📥 Adding notification from SignalR:', notification);
        this.addNotification(notification);
      }
    });
  }

  // Add notification to list
  addNotification(notification: any): void {
    const current = this.notificationsSubject.value;
    const updated = [notification, ...current];
    this.notificationsSubject.next(updated);
    
    // Update unread count
    if (!notification.isRead) {
      this.updateUnreadCount();
    }
  }

  // Load notifications from API
  loadNotifications(): void {
    // Call your API to get notifications
    // this.http.get('/api/Notification/GetNotifications').subscribe(...)
  }

  // Update unread count
  updateUnreadCount(): void {
    const unread = this.notificationsSubject.value.filter(n => !n.isRead).length;
    this.unreadCountSubject.next(unread);
  }
}
```

### 3. Notification Component

Component that displays notifications in the admin panel.

**File**: `naqlah.client/src/app/Pages/admin/notifications/notifications.component.ts`

```typescript
import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { SignalRService } from 'src/app/shared/services/SignalRService';
import { NotificationService } from 'src/app/shared/services/notification.service';
import { AuthService } from 'src/app/Core/services/auth.service';

@Component({
  selector: 'app-notifications',
  templateUrl: './notifications.component.html',
  styleUrls: ['./notifications.component.css']
})
export class NotificationsComponent implements OnInit, OnDestroy {
  notifications: any[] = [];
  unreadCount: number = 0;
  isOpen = false;
  private subscriptions = new Subscription();

  constructor(
    private notificationService: NotificationService,
    private signalRService: SignalRService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    // Load initial notifications
    this.notificationService.loadNotifications();
    this.notificationService.loadUnreadCount();

    // Subscribe to notifications
    this.subscriptions.add(
      this.notificationService.notifications$.subscribe(notifications => {
        console.log('📋 Notifications updated in component, count:', notifications.length);
        this.notifications = notifications;
      })
    );

    // Subscribe to unread count
    this.subscriptions.add(
      this.notificationService.unreadCount$.subscribe(count => {
        console.log('🔢 Unread count updated in component:', count);
        this.unreadCount = count;
      })
    );

    // Subscribe to SignalR notifications BEFORE starting connection
    this.subscriptions.add(
      this.signalRService.ListenForNotifications().subscribe(notification => {
        console.log('🔔 Notification received in component from SignalR:', notification);
        this.notificationService.addNotification(notification);
      })
    );

    // Start SignalR connection
    const token = this.authService.getAccessToken();
    if (token) {
      console.log('Starting SignalR notification connection...');
      this.signalRService.StartNotificationConnection(token);
    } else {
      console.warn('No access token available for SignalR connection');
    }
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
    // Optionally stop SignalR connection
    // this.signalRService.StopNotificationConnection();
  }
}
```

---

## Notification Flow

### Complete Flow Example

1. **Customer Action**: Customer creates a new order
2. **Backend Event**: Order creation triggers notification
3. **NotificationHubService**: Creates notification and sends via SignalR
4. **SignalR Hub**: Broadcasts to all connected admin clients
5. **Client SignalR Service**: Receives notification
6. **Notification Service**: Adds notification to state
7. **Notification Component**: Displays notification in UI

---

## When to Send Notifications

Notifications should be sent in the following scenarios:

### 1. Order-Related Events

#### New Order Created

```csharp
// In OrderAdminController or OrderService
await notificationHubService.SendNotificationAsync(
    arabicTitle: "طلب جديد",
    englishTitle: "New Order",
    arabicMessage: $"تم إنشاء طلب جديد برقم {order.OrderNumber}",
    englishMessage: $"New order #{order.OrderNumber} has been created",
    notificationType: NotificationType.NewOrder,
    orderId: order.Id,
    userId: null  // Send to all admins
);
```

#### Order Status Changed

```csharp
await notificationHubService.SendNotificationAsync(
    arabicTitle: "تغيير حالة الطلب",
    englishTitle: "Order Status Changed",
    arabicMessage: $"تم تغيير حالة الطلب #{order.OrderNumber} إلى {newStatus}",
    englishMessage: $"Order #{order.OrderNumber} status changed to {newStatus}",
    notificationType: NotificationType.OrderStatusChanged,
    orderId: order.Id
);
```

#### Customer Action Required

```csharp
// In CustomerOrderController
await notificationHubService.SendNotificationAsync(
    arabicTitle: "إجراء مطلوب من العميل",
    englishTitle: "Customer Action Required",
    arabicMessage: $"العميل يحتاج إلى تأكيد نقطة التوصيل للطلب #{order.OrderNumber}",
    englishMessage: $"Customer needs to confirm waypoint for order #{order.OrderNumber}",
    notificationType: NotificationType.WaititngCustomerAction,
    orderId: order.Id
);
```

### 2. Implementation in Controllers

**File**: `Presentaion/Controllers/Admin/OrderAdminController.cs`

```csharp
public class OrderAdminController : ControllerBase
{
    private readonly IMediator mediator;
    private readonly IUserSession userSession;
    private readonly NotificationHubService notificationHubService;

    public OrderAdminController(
        IMediator mediator, 
        IUserSession userSession, 
        NotificationHubService notificationHubService)
    {
        this.mediator = mediator;
        this.userSession = userSession;
        this.notificationHubService = notificationHubService;
    }

    [HttpPost]
    [Route("CreateOrder")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var result = await mediator.Send(new CreateOrderCommand { /* ... */ });
        
        if (result.IsSuccess)
        {
            // Send notification to admin
            await notificationHubService.SendNotificationAsync(
                arabicTitle: "طلب جديد",
                englishTitle: "New Order",
                arabicMessage: $"تم إنشاء طلب جديد برقم {result.Value.OrderNumber}",
                englishMessage: $"New order #{result.Value.OrderNumber} has been created",
                notificationType: NotificationType.NewOrder,
                orderId: result.Value.Id
            );
        }
        
        return Ok(result);
    }
}
```

---

## Implementation Examples

### Example 1: Customer Creates Order

**Backend**:
```csharp
// In CustomerOrderController
[HttpPost]
[Route("CreateOrder")]
public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
{
    var result = await mediator.Send(new CreateOrderCommand { /* ... */ });
    
    if (result.IsSuccess)
    {
        // Notify admin about new order
        await notificationHubService.SendNotificationAsync(
            arabicTitle: "طلب جديد من عميل",
            englishTitle: "New Customer Order",
            arabicMessage: $"تم إنشاء طلب جديد برقم {result.Value.OrderNumber} من العميل",
            englishMessage: $"New order #{result.Value.OrderNumber} created by customer",
            notificationType: NotificationType.NewOrder,
            orderId: result.Value.Id
        );
    }
    
    return Ok(result);
}
```

### Example 2: Order Status Update

```csharp
[HttpPost]
[Route("UpdateOrderStatus")]
public async Task<IActionResult> UpdateOrderStatus([FromBody] UpdateOrderStatusRequest request)
{
    var result = await mediator.Send(new UpdateOrderStatusCommand { /* ... */ });
    
    if (result.IsSuccess)
    {
        await notificationHubService.SendNotificationAsync(
            arabicTitle: "تحديث حالة الطلب",
            englishTitle: "Order Status Updated",
            arabicMessage: $"تم تحديث حالة الطلب #{order.OrderNumber}",
            englishMessage: $"Order #{order.OrderNumber} status has been updated",
            notificationType: NotificationType.OrderStatusChanged,
            orderId: order.Id
        );
    }
    
    return Ok(result);
}
```

---

## Best Practices

### 1. Connection Management

- **Start connection after login**: Only connect when user is authenticated
- **Stop connection on logout**: Clean up connections when user logs out
- **Handle reconnection**: Implement automatic reconnection with exponential backoff
- **Monitor connection state**: Track connection status for debugging

### 2. Error Handling

```typescript
connection.onclose((error) => {
  if (error) {
    console.error('SignalR connection closed with error:', error);
    // Implement retry logic
    this.retryConnection();
  }
});
```

### 3. Authentication

- **Use JWT tokens**: Pass access token in SignalR connection
- **Validate on server**: Ensure user is authenticated before accepting connection
- **Refresh tokens**: Handle token expiration and refresh

### 4. Performance

- **Limit notification history**: Don't load all notifications at once
- **Pagination**: Implement pagination for notification list
- **Debounce updates**: Debounce rapid notification updates
- **Clean up subscriptions**: Unsubscribe from observables on component destroy

### 5. User Experience

- **Show connection status**: Display connection indicator in UI
- **Handle offline state**: Show notifications when connection is restored
- **Mark as read**: Implement read/unread functionality
- **Sound/vibration**: Optional sound or vibration for new notifications
- **Badge count**: Show unread count badge

### 6. Security

- **Authorize connections**: Only allow authenticated users to connect
- **Validate user identity**: Ensure users can only receive their own notifications
- **Sanitize data**: Sanitize notification content before sending
- **Rate limiting**: Implement rate limiting for notification sending

### 7. Testing

- **Test connection**: Verify SignalR connection establishes correctly
- **Test notifications**: Verify notifications are received
- **Test reconnection**: Test automatic reconnection
- **Test multiple clients**: Test with multiple admin users connected
- **Test error scenarios**: Test connection failures and recovery

---

## Troubleshooting

### Common Issues

#### 1. Connection Not Establishing

**Symptoms**: SignalR connection fails to start

**Solutions**:
- Verify SignalR hub is mapped in `Program.cs`
- Check CORS configuration allows SignalR endpoints
- Verify JWT token is valid and included
- Check network connectivity
- Verify WebSocket support on server

#### 2. Notifications Not Received

**Symptoms**: Notifications sent but not received on client

**Solutions**:
- Verify `NewNotification` event handler is registered
- Check connection is established (`isConnected === true`)
- Verify notification is being sent to correct clients
- Check browser console for errors
- Verify NgZone is used for change detection

#### 3. Connection Drops Frequently

**Symptoms**: Connection disconnects and reconnects repeatedly

**Solutions**:
- Check network stability
- Increase reconnection intervals
- Verify server timeout settings
- Check for proxy/firewall issues
- Monitor server logs for errors

#### 4. Authentication Failures

**Symptoms**: Connection rejected due to authentication

**Solutions**:
- Verify JWT token is valid
- Check token expiration
- Verify `accessTokenFactory` returns correct token
- Check server authentication configuration
- Verify user has proper permissions

#### 5. Notifications Not Persisting

**Symptoms**: Notifications appear but disappear after refresh

**Solutions**:
- Verify notification is saved to database before sending
- Check notification service `CreateNotificationAsync` is called
- Verify database save operation succeeds
- Check notification retrieval API

### Debugging Tips

1. **Enable SignalR Logging**:
```typescript
.configureLogging(LogLevel.Information)  // or LogLevel.Debug
```

2. **Monitor Server Logs**:
```csharp
System.Console.WriteLine($"[NotificationHub] Client connected: {Context.ConnectionId}");
```

3. **Check Browser Console**:
- Look for SignalR connection messages
- Check for JavaScript errors
- Verify network requests

4. **Test with SignalR Test Client**:
- Use SignalR test tools to verify hub is working
- Test connection and message sending

---

## Summary

This implementation provides a complete real-time notification system using SignalR:

1. **Server-side**: SignalR Hub and NotificationHubService handle connection and broadcasting
2. **Client-side**: Angular SignalR service manages connection and receives notifications
3. **Notifications are sent** when customer-related events occur (order creation, status changes, etc.)
4. **Notifications are persisted** in the database for history
5. **Real-time delivery** ensures admins see notifications immediately
6. **Automatic reconnection** handles connection failures gracefully

For questions or issues, refer to the [SignalR Documentation](https://docs.microsoft.com/en-us/aspnet/core/signalr/introduction).



