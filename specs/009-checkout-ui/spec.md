# Feature Specification: Checkout And Order History UI

**Feature Branch**: `009-checkout-ui-sdd`

**Created**: 2026-06-10

**Status**: Draft

**Input**: User description: "Extend the Angular UI with checkout and order history backed by the checkout/orders API (008): from the cart, a signed-in customer reviews shipping and billing details, places the order, sees a confirmation with the order id, and can browse their order history and order details with status."

## Dependencies

- Requires the checkout and orders API from `specs/008-checkout-orders`.
- Builds on cart UI (007) and account UI (005): checkout starts from the cart view and requires the signed-in state.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Check Out The Cart (Priority: P1)

As a signed-in customer with items in my cart, I want a checkout flow that confirms my shipping and billing details and places the order so that I receive an order id.

**Why this priority**: Completes the storefront purchase journey end to end in the migrated stack.

**Independent Test**: Sign in, add `EST-1`, start checkout from the cart, confirm prefilled contact details, place the order, and verify the confirmation view shows an order id and status `PENDING`.

**Acceptance Scenarios**:

1. **Given** a signed-in customer with a non-empty cart, **When** the customer starts checkout, **Then** the UI shows an order review with cart lines, totals, and contact details prefilled from the account.
2. **Given** the review is confirmed, **When** the order is placed, **Then** the UI shows a confirmation with the order id and the cart indicator drops to zero.
3. **Given** an anonymous visitor with a cart, **When** checkout is started, **Then** the UI redirects to sign-in and returns to checkout after successful sign-in.
4. **Given** validation errors from the backend, **When** placement fails, **Then** the UI shows field-level messages and preserves entered values.

---

### User Story 2 - Browse Order History (Priority: P1)

As a signed-in customer, I want to see my orders and open one to view its lines and status so that I can verify my purchases.

**Why this priority**: The confirmation is transient; order history is the durable proof the flow worked and the surface where later status changes (feature 010) become visible.

**Independent Test**: Place an order, open the orders route, verify the order appears with status `PENDING`, open it, and verify lines and totals match what was ordered.

**Acceptance Scenarios**:

1. **Given** a signed-in customer with orders, **When** the orders route opens, **Then** orders are listed with id, date, total, and status.
2. **Given** an order in the list, **When** it is opened, **Then** the details view shows lines (name, item id, quantity, price, subtotal), shipping details, total, and status.
3. **Given** a customer with no orders, **When** the orders route opens, **Then** an empty state with a link to the catalog is shown.
4. **Given** an unknown or foreign order id in the URL, **When** the route activates, **Then** the UI shows the standard not-found state.

---

### Edge Cases

- The cart changes (or is emptied in another tab) between starting checkout and placing the order; the backend response is authoritative.
- Double-click on the place-order action must not visibly create two orders (respects 008 DP-002).
- Browser refresh on the review, confirmation, orders, and order-details routes.
- Session expiry mid-checkout: the UI must return to sign-in without losing the cart.
- Backend unavailable at each step shows the standard unavailable state.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The cart view MUST offer a checkout action that is enabled only for non-empty carts.
- **FR-002**: Checkout MUST require the signed-in state, redirecting anonymous visitors to sign-in and back.
- **FR-003**: The review step MUST show cart lines, totals, and editable shipping/billing contact details prefilled from the account.
- **FR-004**: Placing the order MUST call the placement API exactly once per user intent and show a confirmation with order id and status.
- **FR-005**: After successful placement the cart indicator MUST reflect the emptied cart.
- **FR-006**: The UI MUST provide an orders route listing the customer's orders with id, date, total, currency, and status.
- **FR-007**: The UI MUST provide an order-details route with lines, shipping details, total, and status, deep-linkable and refresh-safe.
- **FR-008**: The UI MUST render loading, empty, validation-error, not-found, and unavailable states consistently with features 003/005/007.
- **FR-009**: The UI MUST display the legacy status vocabulary verbatim (`PENDING` etc.) or with a documented display mapping.
- **FR-010**: The UI MUST NOT compute order totals client-side; all amounts come from API responses.

### Key Entities *(include if feature involves data)*

- **Checkout Flow State**: Review data, submission state, placement result.
- **Order History View State**: Order list and order details states per route.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A signed-in customer completes catalog → cart → checkout → confirmation in one browser session and sees the order id.
- **SC-002**: The placed order appears in order history with status `PENDING` and correct totals.
- **SC-003**: An anonymous checkout attempt round-trips through sign-in and completes.
- **SC-004**: Refreshing any checkout/orders route reproduces a consistent state.
- **SC-005**: The end-to-end manual flow (account → catalog → cart → checkout → history) completes in under five minutes on a local machine.

## Assumptions

- Manual validation remains the UI test approach unless the plan decides otherwise.
- Payment capture UI follows 008 DP-003; if card data is not stored, the billing step may collect only what the API requires.
- Order status changes beyond `PENDING` will appear in this UI automatically once feature 010 starts transitioning statuses; no UI change should be needed beyond status display.
