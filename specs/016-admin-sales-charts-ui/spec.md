# Feature Specification: Admin Sales Charts UI

**Feature Branch**: `016-admin-sales-charts-ui-sdd`

**Created**: 2026-06-12

**Status**: Draft

**Input**: User description: "Add a modern Angular admin UI for the legacy sales charts. It should show two chart types, like the old admin app: a pie chart for percent of sales per category and a bar chart for number of sales per category. Use a free chart framework if appropriate, or build a simple custom chart if that is better. Capture what data the UI needs so another agent can implement it cleanly."

## Dependencies

- Requires admin analytics data from `specs/015-admin-sales-analytics`.
- Builds on the current Angular application, admin identity, and admin navigation from `specs/003-catalog-angular-ui`, `specs/005-account-ui`, `specs/012-admin-ui`, and `specs/014-supplier-ui-access`.

## Legacy Evidence *(Principle IV)*

- Legacy admin sales UI appears inside the Swing rich client launched from `/admin/AdminRequestProcessor`.
- The legacy sales area contains two tabs:
  - `Pie Chart`: "Showing total % of sales per category"
  - `Bar Chart`: "Showing total # of sales per category"
- The legacy controls allow the administrator to set a start date, end date, and click `Get Data`.
- The legacy client uses hand-drawn Java2D charts. The migrated UI should preserve the business meaning, not the old visual style.
- Legacy category examples visible in charts include `CATS`, `FISH`, and `BIRDS`; categories without sales are not shown in the chart output.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - View Revenue Share Pie Chart (Priority: P1)

As a store administrator, I want a modern pie or donut chart showing revenue share by category so that I can quickly understand which categories contribute the most sales value.

**Why this priority**: This replaces one of the two legacy chart tabs and makes backend revenue analytics visible to the operator.

**Independent Test**: Sign in as admin, open the sales dashboard, choose a date range with known category revenue, and verify the chart and legend show each category's share and amount.

**Acceptance Scenarios**:

1. **Given** an admin is signed in, **When** the sales dashboard opens, **Then** the revenue share chart is available with a default date range.
2. **Given** revenue analytics contain multiple categories, **When** the chart renders, **Then** each category appears with a distinguishable color, label, amount, and percentage.
3. **Given** the administrator changes the date range and requests data, **When** the backend returns new analytics, **Then** the chart updates without requiring a page refresh.

---

### User Story 2 - View Sales Count Bar Chart (Priority: P1)

As a store administrator, I want a bar chart showing sales count by category so that I can compare volume independently from revenue.

**Why this priority**: This replaces the second legacy chart tab and gives a different business view of the same sales period.

**Independent Test**: Sign in as admin, open the sales dashboard, choose a date range with known category quantities, and verify each bar reflects the expected count.

**Acceptance Scenarios**:

1. **Given** count analytics contain category values, **When** the bar chart renders, **Then** each category appears on the category axis with a count value.
2. **Given** one category has a higher sales count than another, **When** both are displayed, **Then** the relative bar heights communicate the difference clearly.
3. **Given** the administrator switches between chart views or tabs, **When** data has already loaded, **Then** the UI does not lose the selected date range.

---

### User Story 3 - Handle No Data, Loading, And Access Boundaries (Priority: P1)

As an administrator or non-admin user, I want clear states for loading, no data, errors, and unauthorized access so that the analytics area is understandable and protected.

**Why this priority**: Charts can otherwise fail silently or expose confusing empty canvases; sales analytics must remain admin-only.

**Independent Test**: Exercise the dashboard with an empty date range, backend unavailable, forbidden user, and successful admin user.

**Acceptance Scenarios**:

