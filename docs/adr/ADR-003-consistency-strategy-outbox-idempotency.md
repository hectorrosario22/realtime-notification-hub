# ADR-003: Consistency Strategy (Outbox + Idempotency)

Date: 2026-02-12  
Status: Accepted

## Context

Creating a notification and publishing an integration event are separate operations that can fail independently. Without coordination, the system can persist notifications without publishing events, or publish events for failed transactions. A consistency strategy is required.

## Decision

Adopt **Transactional Outbox** plus **idempotent consumers**:

- API writes domain state and outbox records in the same database transaction.
- A background dispatcher publishes pending outbox events to RabbitMQ.
- Consumers record processed message/event IDs and ignore duplicates.
- Failed processing uses retry policies and DLQ for manual/automated replay.

This provides practical eventual consistency with recovery mechanisms.

## Consequences

Benefits:

- Prevents lost events between DB commit and message publish.
- Supports replay/recovery without unsafe duplicate side effects.
- Makes failure modes explicit and operable.

Tradeoffs:

- Adds outbox table, dispatcher, and operational complexity.
- Requires cleanup/retention policy for outbox and deduplication records.
- Slight increase in end-to-end latency due to asynchronous dispatch.

## Alternatives considered

- Direct publish from request handler after DB write.
- Distributed transactions across DB and broker.
