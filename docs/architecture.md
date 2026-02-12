# Realtime Notification Hub - Architecture Diagrams

> Reference diagrams for the Clean Architecture + DDD restructuring (Fase 0).
> Render with [Mermaid Preview](https://marketplace.visualstudio.com/items?itemName=bierner.markdown-mermaid) in VS Code or at [mermaid.live](https://mermaid.live).

---

## 1. Project Dependencies

The **dependency rule** of Clean Architecture: inner layers never know about outer layers. Domain has zero external dependencies.

```mermaid
flowchart TB
    subgraph Presentation["Presentation Layer"]
        API["<b>NotificationHub.Api</b><br/>ASP.NET Core Web API<br/>Controllers, Middleware, DI Setup"]
        WE["<b>Workers.Email</b><br/>Background Service"]
        WS["<b>Workers.Sms</b><br/>Background Service"]
        WW["<b>Workers.WhatsApp</b><br/>Background Service"]
    end

    subgraph Application["Application Layer"]
        APP["<b>NotificationHub.Application</b><br/>Commands & Queries (CQRS)<br/>MediatR Handlers & Behaviors<br/>FluentValidation Validators<br/>DTOs, Result&lt;T&gt;, PagedResult&lt;T&gt;"]
    end

    subgraph Domain["Domain Layer"]
        DOM["<b>NotificationHub.Domain</b><br/>Aggregates, Entities, Value Objects<br/>Domain Events, Enums<br/>Repository Interfaces<br/><i>Zero external dependencies</i>"]
    end

    subgraph Infrastructure["Infrastructure Layer"]
        INFRA["<b>NotificationHub.Infrastructure</b><br/>EF Core DbContext & Repos<br/>UnitOfWork + Event Dispatcher<br/>MassTransit (RabbitMQ)<br/>SignalR, JWT, Serilog, Polly"]
    end

    subgraph External["External Services"]
        PG[("PostgreSQL")]
        RMQ[("RabbitMQ")]
        SR["SignalR Clients"]
    end

    API --> APP
    API --> INFRA
    WE --> INFRA
    WS --> INFRA
    WW --> INFRA
    APP --> DOM
    INFRA --> APP
    INFRA --> DOM
    INFRA --> PG
    INFRA --> RMQ
    API --> SR

    style DOM fill:#90EE90,color:#000
    style APP fill:#87CEEB,color:#000
    style INFRA fill:#FFB347,color:#000
    style API fill:#DDA0DD,color:#000
    style WE fill:#F0E68C,color:#000
    style WS fill:#F0E68C,color:#000
    style WW fill:#F0E68C,color:#000
```

---

## 2. Domain Model

### Key change from current state

The current TPH hierarchy (`abstract Notification` → `EmailNotification`, `SmsNotification`, `WhatsAppNotification`, `PushNotification`) is **replaced** by a single `Notification` entity with:
- Static factory methods (`CreateEmail()`, `CreateSms()`, etc.)
- Optional channel-specific fields (`Subject`, `HtmlBody`, `SmsContent`, etc.)
- State transition methods (`MarkAsQueued()`, `MarkAsSent()`, etc.) that enforce business rules and raise domain events

```mermaid
classDiagram
    direction TB

    %% ───────── Base Classes ─────────
    class Entity {
        <<abstract>>
        +Guid Id
        +DateTime CreatedAt
        +DateTime? UpdatedAt
        #List~IDomainEvent~ _domainEvents
        +AddDomainEvent(IDomainEvent)
        +ClearDomainEvents()
        +GetDomainEvents() IReadOnlyList
    }

    class AggregateRoot {
        <<abstract>>
    }

    class ValueObject {
        <<abstract>>
        #GetEqualityComponents()* IEnumerable~object~
        +Equals(object?) bool
        +GetHashCode() int
    }

    class IDomainEvent {
        <<interface>>
        +Guid EventId
        +DateTime OccurredOn
    }

    AggregateRoot --|> Entity

    %% ───────── Value Objects ─────────
    class EmailAddress {
        +string Value
        +Create(string) Result~EmailAddress~$
        +ToString() string
    }

    class PhoneNumber {
        +string Value
        +string? CountryCode
        +Create(string, string?) Result~PhoneNumber~$
        +ToString() string
    }

    class NotificationContent {
        +string Value
        +int Length
        +Create(string, int maxLength) Result~NotificationContent~$
        +ToString() string
    }

    EmailAddress --|> ValueObject
    PhoneNumber --|> ValueObject
    NotificationContent --|> ValueObject

    %% ───────── Enums ─────────
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

    %% ───────── Notification Aggregate ─────────
    class Notification {
        +NotificationChannel Channel
        +NotificationStatus Status
        +NotificationPriority Priority
        +string RecipientId
        +string? Subject
        +string? HtmlBody
        +string? SmsContent
        +string? TemplateName
        +Dictionary~string,string~? TemplateParameters
        +string? PushTitle
        +string? PushContent
        +DateTime? SentAt
        +DateTime? ReadAt
        +DateTime? ScheduledAt
        +int RetryCount
        +int MaxRetries
        +string? ErrorMessage
        +Guid? TemplateId
        -List~DeliveryAttempt~ _deliveryAttempts
        +IReadOnlyList~DeliveryAttempt~ DeliveryAttempts
        +CreateEmail(string recipientId, string subject, string htmlBody, NotificationPriority) Notification$
        +CreateSms(string recipientId, string content, NotificationPriority) Notification$
        +CreateWhatsApp(string recipientId, string templateName, Dictionary params, NotificationPriority) Notification$
        +CreatePush(string recipientId, string title, string content, NotificationPriority) Notification$
        +MarkAsQueued()
        +StartProcessing()
        +MarkAsSent(DateTime sentAt)
        +MarkAsFailed(string error)
        +MarkAsRead()
        +AddDeliveryAttempt(bool success, string? error, string? providerResponse)
        +CanRetry() bool
    }

    class DeliveryAttempt {
        +Guid NotificationId
        +int AttemptNumber
        +DateTime AttemptedAt
        +bool Success
        +string? ErrorMessage
        +string? ProviderResponse
        +TimeSpan? Duration
        +Create(Guid notificationId, int attemptNumber, bool success, string? error, string? providerResponse) DeliveryAttempt$
    }

    Notification --|> AggregateRoot
    DeliveryAttempt --|> Entity
    Notification "1" *-- "0..*" DeliveryAttempt : contains
    Notification ..> NotificationChannel : uses
    Notification ..> NotificationStatus : uses
    Notification ..> NotificationPriority : uses

    %% ───────── Recipient Aggregate ─────────
    class Recipient {
        +string UserId
        +EmailAddress? Email
        +PhoneNumber? PhoneNumber
        +string? DisplayName
        +bool IsActive
        -List~NotificationPreference~ _preferences
        +IReadOnlyList~NotificationPreference~ Preferences
        +Create(string userId, EmailAddress? email, PhoneNumber? phone, string? displayName) Recipient$
        +UpdateEmail(EmailAddress email)
        +UpdatePhoneNumber(PhoneNumber phone)
        +SetPreference(NotificationChannel channel, bool enabled)
        +CanReceive(NotificationChannel channel) bool
        +Deactivate()
        +Activate()
    }

    class NotificationPreference {
        +Guid RecipientId
        +NotificationChannel Channel
        +bool IsEnabled
        +DateTime? OptedInAt
        +DateTime? OptedOutAt
        +Enable()
        +Disable()
    }

    Recipient --|> AggregateRoot
    NotificationPreference --|> Entity
    Recipient "1" *-- "0..*" NotificationPreference : contains
    Recipient ..> EmailAddress : uses
    Recipient ..> PhoneNumber : uses
    NotificationPreference ..> NotificationChannel : uses

    %% ───────── NotificationTemplate Aggregate ─────────
    class NotificationTemplate {
        +string Name
        +NotificationChannel Channel
        +string SubjectTemplate
        +string BodyTemplate
        +List~string~ RequiredVariables
        +bool IsActive
        +Create(string name, NotificationChannel channel, string subject, string body) NotificationTemplate$
        +Render(Dictionary~string,string~ variables) RenderedTemplate
        +Update(string subject, string body)
        +Activate()
        +Deactivate()
    }

    class RenderedTemplate {
        <<value object>>
        +string Subject
        +string Body
    }

    NotificationTemplate --|> AggregateRoot
    NotificationTemplate ..> NotificationChannel : uses
    NotificationTemplate ..> RenderedTemplate : produces

    %% ───────── Domain Events ─────────
    class NotificationCreatedEvent {
        +Guid NotificationId
        +NotificationChannel Channel
        +string RecipientId
        +NotificationPriority Priority
    }

    class NotificationQueuedEvent {
        +Guid NotificationId
        +NotificationChannel Channel
    }

    class NotificationSentEvent {
        +Guid NotificationId
        +DateTime SentAt
    }

    class NotificationFailedEvent {
        +Guid NotificationId
        +string ErrorMessage
        +int RetryCount
    }

    NotificationCreatedEvent ..|> IDomainEvent
    NotificationQueuedEvent ..|> IDomainEvent
    NotificationSentEvent ..|> IDomainEvent
    NotificationFailedEvent ..|> IDomainEvent

    %% ───────── Repository Interfaces ─────────
    class INotificationRepository {
        <<interface>>
        +GetByIdAsync(Guid, CancellationToken) Task~Notification?~
        +GetPagedAsync(NotificationFilter, CancellationToken) Task~PagedResult~
        +AddAsync(Notification, CancellationToken) Task
        +UpdateAsync(Notification, CancellationToken) Task
    }

    class IRecipientRepository {
        <<interface>>
        +GetByIdAsync(Guid, CancellationToken) Task~Recipient?~
        +GetByUserIdAsync(string, CancellationToken) Task~Recipient?~
        +AddAsync(Recipient, CancellationToken) Task
        +UpdateAsync(Recipient, CancellationToken) Task
    }

    class INotificationTemplateRepository {
        <<interface>>
        +GetByIdAsync(Guid, CancellationToken) Task~NotificationTemplate?~
        +GetByNameAsync(string, CancellationToken) Task~NotificationTemplate?~
        +GetAllActiveAsync(CancellationToken) Task~List~
        +AddAsync(NotificationTemplate, CancellationToken) Task
        +UpdateAsync(NotificationTemplate, CancellationToken) Task
    }

    class IUnitOfWork {
        <<interface>>
        +SaveChangesAsync(CancellationToken) Task~int~
    }
```

---

## 3. Event-Driven Architecture / Message Flow

End-to-end flow from notification creation to real-time client update.

```mermaid
flowchart TB
    subgraph API["NotificationHub.Api"]
        CTRL["Controller<br/>POST /api/notifications"]
        CMD["SendNotificationHandler"]
        UOW["UnitOfWork<br/>+ EventDispatcher"]
        STATUSC["NotificationStatusConsumer<br/>(MassTransit Consumer)"]
        HUB["SignalR Hub<br/>/hubs/notifications"]
    end

    subgraph RMQ["RabbitMQ"]
        EX["notifications-exchange<br/>(topic)"]
        EQ["email-queue<br/>routing: notification.email"]
        SQ["sms-queue<br/>routing: notification.sms"]
        WQ["whatsapp-queue<br/>routing: notification.whatsapp"]
        DLQ["dead-letter-queue<br/>(failed after max retries)"]
        SEX["status-exchange<br/>(fanout)"]
        STQ["status-queue"]
    end

    subgraph Workers["Worker Services"]
        EMAILW["Email Worker<br/>Consumer + Polly Retry"]
        SMSW["SMS Worker<br/>Consumer + Polly Retry"]
        WAW["WhatsApp Worker<br/>Consumer + Polly Retry"]
    end

    subgraph DB["Database"]
        PG[("PostgreSQL")]
    end

    subgraph Clients["Real-time Clients"]
        WEB["Web App<br/>group: recipient-123"]
        MOB["Mobile App<br/>group: recipient-456"]
    end

    %% API publishes
    CTRL --> CMD --> UOW
    UOW -->|"NotificationCreated<br/>domain event"| EX

    %% RabbitMQ routing
    EX -->|notification.email| EQ
    EX -->|notification.sms| SQ
    EX -->|notification.whatsapp| WQ

    %% Workers consume
    EQ --> EMAILW
    SQ --> SMSW
    WQ --> WAW

    %% Workers publish status
    EMAILW -->|"NotificationStatusChanged<br/>(Sent/Failed)"| SEX
    SMSW -->|"NotificationStatusChanged<br/>(Sent/Failed)"| SEX
    WAW -->|"NotificationStatusChanged<br/>(Sent/Failed)"| SEX

    %% DLQ on failure
    EQ -.->|"max retries<br/>exceeded"| DLQ
    SQ -.->|"max retries<br/>exceeded"| DLQ
    WQ -.->|"max retries<br/>exceeded"| DLQ

    %% Status update back to API
    SEX --> STQ --> STATUSC
    STATUSC -->|"Update status"| PG
    STATUSC -->|"Broadcast"| HUB

    %% SignalR to clients
    HUB -->|"group: recipient-123"| WEB
    HUB -->|"group: recipient-456"| MOB

    style API fill:#DDA0DD22,stroke:#DDA0DD
    style RMQ fill:#FFD70022,stroke:#FFD700
    style Workers fill:#F0E68C22,stroke:#DAA520
    style DB fill:#4682B422,stroke:#4682B4
    style Clients fill:#90EE9022,stroke:#2E8B57
```

---

## 4. Entity-Relationship Diagram (ERD)

Target database model. The `Notifications` table is a unified model (no more TPH discriminator with separate subclasses). `ChannelProviderSettings` stores external provider credentials (SendGrid, Twilio, etc.) configurable from the frontend.

```mermaid
erDiagram
    NOTIFICATIONS {
        uuid id PK
        int channel "1=Email 2=Sms 3=WhatsApp 4=Push"
        int status "1=Pending 2=Queued 3=Processing 4=Sent 5=Failed 6=Read"
        int priority "1=Low 2=Normal 3=High 4=Critical"
        varchar recipient_id "External user ID"
        varchar subject "Email/Push: subject line"
        text html_body "Email: HTML content"
        varchar sms_content "SMS: message text (max 1600)"
        varchar template_name "WhatsApp: template name"
        jsonb template_parameters "WhatsApp: template vars"
        varchar push_title "Push: notification title"
        text push_content "Push: notification body"
        timestamp sent_at "When delivery confirmed"
        timestamp read_at "Push: when user read it"
        timestamp scheduled_at "For scheduled delivery"
        int retry_count "Current retry count"
        int max_retries "Max allowed retries"
        text error_message "Last error message"
        uuid template_id FK "Optional: NotificationTemplates.id"
        timestamp created_at
        timestamp updated_at
    }

    DELIVERY_ATTEMPTS {
        uuid id PK
        uuid notification_id FK
        int attempt_number
        timestamp attempted_at
        boolean success
        text error_message
        text provider_response "JSON from external provider"
        interval duration
    }

    RECIPIENTS {
        uuid id PK
        varchar user_id UK "Unique external user ID"
        varchar email "Validated email address"
        varchar phone_number "Validated phone number"
        varchar country_code "Phone country code"
        varchar display_name
        boolean is_active
        timestamp created_at
        timestamp updated_at
    }

    NOTIFICATION_PREFERENCES {
        uuid id PK
        uuid recipient_id FK
        int channel "1=Email 2=Sms 3=WhatsApp 4=Push"
        boolean is_enabled
        timestamp opted_in_at
        timestamp opted_out_at
        timestamp created_at
        timestamp updated_at
    }

    NOTIFICATION_TEMPLATES {
        uuid id PK
        varchar name UK "Unique template name"
        int channel "1=Email 2=Sms 3=WhatsApp 4=Push"
        varchar subject_template "For Email"
        text body_template "Body with placeholders"
        jsonb required_variables "Array of var names"
        boolean is_active
        timestamp created_at
        timestamp updated_at
    }

    CHANNEL_PROVIDER_SETTINGS {
        uuid id PK
        int channel "1=Email 2=Sms 3=WhatsApp 4=Push"
        varchar provider_name UK "SendGrid, Twilio, Meta, Firebase"
        jsonb credentials "Encrypted: API keys, SIDs, tokens"
        boolean is_active "Only one active per channel"
        int priority "Failover order"
        jsonb settings "Provider-specific config (sender, region, etc)"
        timestamp created_at
        timestamp updated_at
    }

    NOTIFICATIONS ||--o{ DELIVERY_ATTEMPTS : "has attempts"
    NOTIFICATIONS }o--o| NOTIFICATION_TEMPLATES : "uses template"
    RECIPIENTS ||--o{ NOTIFICATION_PREFERENCES : "has preferences"
    RECIPIENTS ||--o{ NOTIFICATIONS : "receives"
    CHANNEL_PROVIDER_SETTINGS ||--o{ NOTIFICATIONS : "delivered via"
```
