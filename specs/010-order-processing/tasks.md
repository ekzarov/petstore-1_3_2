# Tasks: Order Processing And Approval

**Input**: `specs/010-order-processing/plan.md`, `specs/010-order-processing/spec.md`

**Tests**: Required per Constitution VI.

## Phase 1: Foundational (workflow + audit)

- [X] T001 [P] Create `dotnet/Petstore/OrderProcessing/OrderWorkflow.cs` with the legal transition graph
- [X] T002 [P] Add unit tests covering every legal and illegal transition in `dotnet/Petstore.Tests/OrderWorkflowTests.cs`
- [X] T003 Create `dotnet/Petstore/Data/Entities/OrderStatusTransitionEntity.cs` with configuration; add DbSet; add EF Core migration and verify `dotnet ef database update`
- [X] T004 Create `dotnet/Petstore/OrderProcessing/OrderTransitionRepository.cs` (optimistic `WHERE Status = @from` transition apply + audit row)
- [X] T005 Add `OrderProcessing:AutoApprovalThreshold` (500) to `dotnet/Petstore/appsettings.Development.json`

## Phase 2: User Story 1 - Automatic Approval Of Small Orders (P1)

- [X] T006 [US1] Create `IOrderProcessingService` / `OrderProcessingService.EvaluateNewOrder` (below threshold → APPROVED as `system`; at/above → stays PENDING; non-PENDING no-op) in `dotnet/Petstore/OrderProcessing/`
- [X] T007 [US1] Call evaluation from order placement after commit in `dotnet/Petstore/Orders/OrderPlacementService.cs`
- [X] T008 [US1] Add unit/integration tests (threshold boundary, idempotent re-evaluation, audit actor `system`) in `dotnet/Petstore.Tests/OrderProcessingServiceTests.cs`
- [X] T009 [US1] Verify with `dotnet test dotnet/Petstore/Petstore.slnx`

## Phase 3: User Story 2 - Manual Approval Or Denial (P1)

- [X] T010 [P] [US2] Create DTOs `AdminOrderSummaryDto`, `OrderTransitionDto` in `dotnet/Petstore/Models/`
- [X] T011 [US2] Add contract tests for approve/deny/list-by-status/transitions endpoints (admin 200/409, customer 403, anonymous 401, concurrent race one winner) in `dotnet/Petstore.Tests/AdminOrdersApiContractTests.cs`
- [X] T012 [US2] Implement `dotnet/Petstore/Controllers/AdminOrdersController.cs` (`approve`, `deny`, `GET ?status=`, `GET {id}/transitions`) with `admin` role policy
- [X] T013 [US2] Verify with `dotnet test dotnet/Petstore/Petstore.slnx`

## Phase 4: User Story 3 - Status Visibility For Customers (P2)

- [X] T014 [US3] Add integration test: drive APPROVED and DENIED orders, verify statuses through the unchanged `GET /api/orders` reads in `dotnet/Petstore.Tests/AdminOrdersApiContractTests.cs`
- [X] T015 [US3] Verify with `dotnet test dotnet/Petstore/Petstore.slnx`

## Phase 5: Polish

- [X] T016 Run full suite and manual smoke: place small order → APPROVED automatically; place large order → approve via admin endpoint as seeded `admin`

## Dependencies

- Requires 008 (and 004 for roles). Phase 1 blocks all; US1 → US2 → US3.
