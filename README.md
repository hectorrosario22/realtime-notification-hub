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

- **Real-time Updates**: SignalR WebSocket connection for instant push notifications
- **Async Processing**: RabbitMQ message queue with dedicated worker services
- **Event-Driven Architecture**: Decoupled services communicating via message bus
- **Clean Architecture**: Separation of concerns with layered project structure
- **Container Support**: Complete containerization with Podman/Docker Compose
- **Retry Policies**: Resilient message processing with automatic retries
- **Dead Letter Queue**: Failed message handling and monitoring
- **Health Checks**: API health monitoring endpoints
- **Structured Logging**: Centralized logging with Serilog

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

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 18+](https://nodejs.org/) with pnpm (`npm install -g pnpm`)
- [Podman Desktop](https://podman.io/) or [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### Quick Start (Recommended)

This project is designed to run with Podman Compose. All services (API, workers, database, message queue, and frontend) are orchestrated together.

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

3. **Access the application**
   - Frontend: http://localhost:3000
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

#### Backend (.NET API + Workers)
```bash
cd src/NotificationHub.Api
dotnet restore
dotnet run
```

#### Frontend (React)
```bash
cd client
pnpm install
pnpm dev
```

> **Note**: For the best demonstration experience, use `podman compose up` to run everything together.

## 📁 Project Structure
```
realtime-notification-hub/
├── src/
│   ├── NotificationHub.Api/              # REST API + SignalR Hub
│   ├── NotificationHub.Core/             # Domain models & interfaces
│   ├── NotificationHub.Infrastructure/   # Data access & messaging
│   └── NotificationHub.Workers/          # Background workers
│       ├── NotificationHub.Workers.Email/
│       ├── NotificationHub.Workers.Sms/
│       └── NotificationHub.Workers.WhatsApp/
├── client/                                # React frontend
├── tests/                                 # Unit & integration tests
│   ├── NotificationHub.Api.Tests/
│   └── NotificationHub.Core.Tests/
├── compose.yml                            # Podman/Docker orchestration
└── README.md
```

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
POST   /api/notifications/send          # Send notification to queue
GET    /api/notifications               # Get all notifications
PATCH  /api/notifications/{id}/read     # Mark as read
```

### SignalR Hub
```
/hubs/notifications                      # WebSocket endpoint
```

### Health
```http
GET    /health                           # Health check endpoint
```

## 🎯 Use Cases Demonstrated

- **Async/Await Patterns**: Proper async implementation throughout
- **Dependency Injection**: Clean DI configuration
- **Repository Pattern**: Data access abstraction
- **CQRS Principles**: Command/Query separation
- **Message-Driven Architecture**: Event-based communication
- **Worker Services**: Background processing
- **Real-time Communication**: SignalR WebSockets
- **Containerization**: Multi-container orchestration

## 🧪 Testing
```bash
# Run all tests
dotnet test

# Run specific test project
cd tests/NotificationHub.Api.Tests
dotnet test

cd tests/NotificationHub.Core.Tests
dotnet test
```

## 📊 Monitoring

- **RabbitMQ Management UI**: http://localhost:15672
- **API Health Check**: http://localhost:5000/health
- **Structured Logs**: Console output with Serilog

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
