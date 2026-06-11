# Tasks: Admin Orders And Inventory UI

**Input**: `specs/012-admin-ui/plan.md`, `specs/012-admin-ui/spec.md`

**Tests**: Manual validation per plan.

## Phase 1: Foundational

- [X] T001 Create admin models (order summaries, transitions, inventory) in `frontend/petstore-ui/src/app/admin/admin.models.ts`
- [X] T002 Create `AdminApiService` (list by status, approve, deny, transitions, inventory get/set, fulfillment run) in `frontend/petstore-ui/src/app/admin/admin-api.service.ts`
- [X] T003 Create `adminGuard` (role claim check, redirect to sign-in) in `frontend/petstore-ui/src/app/admin/admin.guard.ts`
- [X] T004 Create admin shell with nav and per-status counts in `frontend/petstore-ui/src/app/admin/admin-shell.component.ts`; wire `/admin` route area in `frontend/petstore-ui/src/app/app.routes.ts`; add role-conditional admin link to `frontend/petstore-ui/src/app/catalog/catalog-shell.component.ts`

## Phase 2: User Story 1 - Review And Decide Pending Orders (P1)

- [X] T005 [US1] Create pending queue (table, checkboxes, per-row and bulk approve/deny, per-order outcome badges incl. 409 already-decided) in `frontend/petstore-ui/src/app/admin/pending-orders.component.ts`
- [X] T006 [P] [US1] Add admin table/badge styles in `frontend/petstore-ui/src/styles.css`
- [X] T007 [US1] Manually validate single and bulk decisions, including the concurrent already-decided case, and that the customer sees the result

## Phase 3: User Story 2 - Monitor Orders Across Statuses (P2)

- [X] T008 [US2] Create status-filtered order list in `frontend/petstore-ui/src/app/admin/admin-order-list.component.ts`
- [X] T009 [US2] Create admin order detail with lines, contacts, status, and transition history in `frontend/petstore-ui/src/app/admin/admin-order-detail.component.ts`; wire routes
- [X] T010 [US2] Manually validate every status filter and the transition history

## Phase 4: User Story 3 - View And Adjust Inventory (P2)

- [X] T011 [US3] Create inventory view (inline non-negative quantity edit, run-fulfillment action) in `frontend/petstore-ui/src/app/admin/inventory.component.ts`; wire route
- [X] T012 [US3] Manually validate the replenish-unblocks-SHIPPED_PART flow end to end

## Phase 5: Polish

- [X] T013 [P] Document the admin area in `frontend/petstore-ui/README.md`
- [X] T014 Run `npx ng build` and full manual quickstart, including customer/anonymous denial of `/admin` routes and deep-link refresh on every admin route

## Dependencies

- Requires 005, 010, 011. Phase 1 blocks all; US1 → US2 → US3.
