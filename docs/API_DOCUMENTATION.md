# NotificationHub API Documentation

## Table of Contents

- [Overview](#overview)
- [Getting Started](#getting-started)
  - [API Base URL](#api-base-url)
  - [Infrastructure Setup](#infrastructure-setup)
  - [Documentation Portal](#documentation-portal)
- [Architecture](#architecture)
- [Authentication & Security](#authentication--security)
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
- [Message Flow Architecture](#message-flow-architecture)
- [Data Models](#data-models)
- [Implementation Status](#implementation-status)
- [Best Practices](#best-practices)
- [Troubleshooting](#troubleshooting)

---

## Overview

NotificationHub is a comprehensive, asynchronous notification delivery system built with ASP.NET Core 10 (.NET 10) using Clean Architecture principles. It supports multiple notification channels with resilient delivery, status tracking, and real-time updates.

### Supported Channels

- **Push Notifications**: Real-time delivery via SignalR to connected clients
- **Email**: Asynchronous delivery via RabbitMQ message queue, powered by Maileroo
- **SMS**: Asynchronous delivery via RabbitMQ message queue (implementation pending)
- **WhatsApp**: Asynchronous delivery via RabbitMQ message queue (implementation pending)

### Key Features

- **RESTful API** — Minimal APIs for lightweight, modern HTTP handler pattern
- **Real-Time Updates** — SignalR integration for instant client notifications
- **Resilient Delivery** — Automatic retries (3 attempts, exponential backoff 5s–1 minute)
- **Dead Letter Queues** — Undeliverable messages are automatically routed for analysis
- **Status Tracking** — Persistent state management via PostgreSQL 16
- **Structured Logging** — Centralized observability via Serilog + Seq
- **Clean Architecture** — Domain-driven design with separation of concerns across 4 layers

---

## Getting Started

### API Base URL

**Local Development**
```
http://localhost:5000
```

**Production** (adjust hostname as needed)
```
https://api.yourapp.com
```

### Infrastructure Setup

The system requires three infrastructure services. Start them first:

```bash
# Start infrastructure (PostgreSQL, RabbitMQ, Seq)
podman compose up postgres rabbitmq seq -d

# Or start everything including the API and workers
podman compose up -d
```

**Required Services**

| Service | Internal Host | Port | Notes |
|---|---|---|---|
| PostgreSQL 16 | `postgres` | `5432` | Notification persistence. DB: `notification_hub`, user: `postgres`/`postgres` |
| RabbitMQ 4 | `rabbitmq` | `5672` (AMQP) / `15672` (Management UI) | Message broker. User: `guest`/`guest` |
| Seq | `seq` | `5341` | Structured logging. UI at `http://localhost:8081` |

### Documentation Portal

**Scalar OpenAPI UI** (interactive API docs)
```
http://localhost:5000/scalar
```

Interactive testing available for all endpoints. Requires running in Development mode.

---

## Architecture

This is a **Clean Architecture** .NET 10 solution structured in four library layers plus three background workers:

```
Domain ◀── Application ◀── Infrastructure ◀── Api
                                        ▲
                            Workers (Email, Sms, WhatsApp)
```

### Layer Responsibilities

**Domain Layer** (`src/NotificationHub.Domain/`)
- Entities: `Notification` aggregate root with TPH subtypes (`EmailNotification`, `SmsNotification`, `WhatsAppNotification`, `PushNotification`)
- Enums: `NotificationStatus` (Pending, Queued, Sent, Failed), `NotificationChannel`, `NotificationPriority`
- Value Objects: `EmailAddress`, `NotificationMetadata`
- Zero external dependencies — pure business logic

**Application Layer** (`src/NotificationHub.Application/`)
- Interfaces: `INotificationService`, `INotificationRepository`, `IMessagePublisher`, `IPushNotificationSender`
- Service: `NotificationService` orchestrates all notification sends
- Contracts: `EmailNotificationMessage`, `PushNotificationMessage`, `NotificationStatusUpdate` (MassTransit messages)
- DTOs: `SendPushNotificationRequest`, `SendEmailNotificationRequest`, `NotificationResponse`

**Infrastructure Layer** (`src/NotificationHub.Infrastructure/`)
- EF Core: `NotificationDbContext` (Npgsql) with fluent configuration
- Repository: `NotificationRepository` for CRUD operations
- Messaging: `MassTransitMessagePublisher` for RabbitMQ publishing
- DI: `AddInfrastructure()` extension for service registration

**API Layer** (`src/NotificationHub.Api/`)
- Endpoints: `NotificationEndpoints` with Minimal APIs
- SignalR Hub: `PushNotificationHub` at `/hubs/notifications`
- Consumer: `NotificationStatusUpdateConsumer` listens for worker status updates
- Sender: `SignalRPushNotificationSender` delivers push notifications via SignalR

### Workers

Each worker is a `Microsoft.NET.Sdk.Worker` background service that:
1. References the Api project for shared contracts and configuration
2. Consumes from a dedicated RabbitMQ queue using MassTransit
3. Applies exponential retry (3 attempts: 5s, 30s, 1 min between retries)
4. Integrates with an external provider (Maileroo for email)
5. Publishes `NotificationStatusUpdate` back to RabbitMQ for the API consumer

**Email Worker** (`src/Workers/NotificationHub.Worker.Email/`)
- Queue: `email-notifications`
- Provider: Maileroo HTTP API (`https://smtp.maileroo.com/api/v2/`)
- Configuration: `Maileroo:ApiKey`, `Maileroo:DefaultFromAddress`, `Maileroo:DefaultFromName`
- Features: HTML/plain text, CC/BCC, reply-to, attachments (downloads and converts to base64)

**SMS Worker** (`src/Workers/NotificationHub.Worker.Sms/`)
- Queue: `sms-notifications`
- Status: Scaffolded, not yet integrated with a provider

**WhatsApp Worker** (`src/Workers/NotificationHub.Worker.WhatsApp/`)
- Queue: `whatsapp-notifications`
- Status: Scaffolded, not yet integrated with a provider

---

## Authentication & Security

**Current Implementation** — No authentication required (development environment)

### Recommendations for Production

1. **API Key Authentication** — Add API key header validation
2. **OAuth 2.0 / OpenID Connect** — For user-based access control
3. **Rate Limiting** — Policy-based `RateLimitPartition` (already in codebase, commented)
4. **HTTPS Only** — Enforce TLS 1.3+ in production
5. **CORS Credentials** — SignalR requires `AllowCredentials()` in CORS policy (do not remove)

---

## Common Concepts

### Notification States

Notifications flow through these states:

```
Pending → Queued → Sent ✓
       ↘
         Failed ✗
```

- **Pending** — Just created, not yet queued (push notifications only)
- **Queued** — Awaiting worker processing (email, SMS, WhatsApp)
- **Sent** — Successfully delivered or published to worker
- **Failed** — All retries exhausted, persistent error

### Notification Channels

- **Push** — Synchronous via SignalR, status updated immediately
- **Email** — Asynchronous via RabbitMQ, status updated by worker
- **SMS** — Asynchronous via RabbitMQ (pending implementation)
- **WhatsApp** — Asynchronous via RabbitMQ (pending implementation)

### Priority Levels

- `Low`
- `Normal` (default)
- `High`
- `Critical`

Priority is informational and passed to external providers; the system does not prioritize queue processing.

### Correlation IDs

All requests are enriched with a `CorrelationId` (autogenerated or from header `X-Correlation-Id`). Use this to trace a notification through logs across all services.

---

## HTTP Status Codes

| Code | Meaning |
|---|---|
| `200 OK` | Request succeeded, response body contains result |
| `201 Created` | Resource created successfully (push notifications return 201) |
| `202 Accepted` | Request accepted for processing, not yet complete (email, SMS, WhatsApp return 202) |
| `400 Bad Request` | Validation error in request body or parameters |
| `404 Not Found` | Notification with given ID does not exist |
| `500 Internal Server Error` | Unhandled server error; check logs for details |

---

## Error Handling

### API Error Response Format

For validation and runtime errors, the API returns a structured error response (ProblemDetails format):

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Field 'To' is required for email notifications.",
  "instance": "/api/notifications/email",
  "traceId": "0HN2V1I8T4J5K:00000001"
}
```

### Common Validation Errors

- Missing required fields (RecipientId, To, Subject, Body, Title, etc.)
- Invalid email addresses
- RecipientId not a valid GUID
- Empty recipient lists

### Handling Failures

**For Push Notifications**
- Exceptions are caught and logged; notification is marked Failed immediately
- Status is returned in the API response with `status: "Failed"` and `errorMessage`

**For Async Notifications (Email, SMS, WhatsApp)**
- Notification is marked Queued initially; status 202 Accepted is returned
- Worker processes asynchronously; failures trigger automatic retries
- After max retries, notification is marked Failed and `NotificationStatusUpdate` is published
- Status updated via SignalR to connected clients in real-time

---

## Endpoints

### Send Push Notification

Send a real-time push notification to a connected recipient via SignalR.

**Endpoint**
```
POST /api/notifications/push
```

**Status Code**
- `201 Created` — Notification sent successfully
- `400 Bad Request` — Validation error
- `500 Internal Server Error` — SignalR send failed

**Request Body**
```json
{
  "recipientId": "550e8400-e29b-41d4-a716-446655440000",
  "title": "Order Confirmation",
  "body": "Your order #12345 has been confirmed.",
  "priority": "Normal",
  "metadata": {
    "orderId": "12345",
    "customField": "customValue"
  }
}
```

**Request Fields**

| Field | Type | Required | Description |
|---|---|---|---|
| `recipientId` | UUID | Yes | Target recipient ID; must match the `recipientId` query param in the SignalR connection |
| `title` | string | Yes | Notification title (max 255 chars) |
| `body` | string | Yes | Notification body/message (max 2000 chars) |
| `priority` | string | No | `Low`, `Normal` (default), `High`, `Critical` |
| `metadata` | object | No | Custom key-value pairs for application use |

**Response**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "recipientId": "550e8400-e29b-41d4-a716-446655440001",
  "title": "Order Confirmation",
  "body": "Your order #12345 has been confirmed.",
  "channel": "Push",
  "status": "Sent",
  "priority": "Normal",
  "createdAt": "2026-03-10T14:30:00Z",
  "sentAt": "2026-03-10T14:30:00Z",
  "failedAt": null,
  "errorMessage": null
}
```

**Example cURL**
```bash
curl -X POST http://localhost:5000/api/notifications/push \
  -H "Content-Type: application/json" \
  -d '{
    "recipientId": "550e8400-e29b-41d4-a716-446655440000",
    "title": "Test Push",
    "body": "This is a test push notification."
  }'
```

**SignalR Connection Details**

Clients must connect to the SignalR hub before push notifications are delivered:

```javascript
const connection = new signalR.HubConnectionBuilder()
  .withUrl("http://localhost:5000/hubs/notifications?recipientId=550e8400-e29b-41d4-a716-446655440000")
  .withAutomaticReconnect()
  .build();

connection.on("ReceiveNotification", (message) => {
  console.log("Push notification received:", message);
});

connection.on("NotificationStatusChanged", (statusUpdate) => {
  console.log("Status updated:", statusUpdate);
});

await connection.start();
```

---

### Send Email Notification

Enqueue an email notification for asynchronous delivery via the Email worker and Maileroo.

**Endpoint**
```
POST /api/notifications/email
```

**Status Code**
- `202 Accepted` — Notification queued for processing
- `400 Bad Request` — Validation error
- `500 Internal Server Error` — Failed to queue message

**Request Body**
```json
{
  "recipientId": "550e8400-e29b-41d4-a716-446655440000",
  "subject": "Welcome to NotificationHub",
  "body": "<p>Welcome! Your account is ready.</p>",
  "to": "user@example.com",
  "from": "noreply@company.com",
  "replyTo": "support@company.com",
  "cc": ["manager@company.com"],
  "bcc": ["archive@company.com"],
  "isHtml": true,
  "attachmentUrls": ["https://example.com/invoice.pdf"],
  "priority": "Normal",
  "metadata": {
    "userId": "user123"
  }
}
```

**Request Fields**

| Field | Type | Required | Description |
|---|---|---|---|
| `recipientId` | UUID | Yes | Internal recipient ID (not the email address) |
| `subject` | string | Yes | Email subject line |
| `body` | string | Yes | Email body content (HTML or plain text) |
| `to` | string | Yes | Recipient email address |
| `from` | string | No | Sender email (defaults to `Maileroo:DefaultFromAddress`) |
| `replyTo` | string | No | Reply-to email address |
| `cc` | array | No | CC recipients (array of email strings) |
| `bcc` | array | No | BCC recipients (array of email strings) |
| `isHtml` | boolean | No | `true` for HTML body, `false` for plain text (default: false) |
| `attachmentUrls` | array | No | Array of publicly accessible URLs to attach (downloaded and base64-encoded) |
| `priority` | string | No | `Low`, `Normal` (default), `High`, `Critical` |
| `metadata` | object | No | Custom key-value pairs for application use |

**Response**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "recipientId": "550e8400-e29b-41d4-a716-446655440001",
  "title": "Welcome to NotificationHub",
  "body": "<p>Welcome! Your account is ready.</p>",
  "channel": "Email",
  "status": "Queued",
  "priority": "Normal",
  "createdAt": "2026-03-10T14:30:00Z",
  "sentAt": null,
  "failedAt": null,
  "errorMessage": null
}
```

**Status Updates via SignalR**

The connected client receives real-time updates as the worker processes the email:

```json
{
  "notificationId": "550e8400-e29b-41d4-a716-446655440000",
  "channel": "Email",
  "success": true,
  "externalReferenceId": "abc123def456789abc123d",
  "timestamp": "2026-03-10T14:31:00Z"
}
```

**Example cURL**
```bash
curl -X POST http://localhost:5000/api/notifications/email \
  -H "Content-Type: application/json" \
  -d '{
    "recipientId": "550e8400-e29b-41d4-a716-446655440000",
    "subject": "Test Email",
    "body": "This is a test email.",
    "to": "user@example.com",
    "isHtml": false
  }'
```

**Maileroo Integration Details**

- **Endpoint**: `https://smtp.maileroo.com/api/v2/emails`
- **Authentication**: `X-API-Key` header with configured API key
- **Attachment Handling**: URLs are downloaded and converted to base64 before sending
- **Reference ID**: Notification ID converted to 24-char hex for Maileroo tracking

---

### Send SMS Notification

**Endpoint**
```
POST /api/notifications/sms
```

**Status**
- ⚠️ **Implementation Pending** — Endpoint accepts requests but does not process them
- Returns `202 Accepted` with a placeholder response

**Request Body** (placeholder structure)
```json
{
  "recipientId": "550e8400-e29b-41d4-a716-446655440000",
  "title": "Notification Title",
  "body": "SMS message content (max 160 chars recommended)",
  "phoneNumber": "+1234567890",
  "priority": "Normal",
  "metadata": {}
}
```

**Next Steps**
1. Integrate with SMS provider (Twilio, AWS SNS, etc.)
2. Implement `ISmsSender` interface in Application layer
3. Create `SmsNotification` entity in Domain layer (already exists as stub)
4. Implement `SmsSender` in Api layer
5. Complete SMS worker with provider integration

---

### Send WhatsApp Notification

**Endpoint**
```
POST /api/notifications/whatsapp
```

**Status**
- ⚠️ **Implementation Pending** — Endpoint accepts requests but does not process them
- Returns `202 Accepted` with a placeholder response

**Request Body** (placeholder structure)
```json
{
  "recipientId": "550e8400-e29b-41d4-a716-446655440000",
  "title": "Notification Title",
  "body": "WhatsApp message content",
  "phoneNumber": "+1234567890",
  "priority": "Normal",
  "metadata": {}
}
```

**Next Steps**
1. Integrate with WhatsApp provider (Twilio, MessageBird, etc.)
2. Implement `IWhatsAppSender` interface in Application layer
3. Create `WhatsAppNotification` entity in Domain layer (already exists as stub)
4. Implement `WhatsAppSender` in Api layer
5. Complete WhatsApp worker with provider integration

---

### Get Notification by ID

Retrieve the status and details of a specific notification.

**Endpoint**
```
GET /api/notifications/{id}
```

**Path Parameters**

| Parameter | Type | Description |
|---|---|---|
| `id` | UUID | The notification ID |

**Status Code**
- `200 OK` — Notification found, details returned
- `404 Not Found` — No notification with that ID exists
- `400 Bad Request` — Invalid UUID format

**Response**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "recipientId": "550e8400-e29b-41d4-a716-446655440001",
  "title": "Order Confirmation",
  "body": "Your order #12345 has been confirmed.",
  "channel": "Email",
  "status": "Sent",
  "priority": "Normal",
  "createdAt": "2026-03-10T14:30:00Z",
  "sentAt": "2026-03-10T14:31:00Z",
  "failedAt": null,
  "errorMessage": null
}
```

**Example cURL**
```bash
curl -X GET http://localhost:5000/api/notifications/550e8400-e29b-41d4-a716-446655440000
```

---

### Get Failed Notifications

Retrieve all notifications that failed after maximum retry attempts.

**Endpoint**
```
GET /api/notifications/failed
```

**Status Code**
- `200 OK` — List of failed notifications returned (may be empty)

**Response**
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "recipientId": "550e8400-e29b-41d4-a716-446655440001",
    "title": "Order Confirmation",
    "body": "Your order #12345 has been confirmed.",
    "channel": "Email",
    "status": "Failed",
    "priority": "Normal",
    "createdAt": "2026-03-10T14:30:00Z",
    "sentAt": null,
    "failedAt": "2026-03-10T14:35:00Z",
    "errorMessage": "Maileroo API returned 550: Service temporarily unavailable"
  }
]
```

**Example cURL**
```bash
curl -X GET http://localhost:5000/api/notifications/failed
```

---

## Real-Time Notifications with SignalR

### Hub Connection

Connect to the NotificationHub to receive real-time push notifications and status updates.

**Hub URL**
```
ws://localhost:5000/hubs/notifications?recipientId={recipientId}
```

**Query Parameters**

| Parameter | Type | Required | Description |
|---|---|---|---|
| `recipientId` | UUID | Yes | The recipient's unique ID; client joins the group `recipient-{recipientId}` |

### Events

#### ReceiveNotification

Fired when a push notification is sent to the recipient.

**Payload**
```typescript
{
  notificationId: string;  // UUID
  title: string;
  body: string;
  priority: string;        // "Low" | "Normal" | "High" | "Critical"
  metadata: Record<string, string>;
  sentAt: string;          // ISO 8601 timestamp
}
```

**Example Listener**
```javascript
connection.on("ReceiveNotification", (message) => {
  console.log("Received:", message.title, message.body);
});
```

#### NotificationStatusChanged

Fired when an asynchronous notification (Email, SMS, WhatsApp) completes processing.

**Payload**
```typescript
{
  notificationId: string;      // UUID
  channel: string;             // "Email" | "Sms" | "WhatsApp"
  success: boolean;
  externalReferenceId: string; // Provider's reference ID (e.g., Maileroo reference)
  timestamp: string;           // ISO 8601 timestamp
}
```

**Example Listener**
```javascript
connection.on("NotificationStatusChanged", (update) => {
  if (update.success) {
    console.log(`${update.channel} sent! Ref: ${update.externalReferenceId}`);
  } else {
    console.log(`${update.channel} failed!`);
  }
});
```

### Full JavaScript Example

```html
<!DOCTYPE html>
<html>
<head>
  <script src="https://cdn.jsdelivr.net/npm/@microsoft/signalr@8/signalr.min.js"></script>
</head>
<body>
  <div id="messages"></div>
  <script>
    const recipientId = "550e8400-e29b-41d4-a716-446655440000";
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`http://localhost:5000/hubs/notifications?recipientId=${recipientId}`)
      .withAutomaticReconnect()
      .build();

    connection.on("ReceiveNotification", (message) => {
      const div = document.createElement("div");
      div.textContent = `PUSH: ${message.title} - ${message.body}`;
      document.getElementById("messages").appendChild(div);
    });

    connection.on("NotificationStatusChanged", (update) => {
      const div = document.createElement("div");
      const status = update.success ? "✓" : "✗";
      div.textContent = `${status} ${update.channel}: ${update.externalReferenceId}`;
      document.getElementById("messages").appendChild(div);
    });

    connection.start().catch(err => console.error(err));
  </script>
</body>
</html>
```

---

## Message Flow Architecture

### Push Notifications

**Synchronous flow (immediate response)**

```
1. Client: POST /api/notifications/push
2. API: Create PushNotification entity (status: Pending)
3. API: Call IPushNotificationSender.SendToRecipientAsync()
4. SignalRPushNotificationSender: Send to recipient group via SignalR
5. API: Update entity status to "Sent"
6. API: Return 201 Created with notification details
7. Client: Receive push via SignalR, UI updates
```

### Email Notifications

**Asynchronous flow (deferred response)**

```
1. Client: POST /api/notifications/email
2. API: Create EmailNotification entity (status: Pending)
3. API: Update status to "Queued"
4. API: Publish EmailNotificationMessage to RabbitMQ
5. API: Return 202 Accepted with notification details
------- BACKGROUND PROCESSING BEGINS -------
6. Email Worker: Consume EmailNotificationMessage from "email-notifications" queue
7. Email Worker: Call Maileroo HTTP API to send email
8. Email Worker: On success/failure, publish NotificationStatusUpdate to RabbitMQ
------- STATUS UPDATE PROCESSING -------
9. API: NotificationStatusUpdateConsumer receives NotificationStatusUpdate
10. API: Update EmailNotification entity status to "Sent" or "Failed"
11. API: Broadcast "NotificationStatusChanged" event via SignalR
12. Client: Receive status update via SignalR, UI updates
```

### Message Queue Details

**RabbitMQ Queues**

| Queue | Message Type | Consumer | Purpose |
|---|---|---|---|
| `email-notifications` | `EmailNotificationMessage` | Email Worker | Queue for email sends |
| `sms-notifications` | `SmsNotificationMessage` | SMS Worker | Queue for SMS sends (pending) |
| `whatsapp-notifications` | `WhatsAppNotificationMessage` | WhatsApp Worker | Queue for WhatsApp sends (pending) |
| `notification-status-updates` | `NotificationStatusUpdate` | API Consumer | Status feedback from workers |

**Retry Strategy**

All workers use exponential backoff:
- **Attempt 1**: Immediate
- **Attempt 2**: 5 seconds later
- **Attempt 3**: 30 seconds later
- **Final attempt**: 1 minute later
- **Max retries**: 3

After all retries fail, the message is routed to a Dead Letter Queue (DLQ) for manual investigation.

---

## Data Models

### NotificationResponse

Response model returned by all notification endpoints.

```typescript
{
  id: string;                    // UUID, unique notification identifier
  recipientId: string;           // UUID, target recipient
  title: string;                 // Notification title
  body: string;                  // Notification body/content
  channel: string;               // "Push" | "Email" | "Sms" | "WhatsApp"
  status: string;                // "Pending" | "Queued" | "Sent" | "Failed"
  priority: string;              // "Low" | "Normal" | "High" | "Critical"
  createdAt: string;             // ISO 8601 timestamp (UTC)
  sentAt?: string;               // ISO 8601 timestamp (UTC), null if not yet sent
  failedAt?: string;             // ISO 8601 timestamp (UTC), null if not failed
  errorMessage?: string;         // Error description if status is "Failed"
}
```

### SendPushNotificationRequest

```typescript
{
  recipientId: string;           // UUID, required
  title: string;                 // required
  body: string;                  // required
  priority?: string;             // optional, default: "Normal"
  metadata?: Record<string, string>; // optional
}
```

### SendEmailNotificationRequest

```typescript
{
  recipientId: string;           // UUID, required
  subject: string;               // required
  body: string;                  // required
  to: string;                    // email address, required
  from?: string;                 // email address, optional
  replyTo?: string;              // email address, optional
  cc?: string[];                 // array of email addresses, optional
  bcc?: string[];                // array of email addresses, optional
  isHtml?: boolean;              // default: false
  attachmentUrls?: string[];     // array of URLs, optional
  priority?: string;             // optional, default: "Normal"
  metadata?: Record<string, string>; // optional
}
```

### NotificationStatusUpdate (Internal)

Published by workers and consumed by the API.

```typescript
{
  notificationId: string;        // UUID
  recipientId: string;           // UUID
  channel: string;               // "Email" | "Sms" | "WhatsApp"
  success: boolean;
  errorMessage?: string;         // null if success=true
  externalReferenceId?: string;  // Provider's reference ID (e.g., Maileroo ref)
  timestamp: string;             // ISO 8601 timestamp (UTC)
}
```

---

## Implementation Status

**Last Updated**: 2026-03-10
**Status**: Production Ready (Core Features)

### Fully Implemented

| Feature | Details |
|---|---|
| Push Notifications | Synchronous delivery via SignalR |
| Email Notifications | Asynchronous delivery via Maileroo |
| SignalR Hub | Real-time status updates, group-based routing |
| Status Tracking | Database persistence via EF Core + PostgreSQL |
| Structured Logging | Serilog + Seq integration |
| Message Queue | RabbitMQ with MassTransit, exponential retry |
| Repository Pattern | CRUD operations, query filters |
| Clean Architecture | Domain-driven design across 4 layers |

### Pending Implementation

| Feature | Status | Notes |
|---|---|---|
| SMS Notifications | Scaffolded | Worker and endpoint ready for provider integration |
| WhatsApp Notifications | Scaffolded | Worker and endpoint ready for provider integration |
| API Key Authentication | Not started | Recommended for production |
| Rate Limiting | Commented code exists | Can be enabled with configuration |
| Input Validation | Basic | Consider FluentValidation for robust rules |
| Health Check Endpoints | Not implemented | Add `/health` endpoint for orchestrators |

---

## Best Practices

### For API Consumers

1. **Always provide RecipientId** — This is your internal user/entity ID, not an email address
2. **Handle 202 Accepted gracefully** — Async notifications are not immediately delivered
3. **Subscribe to SignalR for status updates** — Don't poll `GET /notifications/{id}` repeatedly
4. **Validate email addresses client-side** — Catch invalid addresses before sending
5. **Use metadata sparingly** — Store only essential context; large payloads slow processing
6. **Implement exponential backoff for retries** — If your queue call fails, wait before retrying
7. **Log CorrelationId** — Use for distributed tracing across your application

### For Operations & DevOps

1. **Monitor RabbitMQ queues** — Alert on queue depth > threshold or DLQ messages
2. **Set up Seq dashboards** — View real-time logs grouped by channel/status
3. **Schedule DLQ reviews** — Process dead-lettered messages to identify systemic issues
4. **Back up PostgreSQL daily** — Notification persistence is business-critical
5. **Rotate Maileroo API keys** — Follow provider's security recommendations
6. **Enable CORS credentials for SignalR** — Do not remove `AllowCredentials()` from policy
7. **Use environment-specific configuration** — Separate Development, Staging, Production

### For Development

1. **Use Scalar UI** — Interactive API docs at `http://localhost:5000/scalar`
2. **Check Seq logs** — All requests logged with CorrelationId
3. **Monitor RabbitMQ management UI** — Visible at `http://localhost:15672`
4. **Test with SignalR client** — Don't rely on HTTP responses for async notifications
5. **Run migrations before workers** — Workers depend on the database schema
6. **Keep worker logs handy** — Worker stdout shows consumer lifecycle and errors

### Email-Specific Best Practices

1. **Use HTML templates wisely** — Complex HTML may not render uniformly across clients
2. **Always set ReplyTo** — Improves user experience; sender address may not accept replies
3. **Attach strategically** — Large attachments increase queue latency
4. **Test with real addresses** — Maileroo sandbox testing may behave differently
5. **Monitor Maileroo quota** — API rate limits and per-month sending limits apply
6. **Handle attachment download failures** — Worker skips failed attachments but continues
7. **Prefer plain text for critical messages** — HTML rendering failures don't affect plain text

---

## Troubleshooting

### Common Issues

#### "Notification not found" when fetching by ID

**Cause**: Notification ID does not exist in database

**Solution**:
1. Verify the ID was returned correctly from the send endpoint
2. Check that migrations ran successfully: `dotnet ef database update`
3. Confirm the notification was created: query PostgreSQL directly
4. Check logs for "Notification...not found when processing status update"

#### Email not received, but status shows "Sent"

**Cause**: Maileroo accepted the email, but delivery failed downstream

**Solution**:
1. Log in to Maileroo dashboard to check delivery logs
2. Verify the recipient email address is valid
3. Check sender reputation and domain authentication (SPF, DKIM)
4. Review Maileroo error details in Seq logs
5. Contact Maileroo support if quota exceeded

#### SignalR client not receiving notifications

**Cause**: Client not connected, wrong recipientId, or group mismatch

**Solution**:
1. Verify the client is connected: check connection state in browser console
2. Confirm the `recipientId` query parameter matches the sending request
3. Ensure SignalR hub is accessible: test `ws://localhost:5000/hubs/notifications` in Postman
4. Check CORS policy allows SignalR credentials: `AllowCredentials()` must be present
5. Look for connection errors in browser DevTools > Network > WS

#### "Failed to queue message" when sending async notification

**Cause**: RabbitMQ is unavailable or misconfigured

**Solution**:
1. Verify RabbitMQ is running: `podman ps | grep rabbitmq`
2. Check RabbitMQ logs: `podman logs <rabbitmq-container-id>`
3. Verify connection string in `appsettings.json`: `RabbitMQ:Host`
4. Test RabbitMQ connectivity: telnet `localhost 5672`
5. Restart RabbitMQ if necessary

#### Worker process crashes or doesn't start

**Cause**: Missing configuration, database connection, or worker code error

**Solution**:
1. Verify `appsettings.json` has RabbitMQ settings
2. For Email worker, verify `Maileroo:ApiKey` is set
3. Check worker logs: `dotnet run --project src/Workers/NotificationHub.Worker.Email`
4. Ensure database migrations are applied
5. Check for connection string issues in appsettings

#### High queue latency or long processing times

**Cause**: Worker overloaded, external API slow, or database bottleneck

**Solution**:
1. Scale workers horizontally (run multiple instances)
2. Check Maileroo API performance: `curl -I https://smtp.maileroo.com/api/v2/status`
3. Monitor PostgreSQL slow query logs
4. Review RabbitMQ queue depth in management UI
5. Check worker logs for exceptions during processing
6. Increase retry timeouts if external API is slow

---

## Related Resources

- **CLAUDE.md** — Architecture overview and local development commands
- **Seq Logs** — Structured logging at `http://localhost:8081`
- **RabbitMQ UI** — Message broker management at `http://localhost:15672`
- **Scalar OpenAPI** — Interactive API documentation at `http://localhost:5000/scalar`
- **Clean Architecture** — See Domain, Application, Infrastructure, and Api project files

---

*Last Updated: 2026-03-10*
*NotificationHub API Documentation — Production Ready*
