# Backend API Requirements — Customer App

**Project:** `naqlah_customer` (Flutter customer app)  
**Source:** System requirements PDF (نقاط على السيستم) + customer app implementation  
**Base URL:** `https://naqlah.runasp.net/`  
**Audience:** Backend / API team  
**Date:** June 2026

This document lists **new endpoints**, **changes to existing endpoints**, **response field additions**, and **business rules** the customer app now depends on. Please add all items to Swagger/OpenAPI once implemented.

---

## Priority Summary

| Priority | Area | Blocker for |
|----------|------|-------------|
| P0 | Order create + additional services + vehicle pricing breakdown | Order flow |
| P0 | Payment (Mada, COD, wallet) + discount code | Checkout |
| P0 | Order details fields (`isPaid`, fee breakdown) | Payment timing & invoices |
| P1 | Cancel with IBAN + refund tracking | Cancellation flow |
| P1 | Invoice API + PDF | Billing screen |
| P1 | Notifications history + push payload types | Notification center |
| P2 | Extended order statuses | Order list / tracking labels |
| P2 | Rate order | Post-delivery rating |
| P2 | Profile fields (establishment address/tax) | Profile screen |
| P3 | Wallet refund transaction type | Refund history in wallet |

---

## 1. New APIs (Must Implement)

### 1.1 GET `api/CustomerOrder/GetAdditionalServices`

**Purpose:** Catalog of optional add-on services (cartons, pallets, boxes, wrapping roll) with unit price.

**Auth:** Customer bearer token

**Response `200`:**
```json
[
  {
    "id": 1,
    "name": "Carton",
    "unitPrice": 3.0
  },
  {
    "id": 2,
    "name": "Pallet",
    "unitPrice": 10.0
  }
]
```

**Notes:**
- App accepts `unitPrice` or `price` as field alias.
- Admin manages services and prices (see PDF section 7 — خدمات إضافية).
- If endpoint is unavailable, app falls back to hardcoded items (not acceptable for production).

---

### 1.2 POST `api/CustomerOrder/ValidateDiscountCode`

**Purpose:** Validate promo/discount code and return discount amount for an order.

**Request body:**
```json
{
  "code": "PROMO10",
  "orderId": 123
}
```

**Response `200`:**
```json
{
  "isValid": true,
  "discountAmount": 15.0,
  "message": "Discount applied successfully"
}
```

**Notes:**
- App also accepts `success: true` as alias for `isValid`.
- Discount must be reflected in final order total and invoice.

---

### 1.3 POST `api/CustomerOrder/ProcessPayment`

**Purpose:** Process card payment (Mada) for an order.

**Request body:**
```json
{
  "orderId": 123,
  "paymentMethodId": 1,
  "discountCode": "PROMO10"
}
```

| `paymentMethodId` | Method |
|-------------------|--------|
| `1` | Mada card |
| `2` | Cash on delivery (COD) — confirm intent only; actual charge on pickup |
| `3` | Wallet (prefer dedicated endpoint below) |

**Response `200`:**
```json
{
  "success": true,
  "message": "Payment processed",
  "transactionId": "TXN-001"
}
```

**Business rules (from PDF):**
- Customer-facing total **includes** tax + service fee.
- Captain payout **excludes** tax + service fee.
- Mada payment is triggered after captain accepts pickup assignment.

**Integration:** Connect to Mada payment gateway; return gateway redirect URL or token if needed (app currently expects direct success response).

---

### 1.4 POST `api/CustomerOrder/PayFromWallet`

**Purpose:** Deduct order amount from customer wallet balance.

**Request body:**
```json
{
  "orderId": 123,
  "discountCode": "PROMO10"
}
```

**Response `200`:**
```json
{
  "success": true,
  "message": "Paid from wallet",
  "newBalance": 250.0
}
```

**Business rules:**
- Validate sufficient wallet balance before debit.
- Create wallet transaction linked to `orderId`.
- Issue proforma invoice on payment; final invoice on delivery completion.

---

### 1.5 GET `api/CustomerOrder/GetOrderInvoice/{orderId}`

**Purpose:** Return invoice data and optional PDF URL for an order.

