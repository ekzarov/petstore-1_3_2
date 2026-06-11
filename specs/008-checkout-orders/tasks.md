# Tasks: Checkout And Orders API

**Input**: `specs/008-checkout-orders/plan.md`, `specs/008-checkout-orders/spec.md`

**Tests**: Required per Constitution VI.

## Phase 1: Foundational (data + status vocabulary)

- [X] T001 [P] Create `dotnet/Petstore/Orders/OrderStatus.cs` with the canonical legacy status constants
- [X] T002 [P] Create `dotnet/Petstore/Data/Entities/OrderEntity.cs` and `OrderLineEntity.cs` (sequential int id exposed as string, frozen line fields, contact blocks, status, UTC timestamp) with configurations in `dotnet/Petstore/Data/Configurations/`; add DbSets to `PetstoreCatalogContext`
- [X] T003 Add EF Core migration for order tables under `dotnet/Petstore/Data/Migrations/` and verify `dotnet ef database update`
- [X] T004 Create `dotnet/Petstore/Orders/IOrderRepository.cs` and `OrderRepository.cs` (create, list by user, get by id+user)

## Phase 2: User Story 1 - Place An Order From The Cart (P1)

- [X] T005 [P] [US1] Create DTOs `PlaceOrderRequestDto`, `OrderDto`, `OrderLineDto` in `dotnet/Petstore/Models/`
- [X] T006 [US1] Create `dotnet/Petstore/Orders/OrderPlacementService.cs` (non-empty cart check, price freeze from catalog, server total, transactional create + cart emptying, missing-item validation)
- [X] T007 [P] [US1] Add unit tests in `dotnet/Petstore.Tests/OrderPlacementRulesTests.cs` (totals, freezing, empty-cart rejection, billing defaults to shipping)
- [X] T008 [US1] Add contract tests for `POST /api/orders` (201 PENDING with frozen lines, cart emptied, duplicate submit empty-cart error, anonymous 401, validation 400) in `dotnet/Petstore.Tests/OrdersApiContractTests.cs`
- [X] T009 [US1] Implement `POST /api/orders` in `dotnet/Petstore/Controllers/OrdersController.cs`
- [X] T010 [US1] Verify with `dotnet test dotnet/Petstore/Petstore.slnx`

## Phase 3: User Story 2 - View My Orders (P1)

- [X] T011 [P] [US2] Create `OrderSummaryDto` in `dotnet/Petstore/Models/`
- [X] T012 [US2] Add contract tests for `GET /api/orders` and `GET /api/orders/{orderId}` (own data only, foreign/unknown 404, empty list not an error) in `dotnet/Petstore.Tests/OrdersApiContractTests.cs`
- [X] T013 [US2] Implement list and detail endpoints in `dotnet/Petstore/Controllers/OrdersController.cs`
- [X] T014 [US2] Verify with `dotnet test dotnet/Petstore/Petstore.slnx`

## Phase 4: User Story 3 - Order Data Ready For Processing (P2)

- [X] T015 [US3] Add integration tests in `dotnet/Petstore.Tests/OrderPlacementTransactionTests.cs` (self-contained order record, price-change isolation, transactional atomicity on failure)
- [X] T016 [US3] Verify with `dotnet test dotnet/Petstore/Petstore.slnx`

## Phase 5: Polish

- [X] T017 Run full suite and manual smoke with curl: sign in, add EST-1, place order, list, fetch, verify cart empty

## Dependencies

- Requires 004 and 006. Phase 1 blocks all; US1 → US2 → US3.
