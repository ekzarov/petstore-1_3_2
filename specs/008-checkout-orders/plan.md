# Implementation Plan: Checkout And Orders API

**Branch**: `008-checkout-orders-sdd` | **Date**: 2026-06-10 | **Spec**: `specs/008-checkout-orders/spec.md`

## Summary

Add order placement and order reads to the backend: a signed-in customer turns
their cart into a self-contained order (frozen lines, server-computed total,
status `PENDING`), the cart empties atomically, and customers list/fetch only
their own orders.

## Technical Context

**Language/Version**: C# / .NET 10, existing `dotnet/Petstore` project.

**Primary Dependencies**: EF Core, cart (006), accounts/JWT (004), catalog (002) for placement-time prices.

**Storage**: `Orders` and `OrderLines` tables, EF Core migration, same database; placement runs in a single transaction with cart emptying.

**Testing**: xUnit unit tests for freezing/totaling; contract/integration tests for placement, ownership, and error contracts (Constitution VI).

## Constitution Check

All principles pass. Status vocabulary parity is explicit (FR-012); only `PENDING` is assigned here.

## Design Decisions

- **DD-001 (DP-001)**: Order id is a database-generated sequential integer exposed as a string (legacy-like numeric ids, stable across all later slices).
- **DD-002 (DP-002)**: Double-submission protection via the cart-empties-atomically rule: placement requires a non-empty cart and runs in one transaction; a duplicate submit finds an empty cart and gets the empty-cart error. No idempotency keys in this slice.
- **DD-003 (DP-003)**: No card data captured; the order stores shipping and billing contact info blocks only (billing defaults to shipping when omitted).
- **DD-004**: Placement flow: resolve user cart → reject if empty → load current catalog prices → freeze lines (item id, name, unit price, currency, quantity) → compute total server-side → create order (`PENDING`, UTC timestamp, user id) → delete cart lines → return order. Items now missing from the catalog fail placement with a validation error naming the item ids.
- **DD-005**: Status stored as a string constrained to the canonical set: `PENDING`, `APPROVED`, `DENIED`, `SHIPPED_PART`, `SHIPPED`, `COMPLETED` (`OrderStatus` constants class; single currency per order, from catalog).
- **DD-006**: API surface (all `[Authorize]`):
  - `POST /api/orders` — `{ shippingContact, billingContact? }` → 201 order.
  - `GET /api/orders` — own orders (id, placedAt, total, currency, status), newest first.
  - `GET /api/orders/{orderId}` — own order with lines and contacts; foreign/unknown id → 404 (no ownership disclosure).

## Project Structure

```text
dotnet/Petstore/
|-- Orders/
|   |-- IOrderRepository.cs / OrderRepository.cs
|   |-- OrderPlacementService.cs
|   `-- OrderStatus.cs
|-- Controllers/OrdersController.cs
|-- Data/Entities/OrderEntity.cs, OrderLineEntity.cs
|-- Data/Configurations/OrderEntityConfiguration.cs, OrderLineEntityConfiguration.cs
|-- Models/PlaceOrderRequestDto.cs, OrderDto.cs, OrderSummaryDto.cs, OrderLineDto.cs
`-- Data/Migrations/<new migration>

dotnet/Petstore.Tests/
|-- OrderPlacementRulesTests.cs   (unit: freezing, totals, empty-cart)
|-- OrdersApiContractTests.cs     (contract: place/list/fetch/ownership/errors)
`-- OrderPlacementTransactionTests.cs (integration: atomicity, price freeze)
```

## Test Strategy (Constitution VI)

- Unit: total computation, line freezing, empty-cart rejection, billing-defaults-to-shipping.
- Contract/integration: placement 201 with `PENDING` and frozen lines; cart empty afterwards; duplicate submit → empty-cart error; anonymous 401; foreign order 404; unknown order 404; price change after placement does not alter the order; transaction atomicity (failure leaves cart intact and no order).