**Response `200`:**
```json
{
  "orderId": 123,
  "orderNumber": "ORD-2026-001",
  "customerName": "Ahmed Ali",
  "senderName": "Captain Name",
  "deliveryManName": "Captain Name",
  "isProforma": false,
  "transportAmount": 100.0,
  "serviceFee": 20.0,
  "taxAmount": 18.0,
  "discountAmount": 5.0,
  "totalAmount": 133.0,
  "total": 133.0,
  "refundedAmount": 0.0,
  "pdfUrl": "https://naqlah.runasp.net/invoices/123.pdf",
  "createdDate": "2026-06-08T10:30:00Z",
  "lineItems": [
    {
      "name": "Carton",
      "amount": 15.0,
      "quantity": 5
    },
    {
      "name": "Loading service",
      "amount": 30.0,
      "quantity": 1
    }
  ]
}
```

**Notes:**
- `isProforma: true` = عرض سعر (preliminary); `false` = final invoice.
- `pdfUrl` enables in-app download/share.

---

### 1.6 POST `api/CustomerOrder/RateOrder`

**Purpose:** Submit customer rating after order completion.

**Request body:**
```json
{
  "orderId": 123,
  "rating": 5,
  "comment": "Excellent service"
}
```

**Response `200`:**
```json
{
  "success": true,
  "message": "Rating submitted"
}
```

**Validation:**
- `rating`: integer 1–5
- Only allow rating on `completed` orders
- One rating per order per customer

---

### 1.7 GET `api/Customer/GetNotifications`

**Purpose:** In-app notification history for the logged-in customer.

**Response `200`:**
```json
[
  {
    "id": 1,
    "title": "Order Update",
    "body": "Captain confirmed going to pickup",
    "type": "order_update",
    "notificationType": "order_update",
    "orderId": 123,
    "isRead": false,
    "createdDate": "2026-06-08T09:00:00Z"
  }
]
```

**Supported `type` / `notificationType` values:**

| Value | Meaning |
|-------|---------|
| `order_update` | General order status change |
| `payment_required` | Customer must pay |
| `refund` | Refund initiated/completed |
| `reassignment` | Shipment reassigned to new captain |
| `captain_cancelled` | Captain cancelled; order back to pending |
| `general` | Other / manual admin notification |

---

### 1.8 POST `api/Customer/MarkNotificationRead`

**Purpose:** Mark a notification as read.

**Request body:**
```json
{
  "notificationId": 1
}
```

**Response `200`:**
```json
{
  "success": true
}
```

---

## 2. Changes to Existing APIs

### 2.1 POST `api/CustomerOrder/Create`

**New request field:**

