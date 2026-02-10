# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Realtime Notification Hub is a multi-channel notification system built with .NET 10 demonstrating event-driven architecture, message queuing, and real-time communication patterns. The system supports Email, SMS, WhatsApp, and Push notifications through a unified API with asynchronous worker processing.

**Current Status**: Basic REST API and entities implemented. SignalR hubs, RabbitMQ integration, and worker services need to be built.

## Architecture

### Project Structure (Simplified for Portfolio)
- **NotificationHub.Api**: Main ASP.NET Core Web API containing everything (entities, data access, repositories, controllers, SignalR hub)
- **NotificationHub.Workers.{Email|Sms|WhatsApp}**: Background console apps consuming from RabbitMQ queues

### Technology Stack
- **.NET 10** with nullable reference types enabled
- **SignalR**: Real-time WebSocket push notifications
- **RabbitMQ + MassTransit**: Message broker and distributed app framework
- **Entity Framework Core**: PostgreSQL ORM
- **Polly**: Retry/resilience policies
- **Serilog**: Structured logging
- **xUnit**: Testing framework
- **Podman/Docker Compose**: Container orchestration

### Message Flow
1. API receives notification request → validates → publishes to RabbitMQ queue
2. Worker service consumes message → processes delivery (email/SMS/WhatsApp) → updates DB
3. SignalR hub broadcasts real-time updates to connected clients
4. Failed messages → Dead Letter Queue for monitoring/retry

## Development Commands

### Build & Restore
```bash
dotnet restore                          # Restore all project dependencies
dotnet build                            # Build entire solution
dotnet build src/NotificationHub.Api/   # Build specific project
```

### Run Services
```bash
# Recommended: Run everything with Podman
podman compose up -d                    # Start all services (API, workers, DB, RabbitMQ, frontend)
podman compose down                     # Stop all services

# Alternative: Run infrastructure only, then run .NET services locally
podman compose up postgres rabbitmq -d  # Start just PostgreSQL and RabbitMQ

# Run API locally
cd src/NotificationHub.Api
dotnet run

# Run individual worker
cd src/Workers/NotificationHub.Workers.Email
dotnet run
```

### Testing
```bash
dotnet test                                           # Run all tests
dotnet test tests/NotificationHub.Api.Tests/          # Run API tests
dotnet test tests/NotificationHub.Core.Tests/         # Run Core tests
dotnet test --filter "FullyQualifiedName~EmailService" # Run specific test class
```

### Access Points (when using podman compose)
- Frontend: http://localhost:3000
- API: http://localhost:5000
- RabbitMQ Management UI: http://localhost:15672 (guest/guest)

## Project Structure

The Api project contains:
- **Entities/**: Domain models (Notification)
- **Enums/**: Enumerations (NotificationChannel, NotificationStatus, NotificationType)
- **Interfaces/**: Repository contracts (INotificationRepository)
- **Repositories/**: Repository implementations (NotificationRepository)
- **Data/**: EF Core DbContext and entity configurations
- **Controllers/**: REST API endpoints
- **DTOs/**: Request/Response models
- **Hubs/**: SignalR hubs (to be implemented)

Workers will reference the Api project when implemented to access shared entities and enums.

## Key Implementation Notes

### Configuration
- Connection strings and RabbitMQ settings belong in `appsettings.json` (Api project)
- Use .NET's Options pattern for strongly-typed configuration
- Never commit sensitive credentials; use environment variables or user secrets for local dev

### Database
- Use EF Core migrations for schema changes: `dotnet ef migrations add <MigrationName>` (run from Api project)
- Apply migrations: `dotnet ef database update`
- DbContext is in `Data/NotificationDbContext.cs`
- Entity configurations are in `Data/Configurations/`

### Message Queue
- Define message contracts in Api project (shared between publisher and consumers)
- Use MassTransit for RabbitMQ integration (configure in Program.cs)
- Implement retry policies with Polly (exponential backoff recommended)
- Each worker service should consume from its dedicated queue (email-queue, sms-queue, whatsapp-queue)

### SignalR
- Hub should be in Api project at `/hubs/notifications`
- Send real-time updates when notification status changes
- Clients connect via SignalR client library (@microsoft/signalr for React)

## Container Development

This project is designed for Podman Compose but works with Docker Compose as well. The `compose.yml` file orchestrates:
- PostgreSQL database
- RabbitMQ message broker
- .NET API service
- Three worker services (Email, SMS, WhatsApp)
- React frontend (when implemented)

When making changes to Dockerfiles or compose.yml, rebuild with:
```bash
podman compose up --build
```