1. **Given** the selected range has no sales, **When** analytics load, **Then** the UI shows a no-data state and no misleading chart segments or bars.
2. **Given** analytics are loading, **When** the request is in progress, **Then** the UI shows a loading state that does not resize the page unexpectedly.
3. **Given** a supplier, customer, or anonymous user opens the sales dashboard route, **When** route guards and backend authorization are evaluated, **Then** sales data is not rendered.
4. **Given** the backend returns an error, **When** the UI receives it, **Then** the dashboard shows an actionable error state and preserves the last selected date range.

---

### Edge Cases

- Start date is after end date.
- Date fields are empty or contain invalid values.
- The backend returns zero categories.
- The backend returns a category id without a display name.
- Many categories are returned, making labels crowded.
- One category dominates revenue or count.
- Browser refresh on the sales dashboard route.
- The backend is older and does not expose analytics endpoints yet.
- Chart rendering must remain usable on common laptop and mobile-width viewports.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The admin UI MUST provide a sales analytics entry point available to admin identities only.
- **FR-002**: The UI MUST allow the administrator to choose a start date and end date and request refreshed analytics.
- **FR-003**: The UI MUST show revenue share by category as a pie, donut, or equivalent part-to-whole chart.
- **FR-004**: The UI MUST show sales count by category as a bar chart.
- **FR-005**: The UI MUST display category label, revenue amount, revenue percentage, and sales count in readable text outside or alongside the charts so the information is not only color-dependent.
- **FR-006**: The UI MUST preserve the legacy meaning of the two chart types: revenue percentage for the pie-style chart and sold quantity count for the bar chart.
- **FR-007**: The UI MUST handle loading, empty, validation, unavailable, and forbidden states consistently with the existing Angular app.
- **FR-008**: The UI MUST not expose sales analytics links or data to supplier, customer, or anonymous identities.
- **FR-009**: The UI MUST consume backend analytics data from feature 015 rather than recalculating business metrics from raw orders in the browser.
- **FR-010**: The UI plan MUST choose either a free Angular-compatible chart library or a custom chart implementation, with rationale based on bundle size, accessibility, maintainability, and visual quality.
- **FR-011**: Manual UI validation MUST cover both chart types, date filtering, empty data, and role/access behavior.

### Key Entities *(include if feature involves data)*

- **Sales Dashboard View State**: Selected date range, loading/error/empty states, and returned analytics summary.
- **Revenue Share Chart State**: Category rows with display label, color, revenue amount, and percent.
- **Sales Count Chart State**: Category rows with display label, color, and count.
- **Chart Rendering Strategy**: The selected approach for rendering charts in Angular, decided during planning.

### Decision Points *(must be resolved in plan phase)*

- **DP-001**: Which chart rendering approach should be used: a free Angular-compatible chart library, a lightweight wrapper over a general chart library, or custom SVG/CSS charts?
- **DP-002**: Should the sales dashboard live as a new tab under `/admin`, a separate `/admin/sales` route, or another privileged navigation location?
- **DP-003**: What default date range should the UI use on first load: current month, last 30 days, all available data, or a legacy demo-friendly range?
- **DP-004**: Should the UI show pie and bar charts side by side on wide screens, as tabs matching the legacy app, or as stacked sections?

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: An admin can open sales analytics, select a date range, and see both revenue share and sales count charts without leaving the dashboard.
- **SC-002**: Revenue percentages shown in the UI match backend analytics for the selected date range.
- **SC-003**: Bar chart counts shown in the UI match backend analytics for the selected date range.
- **SC-004**: Supplier, customer, and anonymous users cannot see sales analytics navigation or chart data.
- **SC-005**: A no-data date range produces a clear empty state rather than a blank or misleading chart.

## Assumptions

- This feature is UI-only; backend aggregation, status policy, and authorization enforcement belong to `specs/015-admin-sales-analytics`.
- The migrated UI should look modern and clean rather than copying Swing chart visuals.
- A free charting solution is acceptable if planning selects one; custom charts are acceptable if they keep the implementation simpler and accessible.
- Automated UI tests are not required unless the plan changes the project's current UI testing approach.
