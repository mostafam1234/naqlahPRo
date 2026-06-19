# Backend API Requirements — Naqlah Driver App

**Audience:** Backend / .NET API team  
**Driver app repo:** `naqlah_driver`  
**Base URL (current):** `https://naqlah.runasp.net/`  
**Auth:** Bearer token on all `DeliveryMan` / `DeliveryOrder` endpoints (same as today)  
**Source of truth in app:** `lib/constants/constant_strings.dart`, `lib/models/api/backend_api_contract.dart`

This document lists what the **driver mobile app already calls or expects**. Items marked **NEW** are not confirmed on the live API yet and must be implemented for full PDF compliance.

---

## Priority summary

| Priority | Area | Why |
|----------|------|-----|
| P0 | Registration DTO changes | Driver onboarding will fail or reject payloads without these |
| P0 | Order pricing fields (`driverNetAmount`) | Wrong amounts shown to drivers today |
| P1 | Driver cancel + confirm-pickup endpoints | Order workflow from PDF |
| P1 | Wallet balance / transactions / withdraw | Wallet screen wired in app |
| P1 | Notifications inbox | Notifications screen wired in app |
| P2 | Scheduled jobs (30-day reminder, 2h pickup reminder) | Push-only; no direct HTTP from app |
| P2 | Payment gate (`canProceed`) | Blocks waypoint progress in app when `false` |

---

## 1. Registration — Personal info

**Existing endpoint:** `POST api/DeliveryMan/CreatePersonalInfo`

### Changes required

| Field | Status | Notes |
|-------|--------|-------|
| `dateOfBirth` | **NEW required** | Format: `yyyy-MM-dd`. Replaces `deliveryTypeId` (resident/citizen toggle removed from app). |
| `deliveryTypeId` | **Deprecated** | App no longer sends this. Backend should not require it. |
| `fullName` | Existing | Driver / captain name |
| `phoneNumber` | Existing | From login user |
| `identityNumber` | Existing required | |
| `deliveryLicenseTypeId` | Existing required | `1` = private, `2` = public |
| `address` | Optional | App sends `null` when empty |
| `identityExpirationDate` | Optional | |
| `drivingLicenseExpirationDate` | Optional | |
| `frontIdenitytImage` | Required | Base64 — front of ID only |
| `frontDrivingLicenseImage` | Required | Base64 — front of license only |
| `personalImage` | Optional | Base64 |
| `backIdenitytImage` | **Removed** | App no longer sends |
| `backDrivingLicenseImage` | **Removed** | App no longer sends |

### Validation matrix (backend must enforce)

**Required:** email + password (register step), `fullName`, `phoneNumber`, `identityNumber`, `dateOfBirth`, `deliveryLicenseTypeId`, front ID image, front license image.

**Optional:** address, ID expiry, license expiry, personal photo.

### Login response (unchanged shape, still required)

After `POST api/DeliveryMan/LogIn`, return flags so app routes incomplete onboarding:

```json
{
  "tokenResponse": { "accessToken": "...", "refreshToken": "...", "expiresIn": 3600 },
  "requiredPersonalInfo": false,
  "requiredVehicleInfo": false,
  "requiredCarOwnerInfo": false,
  "carOwnerType": 1
}
```

`carOwnerType`: `1` = individual owner, `2` = company, `3` = renter (unchanged).

---

## 2. Registration — Vehicle info

**Existing endpoint:** `POST api/DeliveryMan/AddVehicle`

### Changes required

| Field | Status | Notes |
|-------|--------|-------|
| `vehicleOwnerName` | **NEW required** | Owner name (individual / company / renter) collected on vehicle step |
| `vehicleOwnerTypeId` | Existing required | `1` individual, `2` company, `3` renter |
| `vehicleTypeId` | Existing required | |
| `vehicleBrandId` | Existing required | |
| `licensePlateNumber` | Existing required | |
| `licenseExpirationDate` | Optional | Istimara / registration expiry |
| `inSuranceExpirationDate` | Optional | |
| `frontImagePath` | Required | Car front — Base64 |
| `sideImagePath` | Required | Car side — Base64 |
| `frontLicenseImagePath` | Required | Istimara — Base64 |
| `frontInsuranceImagePath` | Optional | Base64 |
| `backLicenseImagePath` | **Removed** | App no longer sends |
| `backInsuranceImagePath` | **Removed** | App no longer sends |

