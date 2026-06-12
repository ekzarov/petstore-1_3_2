# Implementation Plan: Supplier Role And Backend Access Split

**Branch**: `013-supplier-role-access-sdd` | **Date**: 2026-06-12 | **Spec**: `specs/013-supplier-role-access/spec.md`

## Summary

Add the `supplier` role and seeded `supplier` user, introduce a
SupplierOperations authorization policy (roles `supplier` + `admin` per
DP-001), move inventory/fulfillment endpoints from `/api/admin/*` to
`/api/supplier/*` (DP-003, no aliases), and lock order administration to
`admin` only with boundary tests in both directions.

## Technical Context

**Language/Version**: C# / .NET 10, existing `dotnet/Petstore` project.

**Primary Dependencies**: Existing JWT auth (004), inventory/fulfillment services (011), admin order endpoints (010/012). No new packages, no schema changes (role is already a string column), no EF migration.

**Testing**: xUnit contract/integration tests (Constitution VI); existing 011 tests retargeted to the new routes.

## Constitution Check

All principles pass. Behavior change is authorization-only; customer-facing APIs untouched (FR-008 regression-checked by the existing suites).

## Design Decisions

- **DD-001 (DP-001)**: Authorization policy `SupplierOperations` = `RequireRole("supplier", "admin")` registered in `Program.cs`. Order administration keeps `[Authorize(Roles = "admin")]`.
- **DD-002 (DP-003)**: `AdminInventoryController` is replaced by `SupplierInventoryController` at `api/supplier` (`GET /api/supplier/inventory`, `PUT /api/supplier/inventory/{itemId}`, `POST /api/supplier/fulfillment/run`). Old admin routes removed.
- **DD-003 (DP-002)**: Inventory update keeps the automatic re-fulfillment of affected orders (011 behavior); the run endpoint stays as recovery.
- **DD-004 (DP-004)**: Blind mode — no new read models; the fulfillment run response remains `{ processed }`.
- **DD-005**: `AccountModelConstants.Roles.Supplier = "supplier"`; `AccountSeeder` adds `supplier`/`supplier` (role `supplier`). Seeding stays idempotent per user id, so existing databases gain the supplier without touching other users (spec edge case).
- **DD-006**: Frontend keeps working only after feature 014 retargets it; within this backend feature the UI temporarily points at removed admin inventory routes — acceptable because 013 and 014 land back to back on the roadmap.

## Project Structure

```text
dotnet/Petstore/
|-- Data/AccountModelConstants.cs        (Roles.Supplier)
|-- Accounts/AccountSeeder.cs            (supplier user)
|-- Controllers/SupplierInventoryController.cs  (replaces AdminInventoryController)
`-- Program.cs                           (SupplierOperations policy)

dotnet/Petstore.Tests/
|-- SupplierRoleApiContractTests.cs      (sign-in, boundary matrix)
|-- SupplierInventoryApiContractTests.cs (renamed/retargeted from AdminInventoryApiContractTests)
`-- FulfillmentIntegrationTests.cs       (retargeted inventory routes)
```

## Test Strategy (Constitution VI)

- Supplier sign-in: token carries role `supplier`, not `admin`; admin token unchanged.
- Boundary matrix: supplier → 403 on every admin order endpoint (list, detail, approve, deny, transitions); admin → 200 as before; customer/anonymous → 403/401 on both areas.
- Inventory at the new routes: supplier and admin both succeed (DP-001), customer 403, anonymous 401; old `/api/admin/inventory` route returns 404 (removed).
- Replenishment flow (FR-007): zero-stock approved order completes after supplier raises inventory — existing 011 integration test retargeted to `/api/supplier/inventory` driven by the supplier identity.
- Seeder idempotency including the new supplier user against a database that already has users.
