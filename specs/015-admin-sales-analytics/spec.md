# Feature Specification: Admin Sales Analytics API

**Feature Branch**: `015-admin-sales-analytics-sdd`

**Created**: 2026-06-12

**Status**: Draft

**Input**: User description: "Add backend support for the legacy admin sales charts. The old admin client shows two chart types: a pie chart for total percent of sales per category and a bar chart for total number of sales per category over a selected date range. Create a backend feature that exposes the same data clearly so the Angular admin UI can render it beautifully later."

## Dependencies

- Requires order, order-line, status, and admin authorization capabilities from `specs/008-checkout-orders`, `specs/010-order-processing`, and `specs/013-supplier-role-access`.
- Provides data for the UI feature in `specs/016-admin-sales-charts-ui`.

## Clarifications

### Session 2026-06-12 (resolved in plan.md Design Decisions)

- DP-001 -> Sales statuses: `COMPLETED`, `SHIPPED`, `SHIPPED_PART`; exclude `PENDING`, `APPROVED`, `DENIED` (DD-001).
- DP-002 -> One combined endpoint returning both metrics per category (DD-002).
- DP-003 -> Filter by order placement date (DD-003).
- DP-004 -> Category id resolved from order line data joined with current catalog; display name falls back to the id when metadata is missing (DD-004).

## Legacy Evidence *(Principle IV)*

- Legacy admin app: `src/apps/admin` launches a Swing rich client from `/admin/AdminRequestProcessor`.
- Legacy admin sales action has two tabs: `Pie Chart` and `Bar Chart`.
- Legacy labels in `src/apps/admin/src/client/resources/petstore.properties` define:
  - `PieChart.description=Showing total % of sales per category`
  - `BarChart.description=Showing total # of sales per category`
- Legacy client data calls in `src/apps/admin/src/client/com/sun/j2ee/blueprints/admin/client/DataSource.java` show:
  - pie chart data uses `server.getRevenue(start, end, category)`
  - bar chart data uses `server.getOrders(start, end, category)`
- Legacy backend accepts XML request types `REVENUE` and `ORDERS` in `src/apps/admin/src/admin/com/sun/j2ee/blueprints/admin/web/ApplRequestProcessor.java` and delegates to the OPC admin facade.
- Legacy line items carry category id, quantity, and unit price; the migrated backend must calculate sales analytics from persisted order/order-line data rather than catalog seed constants.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Revenue Share By Category (Priority: P1)

As a store administrator, I want category revenue totals and percentages for a selected date range so that I can understand which categories drive sales value.

**Why this priority**: This is the legacy pie chart's core business meaning and the most visible missing capability from the old admin sales dashboard.

**Independent Test**: Create completed or processed orders across multiple categories with known prices and quantities, request analytics for a date range that includes them, and verify category revenue totals, percentages, and grand total match the expected amounts.

**Acceptance Scenarios**:

1. **Given** orders with line items in `CATS`, `FISH`, and `BIRDS`, **When** an administrator requests revenue analytics for a date range covering those orders, **Then** the response contains each category with revenue amount, percent of total revenue, and total revenue for the range.
2. **Given** an order outside the requested date range, **When** revenue analytics are requested, **Then** that order's line items do not affect returned totals or percentages.
3. **Given** no qualifying sales in the requested range, **When** revenue analytics are requested, **Then** the response returns an empty category list and zero total revenue.

---

### User Story 2 - Sales Count By Category (Priority: P1)

As a store administrator, I want category sales counts for a selected date range so that I can compare category volume independent of revenue.

**Why this priority**: This is the legacy bar chart's core business meaning and complements revenue share with operational volume.

**Independent Test**: Create orders with line-item quantities across multiple categories, request count analytics, and verify counts are aggregated by category using ordered quantities.

**Acceptance Scenarios**:

1. **Given** multiple order lines in the same category, **When** sales count analytics are requested, **Then** the category count equals the sum of sold quantities for those lines.
2. **Given** multiple categories with sales, **When** sales count analytics are requested, **Then** categories are returned with count values suitable for a bar chart.
3. **Given** no qualifying sales in the requested range, **When** count analytics are requested, **Then** the response returns an empty category list and zero total count.

---

### User Story 3 - Admin-Only Access (Priority: P1)