---

## 3. Registration — Car owner sub-steps

### Individual owner — `POST api/DeliveryMan/AddCarOwnerResidentInfo`

| Field | Required | Notes |
|-------|----------|-------|
| `citizenName` | Yes | |
| `identityNumber` | Yes | |
| `bankAccountNumber` | Per current rules | |
| `frontIdentityImage` | Yes | Base64 front only |
| `backIdentityImage` | **Removed** | App no longer sends |

### Company owner — `POST api/DeliveryMan/AddCarOwnerCompanyInfo`

| Field | Required | Notes |
|-------|----------|-------|
| `companyName` | Yes | |
| `commercialRecordNumber` | Yes | |
| `taxNumber` | Yes | |
| `recordImagePath` | Yes | Commercial register — Base64 |
| `taxCertificateImage` | Optional | App no longer requires it client-side |

### Renter owner — `POST api/DeliveryMan/AddCarOwnerRenterInfo`

| Field | Required | Notes |
|-------|----------|-------|
| `citizenName` | Yes | |
| `identityNumber` | Yes | |
| `frontIdentityImage` | Yes | Base64 front only |
| `rentContractImage` | Optional | Rental document |
| `backIdentityImage` | **Removed** | App no longer sends |

---

## 4. Order statuses (enum extension)

App expects these integer values on all order list/detail DTOs:

| Value | Name | Description |
|-------|------|-------------|
| 1 | Pending | Unassigned |
| 2 | Assigned | Assigned to driver |
| 3 | Cancelled | |
| 4 | Completed | |
| 5 | ConfirmedGoingToPickup | **NEW** — Driver confirmed going to pickup |
| 6 | PickedUpByDriver | **NEW** — Shipment picked up (pending client approval) |
| 7 | NoShow | **NEW** — Driver no-show |

---

## 5. Order DTO fields (all order endpoints)

Add to responses of:

- `GET api/DeliveryOrder/GetPendingOrdersWithinRadius`
- `GET api/DeliveryOrder/GetDeliveryManOrders`
- `GET api/DeliveryOrder/GetCompletedDeliveryManOrders`
- `GET api/DeliveryOrder/GetOrderDetails/{orderId}`

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `driverNetAmount` | decimal | **Yes (P0)** | Amount driver earns — **excludes tax and service fee** |
| `total` | decimal | Yes | Client-facing total (includes tax + service fee) |
| `taxAmount` | decimal | Recommended | For breakdown / tooltip |
| `serviceFeeAmount` | decimal | Recommended | For breakdown / tooltip |
| `totalDesc` | string | Optional | Human-readable price breakdown (already used) |
| `canProceed` | bool | **NEW** | Default `true`. When `false`, app blocks waypoint photo upload |
| `driverConfirmedGoingToPickup` | bool | **NEW** | Driver confirmed ≥2h before scheduled pickup |
| `clientApprovedPickup` | bool | **NEW** | Client approved pickup photo / handoff |
| `expectedPickUpTime` | datetime | Existing | Used for scheduled orders |
| `paymentMethods` | array | Existing | Must be present on accepted-order endpoints |

**Business rule (PDF):** Push notifications and in-app amounts shown to the driver must use `driverNetAmount`, not `total`.

---

## 6. Order actions — NEW endpoints

All are **POST**, authenticated, `{orderId}` in URL. Empty body unless noted.

### 6.1 Driver cancel (reassign, not final cancel)

```
POST api/DeliveryOrder/CancelOrderByDriver/{orderId}
```

**Expected behavior (PDF):**

