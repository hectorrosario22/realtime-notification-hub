# Repository Guidelines

## Project Structure & Module Organization
The solution file is `NotificationHub.slnx` and all production code lives under `src/`.

- `src/NotificationHub.Api/`: ASP.NET Core API (controllers, DTOs, entities, repositories, EF Core data/migrations, SignalR hubs).
- `src/Workers/NotificationHub.Workers.{Email|Sms|WhatsApp}/`: channel-specific worker services.
- `compose.yml`: local multi-service environment (API, workers, PostgreSQL, RabbitMQ).
- `docs/`: project documentation (for example `docs/architecture.md`).

Keep new modules close to their bounded context (API feature code in `NotificationHub.Api`, delivery processing in the matching worker).

## Build, Test, and Development Commands
Use these commands from the repository root unless noted.

- `dotnet restore`: restore NuGet dependencies for all projects.
- `dotnet build`: compile API + workers.
- `dotnet run --project src/NotificationHub.Api`: run API locally.
- `dotnet run --project src/Workers/NotificationHub.Workers.Email`: run one worker locally (swap for Sms/WhatsApp).
- `podman compose up -d`: start full stack in containers.
- `podman compose down`: stop and clean up running services.

## Coding Style & Naming Conventions
This repo uses modern C# on .NET 10 with nullable reference types and implicit usings enabled.

- Use 4-space indentation and UTF-8 source files.
- Public types/members: `PascalCase`; locals/parameters: `camelCase`; interfaces: `I` prefix (for example `INotificationRepository`).
- Keep DTOs in `DTOs/Requests` and `DTOs/Responses`; keep EF configurations in `Data/Configurations`.
- Prefer async APIs for I/O (`Task`/`Task<T>`) and keep controllers thin by delegating to repositories/services.

## Testing Guidelines
Automated tests are not yet committed in this repository. For new features, add xUnit test projects under `tests/` (for example `tests/NotificationHub.Api.Tests`) and include them in the solution.

- Name test files after the unit under test (example: `NotificationsControllerTests.cs`).
- Run tests with `dotnet test`.
- Cover request validation, repository behavior, and notification workflow edge cases.

## Commit & Pull Request Guidelines
Follow the existing Conventional Commit style seen in history: `feat:`, `fix:`, `refactor:`, `docs:`, `chore(scope):`.

- Example: `feat(api): add unread notifications endpoint`.
- Keep commits focused and logically grouped.
- PRs should include: summary, impacted areas, test evidence (`dotnet build`/`dotnet test` output), and linked issue/task.
- For API changes, include sample request/response or Swagger screenshots.

## Security & Configuration Tips
Do not commit secrets. Use environment variables for connection strings and broker credentials. Default local values in `compose.yml` are for development only.
