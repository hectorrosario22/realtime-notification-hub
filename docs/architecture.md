# Realtime Notification Hub - Architecture (Current State)

> This document describes the architecture implemented as of February 12, 2026.
> It intentionally avoids roadmap-only components that are not yet wired.

---

## 1. Project Dependencies

Clean Architecture dependency rule currently implemented:

- `NotificationHub.Domain`: core domain model and abstractions.
- `NotificationHub.Application`: application contracts (repository interfaces).
- `NotificationHub.Api`: presentation + EF Core persistence implementation.
- `NotificationHub.Infrastructure`: scaffolded project, not yet hosting EF/infra code.

```mermaid
flowchart TB
    subgraph Presentation["Presentation Layer"]
        API["<b>NotificationHub.Api</b><br/>ASP.NET Core Web API<br/>Controllers, DTOs, EF DbContext, Repositories"]
        WE["<b>Workers.Email</b>"]
        WS["<b>Workers.Sms</b>"]
        WW["<b>Workers.WhatsApp</b>"]
    end

    subgraph Application["Application Layer"]
        APP["<b>NotificationHub.Application</b><br/>Interfaces (ports):<br/>INotificationRepository<br/>INotificationLogRepository"]
    end

    subgraph Domain["Domain Layer"]
        DOM["<b>NotificationHub.Domain</b><br/>AggregateRoot, Entity, ValueObject<br/>Notification aggregate, enums, domain events"]
    end

    subgraph Infrastructure["Infrastructure Layer"]
        INFRA["<b>NotificationHub.Infrastructure</b><br/>Scaffolded (pending migration of EF code)"]
    end

    subgraph External["External Services"]
        PG[("PostgreSQL")]
    end

    API --> APP
    API --> DOM
    APP --> DOM
    API --> PG

    style DOM fill:#90EE90,color:#000
    style APP fill:#87CEEB,color:#000
    style API fill:#DDA0DD,color:#000
    style INFRA fill:#FFB347,color:#000
```

---

## 2. Domain Model

The current model uses a single `Notification` aggregate (no TPH subclasses).

```mermaid
classDiagram
    direction TB

    class Entity~TId~ {
        <<abstract>>
        +TId Id
    }

    class AggregateRoot~TId~ {
        <<abstract>>
        +DomainEvents IReadOnlyCollection
        +ClearDomainEvents()
    }

    class ValueObject {
        <<abstract>>
    }

    class IDomainEvent {
        <<interface>>
        +DateTime OccurredOnUtc
    }

    AggregateRoot~TId~ --|> Entity~TId~

    class NotificationChannel {
        <<enumeration>>
        Email
        Sms
        WhatsApp
        Push
    }

    class NotificationStatus {
        <<enumeration>>
        Pending
        Queued
        Processing
        Sent
        Failed
        Read
    }

    class NotificationPriority {
        <<enumeration>>
        Low
        Normal
        High
        Critical
    }

    class NotificationEventType {
        <<enumeration>>
        Created
        Queued
        ProcessingStarted
        Delivered
        Failed
        Read
        Retried
    }

    class Notification {
        +NotificationChannel Channel
        +NotificationStatus Status
        +NotificationPriority Priority
        +string RecipientId
        +string? Subject
        +string? HtmlBody
        +string? SmsContent
        +string? TemplateName
        +IReadOnlyDictionary~string,string~? TemplateParameters
        +string? PushTitle
        +string? PushContent
        +DateTime CreatedAtUtc
        +DateTime? UpdatedAtUtc
        +DateTime? ScheduledAtUtc
        +DateTime? SentAtUtc
        +DateTime? ReadAtUtc
        +int RetryCount
        +int MaxRetries
        +string? ErrorMessage
        +CreateEmail(...) Notification$
        +CreateSms(...) Notification$
        +CreateWhatsApp(...) Notification$
        +CreatePush(...) Notification$
        +ScheduleFor(DateTime)
        +MarkAsQueued()
        +StartProcessing()
        +MarkAsSent(DateTime?)
        +MarkAsFailed(string)
        +MarkAsRead(DateTime?)
        +CanRetry() bool
    }

    class NotificationLog {
        +Guid Id
        +Guid NotificationId
        +Notification Notification
        +NotificationChannel Channel
        +NotificationEventType EventType
        +string? Message
        +string? RequestData
        +string? ResponseData
        +string? ErrorDetails
        +DateTime Timestamp
    }

    class NotificationCreatedDomainEvent {
        +Guid NotificationId
        +NotificationChannel Channel
        +string RecipientId
    }

    class NotificationStatusChangedDomainEvent {
        +Guid NotificationId
        +NotificationStatus PreviousStatus
        +NotificationStatus CurrentStatus
    }

    Notification --|> AggregateRoot~Guid~
    Notification ..> NotificationChannel
    Notification ..> NotificationStatus
    Notification ..> NotificationPriority
    NotificationLog ..> NotificationEventType
    NotificationCreatedDomainEvent ..|> IDomainEvent
    NotificationStatusChangedDomainEvent ..|> IDomainEvent
```

---

## 3. Runtime Flow (Current)

```mermaid
flowchart LR
    C[Client] -->|HTTP| API[NotificationsController]
    API -->|Create* factory| DOM[Notification Aggregate]
    API -->|INotificationRepository| REPO[EF Repository]
    API -->|INotificationLogRepository| LOGREPO[EF Log Repository]
    REPO --> DB[(PostgreSQL)]
    LOGREPO --> DB
```

Current endpoints:

- `POST /api/notifications/email`
- `POST /api/notifications/sms`
- `POST /api/notifications/whatsapp`
- `POST /api/notifications/push`
- `GET /api/notifications`
- `GET /api/notifications/{id}`
- `PATCH /api/notifications/{id}/read`

---

## 4. Database Schema (InitialCreate)

Schema generated from `src/NotificationHub.Api/Migrations/20260212222008_InitialCreate.cs`.

```mermaid
erDiagram
    NOTIFICATIONS {
        uuid id PK
        int channel
        int status
        int priority
        varchar recipient_id
        varchar subject
        varchar html_body
        varchar sms_content
        varchar template_name
        text parameters
        varchar push_title
        varchar push_content
        timestamp created_at
        timestamp updated_at
        timestamp scheduled_at
        timestamp sent_at
        timestamp read_at
        int retry_count
        int max_retries
        varchar error_message
    }

    NOTIFICATION_LOGS {
        uuid id PK
        uuid notification_id FK
        int channel
        int event_type
        varchar message
        text request_data
        text response_data
        varchar error_details
        timestamp timestamp
    }

    NOTIFICATIONS ||--o{ NOTIFICATION_LOGS : "has logs"
```

---

## 5. Next Architectural Step

To finish the layering move, migrate EF Core artifacts from `NotificationHub.Api` to `NotificationHub.Infrastructure`:

- `NotificationDbContext`
- EF configurations
- repository implementations
- migrations folder

The API should keep only presentation concerns + DI composition.
