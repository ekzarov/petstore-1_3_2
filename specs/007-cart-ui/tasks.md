# Tasks: Cart UI

**Input**: `specs/007-cart-ui/plan.md`, `specs/007-cart-ui/spec.md`

**Tests**: Manual validation per plan.

## Phase 1: Foundational

- [X] T001 Create cart models matching `CartDto`/`CartLineDto` in `frontend/petstore-ui/src/app/cart/cart.models.ts`
- [X] T002 Create `cart-id` interceptor adding `X-Cart-Id` (localStorage key `petstore.cartId`, GUID generated on first use) in `frontend/petstore-ui/src/app/cart/cart-id.interceptor.ts`; register in `frontend/petstore-ui/src/app/app.config.ts`
- [X] T003 Create signal-based `CartService` (load, add, setQuantity, removeLine, empty; state replaced from API responses; refresh on identity change) in `frontend/petstore-ui/src/app/cart/cart.service.ts`

## Phase 2: User Story 1 - Add To Cart From Item Views (P1)

- [X] T004 [US1] Add add-to-cart button with confirmation to `frontend/petstore-ui/src/app/catalog/item-detail.component.ts`
- [X] T005 [US1] Add add-to-cart button to item cards in `frontend/petstore-ui/src/app/catalog/item-list.component.ts`
- [X] T006 [P] [US1] Add button/confirmation styles in `frontend/petstore-ui/src/styles.css`
- [ ] T007 [US1] Manually validate adds from both views including backend-unavailable feedback

## Phase 3: User Story 2 - View And Edit The Cart (P1)

- [X] T008 [US2] Create cart view (lines with name/id/price/quantity stepper/subtotal, total, remove, empty action, empty state, disabled checkout placeholder) in `frontend/petstore-ui/src/app/cart/cart.component.ts`
- [X] T009 [US2] Wire `/cart` route in `frontend/petstore-ui/src/app/app.routes.ts`
- [X] T010 [P] [US2] Add cart table/stepper styles in `frontend/petstore-ui/src/styles.css`
- [ ] T011 [US2] Manually validate quantity changes, removal, empty-cart, and refresh on `/cart`

## Phase 4: User Story 3 - Persistent Cart Indicator (P2)

- [X] T012 [US3] Add cart indicator (count + link to `/cart`) to `frontend/petstore-ui/src/app/catalog/catalog-shell.component.ts`, loading the cart on app start
- [ ] T013 [US3] Manually validate the indicator across routes and after a full page reload

## Phase 5: Polish

- [X] T014 [P] Document cart id storage and flows in `frontend/petstore-ui/README.md`
- [ ] T015 Run `npx ng build` and the full manual quickstart including the signed-out end-to-end flow

## Dependencies

- Requires feature 006 running locally. Phase 1 blocks all; US1 → US2 → US3.
