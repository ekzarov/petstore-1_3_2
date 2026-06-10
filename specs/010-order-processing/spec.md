# Feature Specification: Order Processing And Approval

**Feature Branch**: `010-order-processing-sdd`

**Created**: 2026-06-10

**Status**: Draft

**Input**: User description: "Migrate the legacy OPC (Order Processing Center) behavior: placed orders move through the legacy status lifecycle — small orders are approved automatically, larger orders await an explicit approval decision, denied orders stop, approved orders are handed to supplier fulfillment, and customers can see status changes in their order history. Includes the customer notification behavior as a decision point."

## Clarifications

### Session 2026-06-10

- Q: Admin identity (DP-002)? -> A: Role column in the shared users table from feature 004; decision endpoints require the `admin` role via JWT.

## Legacy Evidence *(Principle IV)*

- Legacy apps/components: `src/apps/opc` (purchase order intake, customer relations, transitions), `src/components/processmanager` (workflow state per order), `src/components/asyncsender` (JMS submission), `src/components/mailer` (JavaMail notifications), `src/components/xmldocuments` (XML purchase order/invoice documents with XSD/DTD validation).
- Legacy flow: petstore submits the purchase order via JMS; OPC persists it, the process manager tracks workflow status; orders below a locale-specific threshold are auto-approved, others wait for an admin decision; approved orders generate supplier purchase orders; supplier invoices drive `SHIPPED_PART`/`SHIPPED`/`COMPLETED` transitions; customers receive e-mail notifications at key transitions.
- Status vocabulary: `PENDING`, `APPROVED`, `DENIED`, `SHIPPED_PART`, `SHIPPED`, `COMPLETED` (from `OrderStatusNames`).

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Automatic Approval Of Small Orders (Priority: P1)

As the store operator, I want small orders approved automatically so that routine purchases flow to fulfillment without manual work, matching legacy behavior.

**Why this priority**: Auto-approval is the default path of the legacy lifecycle and the first observable status transition.

**Independent Test**: Place an order below the auto-approval threshold and verify its status becomes `APPROVED` without any manual action.

**Acceptance Scenarios**:

1. **Given** a placed order with a total below the configured threshold, **When** order processing handles it, **Then** the order status becomes `APPROVED` and the transition is recorded with a timestamp.
2. **Given** a placed order at or above the threshold, **When** processing handles it, **Then** the order remains `PENDING` awaiting a manual decision.

---

### User Story 2 - Manual Approval Or Denial (Priority: P1)

As a store administrator, I want to approve or deny pending orders so that high-value orders get a human decision, matching the legacy admin workflow.

**Why this priority**: Completes the decision half of the lifecycle; the admin UI (feature 012) consumes this capability.

**Independent Test**: Place an above-threshold order, approve it through the processing API, verify `APPROVED`; repeat with denial and verify `DENIED`.

**Acceptance Scenarios**:

1. **Given** a `PENDING` order, **When** an authorized operator approves it, **Then** the status becomes `APPROVED` and the order proceeds toward fulfillment.
2. **Given** a `PENDING` order, **When** an authorized operator denies it, **Then** the status becomes `DENIED` and the order is excluded from fulfillment.
3. **Given** an order not in `PENDING`, **When** an approval or denial is attempted, **Then** the API rejects the invalid transition with a clear error.
4. **Given** an unauthorized caller, **When** a decision is attempted, **Then** the API rejects it.

---

### User Story 3 - Status Visibility For Customers (Priority: P2)

As a customer, I want my order history to reflect processing status changes so that I can see whether my order was approved, denied, or is progressing.

**Why this priority**: Reuses the order read APIs from 008; verifies the lifecycle is observable end to end.

**Independent Test**: Drive one order to `APPROVED` and another to `DENIED`, then verify the owning customer sees both statuses in order history without UI changes.

**Acceptance Scenarios**:

