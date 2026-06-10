# Feature Specification: Checkout And Orders API

**Feature Branch**: `008-checkout-orders-sdd`

**Created**: 2026-06-10

**Status**: Draft

**Input**: User description: "Migrate the legacy PetStore checkout behavior to a backend API slice: a signed-in customer turns their cart into an order with shipping and billing details, receives an order id, and can list and view their own orders with status, preserving the legacy purchase order structure and PENDING initial status."

## Legacy Evidence *(Principle IV)*

- Legacy components: `src/components/purchaseorder` (purchase order with contact info, credit card, line items), `src/components/uidgen` (unique order id generation), `src/components/lineitem`.
- Legacy flow: checkout requires sign-in; the petstore app builds a purchase order from the session cart and customer data, then submits it asynchronously to OPC. Order ids are stable strings used across petstore, OPC, supplier, and admin.
- Legacy order statuses (from `processmanager` `OrderStatusNames`): `PENDING`, `APPROVED`, `DENIED`, `SHIPPED_PART`, `SHIPPED`, `COMPLETED`. New orders start as `PENDING`.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Place An Order From The Cart (Priority: P1)

As a signed-in customer with items in my cart, I want to confirm shipping and billing details and place an order so that my purchase enters the order lifecycle.

**Why this priority**: Order placement is the core business transaction the entire migration must preserve.

**Independent Test**: Sign in, add `EST-1` to the cart, place an order with valid details, and verify the response contains a new order id with status `PENDING` and lines matching the cart.

**Acceptance Scenarios**:

1. **Given** a signed-in customer with a non-empty cart, **When** the customer places an order with valid shipping and billing details, **Then** the API creates an order with a unique order id, status `PENDING`, the cart's lines frozen as order lines, and a recorded total.
2. **Given** a successful order placement, **When** the cart is fetched afterwards, **Then** the cart is empty.
3. **Given** an empty cart, **When** order placement is attempted, **Then** the API rejects it with a clear empty-cart error and creates nothing.
4. **Given** an anonymous caller, **When** order placement is attempted, **Then** the API rejects it as unauthorized.

---

### User Story 2 - View My Orders (Priority: P1)

As a signed-in customer, I want to list my orders and open one to see its lines, totals, and current status so that I can track what I bought.

**Why this priority**: Order visibility is required to verify placement and to surface downstream status changes from OPC (feature 010).

**Independent Test**: Place an order, list orders for the customer, open the order by id, and verify lines, total, shipping details, and status `PENDING`.

**Acceptance Scenarios**:

1. **Given** a customer with orders, **When** the customer lists orders, **Then** each entry shows order id, placement date, total, currency, and status.
2. **Given** an order id owned by the customer, **When** the order is fetched, **Then** the response contains order lines (item id, name, unit price, quantity, subtotal), shipping contact info, total, and status.
3. **Given** an order id owned by a different customer, **When** it is fetched, **Then** the API responds as not-found (no ownership disclosure).
4. **Given** an unknown order id, **When** it is fetched, **Then** the API returns the standard not-found error contract.

---

### User Story 3 - Order Data Ready For Processing (Priority: P2)

As the future order-processing slice (feature 010), I need each placed order to carry the data OPC needs so that approval and fulfillment can be migrated without re-reading the cart or customer session.

**Why this priority**: Protects the downstream migration phases from data gaps; no direct end-user value yet.

**Independent Test**: Inspect a placed order record and verify it is self-contained: order lines with frozen prices, shipping and billing contact info, customer user id, total, currency, status, and placement timestamp.

**Acceptance Scenarios**:

1. **Given** a placed order, **When** its stored record is inspected, **Then** it contains all fields needed for approval decisions and supplier fulfillment without joining to the live cart.
2. **Given** catalog price changes after placement, **When** the order is re-read, **Then** order line prices remain as frozen at placement time.

---

### Edge Cases

- Cart contents changing between the customer viewing the cart and submitting the order (totals must be computed server-side at placement).
- Duplicate submission (double click): placement must be idempotent per submission or produce clearly distinct orders by design (decision DP-002).
- Items removed from the catalog after being added to the cart but before checkout.
- Missing or invalid shipping/billing fields must name the offending fields.
- Order listing for a customer with no orders returns an empty list, not an error.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The API MUST allow a signed-in customer to place an order from their current cart with shipping and billing contact details.
- **FR-002**: The API MUST generate a unique, stable order id for every order.
- **FR-003**: New orders MUST start in status `PENDING`, matching the legacy status vocabulary.
- **FR-004**: Order lines MUST freeze item id, name, unit price, currency, and quantity at placement time.
- **FR-005**: The order total MUST be computed server-side from the frozen lines.
- **FR-006**: Successful placement MUST empty the customer's cart.
- **FR-007**: Placement MUST be rejected for empty carts, unauthenticated callers, and validation failures, using the standard error contract.
- **FR-008**: The API MUST let a signed-in customer list their own orders with id, date, total, currency, and status.
- **FR-009**: The API MUST let a signed-in customer fetch one of their own orders with full lines, shipping details, and status.
- **FR-010**: Customers MUST NOT be able to read or infer other customers' orders.
- **FR-011**: The stored order record MUST be self-contained for downstream processing (feature 010) per User Story 3.
- **FR-012**: The legacy status vocabulary (`PENDING`, `APPROVED`, `DENIED`, `SHIPPED_PART`, `SHIPPED`, `COMPLETED`) MUST be the canonical status set, even though only `PENDING` is assigned in this slice.

### Key Entities *(include if feature involves data)*

- **Order**: Order id, owning customer user id, placement timestamp, status, currency, total, shipping/billing contact info.
- **Order Line**: Item id, name, frozen unit price, quantity, derived subtotal.

### Decision Points *(must be resolved in plan phase)*

- **DP-001**: Order id format — preserve a legacy-like numeric/string scheme vs new opaque ids; admin and supplier parity checks depend on stability, not format.
- **DP-002**: Double-submission protection — idempotency key, server-side cart-version check, or accept duplicates as distinct orders.
- **DP-003**: Billing payment data handling — whether card details are captured at checkout, stored masked, or not stored at all (relates to 004 DP-002; legacy stores fake card data in plaintext, which will not be preserved).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A signed-in customer can place an order from a cart with `EST-1` and receive an order id with status `PENDING`.
- **SC-002**: The cart is empty immediately after successful placement.
- **SC-003**: The customer sees the new order in their order list and can open its details.
- **SC-004**: Another customer cannot fetch that order by id.
- **SC-005**: Order line prices stay frozen when catalog prices change afterwards.
- **SC-006**: Placement attempts with an empty cart or missing required fields fail with field-level clarity and create nothing.

## Assumptions

- Requires features 004 (accounts/sign-in) and 006 (cart) in place.
- Order approval, denial, shipping, and invoicing are explicitly out of scope; they belong to features 010/011. This slice only creates `PENDING` orders and exposes read access.
- No messaging/queue infrastructure is introduced in this slice; asynchronous processing is deferred to feature 010 where the messaging replacement decision lives.
- Automated backend tests are required per Constitution Principle VI: unit tests for totaling/freezing rules, integration/contract tests for placement, listing, ownership, and error contracts.
