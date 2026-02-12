# 🔔 Realtime Notification Hub

A scalable, multi-channel notification system demonstrating modern software architecture patterns with real-time communication capabilities.

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)
![React](https://img.shields.io/badge/React-18-61DAFB?logo=react)
![SignalR](https://img.shields.io/badge/SignalR-Real--time-512BD4)
![RabbitMQ](https://img.shields.io/badge/RabbitMQ-Messaging-FF6600?logo=rabbitmq)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql)

## 📋 Overview

This project showcases a production-ready notification system that supports multiple delivery channels:
- 📧 **Email** - Asynchronous email delivery
- 📱 **SMS** - Text message notifications
- 💬 **WhatsApp** - WhatsApp messaging
- 🔔 **Push** - Real-time browser notifications via SignalR

Built to demonstrate enterprise-level architectural patterns including message queuing, worker services, real-time communication, and microservices design.

## ✨ Key Features

### Implemented Today

- **REST API**: CRUD-style notification endpoints with Swagger/OpenAPI.
- **Data Layer**: EF Core + PostgreSQL with migrations.
- **Notification Model**: Multi-channel notifications using TPH inheritance.
- **Container Support**: `compose.yml` orchestration for API, workers, PostgreSQL, and RabbitMQ.

### Planned in Roadmap

- **Real-time Updates**: SignalR hub and client group broadcasting.
- **Async Processing**: Worker consumers with broker-driven processing.
- **Reliability**: Outbox, idempotency, retry policies, and DLQ/replay.
- **Security and Operability**: JWT auth, rate limiting, health checks, and observability.

## 🏗️ Architecture
```
┌─────────────────┐
│   React App     │
│  (Vite + TS)    │
└────────┬────────┘
         │ HTTP & WebSocket
         ▼
┌─────────────────┐
│ Notifications   │
│     API         │
│  (.NET 10 API)  │
└────────┬────────┘
         │ Publish
         ▼
┌─────────────────┐      ┌──────────────────┐
│   RabbitMQ      │─────▶│  Worker Services │
│  Message Queue  │      │  (Console Apps)  │
│                 │      │  • EmailWorker   │
│  • Email Queue  │      │  • SmsWorker     │
│  • SMS Queue    │      │  • WhatsAppWorker│
│  • WhatsApp Q   │      └──────────────────┘
└─────────────────┘
         │
         ▼
┌─────────────────┐
│   PostgreSQL    │
│   Database      │
└─────────────────┘
```

Current and target architecture details are documented in:

- `docs/architecture.md`
- `docs/adr/README.md`

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 18+](https://nodejs.org/) with pnpm (`npm install -g pnpm`)
- [Podman Desktop](https://podman.io/) or [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### Quick Start (Recommended)

This project is designed to run with Podman Compose. Current compose orchestration includes API, workers, database, and message queue.

1. **Clone the repository**
```bash
   git clone https://github.com/hectorrosario22/realtime-notification-hub.git
   cd realtime-notification-hub
```

2. **Start all services**
```bash
   podman compose up -d
```
   
   Or with Docker:
```bash
   docker compose up -d
```

3. **Access running services**
   - API: http://localhost:5000
   - RabbitMQ Management: http://localhost:15672 (guest/guest)

4. **Stop all services**
```bash
   podman compose down
```

### Alternative: Local Development

If you need to run services individually for development:

#### Infrastructure only (PostgreSQL + RabbitMQ)
```bash
podman compose up postgres rabbitmq -d
```

##### Backend (.NET API)
```bash
cd src/NotificationHub.Api
dotnet restore
dotnet run
```

Access the API:
- API: http://localhost:5000
- Swagger UI: http://localhost:5000/swagger

#### Frontend (React SPA, planned)
The SPA workspace is reserved at `src/NotificationHub.Web/` and will be added in upcoming phases.

> **Note**: For the best demonstration experience, use `podman compose up` to run everything together.

## 📁 Project Structure

**Simplified for Portfolio** - All backend logic consolidated in one project for easier understanding:

```
realtime-notification-hub/
├── src/
│   ├── NotificationHub.Api/              # Main API project
│   │   ├── Controllers/                  # REST API endpoints
│   │   ├── Entities/                     # Domain models
│   │   ├── Enums/                        # Notification types & statuses
│   │   ├── Interfaces/                   # Repository contracts
│   │   ├── Repositories/                 # Data access implementations
│   │   ├── Data/                         # EF Core DbContext & configurations
│   │   ├── DTOs/                         # Request/Response models
│   │   └── Hubs/                         # SignalR hubs (to be implemented)
│   ├── NotificationHub.Web/              # SPA workspace (planned React app)
│   └── Workers/                          # Background workers
│       ├── NotificationHub.Workers.Email/
│       ├── NotificationHub.Workers.Sms/
│       └── NotificationHub.Workers.WhatsApp/
├── tests/                                 # Automated tests (xUnit, integration)
├── compose.yml                            # Podman/Docker orchestration
└── README.md
```

> **Note**: This architecture prioritizes simplicity and clarity for a portfolio project. Production apps may benefit from additional layers.

## 🧭 Architecture Decision Records

Architectural decisions are tracked as ADRs in `docs/adr/` to keep design intent explicit and avoid documentation drift.

- `ADR-001`: Layering and dependency rule
- `ADR-002`: Delivery semantics (at-least-once)
- `ADR-003`: Consistency strategy (Outbox + idempotency)

## 🗺️ Roadmap Index

Roadmap tracking is managed from this README, with detailed plans in `docs/plans/`.

- [ ] Fase 1 - Foundation and Architecture Baseline  
  Plan detallado: `docs/plans/2026-02-12-backend-senior-roadmap-design.md#fase-1---foundation-and-architectural-baseline-weeks-1-3`
- [ ] Fase 2 - Reliable Messaging Core  
  Plan detallado: `docs/plans/2026-02-12-backend-senior-roadmap-design.md#fase-2---reliable-messaging-core-weeks-4-7`
- [ ] Fase 3 - Resilience and Failure Recovery  
  Plan detallado: `docs/plans/2026-02-12-backend-senior-roadmap-design.md#fase-3---resilience-and-failure-recovery-weeks-8-10`
- [ ] Fase 4 - Scale and Operability Maturity  
  Plan detallado: `docs/plans/2026-02-12-backend-senior-roadmap-design.md#fase-4---scale-and-operability-maturity-weeks-11-13`
- [ ] Fase 5 - Portfolio Packaging and Interview Readiness  
  Plan detallado: `docs/plans/2026-02-12-backend-senior-roadmap-design.md#fase-5---portfolio-packaging-and-interview-readiness-weeks-14-16`

## 🔧 Technology Stack

### Backend
- **.NET 10** - Web API framework
- **SignalR** - Real-time WebSocket communication
- **Entity Framework Core** - ORM for PostgreSQL
- **RabbitMQ** - Message broker
- **MassTransit** - Distributed application framework
- **Polly** - Resilience and retry policies
- **Serilog** - Structured logging

### Frontend
- **React 18** - UI library
- **TypeScript** - Type-safe JavaScript
- **Vite** - Build tool
- **pnpm** - Fast, disk space efficient package manager
- **TanStack Query** - Data fetching & caching
- **SignalR Client** - WebSocket client

### Infrastructure
- **PostgreSQL 16** - Relational database
- **RabbitMQ** - Message queue
- **Podman** - Container engine

## 📝 API Endpoints

### Notifications
```http
POST   /api/notifications/email         # Send email notification
POST   /api/notifications/sms           # Send SMS notification
POST   /api/notifications/whatsapp      # Send WhatsApp notification
POST   /api/notifications/push          # Send push notification
GET    /api/notifications               # Get all notifications
GET    /api/notifications/{id}          # Get notification by ID
PATCH  /api/notifications/{id}/read     # Mark as read
```

### Documentation
```
/swagger                                 # Swagger UI (Development only)
/openapi/v1.json                        # OpenAPI specification
```

### SignalR Hub (To be implemented)
```
/hubs/notifications                      # WebSocket endpoint
```

## 🎯 Patterns & Practices Demonstrated

- **Async/Await Patterns**: Proper async implementation throughout
- **Dependency Injection**: Built-in .NET DI container
- **Repository Pattern**: Data access abstraction layer
- **Entity Framework Core**: Code-first database design with configurations
- **REST API Design**: RESTful endpoints with proper HTTP semantics
- **DTOs**: Request/Response models separate from entities
- **OpenAPI/Swagger**: Auto-generated API documentation
- **CORS Configuration**: Cross-origin resource sharing for frontend
- **Primary Constructors**: Modern C# 12 syntax (where applicable)

## 🧪 Testing

Currently using in-memory database for development. Testing infrastructure to be added.

```bash
# Build the solution
dotnet build

# Run the API
cd src/NotificationHub.Api
dotnet run
```

## 📊 Development Tools

- **Swagger UI**: http://localhost:5000/swagger (interactive API documentation)
- **OpenAPI Spec**: http://localhost:5000/openapi/v1.json
- **In-Memory Database**: Quick development without PostgreSQL setup
- **RabbitMQ Management** (when using containers): http://localhost:15672

## 🚀 Production Considerations

This is a demonstration project. For production deployment, consider:

- ✅ Authentication & Authorization (JWT, OAuth2)
- ✅ Rate limiting and throttling
- ✅ User-specific notification routing
- ✅ Persistent message storage
- ✅ Monitoring & alerting (Application Insights, Prometheus)
- ✅ API versioning
- ✅ HTTPS/TLS everywhere
- ✅ Secret management (Azure Key Vault, AWS Secrets Manager)
- ✅ Horizontal scaling strategies
- ✅ Database migrations management

## 📚 Learning Resources

This project demonstrates concepts from:
- [.NET Microservices Architecture](https://dotnet.microsoft.com/learn/aspnet/microservices-architecture)
- [SignalR Documentation](https://docs.microsoft.com/en-us/aspnet/core/signalr)
- [RabbitMQ Tutorials](https://www.rabbitmq.com/getstarted.html)
- [MassTransit Documentation](https://masstransit.io/)

## 🤝 Contributing

This is a portfolio/demonstration project, but feedback and suggestions are welcome!

## 📄 License

MIT License - feel free to use this project for learning purposes.

## 👤 Author

**Héctor Rosario**
- LinkedIn: [hector-rosario](https://www.linkedin.com/in/hector-rosario)
- GitHub: [@hectorrosario22](https://github.com/hectorrosario22)
- Email: [me@hrosario.dev](mailto:me@hrosario.dev)

---

⭐ If you find this project helpful, please give it a star!