- Order returns to **Pending** (status `1`) for manual/admin reassignment.
- Do **not** treat as client cancellation / refund flow.
- Notify client and other drivers as appropriate.
- App previously called `api/CustomerOrder/CancelOrder/{orderId}` — that must **not** be used for drivers.

**Response:** `200 OK` (body can be `{ "success": true }` or `true`).

---

### 6.2 Confirm going to pickup

```
POST api/DeliveryOrder/ConfirmGoingToPickup/{orderId}
```

**Expected behavior:**

- Sets `driverConfirmedGoingToPickup = true`.
- Updates `orderStatus` to `5` (ConfirmedGoingToPickup) when applicable.
- Only allowed for assigned driver on scheduled orders.

**Backend job (PDF):** Send FCM reminder to driver **≥ 2 hours** before `expectedPickUpTime` asking them to confirm via this endpoint.

---

### 6.3 Confirm pickup by driver

```
POST api/DeliveryOrder/ConfirmPickupByDriver/{orderId}
```

**Expected behavior:**

- Driver confirms shipment was picked up (may include photo flow via existing waypoint API).
- Sets status toward `6` (PickedUpByDriver).
- Remains pending until `clientApprovedPickup = true` (client app / admin confirms).
- Notify client for approval.

---

## 7. Payment & proceed gate

**Existing:** `POST api/DeliveryOrder/ChangeWayPointStatus`

App sends `{ orderId, wayPointId, packImageBase64 }` as today.

**NEW logic backend must implement:**

| Payment type | When fee is settled | `canProceed` |
|--------------|---------------------|--------------|
| Mada (prepaid at order) | After driver accepts order | `true` once paid |
| Cash on delivery (COD) | After driver uploads loaded-shipment photo | `false` until payment recorded |

When `canProceed == false`, app shows *"Cannot proceed until transport fee is settled"* and blocks waypoint completion.

External drivers receive `driverNetAmount` in wallet after delivery; tax and service fee stay with platform.

---

## 8. Wallet — NEW endpoints

App screen: **Settings → My Wallet**

Bank account CRUD already exists (`api/DeliveryMan/BankAccount*`). Below are **additions**.

### 8.1 Get balance

```
GET api/DeliveryMan/Wallet/Balance
```

**Response:**

```json
{
  "balance": 1250.50,
  "pendingBalance": 200.00
}
```

App also accepts `availableBalance` as alias for `balance`.

---

### 8.2 Get transactions

```
GET api/DeliveryMan/Wallet/Transactions?pageNumber=1&pageSize=50
```

**Response:** array of:

```json
{
  "id": 101,
  "amount": 150.00,
  "type": "credit",
  "transactionType": "credit",
  "description": "Order #12345 payout",
  "descriptionAr": "دفعة طلب #12345",
  "status": "completed",
  "createdAt": "2026-06-09T14:30:00Z"
}
```

`type`: `credit` | `debit`

---

### 8.3 Request withdrawal

```
POST api/DeliveryMan/Wallet/Withdraw
Content-Type: application/json
```

**Request:**

```json
{
  "amount": 500.00,
  "bankAccountId": 12
}
```

**Expected behavior:**

- Debit driver wallet; create pending withdrawal record.
- Transfer to IBAN linked to `bankAccountId`.
- Return success or validation error (insufficient balance, invalid account).

**Open question for product:** Confirm this is the **driver wallet** (not client wallet). App assumes driver-only wallet.

---

## 9. Notifications inbox — NEW endpoints

App screen: **Settings → Notifications**

### 9.1 List notifications

```
GET api/DeliveryMan/Notifications?pageNumber=1&pageSize=50
```

**Response** (either shape accepted):

```json
{
  "items": [
    {
      "id": 1,
      "title": "New order nearby",
      "titleAr": "طلب جديد بالقرب منك",
      "body": "Order #456 is available",
      "bodyAr": "الطلب #456 متاح",
      "type": "newOrder",
      "notificationType": "newOrder",
      "isRead": false,
      "orderId": 456,
      "createdAt": "2026-06-09T10:00:00Z"
    }
  ],
  "totalCount": 1
}
```

