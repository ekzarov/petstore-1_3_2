# Implementation Plan: Supplier Fulfillment

**Branch**: `011-supplier-fulfillment-sdd` | **Date**: 2026-06-10 | **Spec**: `specs/011-supplier-fulfillment/spec.md`

## Summary

Add supplier fulfillment to the backend: an inventory table seeded per catalog
item, a fulfillment service that ships available quantities for
approved/partially-shipped orders, decrements stock, records invoices, and
advances order status through the feature-010 workflow; plus admin inventory
read/set endpoints.

## Technical Context

**Language/Version**: C# / .NET 10, existing `dotnet/Petstore` project (supplier as a module, not a separate deployable).

**Primary Dependencies**: Order processing workflow (010), orders (008), JWT roles (004).

**Storage**: `SupplierInventory`, `Shipments` (invoice records with shipped lines) tables, EF Core migration.

**Testing**: xUnit unit tests for shipment arithmetic; integration tests for concurrency, idempotency, status handoff (Constitution VI).

## Constitution Check

All principles pass. Consolidating the supplier into the single backend (vs the legacy separate EAR) is a documented structural change (DD-002); behavior parity (partial shipments, stock decrement, completion) is preserved and tested.

## Design Decisions

- **DD-001 (DP-001)**: Fulfillment triggers synchronously: (a) after an order transitions to `APPROVED` (called from 010's decision/auto-approval paths), and (b) after an inventory adjustment, for all `APPROVED`/`SHIPPED_PART` orders touching that item. `IFulfillmentService.FulfillOrder(orderId)` isolates the trigger for a later queued implementation, mirroring 010 DD-001.
- **DD-002 (DP-002)**: Supplier is a module (`dotnet/Petstore/Supplier/`) in the same backend and database. Rationale: one deployable keeps the slice small; the module boundary preserves the option to split later.
- **DD-003 (DP-003)**: Inventory seeded at 100 units per existing catalog item via explicit EF Core migration data; value chosen to keep manual partial-shipment testing easy (set stock low via the admin endpoint). Application startup must not create or update inventory rows implicitly.
- **DD-004**: Shipment algorithm per order: for each unshipped line quantity, ship `min(remaining, onHand)` inside one transaction with row-level guard `WHERE QuantityOnHand >= @take` style decrements (stock never negative, concurrency safe); write a `Shipment` row with shipped lines; then apply status via 010's transition rules: all lines fully shipped → `SHIPPED` then `COMPLETED` (invoice handling rule: completion applied immediately after the final shipment's invoice record in this slice); some shipped → `SHIPPED_PART`; none → no transition.
- **DD-005**: Idempotency: shipped quantities tracked per order line (`QuantityShipped` column on order lines); re-running fulfillment ships only the remainder; fully shipped orders are no-ops. Missing inventory rows read as zero stock.
- **DD-006**: API surface (admin role):
  - `GET /api/admin/inventory` — items with on-hand quantities.
  - `PUT /api/admin/inventory/{itemId}` — `{ quantity }` set non-negative; triggers re-fulfillment for blocked orders.
  - `POST /api/admin/fulfillment/run` — manual re-run over all eligible orders (operational safety valve).

## Project Structure

```text
dotnet/Petstore/
|-- Supplier/
|   |-- IFulfillmentService.cs / FulfillmentService.cs
|   `-- IInventoryRepository.cs / InventoryRepository.cs
|-- Controllers/AdminInventoryController.cs
|-- Data/Entities/SupplierInventoryEntity.cs, ShipmentEntity.cs, ShipmentLineEntity.cs (+ configurations)
|-- Models/InventoryItemDto.cs, SetInventoryRequestDto.cs
`-- Data/Migrations/<new migration>

dotnet/Petstore.Tests/
|-- FulfillmentRulesTests.cs        (unit: shipment arithmetic)
|-- FulfillmentIntegrationTests.cs  (integration: partial, replenish, concurrency, idempotency)
`-- AdminInventoryApiContractTests.cs
```

## Test Strategy (Constitution VI)

- Unit: min(remaining, onHand) arithmetic, full/partial/none shipment classification.
- Integration: happy path to `COMPLETED` with stock decrement; partial → `SHIPPED_PART` → replenish → `COMPLETED`; two orders competing for stock never overship; re-run no-ops; denied/pending never picked up; customer role 403 on admin endpoints; status changes flow through 010 transitions with audit rows.
