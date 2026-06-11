# Tasks: Supplier Fulfillment

**Input**: `specs/011-supplier-fulfillment/plan.md`, `specs/011-supplier-fulfillment/spec.md`

**Tests**: Required per Constitution VI.

## Phase 1: Foundational (inventory + shipment data)

- [X] T001 [P] Create `dotnet/Petstore/Data/Entities/SupplierInventoryEntity.cs`, `ShipmentEntity.cs`, `ShipmentLineEntity.cs` with configurations; add `QuantityShipped` to `OrderLineEntity`; add DbSets
- [X] T002 Add EF Core migration for inventory/shipments/quantity-shipped under `dotnet/Petstore/Data/Migrations/` and verify `dotnet ef database update`
- [X] T003 Create `dotnet/Petstore/Supplier/InventorySeeder.cs` (100 units per catalog item, idempotent)
- [X] T004 Create `dotnet/Petstore/Supplier/IInventoryRepository.cs` and `InventoryRepository.cs` (read, set non-negative, concurrency-safe decrement)

## Phase 2: User Story 1 - Fulfill With Sufficient Stock (P1)

- [X] T005 [US1] Create `IFulfillmentService` / `FulfillmentService.FulfillOrder` (ship min(remaining, onHand) per line transactionally, write shipment, apply 010 transitions to SHIPPED/COMPLETED) in `dotnet/Petstore/Supplier/`
- [X] T006 [US1] Trigger fulfillment after APPROVED transitions in `dotnet/Petstore/OrderProcessing/OrderProcessingService.cs` and the admin approve path
- [X] T007 [P] [US1] Add unit tests in `dotnet/Petstore.Tests/FulfillmentRulesTests.cs` (arithmetic, classification full/partial/none)
- [X] T008 [US1] Add integration tests: approved order with stock reaches COMPLETED with correct stock decrement; DENIED/PENDING never picked up, in `dotnet/Petstore.Tests/FulfillmentIntegrationTests.cs`
- [X] T009 [US1] Verify with `dotnet test dotnet/Petstore/Petstore.slnx`

## Phase 3: User Story 2 - Partial Shipment (P1)

- [X] T010 [US2] Implement partial-shipment path (`SHIPPED_PART`, remainder tracking via `QuantityShipped`, no-op when nothing shippable) in `dotnet/Petstore/Supplier/FulfillmentService.cs`
- [X] T011 [US2] Add integration tests: partial → SHIPPED_PART → replenish → COMPLETED; concurrency (two orders, limited stock, no overship); re-run idempotency, in `dotnet/Petstore.Tests/FulfillmentIntegrationTests.cs`
- [X] T012 [US2] Verify with `dotnet test dotnet/Petstore/Petstore.slnx`

## Phase 4: User Story 3 - Manage Supplier Inventory (P2)

- [X] T013 [P] [US3] Create DTOs `InventoryItemDto`, `SetInventoryRequestDto` in `dotnet/Petstore/Models/`
- [X] T014 [US3] Add contract tests for `GET /api/admin/inventory`, `PUT /api/admin/inventory/{itemId}` (set triggers re-fulfillment; negative rejected; customer 403), and `POST /api/admin/fulfillment/run` in `dotnet/Petstore.Tests/AdminInventoryApiContractTests.cs`
- [X] T015 [US3] Implement `dotnet/Petstore/Controllers/AdminInventoryController.cs` with re-fulfillment trigger on inventory set
- [X] T016 [US3] Verify with `dotnet test dotnet/Petstore/Petstore.slnx`

## Phase 5: Polish

- [X] T017 Run full suite and manual smoke: low-stock partial shipment unblocked by inventory set, order completes; customer sees SHIPPED_PART/COMPLETED via `GET /api/orders`

## Dependencies

- Requires 008 and 010. Phase 1 blocks all; US1 → US2 → US3.
