# Feature Specification: Admin Orders And Inventory UI

**Feature Branch**: `012-admin-ui-sdd`

**Created**: 2026-06-10

**Status**: Draft

**Input**: User description: "Provide an administrative UI replacing the legacy admin application for the migrated stack: an administrator signs in, reviews pending orders, approves or denies them, monitors order statuses, and views and adjusts supplier inventory, using the order-processing (010) and supplier (011) capabilities."

## Legacy Evidence *(Principle IV)*

- Legacy app: `src/apps/admin` (Swing-based rich client plus web service) used to list `PENDING` orders, approve/deny them in bulk, and view order statistics.
- Legacy supplier app exposed inventory receiving; in the migrated stack inventory operations come from feature 011.
- The legacy admin was a separate application with its own entry point; the migrated admin surface must remain separated from the storefront customer experience.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Review And Decide Pending Orders (Priority: P1)

As a store administrator, I want to list pending orders and approve or deny them so that high-value orders proceed or stop based on my decision.

**Why this priority**: This is the only manual step in the migrated order lifecycle; without it, above-threshold orders are stuck.

**Independent Test**: Create an above-threshold order, sign in as an administrator, find it in the pending list, approve it, and verify it leaves the pending list and the customer sees `APPROVED`.

**Acceptance Scenarios**:

1. **Given** a signed-in administrator, **When** the pending orders view opens, **Then** all `PENDING` orders are listed with order id, date, customer, total, and currency.
2. **Given** a pending order, **When** the administrator approves it, **Then** the order disappears from the pending list and its status becomes `APPROVED`.
3. **Given** a pending order, **When** the administrator denies it, **Then** the order disappears from the pending list and its status becomes `DENIED`.
4. **Given** several selected pending orders, **When** the administrator applies one decision to the selection, **Then** each order receives the decision individually with per-order success/failure feedback.

---

### User Story 2 - Monitor Orders Across Statuses (Priority: P2)

As a store administrator, I want to browse orders by status so that I can see lifecycle health beyond the pending queue.

**Why this priority**: Mirrors the legacy admin's order overview; useful for verifying features 010/011 during migration.

**Independent Test**: Drive orders into different statuses, filter the orders view by each status, and verify the right orders appear.

**Acceptance Scenarios**:

1. **Given** orders in multiple statuses, **When** the administrator filters by a status, **Then** only orders in that status are listed.
2. **Given** an order in the list, **When** it is opened, **Then** the details show lines, totals, customer contact info, status, and the transition history from feature 010.

---

### User Story 3 - View And Adjust Inventory (Priority: P2)

As a store administrator, I want to see supplier inventory and adjust quantities so that blocked partial shipments can complete.

**Why this priority**: The operational lever for the 011 partial-shipment flow; replaces the legacy supplier receiving UI.

**Independent Test**: Find an item with insufficient stock blocking a `SHIPPED_PART` order, increase its inventory, and verify the order completes after the next fulfillment run.

**Acceptance Scenarios**:

1. **Given** the inventory view, **When** it opens, **Then** items are listed with item id and on-hand quantity.
2. **Given** an item row, **When** the administrator sets a new non-negative quantity, **Then** the API-confirmed value is displayed.
3. **Given** invalid input (negative or non-numeric), **When** submission is attempted, **Then** the UI rejects it with a clear message.

---

### Edge Cases

- A storefront customer identity must not be able to open any admin view (route guard plus backend authorization).
- Two administrators deciding the same order concurrently: the second decision shows the invalid-transition error from feature 010.
- Empty pending queue and empty inventory list show clear empty states.
- Backend unavailable on any admin view shows the standard unavailable state.
- Deep links and refresh on every admin route.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The admin UI MUST require administrative sign-in distinct from storefront customer sign-in (per 010 DP-002).
- **FR-002**: The admin UI MUST list `PENDING` orders with id, date, customer, total, and currency.
- **FR-003**: The admin UI MUST allow approving and denying pending orders, individually and for a multi-selection, with per-order outcome feedback.
- **FR-004**: The admin UI MUST allow browsing orders filtered by any legacy status value.
- **FR-005**: The admin order details view MUST show lines, totals, customer contact info, current status, and transition history.
- **FR-006**: The admin UI MUST list supplier inventory and allow setting non-negative quantities via the feature-011 operations.
- **FR-007**: Admin routes MUST be inaccessible to anonymous and customer identities, with backend authorization as the enforcement of record.
- **FR-008**: The admin UI MUST render loading, empty, error, and unavailable states consistently with the storefront patterns.
- **FR-009**: The admin surface MUST be separated from the storefront UX (separate route area or separate app per DP-001 below).

### Key Entities *(include if feature involves data)*

- **Admin Identity State**: Signed-in administrator distinct from storefront customers.
- **Pending Queue View State**: Pending orders plus selection and per-order decision outcomes.
- **Inventory View State**: Item quantities and edit states.

### Decision Points *(must be resolved in plan phase)*

- **DP-001**: Separate Angular application vs an admin route area inside `frontend/petstore-ui` with its own guard; legacy used a fully separate app.
- **DP-002**: Whether order statistics/charts from the legacy admin client are migrated, simplified to counts per status, or dropped with rationale.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: An administrator can sign in, approve one above-threshold order and deny another, and both reflect correctly for the owning customers.
- **SC-002**: A customer identity cannot reach any admin view or operation.
- **SC-003**: Inventory adjustment through the UI unblocks a `SHIPPED_PART` order to completion.
- **SC-004**: Orders can be browsed by every status value with correct filtering.
- **SC-005**: The pending-decision round trip (list → decide → list refresh) completes in under ten seconds locally.

## Assumptions

- Requires features 010 and 011 (decision and inventory operations) and reuses read models from 008.
- Manual validation remains the UI test approach unless the plan decides otherwise.
- The legacy Swing client is retired without visual parity; only its capabilities are preserved.
- Localization of the admin surface is out of scope.
