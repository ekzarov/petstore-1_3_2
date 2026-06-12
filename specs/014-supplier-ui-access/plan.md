# Implementation Plan: Supplier UI And Admin Navigation Split

**Branch**: `014-supplier-ui-access-sdd` | **Date**: 2026-06-12 | **Spec**: `specs/014-supplier-ui-access/spec.md`

## Summary

Move the inventory workspace out of `/admin` into a new `/supplier` route
area guarded for the `supplier` and `admin` roles (admin superuser per 013
DP-001), retarget the UI to the `/api/supplier/*` endpoints from feature 013,
and split shell navigation: Admin link for admins only, Supplier link for
suppliers and admins, nothing privileged for customers/anonymous.

## Technical Context

**Language/Version**: TypeScript, Angular 21 (zoneless, signals) in `frontend/petstore-ui`.

**Primary Dependencies**: Supplier backend API (013), identity service (005), existing admin area (012).

**Testing**: Manual validation across the four-role matrix (admin, supplier, customer, anonymous), consistent with the project's UI deferral.

## Constitution Check

All principles pass; UI-only slice, backend authorization is the enforcement of record (013).

## Design Decisions

- **DD-001 (DP-001)**: New `/supplier` route area hosting the inventory workspace; the `/admin/inventory` route and the Inventory tab in the admin nav are removed.
- **DD-002 (DP-002)**: `supplierGuard` accepts roles `supplier` and `admin`. The shell shows the Supplier link to both roles; the Admin link stays admin-only. `IdentityService` gains an `isSupplier` computed.
- **DD-003 (DP-003)**: Inventory save keeps the backend auto-fulfillment; the UI reloads inventory after save so decremented stock is visible. "Run fulfillment" stays, relabeled as an operational recovery action (FR-009).
- **DD-004**: `SupplierApiService` (inventory get/set, fulfillment run against `/api/supplier/*`) replaces the inventory methods of `AdminApiService`; the inventory component moves to `src/app/supplier/`.
- **DD-005**: Guard redirects: anonymous → `/signin?returnUrl=...`; signed-in without the required role → `/catalog` (safe route, no data rendered). Backend 403s surface through existing unavailable/error states.
- **DD-006 (DP-004)**: Multi-role link logic deferred; admin simply sees both links.

## Project Structure

```text
frontend/petstore-ui/src/app/
|-- supplier/
|   |-- supplier.guard.ts
|   |-- supplier-api.service.ts
|   `-- inventory.component.ts        (moved from admin/, retargeted)
|-- admin/admin-shell.component.ts    (Inventory tab removed)
|-- admin/admin-api.service.ts        (inventory methods removed)
|-- identity/identity.service.ts      (isSupplier)
|-- catalog/catalog-shell.component.ts (Supplier link)
`-- app.routes.ts                     (/supplier area; /admin/inventory removed)
```

## Manual Validation

Role matrix: supplier sees only Supplier link and inventory; direct URLs to `/admin/*` redirect without rendering order data; admin keeps Pending/All orders and can open `/supplier`; customer/anonymous see no privileged links and get redirected from both areas; replenishment unblocks a waiting order end to end through the supplier UI.
