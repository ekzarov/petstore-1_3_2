# Feature Specification: Cart UI

**Feature Branch**: `007-cart-ui-sdd`

**Created**: 2026-06-10

**Status**: Draft

**Input**: User description: "Extend the Angular catalog UI with shopping cart features backed by the cart API (006): add items to the cart from item views, see a cart indicator in the shell, view the cart with totals, change quantities, remove lines, and empty the cart, all without requiring sign-in."

## Dependencies

- Requires the shopping cart API from `specs/006-shopping-cart`.
- Builds on the catalog UI (003) and coexists with the account UI (005); sign-in must not be required for any cart interaction.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Add To Cart From Item Views (Priority: P1)

As a store visitor, I want an add-to-cart action on item lists and item details so that I can collect items while browsing.

**Why this priority**: This is the single action that turns the read-only catalog into a shopping experience.

**Independent Test**: Open Large Angelfish details, add it to the cart, and verify the shell cart indicator increments.

**Acceptance Scenarios**:

1. **Given** an item details view, **When** the visitor clicks add-to-cart, **Then** the UI confirms the add and the cart indicator updates without losing the current view.
2. **Given** an item list view, **When** the visitor adds an item from a card, **Then** the same confirmation and indicator behavior applies.
3. **Given** the backend is unavailable, **When** an add is attempted, **Then** the UI shows the standard unavailable feedback and the indicator does not change.

---

### User Story 2 - View And Edit The Cart (Priority: P1)

As a store visitor, I want a cart view with lines, subtotals, and a total where I can change quantities and remove lines so that the cart matches my intent.

**Why this priority**: The cart view is where order value is confirmed before checkout.

**Independent Test**: With two items in the cart, open the cart route, set one quantity to 3, remove the other line, and verify totals update each time.

**Acceptance Scenarios**:

1. **Given** a cart with items, **When** the cart route opens, **Then** each line shows name, stable item id, unit price, quantity control, and subtotal, plus a cart total.
2. **Given** a quantity change, **When** it is applied, **Then** the line subtotal and cart total refresh from the backend response.
3. **Given** an empty cart, **When** the cart route opens, **Then** the UI shows an empty-cart state with a link back to the catalog.
4. **Given** a cart with lines, **When** the visitor empties the cart, **Then** the empty-cart state is shown.

---

### User Story 3 - Persistent Cart Indicator (Priority: P2)

As a store visitor, I want to always see how many items are in my cart so that I can jump to the cart from anywhere in the catalog.

**Why this priority**: Navigation glue; valuable but secondary to the core cart operations.

**Independent Test**: Add items from different catalog routes and verify the indicator count stays correct on every route and after a page reload.

**Acceptance Scenarios**:

1. **Given** any catalog route, **When** the cart has N items, **Then** the shell shows N and links to the cart route.
2. **Given** a full page reload, **When** the shell loads, **Then** the indicator reflects the backend cart, not stale local state.

---

### Edge Cases

- Add-to-cart for an item that disappeared from the catalog (item-not-found from the API).
- Quantity edits racing each other from quick repeated clicks.
- Browser refresh on the cart route must reload the cart from the backend.
- Cart identity expiry: the UI must degrade to an empty cart without errors.
- Back/forward navigation between cart and catalog must keep states consistent.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The UI MUST provide add-to-cart actions on the item list and item details views.
- **FR-002**: The UI MUST show a cart indicator with the current item count in the catalog shell on all routes.
- **FR-003**: The UI MUST provide a cart route showing lines (name, item id, unit price, quantity, subtotal) and the cart total with currency.
- **FR-004**: The UI MUST allow changing line quantities, removing lines, and emptying the cart.
- **FR-005**: The UI MUST treat backend cart responses as the source of truth for all amounts and counts.
- **FR-006**: The UI MUST show loading, empty, not-found, and unavailable states consistent with the patterns from feature 003.
- **FR-007**: The UI MUST keep all cart interactions available without sign-in.
- **FR-008**: The UI MUST preserve stable item ids in cart views for parity inspection.
- **FR-009**: The cart route MUST be deep-linkable and refresh-safe.

### Key Entities *(include if feature involves data)*

- **Cart View State**: Loading, ready, empty, unavailable; lines plus totals from the API.
- **Cart Indicator State**: Item count shared by the shell across routes.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A visitor can add Large Angelfish from details, see the indicator go to 1, open the cart, and see the correct subtotal and total.
- **SC-002**: Quantity changes and removals immediately produce backend-confirmed totals.
- **SC-003**: A page reload on any route shows the correct indicator count and cart contents.
- **SC-004**: The full add-edit-empty flow works while signed out.
- **SC-005**: The catalog browsing flows of feature 003 remain unaffected.

## Assumptions

- Manual validation remains the UI test approach unless the plan decides otherwise (same deferral pattern as 003/005).
- Checkout actions are intentionally absent from the cart view until feature 009; the cart view may show a disabled or hidden checkout placeholder.
- The cart identity mechanism follows whatever DP-001 of feature 006 decides; the UI must not invent its own identity scheme.
