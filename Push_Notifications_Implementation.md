# Push Notifications Implementation Guide

## Overview

This document provides a comprehensive guide for implementing push notifications to mobile devices using Firebase Cloud Messaging (FCM). The system supports both Android and iOS devices and allows sending notifications to single or multiple devices.

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Firebase Setup](#firebase-setup)
3. [Device Token Storage](#device-token-storage)
4. [API Endpoints](#api-endpoints)
5. [Sending Notifications](#sending-notifications)
6. [Implementation Details](#implementation-details)
7. [Best Practices](#best-practices)

---

## Architecture Overview

The push notification system consists of the following components:

- **Firebase Cloud Messaging (FCM)**: Google's messaging service for sending notifications
- **NotificationService**: Backend service that handles sending notifications
- **Device Token Storage**: Database storage for FCM device tokens
- **Mobile App**: Client application that registers device tokens

### Flow Diagram

```
Mobile App → Register Device Token → Backend API → Store in Database
                                                      ↓
Backend Event → Retrieve Device Tokens → NotificationService → FCM → Mobile Device
```

---

## Firebase Setup

### 1. Prerequisites

- Firebase project created in [Firebase Console](https://console.firebase.google.com/)
- Firebase Admin SDK service account JSON file
- NuGet package: `FirebaseAdmin` (version 3.0.0 or higher)

### 2. Firebase Configuration

#### Server-Side Configuration

1. **Download Firebase Service Account JSON**
   - Go to Firebase Console → Project Settings → Service Accounts
   - Click "Generate New Private Key"
   - Save the JSON file as `FireBaseConfigurations.json` in your server's root directory
   
   **Exact Location**: 
   - Place the file in: `NAQLAH.Server/FireBaseConfigurations.json`
   - This should be at the same level as `Program.cs`, `appsettings.json`, and other root files
   - Example path: `E:\Abdelrahim Projects\naqlahPRo\NAQLAH.Server\FireBaseConfigurations.json`

2. **Configure Firebase in Startup**

```csharp
// In Program.cs or Startup.cs
builder.Services.AddFireBaseConfigurations(
    builder.Configuration, 
    builder.Environment
);
```

3. **Firebase Configuration Extension Method**

```csharp
public static class FireBaseConfigurations
{
    public static IServiceCollection AddFireBaseConfigurations(
        this IServiceCollection services, 
        IConfiguration configuration, 
        IWebHostEnvironment webEnvironment)
    {
        var path = Path.Combine(webEnvironment.ContentRootPath, "FireBaseConfigurations.json");
        FirebaseApp.Create(new AppOptions()
        {
            Credential = GoogleCredential.FromFile(path)
        });
        return services;
    }
}
```

#### Mobile App Configuration

- **Android**: Add `google-services.json` to your Android project
- **iOS**: Add `GoogleService-Info.plist` to your iOS project
- Configure Firebase SDK in your mobile app

---

## Device Token Storage

### When to Store Device Tokens

Device tokens should be stored in the following scenarios:

1. **After User Login**: When a user successfully logs in
2. **After App Installation**: When the app is first installed and Firebase token is generated
3. **Token Refresh**: When Firebase refreshes the device token (tokens can expire)
4. **App Launch**: On app startup to ensure the latest token is stored

### Where to Store Device Tokens

Device tokens are stored in the database as part of user entities:

#### For Customers

- **Table**: `Customers` (or `VO_Customers`)
- **Fields**:
  - `AndriodDevice` (string): Android FCM token
  - `IosDevice` (string): iOS FCM token

### Database Schema

```sql
-- Customer Table
CREATE TABLE NA_Customers (
    Id INT PRIMARY KEY,
    AndriodDevice NVARCHAR(500),  -- FCM token for Android
    IosDevice NVARCHAR(500),      -- FCM token for iOS
    -- other fields...
);

```

---

## API Endpoints

### 1. Store Device Tokens for Customer

**Endpoint**: `POST /api/Customer/AddDevices`

**Authentication**: Required (User must be logged in)

**Request Body**:
```json
{
  "andriodDevice": "fcm_token_android_here",
  "iosDevice": "fcm_token_ios_here"
}
```

**Response**: 
- `200 OK`: Success
- `400 Bad Request`: Error details

**Implementation**:
```csharp
[HttpPost]
[Route("AddDevices")]
public async Task<IActionResult> AddDevices([FromBody] DeliveryDeviceTokensDto request)
{
    var result = await mediator.Send(new SaveFireBaseTokensForCustomerCommand
    {
        AndroidDevice = request.AndriodDevice,
        IosDevice = request.IosDevice
    });

    if (result.IsFailure)
    {
        return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
    }
    return Ok();
}
```

**Command Handler**:
```csharp
public class SaveFireBaseTokensForCustomerCommand : IRequest<Result>
{
    public string AndroidDevice { get; set; } = string.Empty;
    public string IosDevice { get; set; } = string.Empty;
}

// Handler
public async Task<Result> Handle(SaveFireBaseTokensForCustomerCommand request, CancellationToken cancellationToken)
{
    var user = await context.Customers
        .AsTracking()
        .FirstOrDefaultAsync(x => x.UserId == userSession.UserId);

    if (user is null)
    {
        return Result.Failure("User Not Found");
    }

    user.AddFireBaseDevices(request.AndroidDevice, request.IosDevice);
    var saveResult = await context.SaveChangesAsyncWithResult();
    return saveResult;
}
```

### 2. Remove Device Tokens (Optional)

**Endpoint**: `POST /api/DeliveryMan/RemoveDeviceTokens`

**Use Case**: When user logs out or uninstalls the app

---

## Sending Notifications

### Notification Service Interface

```csharp
public interface INotificationService
{
    Task SendNotificationForSingleDevice(NotificationBody notificationBody);
    Task SendNotificationAsyncToMultipleDevices(NotificationBodyForMultipleDevices notificationBody);
}
```

### Notification Body Models

#### Single Device Notification

```csharp
public class NotificationBody
{
    public string Title { get; set; }
    public string Body { get; set; }
    public string FireBaseToken { get; set; }
    public Dictionary<string, string> PayLoad { get; set; }
}
```

#### Multiple Devices Notification

```csharp
public class NotificationBodyForMultipleDevices
{
    public string Title { get; set; }
    public string Body { get; set; }
    public List<string> FireBaseTokens { get; set; }
    public Dictionary<string, string> PayLoad { get; set; }
}
```

### Implementation

#### Send to Single Device

```csharp
public async Task SendNotificationForSingleDevice(NotificationBody notificationBody)
{
    try
    {
        var message = new Message()
        {
            Token = notificationBody.FireBaseToken,
            Notification = new Notification
            {
                Title = notificationBody.Title,
                Body = notificationBody.Body
            },
            Data = notificationBody.PayLoad,
            Android = new AndroidConfig
            {
                Notification = new AndroidNotification
                {
                    ChannelId = "1",
                    ClickAction = "FLUTTER_NOTIFICATION_CLICK",
                    DefaultSound = true,
                    Priority = NotificationPriority.HIGH,
                    EventTimestamp = DateTime.UtcNow
                }
            }
        };

        var result = await FirebaseMessaging.DefaultInstance.SendAsync(message);
        logger.LogInformation("Firebase result is {0} for device {1}", result, notificationBody.FireBaseToken);
    }
    catch (FirebaseMessagingException fireBaseEx)
    {
        logger.LogError(fireBaseEx, "Error in Sending Notifications");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error in Sending Notifications");
    }
}
```

#### Send to Multiple Devices

```csharp
public async Task SendNotificationAsyncToMultipleDevices(NotificationBodyForMultipleDevices notificationBody)
{
    try
    {
        var deviceIds = notificationBody.FireBaseTokens
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();
            
        if (deviceIds.Count == 0)
        {
            return;
        }

        var message = new MulticastMessage()
        {
            Tokens = deviceIds,
            Notification = new Notification
            {
                Title = notificationBody.Title,
                Body = notificationBody.Body
            },
            Data = notificationBody.PayLoad,
            Android = new AndroidConfig
            {
                Notification = new AndroidNotification
                {
                    ChannelId = "1",
                    ClickAction = "FLUTTER_NOTIFICATION_CLICK",
                    DefaultSound = true,
                    Priority = NotificationPriority.HIGH,
                    EventTimestamp = DateTime.UtcNow
                }
            }
        };

        var result = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);
        logger.LogInformation(
            "Firebase success count: {0}, Firebase failed count: {1}", 
            result.SuccessCount, 
            result.FailureCount
        );
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error in Sending Notifications");
    }
}
```

---

## Implementation Details

### When to Send Notifications

Notifications are typically sent in the following scenarios:

1. **Order Status Changes**
   - When delivery man picks up an order
   - When order is delivered
   - When order status changes

2. **Customer Actions**
   - When customer confirms a waypoint
   - When customer rejects a waypoint
   - When new order is available


### Example: Sending Notification After Customer Recieve Order 

```csharp
// In ChangeOrderWayPointStatusCommand handler
var customer = await context.Customers
    .FirstOrDefaultAsync(c => c.Id == order.CustomerId, cancellationToken);

if (customer is not null)
{
    var firebaseTokens = new List<string>
    {
        customer.AndriodDevice,
        customer.IosDevice
    };

    var notificationBody = new NotificationBodyForMultipleDevices
    {
        Title = "New Order Available",
        Body = $"New order #{order.OrderNumber} is available for pickup within your area",
        FireBaseTokens = firebaseTokens.Where(x => !string.IsNullOrEmpty(x)).ToList(),
        PayLoad = new Dictionary<string, string>
        {
            { "orderId", order.Id.ToString() },
            { "orderNumber", order.OrderNumber },
            { "type", ((int)NotificationType.RecieveOrder).ToString() }
        }
    };

    await notificationService.SendNotificationAsyncToMultipleDevices(notificationBody);
}
```


### Payload Structure

The `PayLoad` dictionary should contain relevant data for the mobile app to handle the notification:

```csharp
var payLoad = new Dictionary<string, string>
{
    { "orderId", order.Id.ToString() },
    { "orderNumber", order.OrderNumber },
    { "type", notificationType.ToString() },
    { "action", "open_order_detail" }  // Action to perform when notification is tapped
};
```

---

## Best Practices

### 1. Token Management

- **Always validate tokens**: Check if token is not null or empty before sending
- **Handle token expiration**: FCM tokens can expire; implement token refresh mechanism
- **Remove invalid tokens**: When FCM returns invalid token error, remove it from database
- **Update tokens regularly**: Request fresh tokens from mobile app periodically

### 2. Error Handling

```csharp
var result = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);

// Check for invalid tokens
foreach (var response in result.Responses)
{
    if (response.IsSuccess == false)
    {
        if (response.Exception is FirebaseMessagingException fcmEx)
        {
            // Handle invalid token
            if (fcmEx.ErrorCode == MessagingErrorCode.InvalidArgument ||
                fcmEx.ErrorCode == MessagingErrorCode.Unregistered)
            {
                // Remove invalid token from database
                await RemoveInvalidToken(response.Exception.Message);
            }
        }
    }
}
```

### 3. Notification Content

- **Keep titles short**: Maximum 50-60 characters
- **Keep body concise**: Maximum 100-150 characters
- **Use clear language**: Avoid technical jargon
- **Include actionable information**: Tell users what they need to do

### 4. Performance

- **Batch notifications**: Use `SendEachForMulticastAsync` for multiple devices (max 500 tokens per batch)
- **Async operations**: Always use async/await for notification sending
- **Logging**: Log success/failure counts for monitoring
- **Rate limiting**: Be aware of FCM rate limits

### 5. Security

- **Validate user authentication**: Ensure only authenticated users can register tokens
- **Validate token ownership**: Ensure users can only update their own tokens
- **Secure Firebase credentials**: Never commit `FireBaseConfigurations.json` to version control
- **Use environment variables**: Store sensitive configuration in environment variables

### 6. Mobile App Integration

#### Android (Flutter Example)

```dart
// Get FCM token
String? token = await FirebaseMessaging.instance.getToken();

// Send token to backend
await apiClient.addDevices(
  androidDevice: token,
  iosDevice: null,
);

// Listen for token refresh
FirebaseMessaging.instance.onTokenRefresh.listen((newToken) {
  // Send updated token to backend
  apiClient.addDevices(androidDevice: newToken, iosDevice: null);
});
```

#### iOS (Flutter Example)

```dart
// Get FCM token
String? token = await FirebaseMessaging.instance.getToken();

// Send token to backend
await apiClient.addDevices(
  androidDevice: null,
  iosDevice: token,
);
```

### 7. Testing

- **Test with real devices**: Always test on physical devices, not just emulators
- **Test token registration**: Verify tokens are stored correctly
- **Test notification delivery**: Verify notifications arrive on devices
- **Test notification handling**: Verify app handles notification taps correctly
- **Test token refresh**: Verify token updates work correctly

---

## Troubleshooting

### Common Issues

1. **Notifications not received**
   - Check if device token is stored in database
   - Verify Firebase configuration is correct
   - Check FCM service account permissions
   - Verify mobile app has proper Firebase configuration

2. **Invalid token errors**
   - Token may have expired; implement token refresh
   - User may have uninstalled the app
   - Token may be from a different Firebase project

3. **Notifications delayed**
   - Check FCM service status
   - Verify network connectivity
   - Check device battery optimization settings

4. **Android notifications not showing**
   - Verify notification channel is created
   - Check app notification permissions
   - Verify `ChannelId` matches mobile app configuration

---

## Summary

This implementation provides a complete push notification system using Firebase Cloud Messaging. Key points:

1. **Device tokens are stored** in the database when users log in or when tokens are refreshed
2. **API endpoints** (`/api/Customer/AddDevices`) handle token registration
3. **NotificationService** sends notifications using FCM
4. **Notifications are sent** at appropriate business events (order status changes, user actions, etc.)
5. **Both single and multiple device** notifications are supported

For questions or issues, refer to the [Firebase Cloud Messaging Documentation](https://firebase.google.com/docs/cloud-messaging).

