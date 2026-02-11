# Realtime Notification Hub - Production Roadmap

## Current State
- Single `NotificationHub.Api` project with entities, repos, controllers, DTOs mixed together
- TPH entity hierarchy (Notification → Email/Sms/WhatsApp/Push)
- EF Core + PostgreSQL with one migration
- 3 stub worker projects (`Console.WriteLine("Hello, World!")`)
- Docker Compose with PostgreSQL + RabbitMQ + API + workers
- No message pipeline, no auth, no real-time, no tests

---

## Phase 0: Clean Architecture + DDD Restructuring

### New Project Structure
```
src/
  NotificationHub.Domain/            # No external deps
  NotificationHub.Application/       # Depends on Domain
  NotificationHub.Infrastructure/    # Depends on Application + Domain
  NotificationHub.Api/               # Depends on Application + Infrastructure
  Workers/Email, Sms, WhatsApp/      # Depend on Infrastructure
```

### Domain Layer
- **Base classes**: `Entity`, `AggregateRoot`, `ValueObject`, `IDomainEvent`
- **Value Objects**: `EmailAddress`, `PhoneNumber`, `NotificationContent`
- **Notification Aggregate**: Single `Notification` entity (replaces TPH hierarchy) + `DeliveryAttempt` child entity + domain events
- **Recipient Aggregate** (NEW): `Recipient` + `NotificationPreference` (per-channel opt-in/out)
- **NotificationTemplate Aggregate** (NEW): Template with `{{variable}}` substitution
- **New enum**: `NotificationPriority` (Low, Normal, High, Critical)
- Domain repository interfaces

### Application Layer (CQRS + MediatR)
- Commands: `SendNotification`, `MarkNotificationRead`, `SendBatchNotifications`, `CreateRecipient`, `CreateTemplate`
- Queries: `GetNotificationById`, `GetNotifications` (paged/filtered), `GetRecipientById`, `GetTemplates`
- MediatR pipeline behaviors: `ValidationBehavior`, `LoggingBehavior`
- `Result<T>` pattern, `PagedResult<T>`
- FluentValidation validators per command

### Infrastructure Layer
- EF Core DbContext + entity configurations + repositories
- `UnitOfWork` that dispatches domain events on SaveChanges
- MassTransit publisher + consumers
- SignalR hub
- JWT authentication service
- Serilog, Polly resilience

### Database
- Delete existing migration, create fresh from new model
- Tables: `Notifications`, `DeliveryAttempts`, `Recipients`, `NotificationPreferences`, `NotificationTemplates`

---

## Phase 1: Core Message Pipeline

- **MassTransit + RabbitMQ**: Message contracts, per-channel queues, DLQ, exponential backoff retry
- **Worker Services**: MassTransit consumers with simulated delivery (configurable success/failure rate)
- **SignalR Hub**: `/hubs/notifications`, group-based connections, real-time status broadcasts
- **Polly Resilience**: Retry + circuit breaker in workers

---

## Phase 2: API Maturity

- **JWT Auth**: Login endpoint, roles (Admin/Service/User), `[Authorize]` on controllers
- **Pagination & Filtering**: Page/size/status/channel/recipient/dateRange/sortBy params
- **FluentValidation + Problem Details**: RFC 7807 error responses, correlation ID propagation
- **Rate Limiting**: Sliding window 100 req/min per IP, `429` + `Retry-After`

---

## Phase 3: Observability

- **Serilog**: Console + Seq sinks, enriched with CorrelationId/MachineName/ThreadId
- **Health Checks**: `/health/live` (liveness) + `/health/ready` (PostgreSQL + RabbitMQ)
- **Seq** added to Docker Compose for log aggregation

---

## Phase 4: Advanced Features

- **Scheduling**: `ScheduledAt` field + `BackgroundService` that polls and publishes due notifications
- **Templates**: CRUD API, `{{variable}}` substitution via `template.Render(vars)`
- **Batch Notifications**: `POST /api/notifications/batch` for multi-recipient sends
- **Preferences**: Per-user opt-in/out per channel, checked before sending

---

## Phase 5: Testing

- **Domain Tests**: Aggregate state transitions, value object validation, template rendering
- **Application Tests**: Command handlers (mocked repos), validator rules
- **Integration Tests**: `WebApplicationFactory` + Testcontainers (PostgreSQL + RabbitMQ)

---

## Phase 6: CI/CD + Polish

- **GitHub Actions**: Build + test on PR, `dotnet format` check
- **Dockerfile Updates**: Multi-project COPY for Clean Architecture structure

---

## Execution Order

| # | Phase | Milestone |
|---|-------|-----------|
| 1 | Clean Architecture + DDD | v0.1 |
| 2 | Message Pipeline (RabbitMQ + Workers + SignalR + Polly) | v0.2 |
| 3 | API Maturity (JWT + Pagination + Validation + Rate Limiting) | v0.3 |
| 4 | Observability (Serilog + Health Checks + Seq) | v0.3 |
| 5 | Advanced Features (Scheduling + Templates + Batch + Preferences) | v0.4 |
| 6 | Testing (Unit + Integration) | v0.5 |
| 7 | CI/CD + Polish | v0.6 |
