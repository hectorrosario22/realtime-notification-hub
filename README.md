# 🔔 Realtime Notification Hub

Realtime Notification Hub is a scalable, event-driven notification platform built with .NET 10 microservices, RabbitMQ messaging, and SignalR. Supports real-time push notifications plus async multi-channel delivery via workers (Email, SMS, WhatsApp) with a PostgreSQL backend.

[![.NET Build and Test](https://github.com/hectorrosario22/realtime-notification-hub/actions/workflows/dotnet-build-and-test.yml/badge.svg)](https://github.com/hectorrosario22/realtime-notification-hub/actions/workflows/dotnet-build-and-test.yml)

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)
![SignalR](https://img.shields.io/badge/SignalR-Real--time-512BD4)
![RabbitMQ](https://img.shields.io/badge/RabbitMQ-Messaging-FF6600?logo=rabbitmq)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql)
![Architecture](https://img.shields.io/badge/Focus-Backend%20Architecture-0A7B83)

## 📋 Overview

This project showcases a backend-first, production-oriented notification platform that supports multiple delivery channels:
- 📧 **Email** - Asynchronous email delivery via dedicated worker
- 📱 **SMS** - Asynchronous SMS delivery via dedicated worker
- 💬 **WhatsApp** - Asynchronous WhatsApp delivery via dedicated worker
- 🔔 **Push** - Real-time notifications via SignalR

Built to demonstrate senior-level architectural patterns including event-driven messaging, worker-based processing, real-time communication, and microservices-oriented backend design.

> The React client is intentionally minimal and acts only as a bridge to showcase API and realtime capabilities.

## ✨ Key Features

- **Realtime Push**: SignalR hub broadcasting notification events to connected clients.
- **Async Multi-Channel Delivery**: RabbitMQ-backed worker processing for Email, SMS, and WhatsApp.
- **Scalable Backend Design**: .NET 10 microservices-style architecture with decoupled processing flows.
- **Persistent State**: PostgreSQL as system-of-record for notification lifecycle and delivery status.
- **API-First Platform**: REST endpoints with OpenAPI/Swagger documentation.
- **Containerized Runtime**: `compose.yml` orchestration for API, workers, PostgreSQL, and RabbitMQ.

## 🏗️ Architecture
```text
┌─────────────────┐
│  Thin Client    │
│ (React / UI)    │
└────────┬────────┘
         │ HTTP & WebSocket
         ▼
┌─────────────────┐
│ Notifications   │
│      API        │
│  (.NET 10 API)  │
└────────┬────────┘
         │ Publish Events
         ▼
┌─────────────────┐      ┌──────────────────┐
│   RabbitMQ      │─────▶│  Worker Services │
│ Message Broker  │      │  • Email Worker  │
│                 │      │  • SMS Worker    │
│  • Email Queue  │      │  • WhatsApp Work.│
│  • SMS Queue    │      └──────────────────┘
│  • WhatsApp Q   │
└────────┬────────┘
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
- [Node.js 18+](https://nodejs.org/) with pnpm (`npm install -g pnpm`) for the thin client
- [Podman Desktop](https://podman.io/) or [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### Quick Start (Recommended)

This project is designed to run with Podman Compose. Compose orchestration includes API, workers, database, and message broker.

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
- Swagger UI: http://localhost:5000/swagger
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

#### Backend (.NET API)
```bash
cd src/NotificationHub.Api
dotnet restore
dotnet run
```

> **Note**: For the best end-to-end demonstration, use `podman compose up` to run everything together.

## 🔧 Technology Stack

### Backend
- **.NET 10** - Web API framework
- **SignalR** - Real-time WebSocket communication
- **Entity Framework Core** - ORM for PostgreSQL
- **RabbitMQ** - Message broker
- **MassTransit** - Distributed application framework
- **Polly** - Resilience and retry policies
- **Serilog** - Structured logging

### Supporting Client
- **React 18** - Thin UI bridge for API and realtime visualization
- **TypeScript** - Type-safe JavaScript
- **Vite** - Build tool

### Infrastructure
- **PostgreSQL 16** - Relational database
- **RabbitMQ** - Message queue
- **Podman / Docker** - Container runtime

## 🧪 Testing

```bash
# Build the solution
dotnet build
```

## 🚀 Production Considerations

This is a demonstration project. For production deployment, consider:

- ✅ Authentication & Authorization (JWT, OAuth2)
- ✅ Rate limiting and throttling
- ✅ User-specific notification routing
- ✅ Persistent message storage hardening
- ✅ Monitoring & alerting (Application Insights, Prometheus, OpenTelemetry)
- ✅ API versioning
- ✅ HTTPS/TLS everywhere
- ✅ Secret management (Azure Key Vault, AWS Secrets Manager)
- ✅ Horizontal scaling strategies
- ✅ Database migration discipline

## 📚 Learning Resources

This project demonstrates concepts from:
- [.NET Microservices Architecture](https://dotnet.microsoft.com/learn/aspnet/microservices-architecture)
- [SignalR Documentation](https://learn.microsoft.com/aspnet/core/signalr)
- [RabbitMQ Tutorials](https://www.rabbitmq.com/getstarted.html)
- [MassTransit Documentation](https://masstransit.io/)

## 🤝 Contributing

This is a portfolio/demonstration project, but feedback and suggestions are welcome.

## 📄 License

MIT License - feel free to use this project for learning purposes and architecture reference.

## 👤 Author

**Héctor Rosario**
- LinkedIn: [hector-rosario](https://www.linkedin.com/in/hector-rosario)
- GitHub: [@hectorrosario22](https://github.com/hectorrosario22)
- Email: [me@hrosario.dev](mailto:me@hrosario.dev)

---

⭐ If you find this project helpful, please give it a star!