As a store administrator, I want sales analytics protected as an admin capability so that supplier, customer, and anonymous users cannot inspect business sales data.

**Why this priority**: Sales analytics reveal operational business information and belong to the legacy admin responsibility boundary.

**Independent Test**: Call the analytics endpoints as admin, supplier, customer, and anonymous users; verify only admin receives data.

**Acceptance Scenarios**:

1. **Given** an admin identity, **When** it requests sales analytics, **Then** the API returns the requested data.
2. **Given** a supplier identity, **When** it requests sales analytics, **Then** the API rejects the request as forbidden.
3. **Given** a customer or anonymous caller, **When** it requests sales analytics, **Then** the API rejects the request without leaking sales data.

---

### Edge Cases

- The requested start date is after the end date.
- The date range is valid but contains no orders.
- A category exists in the catalog but has no sales in the selected range.
- Orders contain multiple line items from the same category.
- Orders contain line items with quantity greater than one.
- Orders have statuses that should not represent final or accepted sales.
- Monetary totals require stable rounding for display and tests.
- Category ids exist on line items even if catalog display metadata changes later.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The backend MUST expose admin-only sales analytics for a caller-provided date range.
- **FR-002**: The backend MUST calculate revenue per category from persisted order line quantity and unit price.
- **FR-003**: The backend MUST calculate revenue percentage per category as the category revenue divided by total revenue for the requested range.
- **FR-004**: The backend MUST calculate sales count per category from persisted order line quantities.
- **FR-005**: Analytics MUST exclude orders outside the requested date range.
- **FR-006**: Analytics MUST include only statuses selected in the plan as qualifying sales statuses, with the selected status policy documented and tested.
- **FR-007**: The response MUST include enough data for chart rendering without requiring the UI to recalculate business totals: category id, category display name when available, revenue amount, revenue percentage, sales count, total revenue, and total sales count.
- **FR-008**: Empty ranges MUST return an explicit empty result with zero totals rather than an error.
- **FR-009**: Invalid date ranges MUST return a validation error.
- **FR-010**: Backend tests MUST cover aggregation math, date filtering, status filtering, authorization boundaries, empty results, and representative legacy category values.
- **FR-011**: Analytics data MUST be read-only and MUST NOT change orders, inventory, fulfillment state, or catalog data.

### Key Entities *(include if feature involves data)*

- **Sales Analytics Request**: Date range and metric selection used to retrieve admin sales analytics.
- **Category Revenue Metric**: Category id/name, revenue amount, and percent of total revenue.
- **Category Sales Count Metric**: Category id/name and summed sold quantity.
- **Sales Analytics Summary**: Overall totals and category rows returned to the UI.

### Decision Points *(must be resolved in plan phase)*

- **DP-001**: Which order statuses count as sales for analytics: only `COMPLETED`, or also `APPROVED`, `SHIPPED_PART`, and `SHIPPED`?
- **DP-002**: Should the backend return one combined response containing both revenue and count metrics, or separate endpoints/queries matching the legacy `REVENUE` and `ORDERS` request types?
- **DP-003**: Should date filtering use order creation date, completion date, or the closest migrated equivalent when completion date is unavailable?
- **DP-004**: Should category display names be snapshotted from order lines, joined from current catalog data, or returned as ids only when catalog metadata is missing?

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: An administrator can retrieve category revenue percentages for a selected local date range and the percentages sum to 100% when sales exist.
- **SC-002**: An administrator can retrieve category sales counts for the same range and values match known seeded/test orders.
- **SC-003**: Supplier, customer, and anonymous callers cannot retrieve analytics data.
- **SC-004**: Empty date ranges return a valid zero-data response in under two seconds locally.
- **SC-005**: Automated backend tests fail if line-item quantity, unit price, date range, status policy, or role policy is applied incorrectly.

## Assumptions

- This feature is backend-only; Angular chart rendering belongs to `specs/016-admin-sales-charts-ui`.
- The legacy visual design is not preserved; only the analytics meaning and admin-only capability are preserved.
- Automated backend tests are required by Constitution Principle VI.
- Analytics are read from the migrated SQL Server database used by the .NET backend.
- The first implementation can support category-level analytics only; item-level drilldown from the legacy admin protocol is out of scope unless added by a later spec.