1. **Given** an order whose status changed, **When** the customer views order history or order details, **Then** the new status is shown.
2. **Given** a status transition, **When** it occurs, **Then** an auditable transition record (from-status, to-status, timestamp, actor or `system`) exists for the order.

---

### Edge Cases

- Two concurrent decisions on the same order must produce exactly one final state.
- Processing must be safe to re-run on an already-processed order (idempotent transitions; duplicate handling must not corrupt status).
- Threshold configuration changes must not retroactively change already-decided orders.
- Orders denied must never be handed to fulfillment (feature 011).
- Notification failures must not roll back or block the status transition (if notifications are in scope per DP-003).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST evaluate each newly placed order and auto-approve it when its total is below a configurable threshold.
- **FR-002**: Orders at or above the threshold MUST remain `PENDING` until an explicit decision.
- **FR-003**: The system MUST provide authorized operations to approve or deny a `PENDING` order.
- **FR-004**: Status transitions MUST follow the legacy vocabulary and legal transition graph: `PENDING → APPROVED | DENIED`; `APPROVED → SHIPPED_PART | SHIPPED` and `SHIPPED → COMPLETED` are reserved for feature 011.
- **FR-005**: Illegal transitions MUST be rejected without state change.
- **FR-006**: Every transition MUST be recorded with from-status, to-status, timestamp, and actor (`system` for auto-approval).
- **FR-007**: Approved orders MUST become available to the supplier fulfillment slice (feature 011) through a defined handoff.
- **FR-008**: Denied orders MUST be terminal and excluded from fulfillment.
- **FR-009**: Decision operations MUST be restricted to administrative identities, not storefront customers.
- **FR-010**: Customers MUST see current status through the existing order read APIs (008) with no contract change.
- **FR-011**: Processing MUST be idempotent under repeated evaluation of the same order.

### Key Entities *(include if feature involves data)*

- **Order Workflow State**: Current status per order plus its transition history.
- **Approval Threshold**: Configured monetary limit (and currency) controlling auto-approval.
- **Status Transition Record**: From-status, to-status, timestamp, actor.

### Decision Points *(must be resolved in plan phase)*

- **DP-001**: Processing trigger — synchronous evaluation at placement vs background processing (in-process queue, hosted service, or an external broker as the JMS replacement). This is the migration's messaging decision; the legacy used JMS, but this slice may legitimately start synchronous/in-process if documented.
- **DP-002**: Admin identity — how administrative callers authenticate in this slice (precedes the admin UI in feature 012).
- **DP-003**: Customer notifications — whether legacy order e-mail notifications (mailer) are migrated in this slice, deferred to a later slice, or dropped intentionally; if migrated, the e-mail transport choice.
- **DP-004**: Threshold parity — single global threshold vs the legacy locale-dependent thresholds; the chosen value(s) and currency handling.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: An order below the threshold reaches `APPROVED` without manual action and the transition is recorded as `system`.
- **SC-002**: An order at/above the threshold stays `PENDING` and can be approved or denied exactly once by an authorized operator.
- **SC-003**: Denied orders never appear in the fulfillment handoff.
- **SC-004**: The owning customer sees status changes via the unchanged 008 read APIs.
- **SC-005**: Re-processing an already-decided order changes nothing and produces no duplicate transitions.

## Assumptions

- Requires feature 008 (orders exist with `PENDING` status and self-contained data).
- Supplier fulfillment transitions (`SHIPPED_PART`, `SHIPPED`, `COMPLETED`) are out of scope here and belong to feature 011; this slice ends at `APPROVED`/`DENIED` plus the fulfillment handoff.
- Legacy XML document exchange (XSD/DTD purchase orders, invoices) is treated as an internal legacy mechanism, not a contract to preserve, unless the plan finds an external consumer.
- Automated backend tests are required per Constitution Principle VI: unit tests for threshold and transition rules, integration tests for handoff, idempotency, and authorization boundaries.
