# Realtime Notification Hub - Backend Senior Portfolio Roadmap

Date: 2026-02-12  
Owner: @hrosario  
Primary Goal: Demonstrate senior backend capability in designing and delivering distributed, production-ready systems.  
Differentiator: Distributed architecture reliability (Outbox, idempotency, retries, DLQ, eventual consistency).

## 1. Portfolio Positioning

This roadmap optimizes for backend senior interviews, not just feature count.  
The project should prove:

- You can design clean boundaries and evolve architecture incrementally.
- You can implement reliable asynchronous processing with failure handling.
- You can validate correctness under distributed system constraints.
- You can communicate tradeoffs with decision records and measurable outcomes.

## 2. Roadmap Structure

Total horizon: 12 to 16 weeks.  
Execution model: 5 phases, each ending with a demo artifact and objective acceptance criteria.

- Fase 1: Foundation and Architectural Baseline (Weeks 1-3)
- Fase 2: Reliable Messaging Core (Weeks 4-7)
- Fase 3: Resilience and Failure Recovery (Weeks 8-10)
- Fase 4: Scale and Operability Maturity (Weeks 11-13)
- Fase 5: Portfolio Packaging and Interview Readiness (Weeks 14-16)

## 3. Plan by Phase

### Fase 1 - Foundation and Architectural Baseline (Weeks 1-3)

Objective: Make current and target architecture explicit, then create a solid base for distributed workflows.

Scope:

- Split solution into clear projects/layers:
- `NotificationHub.Domain`
- `NotificationHub.Application`
- `NotificationHub.Infrastructure`
- Keep `NotificationHub.Api` and workers as composition/hosting entry points.
- Define core domain contracts for notification lifecycle and channel-agnostic behavior.
- Introduce command/query use cases for create + retrieve notification workflows.
- Add baseline validation and uniform API error contract.
- Add Architecture Decision Records (ADR-001 to ADR-003):
- Layering approach
- Messaging guarantees (at-least-once)
- Persistence and consistency strategy overview

Deliverables:

- Updated solution with project boundaries and dependency rule enforced.
- `docs/architecture.md` split into `Current State` and `Target State`.
- Initial ADR set under `docs/adr/`.
- End-to-end local run via compose with no broken flows.

Evidence for portfolio:

- Dependency diagram mapped to actual code projects.
- Before/after architecture explanation in README.

Definition of done:

- Code compiles and runs in compose.
- Layering violations are absent (Domain has no infra refs).
- At least 10 meaningful tests added (domain + application unit tests).

### Fase 2 - Reliable Messaging Core (Weeks 4-7)

Objective: Implement trustworthy asynchronous dispatch from API to workers.

Scope:

- Introduce integration event contracts with explicit versioning (e.g., `NotificationCreatedV1`, `NotificationStatusChangedV1`).
- Implement Outbox pattern in API/infrastructure so DB write and event publication are atomically coordinated.
- Create publisher background dispatcher that reads Outbox and publishes to RabbitMQ.
- Implement worker consumers (email, sms, whatsapp) that process events and publish status updates.
- Add idempotency on consumers:
- Message deduplication store by message id/event id.
- Safe reprocessing without duplicate side effects.
- Persist delivery attempts and status transitions in a consistent model.

Deliverables:

- Real workers replacing Hello World.
- Outbox table + outbox processor.
- Event contract package/folder with versioning policy.
- Consumer idempotency middleware/service.

Evidence for portfolio:

- Sequence diagram: request -> DB -> Outbox -> broker -> worker -> status event.
- Demo script showing duplicate message replay without duplicated final outcome.

Definition of done:

- Notification submission triggers async worker processing.
- Duplicate event delivery is safely handled.
- At least 2 integration tests proving outbox dispatch and idempotent consumption.

### Fase 3 - Resilience and Failure Recovery (Weeks 8-10)

Objective: Show production-grade behavior under failure.

Scope:

- Configure retry policies with exponential backoff and jitter for transient provider failures.
- Route poison/failure cases to DLQ after max attempts.
- Implement DLQ inspection and replay endpoint/tooling.
- Distinguish transient vs permanent failures and track reason codes.
- Add compensating status transitions and audit log enrichment.
- Add chaos/failure simulation mode (provider timeout, broker disconnect, DB transient exception).

Deliverables:

- Retry strategy per channel with configurable limits.
- DLQ + replay flow documented and executable.
- Failure taxonomy documentation.

Evidence for portfolio:

- Short incident demo: force failures, observe retries, verify DLQ behavior, replay successfully.
- Metrics snapshot: retry count, failure rate, recovery success.

Definition of done:

- Failed messages are never silently dropped.
- Replay from DLQ reaches consistent final status.
- Integration tests for transient failure, permanent failure, and replay path.

### Fase 4 - Scale and Operability Maturity (Weeks 11-13)

Objective: Demonstrate that the design can operate and scale predictably.

