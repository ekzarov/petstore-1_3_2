# Research: Admin Sales Charts UI

## Decision: Custom SVG/CSS charts

Build a lightweight custom donut/pie-style chart and bar chart with SVG/CSS in
Angular components.

**Rationale**: This feature needs only two simple chart types. Avoiding a new
chart dependency reduces bundle churn, removes Angular-version compatibility
risk, and gives full control over the current PetStore visual style.

**Alternatives considered**:

- Chart.js with an Angular wrapper: free and mature, but adds dependency and
  wrapper compatibility considerations for Angular 21.
- D3: powerful, but heavier and more complex than needed for two static charts.
- Pure tables only: accessible but loses the visual parity of the legacy charts.

## Decision: Dedicated admin sales route

Place the dashboard at `/admin/sales`.

**Rationale**: Sales analytics belong to the admin workspace but are distinct
from order decisions. A route keeps direct links and refresh behavior clean.

**Alternatives considered**:

- Add as a tab inside existing admin order page: easier discovery but crowds the
  order workflow.
- Separate top-level `/sales`: less clear authorization boundary.

## Decision: Broad demo-friendly default date range

Use `2001-01-01` through current local date as the initial range.

**Rationale**: Legacy PetStore demo data can be broad and old. A wide default
maximizes the chance that the first chart render has meaningful data.

**Alternatives considered**:

- Current month or last 30 days: common in real dashboards, but likely empty in
  local demo data.
- Require user input before loading: extra friction for demos.

## Decision: Text-first chart accessibility

Every chart value must also appear in a readable legend/table.

**Rationale**: Color-only charts are inaccessible and difficult to verify. Text
rows also help during manual validation.

**Alternatives considered**:

- Tooltips only: not enough for keyboard/mobile users.
- Labels inside chart segments only: can become cramped.
