# NotificationHub API Documentation

## Table of Contents

- [Overview](#overview)
- [API Base URL](#api-base-url)
- [Authentication](#authentication)
- [Common Concepts](#common-concepts)
- [HTTP Status Codes](#http-status-codes)
- [Error Handling](#error-handling)
- [Endpoints](#endpoints)
  - [Send Push Notification](#send-push-notification)
  - [Send Email Notification](#send-email-notification)
  - [Send SMS Notification](#send-sms-notification)
  - [Send WhatsApp Notification](#send-whatsapp-notification)
  - [Get Notification by ID](#get-notification-by-id)
  - [Get Failed Notifications](#get-failed-notifications)
- [Real-Time Notifications with SignalR](#real-time-notifications-with-signalr)
- [Data Models](#data-models)
- [Implementation Status](#implementation-status)
- [Best Practices](#best-practices)

---

## Overview

NotificationHub is a comprehensive notification delivery system built with ASP.NET Core that supports multiple channels:

- **Push Notifications**: Real-time delivery via SignalR to connected clients
- **Email**: Asynchronous delivery via RabbitMQ message queue
- **SMS**: Asynchronous delivery via RabbitMQ message queue
- **WhatsApp**: Asynchronous delivery via RabbitMQ message queue

The API follows RESTful principles with OpenAPI documentation available at `/scalar/v1` in development environments. All async notification types use a resilient queue-based architecture with automatic retries (up to 3 attempts), dead letter queue (DLQ) handling, and comprehensive status tracking.

### Architecture Highlights

- **Minimal APIs**: Lightweight HTTP handler pattern using ASP.NET Core Minimal APIs
- **Domain-Driven Design**: Table-Per-Hierarchy (TPH) inheritance for Notification entities
- **Message-Driven**: MassTransit 9.x integration with RabbitMQ for async channels
- **Resilience**: Automatic retry with exponential backoff and DLQ fallback
- **Real-Time**: SignalR hub for push notification delivery and status updates
- **Structured Logging**: Serilog integration with correlation IDs for request tracing

---

## API Base URL

```
http://localhost:5000/api/notifications
```

When deployed, replace `localhost:5000` with your actual host and port.

---

## Authentication

**Status**: Not yet implemented

Authentication will be added in a future release. Currently, all endpoints are publicly accessible without authentication. When implemented, the API will support:

- Bearer token authentication (JWT)
- Per-user recipient isolation
- Rate limiting per authenticated user

**Note**: In a production environment, implement authentication before deploying.

---

## Common Concepts

### Notification Channels

The NotificationHub supports four distinct notification channels:

| Channel | Type | Delivery | Use Case |
|---------|------|----------|----------|
| `Push` (0) | Real-time | SignalR | Immediate in-app notifications |
| `Email` (1) | Async | RabbitMQ → Worker | Formal communications, newsletters |
| `Sms` (2) | Async | RabbitMQ → Worker | Urgent alerts, time-sensitive info |
| `WhatsApp` (3) | Async | RabbitMQ → Worker | Customer engagement, two-way messaging |

### Notification Status

Notifications progress through the following states:

| Status | Value | Description |
|--------|-------|-------------|
| `Pending` (0) | Initial state | Just created, not yet queued |
| `Queued` (1) | Processing | Enqueued for delivery (async channels) |
| `Sent` (2) | In transit | Transmitted to recipient/carrier |
| `Delivered` (3) | Complete | Successfully delivered to recipient |
| `Failed` (4) | Error | Delivery failed after max retries |
| `Cancelled` (5) | Aborted | Cancelled before delivery |

Transitions follow a strict state machine:
```
Pending → Queued → Sent → Delivered
  ↓         ↓       ↓
Cancelled  Failed  Failed (but can retry)
```

### Notification Priority

Priority levels affect queue processing order in async channels:

| Priority | Value | Description |
|----------|-------|-------------|
| `Low` (0) | Lowest | Non-urgent notifications, process when available |
| `Normal` (1) | Default | Standard priority, processed in FIFO order |
| `High` (2) | Elevated | Important notifications, process before normal |
| `Critical` (3) | Highest | Urgent alerts, prioritized immediately |

### Recipient ID

All notifications require a `recipientId` (UUID/GUID) that identifies the intended recipient. This is a logical identifier unique to your application's user model and is used for:

- Group-based SignalR delivery (recipient groups: `recipient-{recipientId}`)
- Database queries for notification history
- Audit and tracking purposes

---

## HTTP Status Codes

The API uses standard HTTP status codes to indicate operation outcomes:

| Code | Meaning | Description |
|------|---------|-------------|
| `200 OK` | Success | Request completed successfully, response body contains data |
| `202 Accepted` | Accepted | Async request queued for processing (typical for async channels) |
| `400 Bad Request` | Invalid Input | Request validation failed (malformed JSON, invalid values) |
| `404 Not Found` | Not Found | Requested resource does not exist |
| `422 Unprocessable Entity` | Validation Failed | Domain validation failed (e.g., invalid email format) |
| `500 Internal Server Error` | Server Error | Unexpected server-side error |
| `503 Service Unavailable` | Service Down | RabbitMQ or database connectivity issues |

---

## Error Handling

### Error Response Format

When errors occur, the API returns a structured error response:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Validation failed: Invalid email address format",
  "traceId": "0HN1GRST7HG4V:00000001"
}
```

### Common Error Scenarios

**Invalid Email Address**
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Unprocessable Entity",
  "status": 422,
  "detail": "Email address validation failed: 'invalid-email' is not a valid email address"
}
```

**Invalid Phone Number**
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Unprocessable Entity",
  "status": 422,
  "detail": "Phone number validation failed: '12345' is not a valid phone number (must be 10-15 digits)"
}
```

**RabbitMQ Connection Failed**
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.3",
  "title": "Service Unavailable",
  "status": 503,
  "detail": "Message broker is unavailable. Please try again later."
}
```

**Notification Not Found**
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.4.5",
  "title": "Not Found",
  "status": 404,
  "detail": "Notification with ID 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx' not found"
}
```

### Error Tracing

Every error response includes a `traceId` that correlates the error with server logs. Use this ID when reporting issues to support or developers. Log entries include CorrelationId, MachineName, and ThreadId for comprehensive debugging.

---

## Endpoints

### Send Push Notification

Send a real-time notification to connected clients via SignalR. Push notifications are delivered immediately to any connected client in the recipient's group.

**Endpoint**: `POST /api/notifications/push`

**Status**: Implementation pending - endpoint structure defined, handler not yet implemented

**Request Body**

```json
{
  "recipientId": "550e8400-e29b-41d4-a716-446655440000",
  "title": "New Message",
  "body": "You have a new message from John Doe",
  "priority": 1,
  "metadata": {
    "source": "chat-service",
    "threadId": "thread-123"
  }
}
```

**Parameters**

| Field | Type | Required | Constraints | Description |
|-------|------|----------|-------------|-------------|
| `recipientId` | UUID | Yes | Non-empty GUID | The recipient's user ID |
| `title` | String | Yes | 1-500 chars | Notification title/subject |
| `body` | String | Yes | 1-5000 chars | Notification message content |
| `priority` | Integer | No | 0-3 | Priority level (0=Low, 1=Normal, 2=High, 3=Critical). Defaults to 1 (Normal) |
| `metadata` | Object | No | Key-value pairs | Custom metadata for tracking and filtering |

**Response**

Status: `200 OK`

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440001",
  "recipientId": "550e8400-e29b-41d4-a716-446655440000",
  "channel": 0,
  "status": 2,
  "title": "New Message",
  "body": "You have a new message from John Doe",
  "priority": 1,
  "createdAt": "2026-03-09T10:30:00Z",
  "sentAt": "2026-03-09T10:30:00Z",
  "deliveredAt": null,
  "failedAt": null,
  "retryCount": 0,
  "metadata": {
    "source": "chat-service",
    "threadId": "thread-123"
  }
}
```

**Response Fields**

| Field | Type | Description |
|-------|------|-------------|
| `id` | UUID | Unique notification identifier (generated server-side) |
| `recipientId` | UUID | The recipient's user ID |
| `channel` | Integer | Channel type: 0=Push, 1=Email, 2=Sms, 3=WhatsApp |
| `status` | Integer | Current status: 0=Pending, 1=Queued, 2=Sent, 3=Delivered, 4=Failed, 5=Cancelled |
| `title` | String | Notification title |
| `body` | String | Notification content |
| `priority` | Integer | Priority level assigned |
| `createdAt` | DateTime (ISO 8601) | Notification creation timestamp (UTC) |
| `sentAt` | DateTime (ISO 8601) | Timestamp when sent (null if not yet sent) |
| `deliveredAt` | DateTime (ISO 8601) | Timestamp when delivered (null if not yet delivered) |
| `failedAt` | DateTime (ISO 8601) | Timestamp when failed (null if not failed) |
| `retryCount` | Integer | Number of retry attempts (0 for push) |
| `metadata` | Object | Custom metadata returned as-is |

**Error Responses**

| Status | Scenario |
|--------|----------|
| 400 Bad Request | Malformed JSON or missing required fields |
| 422 Unprocessable Entity | Validation failed (e.g., empty title or recipientId) |
| 500 Internal Server Error | Unexpected error during processing |

**Example Usage (JavaScript/TypeScript)**

```typescript
async function sendPushNotification() {
  const response = await fetch('/api/notifications/push', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      recipientId: '550e8400-e29b-41d4-a716-446655440000',
      title: 'New Message',
      body: 'You have a new message from John Doe',
      priority: 2, // High priority
      metadata: {
        source: 'chat-service',
        actionUrl: '/messages/thread-123'
      }
    })
  });

  if (response.ok) {
    const notification = await response.json();
    console.log('Push sent:', notification.id);
  } else {
    const error = await response.json();
    console.error('Error:', error.detail);
  }
}
```

**Implementation Notes**

- Push notifications are delivered synchronously to connected clients
- Notification is persisted to database with status `Sent` immediately upon receipt
- Clients must be connected to SignalR hub and joined to the appropriate recipient group
- If no clients are connected, the notification is still persisted (status `Sent`) but not visible to any client

---

### Send Email Notification

Queue an email notification for asynchronous delivery. The request is accepted immediately and processed by the Email Worker service.

**Endpoint**: `POST /api/notifications/email`

**Status**: Implementation pending - endpoint structure defined, handler not yet implemented

**Request Body**

```json
{
  "recipientId": "550e8400-e29b-41d4-a716-446655440000",
  "subject": "Welcome to NotificationHub",
  "body": "<h1>Welcome!</h1><p>Thank you for signing up.</p>",
  "to": "user@example.com",
  "from": "noreply@notificationhub.com",
  "replyTo": "support@notificationhub.com",
  "cc": ["manager@example.com"],
  "bcc": ["archive@example.com"],
  "isHtml": true,
  "attachmentUrls": ["https://example.com/files/document.pdf"],
  "priority": 1,
  "metadata": {
    "template": "welcome",
    "userId": "user-123"
  }
}
```

**Parameters**

| Field | Type | Required | Constraints | Description |
|-------|------|----------|-------------|-------------|
| `recipientId` | UUID | Yes | Non-empty GUID | The recipient's user ID (logical identifier) |
| `subject` | String | Yes | 1-500 chars | Email subject line |
| `body` | String | Yes | 1-50000 chars | Email body (plain text or HTML) |
| `to` | String | Yes | Valid email | Primary recipient email address |
| `from` | String | No | Valid email | Sender email address (defaults to noreply if not provided) |
| `replyTo` | String | No | Valid email | Reply-to email address |
| `cc` | Array[String] | No | Valid emails | Carbon copy recipients (max 50) |
| `bcc` | Array[String] | No | Valid emails | Blind carbon copy recipients (max 50) |
| `isHtml` | Boolean | No | true/false | Whether body contains HTML (defaults to false) |
| `attachmentUrls` | Array[String] | No | Valid URLs | URLs to files to attach (max 10 attachments) |
| `priority` | Integer | No | 0-3 | Priority level (0=Low, 1=Normal, 2=High, 3=Critical). Defaults to 1 |
| `metadata` | Object | No | Key-value pairs | Custom metadata for tracking |

**Response**

Status: `202 Accepted`

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440002",
  "recipientId": "550e8400-e29b-41d4-a716-446655440000",
  "channel": 1,
  "status": 1,
  "title": "Welcome to NotificationHub",
  "body": "<h1>Welcome!</h1><p>Thank you for signing up.</p>",
  "priority": 1,
  "createdAt": "2026-03-09T10:30:00Z",
  "queuedAt": "2026-03-09T10:30:01Z",
  "sentAt": null,
  "deliveredAt": null,
  "failedAt": null,
  "retryCount": 0
}
```

**Email-Specific Response Fields**

| Field | Type | Description |
|-------|------|-------------|
| `to` | String | Primary recipient email |
| `from` | String | Sender email address |
| `replyTo` | String | Reply-to email address |
| `cc` | Array[String] | Carbon copy recipients |
| `bcc` | Array[String] | Blind carbon copy recipients |
| `isHtml` | Boolean | Whether HTML formatting is enabled |
| `attachmentUrls` | Array[String] | Attached file URLs |

**Error Responses**

| Status | Scenario |
|--------|----------|
| 400 Bad Request | Malformed JSON or missing required fields |
| 422 Unprocessable Entity | Email validation failed (invalid format, too many recipients) |
| 503 Service Unavailable | RabbitMQ connection unavailable |

**Example Usage (cURL)**

```bash
curl -X POST http://localhost:5000/api/notifications/email \
  -H "Content-Type: application/json" \
  -d '{
    "recipientId": "550e8400-e29b-41d4-a716-446655440000",
    "subject": "Welcome to NotificationHub",
    "body": "Thank you for signing up!",
    "to": "user@example.com",
    "from": "noreply@notificationhub.com",
    "isHtml": false,
    "priority": 1
  }'
```

**Example Usage (C#)**

```csharp
using System.Net.Http.Json;

var client = new HttpClient();

var emailRequest = new
{
    recipientId = Guid.NewGuid(),
    subject = "Welcome to NotificationHub",
    body = "Thank you for signing up!",
    to = "user@example.com",
    from = "noreply@notificationhub.com",
    isHtml = false,
    priority = 1
};

var response = await client.PostAsJsonAsync(
    "http://localhost:5000/api/notifications/email",
    emailRequest
);

if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
{
    var notification = await response.Content.ReadAsAsync<dynamic>();
    Console.WriteLine($"Email queued: {notification.id}");
}
```

**Implementation Notes**

- Request returns `202 Accepted` immediately; actual delivery is asynchronous
- Notification status is `Queued` upon acceptance
- Email Worker service processes messages from the email queue
- Failed deliveries are retried up to 3 times with exponential backoff
- After max retries, notification is moved to Dead Letter Queue (DLQ)
- Status updates are published back to the API for database persistence and SignalR broadcast

---

### Send SMS Notification

Queue an SMS notification for asynchronous delivery. The request is accepted immediately and processed by the SMS Worker service.

**Endpoint**: `POST /api/notifications/sms`

**Status**: Implementation pending - endpoint structure defined, handler not yet implemented

**Request Body**

```json
{
  "recipientId": "550e8400-e29b-41d4-a716-446655440000",
  "title": "Order Confirmation",
  "body": "Your order #12345 has been confirmed. Track it at: example.com/orders/12345",
  "phoneNumber": "+1234567890",
  "fromNumber": "+1000000000",
  "priority": 2,
  "metadata": {
    "orderId": "order-12345",
    "provider": "twilio"
  }
}
```

**Parameters**

| Field | Type | Required | Constraints | Description |
|-------|------|----------|-------------|-------------|
| `recipientId` | UUID | Yes | Non-empty GUID | The recipient's user ID |
| `title` | String | Yes | 1-500 chars | SMS title (for internal reference) |
| `body` | String | Yes | 1-1600 chars | SMS message content (typically 160 chars per part) |
| `phoneNumber` | String | Yes | Valid E.164 format | Recipient phone number (10-15 digits, may include +) |
| `fromNumber` | String | No | Valid E.164 format | Sender phone number (SMS shortcode or virtual number) |
| `priority` | Integer | No | 0-3 | Priority level (0=Low, 1=Normal, 2=High, 3=Critical). Defaults to 1 |
| `metadata` | Object | No | Key-value pairs | Custom metadata for tracking |

**Phone Number Format**

Phone numbers must be in E.164 format or cleaned to contain only digits and '+':
- Valid: `+1234567890`, `12345678900`, `+44-20-7946-0958`
- Invalid: `(123) 456-7890` (will be cleaned), `12345` (too short)
- Range: 10-15 digits after cleaning

**Response**

Status: `202 Accepted`

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440003",
  "recipientId": "550e8400-e29b-41d4-a716-446655440000",
  "channel": 2,
  "status": 1,
  "title": "Order Confirmation",
  "body": "Your order #12345 has been confirmed. Track it at: example.com/orders/12345",
  "priority": 2,
  "createdAt": "2026-03-09T10:35:00Z",
  "queuedAt": "2026-03-09T10:35:01Z",
  "sentAt": null,
  "deliveredAt": null,
  "failedAt": null,
  "retryCount": 0
}
```

**SMS-Specific Response Fields**

| Field | Type | Description |
|-------|------|-------------|
| `phoneNumber` | String | Recipient phone number in E.164 format |
| `fromNumber` | String | Sender phone number in E.164 format |

**Error Responses**

| Status | Scenario |
|--------|----------|
| 400 Bad Request | Malformed JSON or missing required fields |
| 422 Unprocessable Entity | Phone validation failed (invalid format or length) |
| 503 Service Unavailable | RabbitMQ connection unavailable |

**Example Usage (JavaScript/Fetch)**

```javascript
async function sendSmsNotification() {
  const response = await fetch('/api/notifications/sms', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      recipientId: '550e8400-e29b-41d4-a716-446655440000',
      title: 'Order Confirmation',
      body: 'Your order has been confirmed.',
      phoneNumber: '+1234567890',
      fromNumber: '+1000000000',
      priority: 2,
      metadata: {
        orderId: 'order-12345'
      }
    })
  });

  if (response.status === 202) {
    const notification = await response.json();
    console.log('SMS queued:', notification.id);
  } else {
    const error = await response.json();
    console.error('Error:', error.detail);
  }
}
```

**Implementation Notes**

- Request returns `202 Accepted` immediately; actual delivery is asynchronous
- Phone number validation accepts various formats but stores in E.164 standard
- SMS Worker processes messages from the SMS queue
- Automatic retries on failure (up to 3 attempts with exponential backoff)
- Status updates flow back to API for database persistence
- Consider message length when composing body (SMS may split into multiple parts)

---

### Send WhatsApp Notification

Queue a WhatsApp notification for asynchronous delivery. The request is accepted immediately and processed by the WhatsApp Worker service.

**Endpoint**: `POST /api/notifications/whatsapp`

**Status**: Implementation pending - endpoint structure defined, handler not yet implemented

**Request Body**

```json
{
  "recipientId": "550e8400-e29b-41d4-a716-446655440000",
  "title": "Appointment Reminder",
  "body": "Your appointment is tomorrow at 2:00 PM. Reply CONFIRM to confirm.",
  "phoneNumber": "+1234567890",
  "fromNumber": "+1000000000",
  "priority": 1,
  "metadata": {
    "appointmentId": "appt-789",
    "type": "reminder"
  }
}
```

**Parameters**

| Field | Type | Required | Constraints | Description |
|-------|------|----------|-------------|-------------|
| `recipientId` | UUID | Yes | Non-empty GUID | The recipient's user ID |
| `title` | String | Yes | 1-500 chars | Message title (for internal reference) |
| `body` | String | Yes | 1-4096 chars | WhatsApp message content |
| `phoneNumber` | String | Yes | Valid E.164 format | Recipient WhatsApp phone number |
| `fromNumber` | String | No | Valid E.164 format | Sender WhatsApp number or business account ID |
| `priority` | Integer | No | 0-3 | Priority level (0=Low, 1=Normal, 2=High, 3=Critical). Defaults to 1 |
| `metadata` | Object | No | Key-value pairs | Custom metadata for tracking and templating |

**Response**

Status: `202 Accepted`

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440004",
  "recipientId": "550e8400-e29b-41d4-a716-446655440000",
  "channel": 3,
  "status": 1,
  "title": "Appointment Reminder",
  "body": "Your appointment is tomorrow at 2:00 PM. Reply CONFIRM to confirm.",
  "priority": 1,
  "createdAt": "2026-03-09T10:40:00Z",
  "queuedAt": "2026-03-09T10:40:01Z",
  "sentAt": null,
  "deliveredAt": null,
  "failedAt": null,
  "retryCount": 0
}
```

**WhatsApp-Specific Response Fields**

| Field | Type | Description |
|-------|------|-------------|
| `phoneNumber` | String | Recipient WhatsApp phone number |
| `fromNumber` | String | Sender WhatsApp business number |

**Error Responses**

| Status | Scenario |
|--------|----------|
| 400 Bad Request | Malformed JSON or missing required fields |
| 422 Unprocessable Entity | Phone validation failed |
| 503 Service Unavailable | RabbitMQ connection unavailable |

**Example Usage (Python)**

```python
import requests
import json
from uuid import uuid4

def send_whatsapp_notification():
    url = 'http://localhost:5000/api/notifications/whatsapp'

    payload = {
        'recipientId': str(uuid4()),
        'title': 'Appointment Reminder',
        'body': 'Your appointment is tomorrow at 2:00 PM.',
        'phoneNumber': '+1234567890',
        'fromNumber': '+1000000000',
        'priority': 1,
        'metadata': {
            'appointmentId': 'appt-789',
            'type': 'reminder'
        }
    }

    response = requests.post(url, json=payload)

    if response.status_code == 202:
        notification = response.json()
        print(f"WhatsApp queued: {notification['id']}")
    else:
        error = response.json()
        print(f"Error: {error['detail']}")

if __name__ == '__main__':
    send_whatsapp_notification()
```

**Implementation Notes**

- Request returns `202 Accepted` immediately; actual delivery is asynchronous
- WhatsApp Worker processes messages from the WhatsApp queue
- Supports rich formatting via WhatsApp message templates
- Automatic retries on failure (up to 3 attempts)
- Phone number must be registered with WhatsApp Business API
- Metadata can include template variables for dynamic messaging

---

### Get Notification by ID

Retrieve detailed information about a specific notification, including its current status and delivery history.

**Endpoint**: `GET /api/notifications/{id:guid}`

**Status**: Implementation pending - endpoint structure defined, handler not yet implemented

**Path Parameters**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | UUID | Yes | The notification ID to retrieve |

**Response**

Status: `200 OK`

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440002",
  "recipientId": "550e8400-e29b-41d4-a716-446655440000",
  "channel": 1,
  "status": 3,
  "title": "Welcome to NotificationHub",
  "body": "<h1>Welcome!</h1><p>Thank you for signing up.</p>",
  "priority": 1,
  "createdAt": "2026-03-09T10:30:00Z",
  "queuedAt": "2026-03-09T10:30:01Z",
  "sentAt": "2026-03-09T10:30:15Z",
  "deliveredAt": "2026-03-09T10:30:20Z",
  "failedAt": null,
  "retryCount": 0,
  "errorMessage": null,
  "metadata": {
    "template": "welcome",
    "userId": "user-123"
  }
}
```

**Response Fields**

| Field | Type | Description |
|-------|------|-------------|
| `id` | UUID | Notification identifier |
| `recipientId` | UUID | Recipient's user ID |
| `channel` | Integer | Channel type (0=Push, 1=Email, 2=Sms, 3=WhatsApp) |
| `status` | Integer | Current status (0=Pending, 1=Queued, 2=Sent, 3=Delivered, 4=Failed, 5=Cancelled) |
| `title` | String | Notification title |
| `body` | String | Notification content |
| `priority` | Integer | Priority level |
| `createdAt` | DateTime | Creation timestamp (UTC) |
| `queuedAt` | DateTime | Queued timestamp (null if not queued) |
| `sentAt` | DateTime | Sent timestamp (null if not sent) |
| `deliveredAt` | DateTime | Delivered timestamp (null if not delivered) |
| `failedAt` | DateTime | Failed timestamp (null if not failed) |
| `retryCount` | Integer | Number of retry attempts |
| `errorMessage` | String | Error description if failed (null if successful) |
| `metadata` | Object | Custom metadata attached to notification |

**Error Responses**

| Status | Scenario | Response |
|--------|----------|----------|
| 404 Not Found | Notification does not exist | `{ "detail": "Notification with ID 'xxx-xxx-xxx' not found" }` |
| 400 Bad Request | Invalid UUID format | `{ "detail": "Invalid notification ID format" }` |

**Example Usage**

```bash
# Retrieve notification details
curl -X GET http://localhost:5000/api/notifications/550e8400-e29b-41d4-a716-446655440002

# Check status
curl -X GET http://localhost:5000/api/notifications/550e8400-e29b-41d4-a716-446655440002 \
  | jq '.status'
```

**Example Usage (TypeScript)**

```typescript
async function getNotificationStatus(notificationId: string) {
  const response = await fetch(
    `/api/notifications/${notificationId}`
  );

  if (response.ok) {
    const notification = await response.json();

    const statusMap = {
      0: 'Pending',
      1: 'Queued',
      2: 'Sent',
      3: 'Delivered',
      4: 'Failed',
      5: 'Cancelled'
    };

    console.log(`Status: ${statusMap[notification.status]}`);
    console.log(`Delivered at: ${notification.deliveredAt}`);

    if (notification.status === 4) {
      console.error(`Error: ${notification.errorMessage}`);
    }
  } else {
    console.error('Notification not found');
  }
}
```

**Implementation Notes**

- Returns comprehensive notification object with full history
- Includes error details if delivery failed
- Used for polling status or integration with monitoring systems
- Should be queried periodically or triggered by SignalR status updates

---

### Get Failed Notifications

Retrieve all notifications that failed after maximum retry attempts. Useful for monitoring and recovery operations.

**Endpoint**: `GET /api/notifications/failed`

**Status**: Implementation pending - endpoint structure defined, handler not yet implemented

**Query Parameters**

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `skip` | Integer | No | 0 | Number of records to skip for pagination |
| `take` | Integer | No | 100 | Number of records to return (max 1000) |
| `channel` | Integer | No | All | Filter by channel (0=Push, 1=Email, 2=Sms, 3=WhatsApp) |
| `startDate` | DateTime | No | 7 days ago | Filter from date (ISO 8601 format) |
| `endDate` | DateTime | No | Now | Filter to date (ISO 8601 format) |

**Response**

Status: `200 OK`

```json
{
  "totalCount": 5,
  "pageSize": 100,
  "pageNumber": 1,
  "items": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440005",
      "recipientId": "550e8400-e29b-41d4-a716-446655440010",
      "channel": 1,
      "status": 4,
      "title": "Password Reset",
      "body": "Click here to reset your password...",
      "priority": 2,
      "createdAt": "2026-03-08T14:20:00Z",
      "queuedAt": "2026-03-08T14:20:05Z",
      "sentAt": null,
      "deliveredAt": null,
      "failedAt": "2026-03-08T14:25:30Z",
      "retryCount": 3,
      "errorMessage": "Invalid recipient email address",
      "metadata": {}
    },
    {
      "id": "550e8400-e29b-41d4-a716-446655440006",
      "recipientId": "550e8400-e29b-41d4-a716-446655440011",
      "channel": 2,
      "status": 4,
      "title": "Verification Code",
      "body": "Your verification code is: 123456",
      "priority": 3,
      "createdAt": "2026-03-08T12:10:00Z",
      "queuedAt": "2026-03-08T12:10:02Z",
      "sentAt": null,
      "deliveredAt": null,
      "failedAt": "2026-03-08T12:15:45Z",
      "retryCount": 3,
      "errorMessage": "SMS provider connection timeout",
      "metadata": {}
    }
  ]
}
```

**Response Structure**

| Field | Type | Description |
|-------|------|-------------|
| `totalCount` | Integer | Total number of failed notifications matching criteria |
| `pageSize` | Integer | Number of items returned |
| `pageNumber` | Integer | Current page number (1-based) |
| `items` | Array | Array of failed notification objects |

**Each Item Contains**

Same fields as [Get Notification by ID](#get-notification-by-id), with emphasis on:
- `status`: Always 4 (Failed)
- `retryCount`: Always 3 (max retry limit reached)
- `errorMessage`: Description of the failure reason
- `failedAt`: When the final failure occurred

**Error Responses**

| Status | Scenario |
|--------|----------|
| 400 Bad Request | Invalid query parameters or date format |

**Example Usage**

```bash
# Get all failed notifications from the last 7 days
curl "http://localhost:5000/api/notifications/failed"

# Get failed emails only
curl "http://localhost:5000/api/notifications/failed?channel=1"

# Get failed SMS with pagination
curl "http://localhost:5000/api/notifications/failed?channel=2&skip=20&take=20"

# Get failed notifications between dates
curl "http://localhost:5000/api/notifications/failed?startDate=2026-03-01T00:00:00Z&endDate=2026-03-09T00:00:00Z"
```

**Example Usage (JavaScript)**

```javascript
async function getFailedNotifications(channel = null) {
  let url = '/api/notifications/failed';

  if (channel !== null) {
    url += `?channel=${channel}`;
  }

  const response = await fetch(url);
  const data = await response.json();

  console.log(`Found ${data.totalCount} failed notifications`);

  data.items.forEach(notif => {
    console.log(`
      ID: ${notif.id}
      Channel: ${['Push', 'Email', 'SMS', 'WhatsApp'][notif.channel]}
      Recipient: ${notif.recipientId}
      Error: ${notif.errorMessage}
      Failed At: ${notif.failedAt}
    `);
  });
}
```

**Implementation Notes**

- Returns paginated results for large datasets
- Failed notifications are those that exceeded maximum retry attempts
- Useful for monitoring, alerting, and manual recovery
- Can be filtered by channel and date range
- Consider implementing automatic retries or human review workflow for critical failures

---

## Real-Time Notifications with SignalR

Push notifications are delivered in real-time to connected clients via the SignalR hub.

### Hub Endpoint

**URL**: `ws://localhost:5000/hubs/notifications` (or `wss://` for HTTPS)

**Hub Class**: `PushNotificationHub`

### Connection Flow

1. **Client Initiates Connection**: Connect to the SignalR hub with a valid `recipientId`
2. **OnConnectedAsync**: Server adds client to group `recipient-{recipientId}`
3. **OnDisconnectedAsync**: Server removes client from the group on disconnect

### Automatic Group Assignment

Clients are automatically added to a group based on their recipient ID:
- Group name format: `recipient-{recipientId}`
- Example: `recipient-550e8400-e29b-41d4-a716-446655440000`

**Implementation Status**: OnConnectedAsync and OnDisconnectedAsync methods need implementation to extract recipientId and manage group membership.

### Receiving Notifications

When a push notification is sent via the API, the server broadcasts it to all clients in the recipient's group:

```csharp
// On server side (when notification is received)
await Clients.Group($"recipient-{recipientId}")
    .SendAsync("ReceiveNotification", notification);
```

### Example Client Implementation (TypeScript)

```typescript
import * as signalR from "@microsoft/signalr";

const hubUrl = "http://localhost:5000/hubs/notifications";

const connection = new signalR.HubConnectionBuilder()
    .withUrl(hubUrl, {
        accessTokenFactory: () => "your-token-here" // Add auth token
    })
    .withAutomaticReconnect()
    .build();

// Handle incoming notifications
connection.on("ReceiveNotification", (notification) => {
    console.log("Received notification:", notification);
    console.log(`Title: ${notification.title}`);
    console.log(`Body: ${notification.body}`);
    console.log(`Priority: ${notification.priority}`);

    // Update UI with notification
    showNotification(notification);
});

// Handle connection state
connection.onreconnected((connectionId) => {
    console.log(`Reconnected with ID: ${connectionId}`);
});

connection.onreconnecting((error) => {
    console.error(`Reconnecting due to error: ${error}`);
});

connection.onclose((error) => {
    console.log("Connection closed");
});

// Start connection
try {
    await connection.start();
    console.log("SignalR connected");
} catch (err) {
    console.error("Failed to connect:", err);
    setTimeout(() => connection.start(), 5000);
}
```

### Example Client Implementation (JavaScript)

```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/notifications")
    .withAutomaticReconnect()
    .build();

connection.on("ReceiveNotification", function(notification) {
    // Handle the notification
    const message = `${notification.title}: ${notification.body}`;

    // Show desktop notification if available
    if ("Notification" in window && Notification.permission === "granted") {
        new Notification(notification.title, {
            body: notification.body,
            icon: "/notification-icon.png"
        });
    }
});

connection.start()
    .catch(err => {
        console.error("Failed to connect:", err);
        return new Promise(resolve => setTimeout(() => resolve(connection.start()), 5000));
    });
```

### Message Format

Notifications are sent as JSON objects with the following structure:

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440001",
  "recipientId": "550e8400-e29b-41d4-a716-446655440000",
  "channel": 0,
  "status": 2,
  "title": "New Message",
  "body": "You have a new message from John Doe",
  "priority": 1,
  "createdAt": "2026-03-09T10:30:00Z",
  "sentAt": "2026-03-09T10:30:00Z",
  "metadata": {
    "source": "chat-service",
    "threadId": "thread-123"
  }
}
```

### Connection Requirements

**Implementation Status**: The following features need to be implemented:

- Extract `recipientId` from query string or authentication claims
- Validate `recipientId` is a valid GUID
- Add client connection to the appropriate group
- Handle reconnection scenarios
- Gracefully remove client from group on disconnect

**Authentication**: Currently not implemented. Will require:
- Extracting user identity from JWT token or query parameters
- Mapping authenticated user to their `recipientId`
- Validating user can only connect for their own `recipientId`

---

## Data Models

### Notification (Base Entity)

All notifications inherit from the abstract `Notification` base class using Table-Per-Hierarchy (TPH) inheritance.

**Properties**

```csharp
public abstract class Notification : AggregateRoot<Guid>
{
    public const int MaxRetryCount = 3;

    // Identification
    public Guid Id { get; }                              // Generated UUID v7
    public Guid RecipientId { get; }                     // User identifier

    // Content
    public string Title { get; }                         // 1-500 characters
    public string Body { get; }                          // 1-50000 characters

    // Classification
    public NotificationChannel Channel { get; }          // Push, Email, Sms, WhatsApp
    public NotificationPriority Priority { get; }        // Low, Normal, High, Critical

    // Status
    public NotificationStatus Status { get; set; }       // Current state
    public int RetryCount { get; set; }                  // 0-3
    public string? ErrorMessage { get; set; }            // Failure reason

    // Metadata
    public NotificationMetadata Metadata { get; }        // Custom key-value data

    // Timestamps (UTC)
    public DateTime CreatedAt { get; }                   // When created
    public DateTime? QueuedAt { get; set; }              // When enqueued
    public DateTime? SentAt { get; set; }                // When transmitted
    public DateTime? DeliveredAt { get; set; }           // When delivered
    public DateTime? FailedAt { get; set; }              // When failed
}
```

**State Transitions**

```
┌─────────┐
│Pending  │ ← Created
└────┬────┘
     │
     ├──→ Queued ──→ Sent ──→ Delivered
     │      ↑         ↑
     │      └─────────┘ (on retry)
     │       ↓
     │      Failed ─→ Retry if CanRetry()
     │       ↓
     └──→ Cancelled
```

### EmailNotification

Extends `Notification` with email-specific properties.

**Additional Properties**

```csharp
public sealed class EmailNotification : Notification
{
    public EmailAddress To { get; }                      // Required
    public EmailAddress? From { get; }                   // Optional
    public EmailAddress? ReplyTo { get; }                // Optional
    public List<EmailAddress> Cc { get; }                // Multiple allowed
    public List<EmailAddress> Bcc { get; }               // Multiple allowed
    public bool IsHtml { get; }                          // HTML vs plain text
    public List<string> AttachmentUrls { get; }          // File URLs to attach
}
```

**Validation Rules**

- `To`: Required, must be valid email
- `From`: Optional, defaults to configured noreply address
- `Cc/Bcc`: Maximum 50 recipients each
- `Body`: Treated as HTML if `IsHtml` is true
- `AttachmentUrls`: Maximum 10 URLs

### SmsNotification

Extends `Notification` with SMS-specific properties.

**Additional Properties**

```csharp
public sealed class SmsNotification : Notification
{
    public PhoneNumber PhoneNumber { get; }              // Required
    public PhoneNumber? FromNumber { get; }              // Optional (sender)
}
```

**Validation Rules**

- `PhoneNumber`: Required, E.164 format (10-15 digits after cleaning)
- `FromNumber`: Optional, sender's virtual number
- `Body`: Maximum 1600 characters (4 SMS parts)

### WhatsAppNotification

Extends `Notification` with WhatsApp-specific properties.

**Additional Properties**

```csharp
public sealed class WhatsAppNotification : Notification
{
    public PhoneNumber PhoneNumber { get; }              // Required
    public PhoneNumber? FromNumber { get; }              // Optional (business account)
}
```

**Validation Rules**

- `PhoneNumber`: Required, E.164 format
- `FromNumber`: Optional, business account number
- `Body`: Maximum 4096 characters
- Supports rich formatting and templates

### PushNotification

Extends `Notification` with no additional channel-specific properties.

```csharp
public sealed class PushNotification : Notification
{
    // No additional properties
    // Uses base properties: Title, Body, Priority, Metadata
}
```

### NotificationMetadata (Value Object)

Immutable dictionary of custom key-value pairs.

```csharp
public sealed class NotificationMetadata : ValueObject
{
    public IReadOnlyDictionary<string, string> Data { get; }

    public static NotificationMetadata Empty();
    public static NotificationMetadata Create(Dictionary<string, string>? data);
}
```

**Usage**

```json
{
  "metadata": {
    "template": "welcome",
    "userId": "user-123",
    "campaignId": "campaign-456",
    "source": "api",
    "actionUrl": "/messages/123"
  }
}
```

### EmailAddress (Value Object)

Validates and encapsulates email addresses.

```csharp
public sealed class EmailAddress : ValueObject
{
    public string Value { get; }

    public static EmailAddress Create(string email);
}
```

**Validation Rules**

- Not null or empty
- Contains '@' character
- Maximum 254 characters
- Converted to lowercase for comparison

### PhoneNumber (Value Object)

Validates and encapsulates phone numbers.

```csharp
public sealed class PhoneNumber : ValueObject
{
    public string Value { get; }

    public static PhoneNumber Create(string phoneNumber);
}
```

**Validation Rules**

- Not null or empty
- After cleaning (keeping only digits and '+'), must be 10-15 characters
- Supports E.164 format
- Automatically cleaned and normalized

---

## Implementation Status

### Current State

The NotificationHub API is in **early development** with endpoints and core domain logic defined but handlers not yet implemented. The following components require completion:

### Completed Tasks

- **Domain Layer**: All entities, value objects, enums, and domain events defined
- **API Endpoints**: Minimal API route mappings with OpenAPI contracts
- **SignalR Hub**: Hub class and connection setup
- **Configuration**: Serilog, CORS, and service registration structure
- **Infrastructure Setup**: Docker Compose for PostgreSQL and RabbitMQ

### Pending Implementation

#### API Handlers

Each endpoint handler needs to be implemented to:

1. **SendPushNotification**
   - Inject `INotificationService` and `IHubContext<PushNotificationHub>`
   - Create `PushNotification` entity
   - Persist to database
   - Broadcast to SignalR group
   - Return `200 OK` with notification object

2. **SendEmailNotification**
   - Inject `INotificationService` and `IPublishEndpoint` (MassTransit)
   - Create `EmailNotification` entity
   - Persist to database with status `Queued`
   - Publish to email queue contract
   - Return `202 Accepted` with notification object

3. **SendSmsNotification**
   - Similar to SendEmailNotification but for SMS channel
   - Publish to SMS queue

4. **SendWhatsAppNotification**
   - Similar to SendEmailNotification but for WhatsApp channel
   - Publish to WhatsApp queue

5. **GetNotificationById**
   - Inject `INotificationService`
   - Query database by ID
   - Return `200 OK` or `404 Not Found`

6. **GetFailedNotifications**
   - Inject `INotificationService`
   - Query failed notifications with filters
   - Return paginated results

#### Application Services

- **INotificationService**: CRUD and query operations for notifications
- **NotificationService**: Implementation with database access

#### Infrastructure Services

- **Database Access**: Entity Framework Core DbContext and repositories
- **RabbitMQ Integration**: MassTransit consumer endpoints for workers
- **SignalR Hub Methods**: Notification broadcasting and group management

#### Configuration

- **AddInfrastructure()**: Database, RabbitMQ, repositories
- **AddApplication()**: Services, validators, mapping profiles

#### SignalR Hub

- **OnConnectedAsync()**: Extract recipientId and add to group
- **OnDisconnectedAsync()**: Remove from group

---

## Best Practices

### Request Design

1. **Always provide recipientId**: Ensures proper targeting and tracking
2. **Use appropriate priority levels**: Helps workers prioritize critical alerts
3. **Include meaningful metadata**: Enables better tracking and filtering
4. **Validate inputs locally**: Reduce unnecessary API calls
5. **Use proper email format**: Prevents invalid delivery attempts

### Error Handling

1. **Check HTTP status codes**: Distinguish between client and server errors
2. **Log error messages**: Include trace IDs for debugging
3. **Implement retry logic**: For transient failures with exponential backoff
4. **Monitor failed notifications**: Set up alerts for critical channels
5. **Manual recovery**: Use GetFailedNotifications endpoint to identify problematic notifications

### Channel Selection

**Use Push for**:
- Real-time alerts (approvals, alerts)
- Time-sensitive updates (status changes)
- User interactions (messages, mentions)
- Low-latency requirements

**Use Email for**:
- Formal communications (contracts, receipts)
- Long-form content (newsletters, reports)
- Permanent records (receipts, confirmations)
- Scheduled delivery

**Use SMS for**:
- Urgent alerts (security, OTP codes)
- Time-sensitive OTP delivery
- Simple, critical messages
- Users without apps installed

**Use WhatsApp for**:
- Rich messaging (media, buttons)
- Two-way conversations
- Customer engagement
- Markets with high WhatsApp adoption

### Rate Limiting

**Implementation pending**: Once authentication is added, implement per-user rate limits:
- Push: 100/minute per user
- Email: 10/minute per user
- SMS: 5/minute per user (due to cost)
- WhatsApp: 20/minute per user

### Monitoring & Alerting

1. **Track delivery rates** by channel
2. **Alert on failure spikes** (> 5% of notifications)
3. **Monitor queue depth** in RabbitMQ
4. **Log timing metrics** (queued → sent → delivered)
5. **Track retry patterns** to identify systemic issues

### Database Considerations

1. **Index by RecipientId**: For fast user-specific queries
2. **Index by Channel and Status**: For analytics and reporting
3. **Partition by date**: For large-scale deployments
4. **Archive old records**: Move historical data to separate storage
5. **Maintain referential integrity**: Keep foreign keys consistent

### RabbitMQ Configuration

1. **DLQ per queue**: Failed messages moved after max retries
2. **TTL on messages**: Prevent stale notifications
3. **Priority queues**: For high-priority channels
4. **Consumer prefetch**: Balance between throughput and responsiveness
5. **Connection pooling**: Efficient resource usage

### Testing

```csharp
// Unit test example
[Fact]
public async Task SendEmailNotification_WithValidInput_ReturnsAccepted()
{
    // Arrange
    var request = new EmailNotificationRequest
    {
        RecipientId = Guid.NewGuid(),
        Subject = "Test",
        Body = "Test email",
        To = "test@example.com"
    };

    // Act
    var response = await _client.PostAsJsonAsync("/api/notifications/email", request);

    // Assert
    Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
}
```

### Security Considerations

1. **Sanitize metadata**: Prevent injection attacks
2. **Validate all inputs**: Check data types and lengths
3. **Authenticate all requests**: Once auth is implemented
4. **Rate limit by user**: Prevent abuse
5. **Secure RabbitMQ**: Use credentials and encryption
6. **Encrypt sensitive data**: Email addresses, phone numbers in logs
7. **HTTPS only**: In production
8. **CORS configuration**: Restrict to trusted origins

---

## OpenAPI Documentation

Full interactive API documentation is available during development:

**Access**: `http://localhost:5000/scalar/v1`

This provides:
- Browsable endpoint documentation
- Try-it-out functionality
- Request/response examples
- Schema visualization

---

## Support & Feedback

For issues, questions, or suggestions regarding this API:

1. Check this documentation for common scenarios
2. Review error messages and trace IDs in logs
3. Consult the project repository for updates
4. Submit issues with reproduction steps and trace IDs

---

**Last Updated**: 2026-03-09
**API Version**: 1.0.0 (Development)
**Status**: Early Development - Endpoints defined, implementation pending
