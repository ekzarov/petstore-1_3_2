# Feature Specification: Supplier Fulfillment

**Feature Branch**: `011-supplier-fulfillment-sdd`

**Created**: 2026-06-10

**Status**: Draft

**Input**: User description: "Migrate the legacy Supplier behavior: approved orders become supplier purchase orders, inventory is checked and decremented on shipment, shipped lines produce invoices back to order processing, and order statuses advance through SHIPPED_PART, SHIPPED, and COMPLETED, preserving the legacy partial-shipment behavior when inventory is insufficient."

## Legacy Evidence *(Principle IV)*

- Legacy apps/components: `src/apps/supplier` (supplier app with inventory and receiver), `src/components/supplierpo` (supplier purchase orders), `src/components/xmldocuments` (XML PO/invoice exchange), OPC invoice handling (`src/apps/opc` transitions to shipped/completed).
- Legacy flow: OPC sends approved orders to the Supplier via JMS as XML purchase orders; the Supplier checks inventory, ships what is available, decrements stock, and returns invoices; OPC consumes invoices and advances order status; insufficient stock produces partial shipment and `SHIPPED_PART` until remaining lines ship; fully shipped and invoiced orders become `COMPLETED`.
- Supplier inventory is seeded per item id; the legacy admin/supplier UI allows inventory adjustment.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Fulfill An Approved Order With Sufficient Stock (Priority: P1)

As the store operator, I want approved orders to ship automatically when inventory is available so that the happy-path lifecycle completes without manual work.

**Why this priority**: This is the legacy happy path proving the approval-to-completion pipeline end to end.

**Independent Test**: Seed sufficient inventory for `EST-1`, drive an order to `APPROVED`, run fulfillment, and verify the order reaches `COMPLETED` and inventory decreased by the ordered quantity.

**Acceptance Scenarios**:

1. **Given** an `APPROVED` order whose lines are all in stock, **When** fulfillment processes it, **Then** stock is decremented, all lines are marked shipped, and the order advances to `SHIPPED` and then `COMPLETED` per the invoice handling rules.
2. **Given** a `DENIED` or `PENDING` order, **When** fulfillment runs, **Then** the order is never picked up.

---

### User Story 2 - Partial Shipment On Insufficient Stock (Priority: P1)

As the store operator, I want orders with partially available stock to ship what is available and complete later so that customers receive goods as soon as possible, matching legacy `SHIPPED_PART` behavior.

**Why this priority**: Partial shipment is the distinctive legacy supplier behavior that parity checks must cover.

**Independent Test**: Seed stock covering only one of two ordered lines, run fulfillment, verify `SHIPPED_PART`; replenish stock, run again, verify the order completes.

**Acceptance Scenarios**:

1. **Given** an approved order where only some lines (or quantities) are in stock, **When** fulfillment processes it, **Then** available quantities ship, stock decrements only for shipped quantities, and the order status becomes `SHIPPED_PART`.
2. **Given** a `SHIPPED_PART` order and replenished inventory, **When** fulfillment processes the remainder, **Then** the remaining quantities ship and the order advances to `SHIPPED`/`COMPLETED`.
3. **Given** no stock for any line, **When** fulfillment runs, **Then** nothing ships, stock is unchanged, and the order stays `APPROVED` awaiting inventory.

---

### User Story 3 - Manage Supplier Inventory (Priority: P2)

As a store administrator, I want to view and adjust supplier inventory per item so that fulfillment can be unblocked when stock arrives.

**Why this priority**: Inventory adjustment is the operational lever for the partial-shipment scenarios; consumed later by the admin UI (012).

**Independent Test**: Read inventory for `EST-1`, set a new quantity, and verify the new value is returned and used by the next fulfillment run.

**Acceptance Scenarios**:

1. **Given** an authorized operator, **When** inventory for an item is read, **Then** the current on-hand quantity is returned.
2. **Given** an authorized operator, **When** inventory is set to a new non-negative quantity, **Then** subsequent reads and fulfillment use the new value.
3. **Given** a storefront customer identity, **When** inventory operations are attempted, **Then** they are rejected.

---

### Edge Cases

- Concurrent fulfillment of two orders competing for the same stock must never ship more than on-hand quantity.
- Re-running fulfillment on an already-shipped order must not double-decrement stock (idempotency).
- Inventory rows missing for an ordered item id are treated as zero stock, not an error.
- Invoice/status handoff failures must leave a consistent state that a later run can repair.
- Negative inventory adjustments are rejected; setting stock to zero is allowed.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Fulfillment MUST pick up only `APPROVED` (and `SHIPPED_PART`) orders from the feature-010 handoff.
- **FR-002**: Fulfillment MUST ship line quantities only up to available stock and decrement stock by exactly the shipped quantities.
- **FR-003**: Orders fully shipped MUST advance to `SHIPPED` and then `COMPLETED` per the invoice handling rule defined in the plan.
- **FR-004**: Orders partially shipped MUST advance to `SHIPPED_PART` and remain eligible for later completion.
- **FR-005**: Orders with nothing shippable MUST remain `APPROVED` without state corruption.
- **FR-006**: Each shipment MUST produce an invoice record (order id, shipped lines and quantities, timestamp) driving the status transitions.
- **FR-007**: Status transitions MUST go through the feature-010 transition rules and audit records; fulfillment must not bypass them.
- **FR-008**: Inventory MUST be readable and settable per item id by authorized operators only.
- **FR-009**: Fulfillment MUST be idempotent and safe under concurrent execution.
- **FR-010**: Stock MUST never go negative.

### Key Entities *(include if feature involves data)*

- **Supplier Inventory**: Item id and on-hand quantity.
- **Supplier Purchase Order / Shipment**: The fulfillment work derived from an approved order, with shipped quantities per line.
- **Invoice Record**: Shipment evidence consumed by order status transitions.

### Decision Points *(must be resolved in plan phase)*

- **DP-001**: Fulfillment trigger — processed inline with approval, on a schedule, or message-driven (must align with 010 DP-001).
- **DP-002**: Supplier as a separate deployable vs a module inside the single migrated backend; legacy ran it as a separate EAR, but the migration may consolidate with documented rationale.
- **DP-003**: Inventory seed values for parity testing (legacy seeds per item id).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: An approved order with sufficient stock reaches `COMPLETED` and stock decreases by the ordered quantities.
- **SC-002**: An approved order with partial stock reaches `SHIPPED_PART`, then `COMPLETED` after replenishment, with arithmetically correct stock at each step.
- **SC-003**: Two orders competing for limited stock never ship more than was on hand in total.
- **SC-004**: Re-running fulfillment changes nothing for already-shipped quantities.
- **SC-005**: The customer sees `SHIPPED_PART`/`SHIPPED`/`COMPLETED` through the unchanged 008 read APIs.

## Assumptions

- Requires features 008 and 010.
- Legacy XML PO/invoice document formats are internal mechanics, not preserved contracts, unless the plan identifies an external consumer.
- The legacy supplier web UI for receiving stock is replaced by the inventory operations here plus the admin UI (012).
- Automated backend tests are required per Constitution Principle VI: unit tests for shipment/stock arithmetic, integration tests for concurrency, idempotency, and the status handoff.
