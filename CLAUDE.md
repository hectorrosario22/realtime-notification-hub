# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

### Build

```bash
# Build entire solution
dotnet build NotificationHub.slnx

# Build a specific project
dotnet build src/NotificationHub.Api/NotificationHub.Api.csproj
```

### Run (local dev — infrastructure must be running first)

```bash
# Start only infrastructure services (Postgres, RabbitMQ, Seq)
podman compose up postgres rabbitmq seq -d

# Run the API
dotnet run --project src/NotificationHub.Api

# Run a worker (each in its own terminal)
dotnet run --project src/Workers/NotificationHub.Worker.Email
dotnet run --project src/Workers/NotificationHub.Worker.Sms
dotnet run --project src/Workers/NotificationHub.Worker.WhatsApp
```

### Run all services via compose

```bash
podman compose up -d
podman compose down
```

### Tests

```bash
# Run all tests
dotnet test NotificationHub.slnx

# Run tests for a specific project
dotnet test tests/NotificationHub.Domain.Tests
dotnet test tests/NotificationHub.Api.Tests

# Run a specific test class or method
dotnet test tests/NotificationHub.Domain.Tests --filter "FullyQualifiedName~NotificationStateTests"
dotnet test tests/NotificationHub.Api.Tests --filter "FullyQualifiedName~PushNotificationEndpointTests"
```

### EF Core migrations

```bash
# Add a migration (run from solution root)
dotnet ef migrations add <MigrationName> \
  --project src/NotificationHub.Infrastructure \
  --startup-project src/NotificationHub.Api

# Apply migrations manually (auto-applied in Development on startup)
dotnet ef database update \
  --project src/NotificationHub.Infrastructure \
  --startup-project src/NotificationHub.Api
```

## Architecture

This is a **Clean Architecture** .NET 10 solution with four library layers and three worker services:

```
Domain ◀── Application ◀── Infrastructure ◀── Api
                                    ▲
                                 Workers (Email, Sms, WhatsApp)
```

### Layer responsibilities

- **Domain** (`src/NotificationHub.Domain/`) — Entities, enums, value objects, domain events, domain exceptions. Zero external dependencies. `Notification` is the aggregate root; `EmailNotification`, `SmsNotification`, `WhatsAppNotification`, `PushNotification` are subtypes stored via TPH (Table-Per-Hierarchy).
- **Application** (`src/NotificationHub.Application/`) — Interfaces (`INotificationService`, `INotificationRepository`, `IMessagePublisher`, `IPushNotificationSender`), MassTransit message contracts (`EmailNotificationMessage`, `NotificationStatusUpdate`, etc.), DTOs, and the `NotificationService` orchestrator.
- **Infrastructure** (`src/NotificationHub.Infrastructure/`) — EF Core `NotificationDbContext` (Npgsql), `NotificationRepository`, `MassTransitMessagePublisher`, and the DI extension `AddInfrastructure()`.
- **Api** (`src/NotificationHub.Api/`) — ASP.NET Core Minimal APIs wired via `MapNotificationEndpoints()`, SignalR hub at `/hubs/notifications` (`PushNotificationHub`), `NotificationStatusUpdateConsumer` (listens for worker status updates, writes to DB and broadcasts via SignalR), and `SignalRPushNotificationSender`.

### Workers

Each worker (`src/Workers/NotificationHub.Worker.{Email|Sms|WhatsApp}/`) is a `Microsoft.NET.Sdk.Worker` project that **references the Api project** to share contracts and entities. Workers consume from dedicated RabbitMQ queues (`email-notifications`, etc.) using MassTransit, apply exponential retry (3 attempts, 5s–1min), then publish a `NotificationStatusUpdate` back to RabbitMQ for the API consumer.

The **Email** worker uses the Maileroo HTTP API (`https://smtp.maileroo.com/api/v2/`) configured via `Maileroo:ApiKey` and `Maileroo:DefaultFromAddress`.

### Message flow (async channels)

```
POST /api/notifications/email
  → NotificationService persists entity (Pending) + publishes EmailNotificationMessage
  → Worker.Email consumes → calls Maileroo → publishes NotificationStatusUpdate
  → API NotificationStatusUpdateConsumer updates DB status + SignalR broadcast
```

### Infrastructure services (compose.yml)

| Service | Port | Notes |
|---|---|---|
| PostgreSQL 16 | `5432` | DB: `notification_hub`, user: `postgres`/`postgres` |
| RabbitMQ 4 | `5672` / `15672` | Management UI at `:15672`, user: `guest`/`guest` |
| Seq | `5341` / `8081` | Structured log UI at `http://localhost:8081` |
| API | `5000` | Scalar docs at `http://localhost:5000/scalar` |

### Testing approach

- **Domain tests** — pure unit tests, no mocks needed.
- **API integration tests** — `NotificationHubWebAppFactory : WebApplicationFactory<Program>` replaces EF Core (Npgsql) with an in-memory provider per test run. Uses `Testing` environment (`appsettings.Testing.json`). MassTransit is not replaced — tests that need it must handle it separately.
- **Application/Infrastructure tests** — currently scaffolded (placeholder files).

### Key configuration

`appsettings.json` (container defaults — override for local dev via `appsettings.Development.json` or env vars):

```json
{
  "ConnectionStrings": { "DefaultConnection": "Host=postgres;Port=5432;Database=notification_hub;Username=postgres;Password=postgres" },
  "RabbitMQ": { "Host": "rabbitmq", "VirtualHost": "/", "Username": "guest", "Password": "guest" },
  "Cors": { "AllowedOrigins": ["http://localhost:3000"] }
}
```

For local development override `Host=postgres` → `Host=localhost`.

### Important implementation notes

- SMS and WhatsApp endpoints in `NotificationEndpoints.cs` are currently stubbed (`TODO` comments). Only Push and Email are fully implemented.
- DB migrations run automatically in `Development` environment on startup (`db.Database.MigrateAsync()`).
- Worker Dockerfiles must `COPY` both the `NotificationHub.Api/` and the worker project directories because workers reference the Api project.
- CORS `AllowCredentials()` is required for SignalR — do not remove it when modifying CORS policy.
- Rate limiting uses the policy-based `RateLimitPartition` API (not the removed `AddSlidingWindowLimiter`/`AddFixedWindowLimiter` extension methods from older .NET versions).
