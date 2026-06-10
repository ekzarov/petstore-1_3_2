# Implementation Plan: Order Processing And Approval

**Branch**: `010-order-processing-sdd` | **Date**: 2026-06-10 | **Spec**: `specs/010-order-processing/spec.md`

## Summary

Add the order workflow to the backend: auto-approval of below-threshold orders
evaluated synchronously at placement, admin approve/deny endpoints guarded by
the `admin` role, a legal-transition rule set with an audit trail of
transitions, and a fulfillment handoff consumed by feature 011.

## Technical Context

**Language/Version**: C# / .NET 10, existing `dotnet/Petstore` project.

**Primary Dependencies**: Orders (008), JWT + role (004). No message broker.

**Storage**: `OrderStatusTransitions` audit table; threshold in configuration (`OrderProcessing:AutoApprovalThreshold`, default 500 USD).

**Testing**: xUnit unit tests for threshold/transition rules; integration tests for idempotency, concurrency, authorization (Constitution VI).

## Constitution Check

All principles pass. The JMS replacement decision is explicitly resolved (DD-001) as a documented intentional simplification, reversible later behind the same service interface.

## Design Decisions

- **DD-001 (DP-001)**: Synchronous in-process evaluation: `OrderProcessingService.EvaluateNewOrder(orderId)` is called by order placement (008) immediately after commit. No broker/queue in this slice; the service interface isolates the trigger so a background/queued implementation can replace it later without contract changes. Rationale: smallest verifiable slice; the legacy JMS hop is an implementation detail, not observable behavior.
- **DD-002 (DP-002)**: Admin decisions require the `admin` role claim (shared users table from 004).
- **DD-003 (DP-003)**: Customer e-mail notifications deferred to a later feature; recorded here as an intentional deferral.
- **DD-004 (DP-004)**: Single global threshold, `OrderProcessing:AutoApprovalThreshold` = 500 (order currency is uniform USD in migrated data). Threshold read at evaluation time; decided orders never re-evaluated.
- **DD-005**: Transition rules centralized in `OrderWorkflow`: `PENDING → APPROVED|DENIED`; `APPROVED → SHIPPED_PART|SHIPPED`; `SHIPPED_PART → SHIPPED_PART|SHIPPED`; `SHIPPED → COMPLETED`. Anything else rejected. Every applied transition writes an `OrderStatusTransitions` row (orderId, from, to, actor, UTC timestamp). Actor is `system` for auto-approval, the admin user id for decisions.
- **DD-006**: Concurrency/idempotency: transitions applied with an optimistic `WHERE Status = @from` update; zero rows affected → invalid-transition error, no audit row. Re-evaluation of a non-`PENDING` order is a no-op.
- **DD-007**: Fulfillment handoff = the order status itself: feature 011 queries `APPROVED`/`SHIPPED_PART` orders. No separate handoff table.
- **DD-008**: API surface:
  - `POST /api/admin/orders/{orderId}/approve` — admin role; 200 or 409 invalid transition.
  - `POST /api/admin/orders/{orderId}/deny` — admin role; 200 or 409.
  - `GET /api/admin/orders?status=PENDING` — admin role; orders by status (also serves feature 012).
  - `GET /api/admin/orders/{orderId}/transitions` — admin role; audit trail.

## Project Structure

```text
dotnet/Petstore/
|-- OrderProcessing/
|   |-- OrderWorkflow.cs                 (legal transitions)
|   |-- IOrderProcessingService.cs / OrderProcessingService.cs
|   `-- OrderTransitionRepository.cs
|-- Controllers/AdminOrdersController.cs
|-- Data/Entities/OrderStatusTransitionEntity.cs (+ configuration)
|-- Models/OrderTransitionDto.cs, AdminOrderSummaryDto.cs
`-- Data/Migrations/<new migration>

dotnet/Petstore.Tests/
|-- OrderWorkflowTests.cs            (unit: transition graph)
|-- OrderProcessingServiceTests.cs   (unit/integration: threshold, idempotency)
`-- AdminOrdersApiContractTests.cs   (contract: decisions, roles, audit)
```

## Test Strategy (Constitution VI)

- Unit: full transition graph (legal and illegal), threshold boundary (at threshold stays PENDING, below auto-approves).
- Integration: evaluation idempotency, concurrent decision race yields one winner and one 409, audit rows complete, customer role gets 403 on admin endpoints, status visible through unchanged 008 reads.
