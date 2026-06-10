# Tasks: Checkout And Order History UI

**Input**: `specs/009-checkout-ui/plan.md`, `specs/009-checkout-ui/spec.md`

**Tests**: Manual validation per plan.

## Phase 1: Foundational

- [ ] T001 Create order models matching the 008 contract in `frontend/petstore-ui/src/app/orders/order.models.ts`
- [ ] T002 Create `OrderApiService` (place, list, getById, normalized errors) in `frontend/petstore-ui/src/app/orders/order-api.service.ts`

## Phase 2: User Story 1 - Check Out The Cart (P1)

- [ ] T003 [US1] Create checkout component (review lines/totals, editable shipping prefilled from account, same-as-shipping billing toggle, disabled-on-submit place button, confirmation state with order id) in `frontend/petstore-ui/src/app/orders/checkout.component.ts`
- [ ] T004 [US1] Activate the checkout button in `frontend/petstore-ui/src/app/cart/cart.component.ts` and wire the guarded `/checkout` route in `frontend/petstore-ui/src/app/app.routes.ts`
- [ ] T005 [P] [US1] Add checkout/confirmation styles in `frontend/petstore-ui/src/styles.css`
- [ ] T006 [US1] Manually validate: signed-in checkout, anonymous redirect round-trip, double-click placement, validation errors, cart indicator drop to zero

## Phase 3: User Story 2 - Browse Order History (P1)

- [ ] T007 [US2] Create order list component (id, date, total, status badge, empty state) in `frontend/petstore-ui/src/app/orders/order-list.component.ts`
- [ ] T008 [US2] Create order detail component (lines, shipping details, total, status, not-found state) in `frontend/petstore-ui/src/app/orders/order-detail.component.ts`
- [ ] T009 [US2] Wire guarded `/orders` and `/orders/:orderId` routes and an orders link in the shell header in `frontend/petstore-ui/src/app/app.routes.ts` and `frontend/petstore-ui/src/app/catalog/catalog-shell.component.ts`
- [ ] T010 [P] [US2] Add order list/detail/status-badge styles in `frontend/petstore-ui/src/styles.css`
- [ ] T011 [US2] Manually validate history list, details, refresh, unknown/foreign order not-found

## Phase 4: Polish

- [ ] T012 [P] Document checkout and orders flows in `frontend/petstore-ui/README.md`
- [ ] T013 Run `npx ng build` and the full end-to-end manual quickstart (account → catalog → cart → checkout → history) under five minutes

## Dependencies

- Requires 005, 007, 008. Phase 1 blocks both stories; US1 → US2.
