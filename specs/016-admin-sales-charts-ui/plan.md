# Implementation Plan: Admin Sales Charts UI

**Branch**: `codex/sdd-admin-sales-charts` | **Date**: 2026-06-12 | **Spec**: `specs/016-admin-sales-charts-ui/spec.md`

## Summary

Add a modern admin sales dashboard to the Angular app. The dashboard consumes
the backend analytics API from feature 015 and renders two legacy-equivalent
views for a selected date range: revenue share by category and sales count by
category.

## Technical Context

**Language/Version**: TypeScript 5.9 / Angular 21, existing
`frontend/petstore-ui` project.

**Primary Dependencies**: Angular router/forms/http, existing identity/admin
guards and shared loading/empty/unavailable states. No new chart dependency for
the first implementation.

**Storage**: N/A; UI state only.

**Testing**: Manual UI validation for this feature, matching current UI
practice. Backend correctness and authorization are covered by feature 015.

**Target Platform**: Browser UI served by Angular dev server with existing API
proxy to the ASP.NET Core backend.

**Project Type**: Angular frontend.

**Performance Goals**: Render normal demo-sized chart data instantly after the
API response; avoid layout shift between loading, empty, and chart states.

**Constraints**: Admin-only navigation/route access, no client-side business
aggregation from raw orders, accessible chart information not dependent on
color alone.

**Scale/Scope**: One admin sales dashboard route with two chart sections.

## Constitution Check

All principles pass.

- Principle II: Preserves the legacy admin chart capabilities at behavior level.
- Principle IV: The spec records legacy chart evidence before replacement.
- Principle VI: Backend tests belong to feature 015; this UI feature keeps the
  current manual UI validation approach unless project policy changes.

## Design Decisions

- **DD-001 (DP-001)**: Use custom accessible SVG/CSS charts instead of adding a
  chart library in this slice. Rationale: only one donut/pie-style chart and one
  bar chart are needed, Angular 21 library compatibility may add friction, and a
  custom implementation avoids bundle and dependency churn while keeping full
  styling control. A later feature may replace this with Chart.js or another
  free library if chart needs grow.
- **DD-002 (DP-002)**: Add a dedicated `/admin/sales` route and a `Sales` entry
  in the existing admin navigation. Rationale: keeps charts with admin order
  tools without crowding pending/all-orders pages.
- **DD-003 (DP-003)**: Default date range is all demo-friendly data from
  `2001-01-01` through the current local date. Rationale: legacy demos often
  used broad date ranges and local demo data may have old seeded or migrated
  dates.
- **DD-004 (DP-004)**: Show charts as stacked responsive sections, not legacy
  tabs. On wider screens they may sit in a two-column layout if space allows.
  Rationale: the migrated UI should be modern and scan-friendly while preserving
  both chart types on one dashboard.

## Project Structure

```text
frontend/petstore-ui/src/app/admin/
|-- admin.models.ts
|-- admin-api.service.ts
|-- admin-shell.component.ts
`-- sales-dashboard.component.ts

frontend/petstore-ui/src/app/
`-- app.routes.ts

frontend/petstore-ui/src/
`-- styles.css
```

## Data Model

See `specs/016-admin-sales-charts-ui/data-model.md`.

## UI Contract

See `specs/016-admin-sales-charts-ui/contracts/admin-sales-charts-ui.md`.

## Quickstart

See `specs/016-admin-sales-charts-ui/quickstart.md`.

## Post-Design Constitution Check

Still passes. The UI depends on the tested backend analytics API and does not
alter the legacy Docker baseline or backend runtime behavior.
