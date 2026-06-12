# Tasks: Supplier UI And Admin Navigation Split

**Input**: `specs/014-supplier-ui-access/plan.md`, `specs/014-supplier-ui-access/spec.md`

**Tests**: Manual validation per plan (four-role matrix).

## Phase 1: Foundational

- [X] T001 Add `isSupplier` computed to `frontend/petstore-ui/src/app/identity/identity.service.ts`
- [X] T002 Create `supplierGuard` (roles supplier+admin; anonymous → signin with returnUrl; customer → catalog) in `frontend/petstore-ui/src/app/supplier/supplier.guard.ts`
- [X] T003 Create `SupplierApiService` against `/api/supplier/*` in `frontend/petstore-ui/src/app/supplier/supplier-api.service.ts`; remove inventory/fulfillment methods from `frontend/petstore-ui/src/app/admin/admin-api.service.ts`

## Phase 2: User Story 1 - Supplier Accesses Inventory Workspace (P1)

- [X] T004 [US1] Move the inventory component to `frontend/petstore-ui/src/app/supplier/inventory.component.ts`, retarget it to `SupplierApiService`, and relabel "Run fulfillment" as an operational recovery action
- [X] T005 [US1] Wire the guarded `/supplier` route area in `frontend/petstore-ui/src/app/app.routes.ts`; remove the `/admin/inventory` route
- [ ] T006 [US1] Manually validate: supplier signs in, sees inventory, edits stock; direct `/admin` URLs redirect without rendering order data

## Phase 3: User Story 2 - Admin Keeps Order Management Workspace (P1)

- [X] T007 [US2] Remove the Inventory tab from `frontend/petstore-ui/src/app/admin/admin-shell.component.ts`; add the Supplier link (visible to supplier and admin) and keep the Admin link admin-only in `frontend/petstore-ui/src/app/catalog/catalog-shell.component.ts`
- [ ] T008 [US2] Manually validate: admin keeps Pending queue / All orders and can open `/supplier` as superuser

## Phase 4: User Story 3 - Customer And Anonymous Cannot Reach Privileged Workspaces (P1)

- [ ] T009 [US3] Manually validate the remaining role matrix: customer and anonymous see no privileged links, direct URLs to `/admin/*` and `/supplier` redirect safely, refresh on privileged routes behaves

## Phase 5: Polish

- [X] T010 [P] Update `frontend/petstore-ui/README.md` (route table: `/supplier`; admin section without inventory)
- [ ] T011 Run `npx ng build` and the full manual quickstart including the supplier replenishment flow unblocking a waiting order

## Dependencies

- Requires 013 deployed locally. Phase 1 blocks all; US1 → US2 → US3.