Scope:

- Add observability foundation:
- Structured logs with correlation ids.
- Metrics (throughput, processing latency, retry count, DLQ depth).
- Distributed tracing across API -> outbox processor -> worker.
- Add health/readiness checks for API and workers (DB, RabbitMQ connectivity).
- Add basic throughput controls:
- Consumer prefetch/concurrency tuning.
- Backpressure awareness and queue-depth driven alerts.
- Document SLO candidates and error budget policy.

Deliverables:

- Telemetry dashboard screenshots or exported panels.
- Correlation id propagated through the full async chain.
- Health endpoints and operational checklist.

Evidence for portfolio:

- Demo showing trace of single notification across services.
- Comparative run showing effect of concurrency tuning.

Definition of done:

- Operational signals are visible without reading raw DB tables.
- Latency and error metrics are queryable and explained in docs.

### Fase 5 - Portfolio Packaging and Interview Readiness (Weeks 14-16)

Objective: Convert implementation into interview leverage.

Scope:

- Create guided demo scenarios:
- Happy path
- Duplicate delivery path (idempotency proof)
- Failure + retry + DLQ + replay path
- Create architecture narrative docs:
- System context, container/component diagrams
- Tradeoff notes and rejected alternatives
- Publish engineering quality artifacts:
- Test strategy and coverage summary
- Non-functional requirements and achieved metrics
- Backlog of next production hardening steps
- Polish README for recruiter speed:
- 5-minute quickstart
- 15-minute deep demo
- "What this project proves"

Deliverables:

- Portfolio-ready documentation pack under `docs/portfolio/`.
- Demo runbook and interview Q&A sheet.
- Final roadmap status matrix (`implemented`, `in-progress`, `planned`).

Evidence for portfolio:

- Recorded walkthrough or reproducible terminal script.
- ADR index showing engineering decision maturity.

Definition of done:

- A reviewer can clone, run, and understand technical depth in under 20 minutes.
- You can explain reliability guarantees and limitations clearly.

## 4. Weekly Milestone View

- Week 1: Project split and dependency boundaries.
- Week 2: Application use cases + validation + initial tests.
- Week 3: Current/target architecture docs + ADR baseline.
- Week 4: Event contracts and queue topology.
- Week 5: Outbox implementation and dispatcher.
- Week 6: Worker consumers and status updates.
- Week 7: Idempotency and integration test suite.
- Week 8: Retry policies and failure classification.
- Week 9: DLQ routing and replay tooling.
- Week 10: Failure simulation and resilience tests.
- Week 11: Logs, metrics, trace correlation.
- Week 12: Health checks, readiness, runbook.
- Week 13: Concurrency tuning and scale notes.
- Week 14: Demo scenarios and scripts.
- Week 15: Portfolio docs, ADR refinements, README optimization.
- Week 16: Interview dry-run package and final polish.

## 5. Metrics That Strengthen Senior Signal

Track these from Fase 2 onward:

- End-to-end processing latency (P50/P95/P99).
- Event publish success rate from outbox.
- Consumer idempotency hit rate.
- Retry distribution by channel.
- DLQ depth over time and replay success ratio.
- Message processing throughput by worker.

Target examples (adjust after baseline):

- Outbox publish success >= 99.9%.
- Duplicate side-effect rate = 0 under replay tests.
- DLQ replay success >= 95% for transient-failure cases.

## 6. Risk Register and Mitigation

- Risk: Overengineering too early.
- Mitigation: Phase gate by demonstrable value; no advanced feature without test evidence.
- Risk: Documentation drift from code.
- Mitigation: Add "Current vs Target" section update as PR checklist item.
- Risk: Flaky integration tests with infra dependencies.
- Mitigation: Use deterministic test containers and stable seed/test fixtures.
- Risk: Time overrun.
- Mitigation: Keep must-have scope focused on reliability path first.

## 7. Non-Negotiable Scope (Must-Have)

- Outbox pattern implemented and tested.
- Consumer idempotency implemented and tested.
- Retry + DLQ + replay implemented and tested.
- Architecture docs aligned with real implementation state.
- At least one reproducible failure-recovery demo.

## 8. Stretch Scope (Optional)

- Multi-tenant isolation strategy and per-tenant throttling.
- Provider failover chain by channel.
- Cost-aware routing optimization.
- Blue/green event schema migration strategy.

## 9. Immediate Next Sprint (Start Here)

Sprint Goal (2 weeks): Finish Fase 1 foundation.

Backlog:

- Create Domain/Application/Infrastructure projects and wire references.
- Move core notification lifecycle behavior out of controller into application use cases.
- Define first ADR set (layering, consistency, delivery semantics).
- Add test project and baseline unit tests.
- Update architecture doc into `Current` and `Target` sections.

Exit criteria:

- Build passes.
- Tests pass in CI/local.
- Documentation reflects reality with explicit roadmap status.
