# Feature Specification: Shopping Cart API

**Feature Branch**: `006-shopping-cart-sdd`

**Created**: 2026-06-10

**Status**: Draft

**Input**: User description: "Migrate the legacy PetStore shopping cart behavior to a backend API slice: add catalog items to a cart, view the cart with line totals, change quantities, remove items, and empty the cart, preserving the legacy behavior that a visitor can build a cart before signing in."

## Legacy Evidence *(Principle IV)*

- Legacy components: `src/components/cart` (session shopping cart EJB), `src/components/lineitem` (item id, quantity, unit price line items).
- Legacy behavior: the cart lives with the visitor's session; sign-in is required only at checkout. Cart lines reference sellable items by stable item id (for example `EST-1`) and use the item's catalog price.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Add Items To A Cart (Priority: P1)

As a store visitor, I want to add a sellable item to my cart so that I can collect what I intend to buy while browsing.

**Why this priority**: Adding to cart is the bridge from the migrated catalog to the future checkout and the smallest useful cart slice.

**Independent Test**: Add `EST-1` to a fresh cart and verify the cart shows one line with the item name, unit price, quantity 1, and a matching total.

**Acceptance Scenarios**:

1. **Given** an empty cart, **When** the visitor adds item `EST-1`, **Then** the cart contains one line for `EST-1` with quantity 1 and the catalog unit price.
2. **Given** a cart already containing `EST-1`, **When** the visitor adds `EST-1` again, **Then** the existing line's quantity increases instead of creating a duplicate line.
3. **Given** an unknown item id, **When** an add is attempted, **Then** the API reports item-not-found and the cart is unchanged.

---

### User Story 2 - View Cart With Totals (Priority: P1)

As a store visitor, I want to view my cart with line subtotals and a cart total so that I know what an order would cost.

**Why this priority**: Cart contents and totals are required by both the cart UI and the future checkout calculation.

**Independent Test**: Add two different items with different quantities, fetch the cart, verify each line subtotal equals quantity times unit price and the cart total equals the sum of subtotals.

**Acceptance Scenarios**:

1. **Given** a cart with items, **When** the cart is fetched, **Then** each line shows item id, item name, unit price, quantity, currency, and line subtotal, and the cart shows an item count and total.
2. **Given** an empty or never-used cart, **When** the cart is fetched, **Then** the API returns an empty cart, not an error.

---

### User Story 3 - Update Quantities And Remove Items (Priority: P1)

As a store visitor, I want to change line quantities, remove lines, and empty the whole cart so that the cart reflects what I actually want.

**Why this priority**: Quantity correction and removal are part of legacy cart parity and are needed before checkout totals can be trusted.

**Independent Test**: Set a line quantity to 3, verify totals; remove the line, verify the cart is empty; re-add and empty the cart in one call.

**Acceptance Scenarios**:

1. **Given** a cart line with quantity 1, **When** the quantity is set to 3, **Then** the line subtotal and cart total update accordingly.
2. **Given** a line quantity set to 0, **When** the update is applied, **Then** the line is removed from the cart.
3. **Given** a cart with several lines, **When** the visitor empties the cart, **Then** the cart returns to the empty state.

---

### User Story 4 - Keep A Cart Across Requests (Priority: P2)

As a store visitor, I want my cart to survive page navigation and reloads during my visit so that I do not lose selections while browsing.

**Why this priority**: Required for a usable cart UI but secondary to core line operations.

**Independent Test**: Add an item, simulate a new request with the same cart identity, and verify the cart still holds the item.

**Acceptance Scenarios**:

1. **Given** a visitor with a cart identity, **When** a later request presents the same identity, **Then** the same cart contents are returned.
2. **Given** a signed-in customer (feature 004), **When** the customer's cart identity is used, **Then** the cart follows the customer identity as decided by DP-001 below.

---

### Edge Cases

- Adding an item whose product or category still exists but which itself was removed from the catalog.
- Quantity values that are negative, zero (means remove), or unreasonably large (an upper bound must be validated).
- Price changes in the catalog between add and fetch: the cart must consistently use a single pricing rule (decision DP-002).
- Two rapid updates to the same line must not corrupt quantity (last-write-wins is acceptable, corruption is not).
- An expired or unknown cart identity must yield an empty cart, not an error.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The API MUST allow adding a sellable catalog item to a cart by stable item id.
- **FR-002**: The API MUST merge repeated adds of the same item into one line with an increased quantity.
- **FR-003**: The API MUST return the cart with line item id, name, unit price, currency, quantity, line subtotal, item count, and cart total.
- **FR-004**: The API MUST allow setting a line quantity, treating 0 as removal, and rejecting negative or above-bound values.
- **FR-005**: The API MUST allow removing a single line and emptying the entire cart.
- **FR-006**: The API MUST report item-not-found for unknown item ids using the established error contract (`ApiErrorDto`).
- **FR-007**: The API MUST support carts for anonymous visitors; sign-in MUST NOT be required for any cart operation.
- **FR-008**: The API MUST keep a cart addressable across requests within a visitor session lifetime.
- **FR-009**: Cart monetary amounts MUST come from the migrated catalog data, never from client-supplied prices.
- **FR-010**: The cart MUST preserve stable legacy item ids for future order creation parity.

### Key Entities *(include if feature involves data)*

- **Cart**: A visitor-scoped collection of cart lines with derived item count and total.
- **Cart Line**: Item id, display name, unit price, currency, quantity, derived subtotal.

### Decision Points *(must be resolved in plan phase)*

- **DP-001**: Cart identity mechanism — server session cookie, client-generated cart id header, or customer-bound cart after sign-in; and whether an anonymous cart merges into the customer cart on sign-in.
- **DP-002**: Pricing rule — snapshot the unit price at add time vs always reflect the current catalog price until checkout.
- **DP-003**: Cart storage — in the migrated SQL database vs in-memory/session store; affects multi-instance behavior and test strategy.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A visitor can add `EST-1` and `EST-2`, change a quantity, and see arithmetically correct subtotals and total.
- **SC-002**: Repeated adds of one item never create duplicate lines.
- **SC-003**: A cart survives across requests within the chosen identity lifetime.
- **SC-004**: All cart operations work without sign-in.
- **SC-005**: An unknown item id is rejected with the standard error contract and leaves the cart unchanged.

## Assumptions

- Catalog API (002) remains the source of item names and prices.
- Order creation from the cart is out of scope here; it belongs to the checkout feature (008).
- Automated backend tests are required per Constitution Principle VI: unit tests for total/quantity rules, integration/contract tests for the API and storage boundary.
