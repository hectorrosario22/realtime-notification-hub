# ADR-002: Delivery Semantics (At-Least-Once)

Date: 2026-02-12  
Status: Accepted

## Context

Notification delivery spans database writes, message broker publishing, worker processing, and external providers. In distributed systems, exactly-once end-to-end delivery is expensive and typically impractical. The system needs a realistic reliability model suitable for production workloads.

## Decision

Use **at-least-once** delivery semantics across asynchronous pipelines.

Key implications:

- Messages can be delivered more than once.
- Consumers must be idempotent.
- Retries are required for transient failures.
- Dead-letter queues (DLQ) handle poison/unrecoverable messages.

System behavior will prioritize durability and recoverability over strict exactly-once guarantees.

## Consequences

Benefits:

- Robust handling of broker/network/provider failures.
- Better alignment with RabbitMQ and worker-based architectures.
- Predictable operational model for retries and replay.

Tradeoffs:

- Duplicate delivery attempts are possible.
- Requires idempotency design and deduplication strategy.
- Monitoring must include retry and DLQ metrics.

## Alternatives considered

- At-most-once delivery (lower duplication risk, higher loss risk).
- Exactly-once semantics across all components (high complexity and cost).
