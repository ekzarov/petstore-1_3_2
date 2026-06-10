# Implementation Plan: Shopping Cart API

**Branch**: `006-shopping-cart-sdd` | **Date**: 2026-06-10 | **Spec**: `specs/006-shopping-cart/spec.md`

## Summary

Add a SQL-backed cart to the backend: carts keyed by an anonymous client cart
id or by the signed-in user id, lines storing item id + quantity only, prices
joined live from the catalog at read time, add/update/remove/empty operations,
and an anonymous-to-customer cart merge on sign-in.

## Technical Context

**Language/Version**: C# / .NET 10, existing `dotnet/Petstore` project.

**Primary Dependencies**: EF Core (existing), catalog repository (002), optional JWT identity (004).

**Storage**: `Carts` and `CartLines` tables in the same SQL Server database, EF Core migration.

**Testing**: xUnit unit tests for quantity/total rules; contract/integration tests for API, identity resolution, and merge behavior (Constitution VI).

## Constitution Check

All principles pass. Cart is an additive slice; catalog and accounts unchanged; legacy runtime untouched.

## Design Decisions

- **DD-001 (DP-001)**: Cart identity resolution per request: if the caller is authenticated (JWT), the cart key is `user:<userId>`; otherwise the client supplies a self-generated GUID in the `X-Cart-Id` header and the key is `anon:<guid>`. Requests with neither return an empty cart (reads) or 400 (writes). When an authenticated request also carries `X-Cart-Id` and an anonymous cart exists, its lines merge (quantities summed) into the user cart once, then the anonymous cart is deleted.
- **DD-002 (DP-002)**: Lines store `ItemId` + `Quantity` only. Names, unit prices, currency, subtotals, and totals are computed at read time from the catalog (live price until checkout; freezing happens at order placement in 008).
- **DD-003 (DP-003)**: SQL storage (survives restarts, honest integration tests). `Carts`: `CartKey` PK, `UpdatedAt`. `CartLines`: cart FK + `ItemId` (unique per cart) + `Quantity`.
- **DD-004**: Quantity bounds: 1..99; 0 means remove; negative or >99 rejected with validation error. Lines referencing items no longer in the catalog are returned flagged-as-unavailable and excluded from the total.
- **DD-005**: API surface (all anonymous-capable):
  - `GET /api/cart` — cart view (lines with name, unit price, currency, quantity, subtotal; item count; total).
  - `POST /api/cart/items` — `{ itemId }` add (merge into existing line, +1).
  - `PUT /api/cart/items/{itemId}` — `{ quantity }` set (0 = remove).
  - `DELETE /api/cart/items/{itemId}` — remove line.
  - `DELETE /api/cart` — empty cart.
  - Unknown item id → 404 `ApiErrorDto`, cart unchanged.

## Project Structure

```text
dotnet/Petstore/
|-- Cart/
|   |-- ICartRepository.cs / CartRepository.cs
|   |-- CartIdentityResolver.cs
|   `-- CartViewBuilder.cs              (joins catalog prices, computes totals)
|-- Controllers/CartController.cs
|-- Data/Entities/CartEntity.cs, CartLineEntity.cs
|-- Data/Configurations/CartEntityConfiguration.cs, CartLineEntityConfiguration.cs
|-- Models/CartDto.cs, CartLineDto.cs, AddCartItemRequestDto.cs, SetCartQuantityRequestDto.cs
`-- Data/Migrations/<new migration>

dotnet/Petstore.Tests/
|-- CartRulesTests.cs            (unit: quantities, totals, merge math)
|-- CartApiContractTests.cs      (contract: CRUD, identity, errors)
`-- CartMergeTests.cs            (integration: anon -> user merge)
```

## Test Strategy (Constitution VI)

- Unit: quantity bounds, 0-removes, total arithmetic, merge quantity summing.
- Contract/integration: add/duplicate-add merge, view with live prices, set/remove/empty, unknown item 404 leaves cart unchanged, anonymous via header, authenticated via JWT, merge-once semantics, unknown cart id yields empty cart.
