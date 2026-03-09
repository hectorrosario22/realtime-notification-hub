# Realtime Notification Hub

An event-driven notification platform built with .NET 10, Clean Architecture, RabbitMQ, and SignalR. Supports real-time push notifications and async multi-channel delivery (Email, SMS, WhatsApp) through dedicated worker services.

[![.NET Build and Test](https://github.com/hectorrosario22/realtime-notification-hub/actions/workflows/dotnet-build-and-test.yml/badge.svg)](https://github.com/hectorrosario22/realtime-notification-hub/actions/workflows/dotnet-build-and-test.yml)
![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)
![SignalR](https://img.shields.io/badge/SignalR-Real--time-512BD4)
![RabbitMQ](https://img.shields.io/badge/RabbitMQ-4.x-FF6600?logo=rabbitmq)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql)
![Architecture](https://img.shields.io/badge/Clean%20Architecture-.NET%2010-0A7B83)

---

## About

This project is inspired by a notification module I built for an insurance company in Mexico. I recreated it as the centerpiece of my portfolio to demonstrate senior-level backend patterns: async messaging, concurrent worker processing, real-time communication, and maintainability-focused design.

The focus is entirely on the backend. The web client is intentionally minimal — it exists so that anyone (including recruiters) can interact with the system without needing Swagger, Postman, or any other tool.

### Supported channels

| Channel | Mechanism | Flow |
|---------|-----------|------|
| **Push** | SignalR | API → WebSocket → browser (real-time) |
| **Email** | RabbitMQ + Worker | API → queue → worker → retries → DB |
| **SMS** | RabbitMQ + Worker | API → queue → worker → retries → DB |
| **WhatsApp** | RabbitMQ + Worker | API → queue → worker → retries → DB |

Async channels support up to **3 retry attempts**. Failures are persisted in the database and queryable through the API.

---

## Architecture

### Delivery flows

```
┌─────────────────────────────────────────────────────┐
│  PUSH (real-time)                                   │
│                                                     │
│  Client ──HTTP──▶ API ──SignalR──▶ Browser          │
└─────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────┐
│  EMAIL / SMS / WHATSAPP (async)                     │
│                                                     │
│  Client ──HTTP──▶ API ──publish──▶ RabbitMQ         │
│                    │                    │           │
│                    ◀── 202 Accepted     │           │
│                                         ▼           │
│                               Worker (consume)      │
│                               │  ├─ attempt 1      │
│                               │  ├─ attempt 2      │
│                               │  └─ attempt 3      │
│                               │                    │
│                               ▼                    │
│                           PostgreSQL               │
│                     (status + failures)            │
└─────────────────────────────────────────────────────┘
```

### Solution structure (Clean Architecture)

```
src/
├── NotificationHub.Domain/          # Entities, enums, value objects — no external dependencies
├── NotificationHub.Application/     # Interfaces, MassTransit contracts, DTOs, services
├── NotificationHub.Infrastructure/  # EF Core, MassTransit/RabbitMQ, service implementations
├── NotificationHub.Api/             # ASP.NET Core Minimal API + SignalR hub
└── Workers/
    ├── NotificationHub.Worker.Email/
    ├── NotificationHub.Worker.Sms/
    └── NotificationHub.Worker.WhatsApp/

tests/
├── NotificationHub.Domain.Tests/
├── NotificationHub.Application.Tests/
├── NotificationHub.Infrastructure.Tests/
└── NotificationHub.Api.Tests/       # Integration tests with WebApplicationFactory
```

### Layer dependencies

```
Domain ◀── Application ◀── Infrastructure ◀── Api
                                    ▲
                                 Workers
```

---

## API Endpoints

```
POST /api/notifications/push        Send a real-time push notification via SignalR
POST /api/notifications/email       Enqueue an email notification
POST /api/notifications/sms         Enqueue an SMS notification
POST /api/notifications/whatsapp    Enqueue a WhatsApp notification

GET  /api/notifications/{id}        Get the status of a notification by ID
GET  /api/notifications/failed      List notifications that exhausted all retry attempts
```

WebSocket: `ws://localhost:5000/hubs/notifications`

Interactive docs: [http://localhost:5000/scalar](http://localhost:5000/scalar)

---

## Services (compose.yml)

| Service | Image / Build | Port |
|---------|---------------|------|
| `api` | Build from `src/NotificationHub.Api/` | `5000` |
| `worker-email` | Build from `src/Workers/.../Email/` | — |
| `worker-sms` | Build from `src/Workers/.../Sms/` | — |
| `worker-whatsapp` | Build from `src/Workers/.../WhatsApp/` | — |
| `postgres` | `postgres:16-alpine` | `5432` |
| `rabbitmq` | `rabbitmq:4-management-alpine` | `5672` / `15672` |

---

## Quick Start

**Requirements:** [Podman](https://podman.io/) or [Docker](https://www.docker.com/products/docker-desktop/) with Compose.

```bash
git clone https://github.com/hectorrosario22/realtime-notification-hub.git
cd realtime-notification-hub

# Start all services
podman compose up -d

# Follow logs
podman compose logs -f

# Stop
podman compose down
```

Once running:
- **API + interactive docs:** http://localhost:5000/scalar
- **RabbitMQ Management:** http://localhost:15672 (`guest` / `guest`)

### Local development (without containerizing the app)

```bash
# Start infrastructure only
podman compose up postgres rabbitmq -d

# Run the API
cd src/NotificationHub.Api
dotnet run

# Run workers (each in a separate terminal)
cd src/Workers/NotificationHub.Worker.Email
dotnet run
```

---

## Tech Stack

**Backend**
- .NET 10 — ASP.NET Core Minimal APIs
- SignalR — WebSocket-based real-time push
- MassTransit 9 — RabbitMQ abstraction (publish, consume, retry, DLQ)
- Entity Framework Core 10 — ORM with PostgreSQL
- Polly — resilience and retry policies
- Serilog — structured logging

**Infrastructure**
- PostgreSQL 16 — persistent notification state
- RabbitMQ 4 — message broker, one queue per channel
- Podman / Docker — container runtime

**Testing**
- xUnit — unit tests per layer
- `Microsoft.AspNetCore.Mvc.Testing` — API integration tests

---

## Production Considerations

This is a portfolio/demonstration project. For a production deployment, the following would be added:

- Authentication and authorization (JWT / OAuth2)
- Per-channel and per-client rate limiting
- Recipient-specific notification routing
- Observability: OpenTelemetry, distributed tracing, metrics
- Secret management (Azure Key Vault, AWS Secrets Manager)
- API versioning
- TLS across all services
- Horizontal scaling strategies for workers

---

## Author

**Héctor Rosario** — Backend Engineer (.NET)

- LinkedIn: [hector-rosario](https://www.linkedin.com/in/hector-rosario)
- GitHub: [@hectorrosario22](https://github.com/hectorrosario22)
- Email: [me@hrosario.dev](mailto:me@hrosario.dev)

---

MIT License
