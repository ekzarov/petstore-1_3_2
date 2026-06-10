# Implementation Plan: Admin Orders And Inventory UI

**Branch**: `012-admin-ui-sdd` | **Date**: 2026-06-10 | **Spec**: `specs/012-admin-ui/spec.md`

## Summary

Add an `/admin` route area to the existing Angular app guarded by the `admin`
role: a pending-orders queue with approve/deny (single and multi-select), an
orders browser filtered by status with details + transition history, and an
inventory view with quantity editing.

## Technical Context

**Language/Version**: TypeScript, Angular 21 (zoneless, signals) in `frontend/petstore-ui`.

**Primary Dependencies**: Admin orders API (010), admin inventory API (011), identity service with role claim (005).

**Testing**: Manual validation (consistent deferral).

## Constitution Check

All principles pass; frontend-only slice consuming tested backend capabilities.

## Design Decisions

- **DD-001 (DP-001)**: Admin lives as a guarded route area `/admin/...` inside `frontend/petstore-ui` (single app, single sign-in via the shared users table). An `adminGuard` checks the role claim; the backend role policy remains the enforcement of record. Rationale: avoids a second Angular workspace for three views; the route-area boundary keeps a later split possible.
- **DD-002 (DP-002)**: Legacy statistics reduced to counts per status shown on the admin landing view, derived from the list-by-status endpoint. Charts dropped with rationale (no analytical requirement in the migrated scope).
- **DD-003**: Pending queue: table with checkboxes, per-row approve/deny, bulk apply to selection executed as sequential per-order calls with per-order outcome badges (success / 409 already-decided).
- **DD-004**: Orders browser: status filter tabs (all six legacy values), detail view reusing the 008/010 read models with transition history list.
- **DD-005**: Inventory view: table of item id + on-hand quantity with inline numeric edit (non-negative client validation, backend confirmation), plus a "run fulfillment" action calling the 011 safety-valve endpoint.
- **DD-006**: Admin link appears in the shell header only for `admin` role identities.

## Project Structure

```text
frontend/petstore-ui/src/app/
|-- admin/
|   |-- admin.guard.ts
|   |-- admin-api.service.ts
|   |-- admin.models.ts
|   |-- admin-shell.component.ts        (admin nav + status counts)
|   |-- pending-orders.component.ts
|   |-- admin-order-list.component.ts
|   |-- admin-order-detail.component.ts
|   `-- inventory.component.ts
|-- app.routes.ts                       (/admin children, adminGuard)
`-- catalog/catalog-shell.component.ts  (conditional admin link)
```

## Manual Validation

Sign in as seeded `admin`; approve and deny above-threshold orders (single and bulk with a mixed already-decided case); browse every status filter; open transition history; lower inventory, observe SHIPPED_PART, raise inventory, observe COMPLETED; verify customer and anonymous identities cannot reach `/admin`.
