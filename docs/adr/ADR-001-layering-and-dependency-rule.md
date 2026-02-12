# ADR-001: Layering and Dependency Rule

Date: 2026-02-12  
Status: Accepted

## Context

The current codebase started as a single API-centric project, which is useful for speed but causes coupling between HTTP, business rules, and persistence details. The target architecture requires clearer boundaries to support maintainability, testing, and future evolution of asynchronous workflows.

## Decision

Adopt a layered architecture with explicit dependency direction:

- `NotificationHub.Domain`: entities, value objects, domain behavior, domain interfaces. No framework dependencies.
- `NotificationHub.Application`: use cases, orchestration, validation, application contracts. Depends on Domain only.
- `NotificationHub.Infrastructure`: EF Core, messaging, external providers, repository implementations. Depends on Domain and Application contracts.
- `NotificationHub.Api` and workers: composition/hosting layers. Depend on Application and Infrastructure.

Dependency rule: inner layers must not reference outer layers.

## Consequences

Benefits:

- Improves separation of concerns and long-term maintainability.
- Enables focused unit testing in Domain/Application without infrastructure setup.
- Makes migrations to new providers/transport simpler.

Tradeoffs:

- More projects and interfaces increase initial complexity.
- Requires disciplined enforcement to avoid leaking infrastructure concerns into core layers.

## Alternatives considered

- Keep all logic in a single API project.
- Vertical slices inside one project without explicit project boundaries.