```json
{
  "orderPackId": 2,
  "orderTypeId": 1,
  "mainCategoryIds": [1, 3],
  "orderServiceIds": [2],
  "additionalServices": [
    { "serviceId": 1, "quantity": 5 },
    { "serviceId": 3, "quantity": 2 }
  ],
  "isScheduled": false,
  "expectedPickUpTime": null,
  "wayPoints": [ ... ]
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `additionalServices` | array | No | Selected add-on services with quantity |
| `additionalServices[].serviceId` | int | Yes | From `GetAdditionalServices` |
| `additionalServices[].quantity` | int | Yes | Must be > 0 |
| `orderPackId` | int | Yes* | Shipment pack/size ID (*required when packs exist) |

**Expected behavior:**
- Calculate additional services total = `unitPrice × quantity` per item.
- Include in order cost breakdown shown to customer.

---

### 2.2 POST `api/CustomerOrder/SelectVehicleType`

**New optional request field:**

```json
{
  "orderId": 123,
  "vehicleTypeId": 4,
  "paymentMethodId": 1
}
```

| `paymentMethodId` | Method |
|-------------------|--------|
| `1` | Mada |
| `2` | COD on pickup |
| `3` | Wallet |

Store selected payment method on the order for payment-timing logic.

---

### 2.3 POST `api/CustomerOrder/CancelOrder/{orderId}`

**New optional request body (when order is already paid):**

```json
{
  "iban": "SA0380000000608010167519"
}
```

**Expected behavior (from PDF):**

| Scenario | Backend action |
|----------|----------------|
| Cancelled by customer, **not paid** | Cancel order; notify customer + captain |
| Cancelled by customer, **paid** | Require IBAN; initiate refund within 2 business days; notify customer |
| Cancelled by captain | Return order to `pending`; notify customer + captain for reassignment |

**Response should include:**
```json
{
  "success": true,
  "message": "Order cancelled",
  "refundInitiated": true,
  "refundStatus": "pending"
}
```

---

### 2.4 GET `api/CustomerOrder/GetOrderDetails/{orderId}`

**New response fields required:**

```json
{
  "id": 123,
  "status": 7,
  "statusName": "Awaiting Payment",
  "total": 133.0,
  "isPaid": false,
  "transportAmount": 100.0,
  "baseAmount": 100.0,
  "serviceFee": 20.0,
  "taxAmount": 18.0,
  "discountAmount": 5.0,
  "paymentMethods": [
    {
      "paymentMethodId": 1,
      "paymentMethodName": "Mada",
      "amount": 133.0
    }
  ],
  ...
}
```

| Field | Type | Description |
|-------|------|-------------|
| `isPaid` | bool | Whether customer has paid transport cost |
| `transportAmount` / `baseAmount` | double | Base transport cost (no tax/fees) |
| `serviceFee` | double | Service fee amount |
| `taxAmount` | double | VAT amount |
| `discountAmount` | double | Applied discount |

**Payment prompt logic in app:**
- **Mada:** show payment when `status >= assigned` and `isPaid == false`
- **COD:** show payment when origin waypoint has `packImagePath` uploaded and `isPaid == false`

---

### 2.5 POST `api/CustomerOrder/Create` — Response (`matchingVehicles`)

**Extended vehicle object in create response:**

```json
{
  "orderId": 123,
  "matchingVehicles": [
    {
      "id": 4,
      "name": "Dina 5 Ton",
      "iconPath": "https://...",
      "price": 133.0,
      "totalPrice": 133.0,
      "basePrice": 100.0,
      "transportAmount": 100.0,
      "serviceFee": 20.0,
      "taxAmount": 18.0,
      "discountAmount": 0.0
    }
  ]
}
```

Customer sees **total with tax + service fee**. Captain sees amount **without** tax and service fee.

---

### 2.6 GET `api/Customer/GetCustomerInfo`

**Ensure establishment profile returns (for profile screen):**

```json
{
  "id": 1,
  "phoneNumber": "05xxxxxxxx",
  "customerType": 2,
  "walletBalance": 500.0,
  "establishment": {
    "id": 1,
    "name": "Company Name",
    "mobileNumber": "05xxxxxxxx",
    "address": "National address short code / full address",
    "taxRegistrationNumber": "300xxxxxxxxxxxxx",
    "recordImagePath": "https://...",
    "taxRegistrationImagePath": "https://...",
    "representative": { ... }
  }
}
```

---

### 2.7 GET `api/CustomerWallet/GetTransactions`

**Add refund transaction support:**

```json
[
  {
    "id": 10,
    "description": "Refund for order #123",
    "amount": 133.0,
    "isWithdraw": false,
    "date": "2026-06-08T12:00:00Z",
    "orderId": 123,
    "transactionType": "refund"
  }
]
```

Suggested `transactionType` values: `charge`, `payment`, `refund`, `withdraw`.

---

## 3. Order Status Enum — Extend

Current app constants (must match backend):

| ID | Name (EN) | Arabic (PDF) |
|----|-----------|--------------|
| `1` | Pending | معلقة |
| `2` | Assigned | منسوبة |
| `3` | Cancelled | ملغية |
| `4` | Completed | مكتملة |
| `5` | Confirmed going to pickup | تم تأكيد الذهاب لالتقاط الشحنة |
| `6` | Pickup from delegate | التقاط الطلب من المندوب |
| `7` | Awaiting payment | في انتظار الدفع |

**Status `5` business rule:** Captain receives reminder 2 hours before pickup; captain confirms via app; marked with checkmark.

**Status `6` business rule:** Set when customer approves loaded shipment photo (waypoint confirm API already exists).

Return `statusName` localized based on `Accept-Language` header if possible.

---

## 4. Payment Method Enum

| ID | Name |
|----|------|
| `1` | Mada card |
| `2` | Cash on delivery (عند الاستلام) |
| `3` | Wallet |

---

## 5. Push Notification Payload Requirements

FCM data payload must include routable fields for the customer app:

```json
{
  "notificationType": "captain_cancelled",
  "orderId": "123",
  "title": "Order Update",
  "body": "Captain unlinked from shipment. Reassigning..."
}
```

**Events that must send push + appear in notification history:**

| Event | `notificationType` |
|-------|-------------------|
| Captain confirmed going to pickup | `order_update` |
| Captain uploaded loaded shipment photo | `payment_required` |
| Payment required (Mada) | `payment_required` |
| Order cancelled by captain | `captain_cancelled` |
| Shipment reassigned | `reassignment` |
| Refund initiated | `refund` |
| Refund completed | `refund` |
| Manual admin alert | `general` |

---

## 6. Business Rules Checklist (PDF)

Please confirm backend implements:

- [ ] **Pricing:** Customer total = transport + service fee + tax − discount + additional services
- [ ] **Captain payout:** Amount shown to captain excludes tax and service fee
- [ ] **Mada timing:** Charge customer after captain accepts pickup
- [ ] **COD timing:** Charge/settle when captain uploads loaded shipment photo; captain cannot leave until transport fee is settled
- [ ] **Wallet:** Preliminary proforma on payment; final invoice on delivery completion
- [ ] **Cancel + refund:** Paid cancellation requires IBAN; refund within 2 business days
- [ ] **Captain cancel:** Order returns to pending for manual reassignment
- [ ] **Additional services:** Admin-managed catalog with per-unit pricing (carton, pallet, box, wrapping roll)
- [ ] **Discount codes:** Admin-managed codes with usage limits and expiry

---

## 7. Existing APIs — Documentation Only

These are already used by the app but missing from Swagger (see also `APIs_NOT_IN_SWAGGER.md`):

| Method | Endpoint |
|--------|----------|
| POST | `api/CustomerWallet/Charge` |
| GET | `api/CustomerWallet/GetTransactions` |
| GET | `api/CustomerWallet/GetBalance` |
| POST | `api/SuggestionAndComplaint/Add` |
| GET | `api/TermsAndConditions/GetActiveTermsAndConditions` |

Please document in Swagger and confirm payloads are stable.

---

## 8. Field Name Clarifications

Please confirm correct spellings with backend (app currently sends these as-is):

| App field | Suggested correct name |
|-----------|------------------------|
| `andriodDevice` | `androidDevice`? |
| `recoredImage` | `recordImage`? |
| `representitveName` | `representativeName`? |
| `representitvePhoneNumber` | `representativePhoneNumber`? |
| `isOrgin` / `isDestenation` | `isOrigin` / `isDestination`? |

Reply with canonical names so we can align the app in a follow-up PR.

---

## 9. Suggested Implementation Order for Backend

```
Phase 1 (order flow)
  → GetAdditionalServices
  → Create (additionalServices field)
  → matchingVehicles price breakdown fields

Phase 2 (payment)
  → ValidateDiscountCode
  → ProcessPayment (Mada gateway)
  → PayFromWallet
  → GetOrderDetails (isPaid + fee fields)
  → SelectVehicleType (paymentMethodId)

Phase 3 (post-order)
  → CancelOrder (IBAN + refund)
  → GetOrderInvoice
  → GetNotifications + MarkNotificationRead
  → Extended order statuses (5, 6, 7)
  → RateOrder
  → Wallet refund transaction type
```

---

## 10. Contact / Questions

When implementing, please confirm:

1. Exact Mada gateway integration approach (redirect vs SDK token).
2. Whether `GetAssistantWorks` should return `unitPrice` per service.
3. Whether assistant works should support **multi-select** (app currently single-select).
4. Account activation flow — is `Activate` API required after registration?
5. Final canonical field names for typos listed in section 8.

---

*Generated from customer app implementation aligned with نقاط على السيستم.pdf requirements.*