Or a plain JSON array of notification objects.

**Suggested `type` values:**

| type | When |
|------|------|
| `newOrder` | New pending order in radius |
| `orderReassigned` | Driver unlinked; order reassigned |
| `incompleteRegistration` | 30-day profile reminder |
| `pickupReminder` | 2h before scheduled pickup |
| `general` | Manual admin notifications |

---

### 9.2 Mark as read

```
POST api/DeliveryMan/Notifications/{notificationId}/Read
```

**Response:** `200 OK`

---

## 10. Scheduled jobs (backend-only, no HTTP from app)

### 10.1 Incomplete registration — 30-day reminder

**Trigger:** Daily job.

**Condition:** Driver has `requiredPersonalInfo`, `requiredVehicleInfo`, or `requiredCarOwnerInfo` still `true` for **≥ 30 days** since registration.

**Actions:**

1. Send FCM push to driver device token(s).
2. Insert row in notifications table (`type = incompleteRegistration`).
3. **Audit log** entry: `{ driverId, sentAt, reminderType, legalDisclaimerAck }` for legal compliance.

App shows an in-app banner when login flags are true; push + inbox are backend responsibilities.

---

### 10.2 Scheduled pickup — 2-hour reminder

**Trigger:** Job runs frequently (e.g. every 15 min).

**Condition:** Order has `expectedPickUpTime` and driver is assigned; pickup is in **≤ 2 hours** and driver has not called `ConfirmGoingToPickup`.

**Actions:**

1. FCM to assigned driver with confirm action.
2. Notification inbox entry (`type = pickupReminder`).

---

## 11. Existing endpoints — no URL change (verify behavior)

| Endpoint | Notes |
|----------|-------|
| `POST api/DeliveryMan/Register` | Unchanged |
| `POST api/DeliveryMan/LogIn` | Must keep onboarding flags |
| `POST /api/DeliveryOrder/AssignOrder` | Unchanged |
| `POST /api/DeliveryOrder/ChangeWayPointStatus` | Must respect `canProceed` |
| `GET api/DeliveryMan/VehicleTypes` | Vehicle taxonomy may change in admin; same response shape |
| `GET api/DeliveryMan/VehicleBrands` | Unchanged |
| `POST api/DeliveryMan/BankAccount` | Unchanged |
| `POST api/SuggestionAndComplaint/Add` | Unchanged (admin handles resolution) |

---

## 12. Out of scope for driver app (admin / client backend)

These PDF items are **not** implemented in `naqlah_driver` but may need separate backend work:

- Registered clients list, Excel exports, Operations Center dashboard
- Cost calculator / discount codes admin UI
- Point-in-time database restore
- Captain review admin page, billing admin module
- Client-side pickup approval UI (needed for `clientApprovedPickup`)
- Invoice download API (driver invoice screen still mock)

---

## 13. Acceptance checklist for backend QA

- [ ] `CreatePersonalInfo` accepts `dateOfBirth` without `deliveryTypeId`
- [ ] Back-side image fields are optional / ignored when omitted
- [ ] `AddVehicle` accepts `vehicleOwnerName`
- [ ] All order list/detail APIs return `driverNetAmount` (≠ `total` when fees apply)
- [ ] `CancelOrderByDriver` sets order back to pending, not cancelled
- [ ] `ConfirmGoingToPickup` and `ConfirmPickupByDriver` update status + flags
- [ ] `canProceed` blocks waypoint when payment pending (COD flow)
- [ ] Wallet Balance / Transactions / Withdraw return valid data
- [ ] Notifications list + mark-read work with Bearer auth
- [ ] 30-day and 2-hour reminder jobs send FCM + persist notifications + audit log

---

## 14. Contact / references

- Driver app contract class: `lib/models/api/backend_api_contract.dart`
- URL constants: `lib/constants/constant_strings.dart`
- Order models: `lib/models/request/accepted_order_details.dart`, `lib/models/request/cycle_models.dart`

**Document version:** 2026-06-09 — aligned with naqlah_driver PDF implementation plan.
