# Issue #3 - Initial Domain Lifecycle Model Design

## Objective

Define the domain model baseline required to complete issue #3, including:

- state meanings
- valid transitions
- invalid transitions
- transition ownership (domain vs application/infrastructure)
- delivery plan for aggregate hardening and domain tests

This document is the contract for implementation and validation of the full story.

## Source of truth

- Aggregate: `src/NotificationHub.Domain/Notifications/Notification.cs`
- Status enum: `src/NotificationHub.Domain/Notifications/NotificationStatus.cs`

## Status definitions

- `Pending`: notification is created and not yet queued for delivery.
- `Queued`: notification is accepted for processing by delivery workers.
- `Processing`: worker has started handling delivery.
- `Sent`: provider accepted/finished delivery attempt successfully.
- `Failed`: delivery attempt failed.
- `Read`: recipient consumed the notification (currently used by push flow).

## Valid transitions

| Current | Next | Trigger method |
|---|---|---|
| `Pending` | `Queued` | `MarkAsQueued()` |
| `Failed` | `Queued` | `MarkAsQueued()` (retry path) |
| `Queued` | `Processing` | `StartProcessing()` |
| `Queued` | `Sent` | `MarkAsSent(...)` |
| `Processing` | `Sent` | `MarkAsSent(...)` |
| `Queued` | `Failed` | `MarkAsFailed(...)` |
| `Processing` | `Failed` | `MarkAsFailed(...)` |
| `Sent` | `Read` | `MarkAsRead(...)` |

## Invalid transitions policy

Any transition not listed above is invalid and must be rejected in the domain with `InvalidOperationException`.

Examples:

- `Pending -> Sent`
- `Pending -> Failed`
- `Sent -> Queued`
- `Failed -> Read`
- `Read -> *` (terminal status)

## Invariants coupled to lifecycle

- `RetryCount` increases only when entering `Failed`.
- `SentAtUtc` is set when entering `Sent`.
- `ReadAtUtc` is set when entering `Read`.
- `ErrorMessage` is required when entering `Failed`.
- `ErrorMessage` is cleared when entering `Queued` and `Sent`.

## Ownership and boundaries

- Lifecycle rules and transition guards belong to Domain (`Notification` aggregate).
- Application/API can add use-case rules on top (example: only Push can call read endpoint).
- Infrastructure must not define/override transition rules.

## Execution plan for issue #3

1. Finalize and validate lifecycle contract (this document).
2. Review aggregate methods and ensure all status updates are protected by domain methods only.
3. Harden invariants that are coupled to transitions (timestamps, retry count, error message behavior).
4. Add domain tests for:
   - valid transitions
   - invalid transitions
   - channel creation invariants
   - retry behavior and limits
5. Validate architectural boundary:
   - no EF Core dependency in Domain
   - no transition business rules in Infrastructure
6. Run `dotnet build` and `dotnet test` and attach evidence to issue #3.
