# Research: Admin Sales Analytics API

## Decision: Combined analytics endpoint

Use one admin-only date-range endpoint returning both revenue share and sales
count metrics.

**Rationale**: The migrated UI needs both chart types for the same date range.
One endpoint keeps the UI simple, avoids duplicate queries, and still preserves
the legacy meaning of separate `REVENUE` and `ORDERS` requests.

**Alternatives considered**:

- Separate `revenue` and `orders` endpoints matching legacy request types:
  closer protocol parity, but more round trips and duplicated filtering.
- Raw orders endpoint with client-side aggregation: rejected because business
  totals must be backend-owned and testable.

## Decision: Qualifying sales statuses

Count `COMPLETED`, `SHIPPED`, and `SHIPPED_PART`; exclude `PENDING`, `APPROVED`,
and `DENIED`.

**Rationale**: The chart is sales analytics, not approval forecasting. Shipped
and completed states represent accepted fulfillment activity. Pending/approved
orders may still be denied or blocked.

**Alternatives considered**:

- Only `COMPLETED`: cleanest accounting definition, but hides active shipped
  sales in the current demo flow.
- Include `APPROVED`: more optimistic, but overstates sales before fulfillment.

## Decision: Date filtering source

Filter by order creation/placement date.

**Rationale**: This date is available for every migrated order. The current data
model does not require a completion timestamp to preserve the visible legacy
date-range behavior.

**Alternatives considered**:

- Completion date: better accounting semantics, but unavailable for all current
  migrated orders without additional schema.
- Transition date: more complex and would require status-specific semantics.

## Decision: Category labels

Use category id from the order line and join current catalog metadata for the
display name when available.

**Rationale**: The order line category id is the historical sales dimension.
Current catalog metadata gives a nicer label without changing totals.

**Alternatives considered**:

- Snapshot category display names on order lines: more historically pure, but
  requires schema changes not needed for this slice.
- Return ids only: simpler, but less useful for the UI.
