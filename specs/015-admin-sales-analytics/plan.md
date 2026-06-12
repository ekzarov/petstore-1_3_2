# Implementation Plan: Admin Sales Analytics API

**Branch**: `codex/sdd-admin-sales-charts` | **Date**: 2026-06-12 | **Spec**: `specs/015-admin-sales-analytics/spec.md`

## Summary

Add an admin-only backend analytics slice that calculates the two legacy sales
chart metrics from persisted orders and order lines: revenue share by category
for a pie/donut chart and sold quantity count by category for a bar chart.
Expose one combined read-only API response so the Angular UI can render both
charts from the same date-range request.

## Technical Context

**Language/Version**: C# / .NET 10, existing `dotnet/Petstore` project.

**Primary Dependencies**: EF Core SQL Server context, orders from feature 008,
admin authorization from features 010/013.

**Storage**: Existing `Orders`, `OrderLines`, and catalog category/product/item
tables. No new tables or migrations are expected.

**Testing**: xUnit unit tests for aggregation math and date validation;
SQL Server integration/API contract tests categorized with the existing
database integration category for EF query, authorization, empty result, and
status-policy behavior.

**Target Platform**: Local ASP.NET Core backend and SQL Server database used by
the migrated PetStore app.

**Project Type**: Web API backend.

**Performance Goals**: Return local analytics for normal demo-sized PetStore
data in under two seconds; keep aggregation server-side.

**Constraints**: Admin-only, read-only, no order/inventory/catalog mutation,
stable decimal rounding for revenue and percentages.

**Scale/Scope**: Category-level analytics only. Legacy item-level drilldown is
out of scope for this feature.

## Constitution Check

All principles pass.

- Principle II: Preserves legacy admin sales chart behavior while documenting a
  modern API shape.
- Principle IV: Legacy evidence is captured in the spec before replacement.
- Principle VI: Backend runtime behavior includes unit and integration/contract
  tests. Real database tests must use the existing database integration category.

## Design Decisions

- **DD-001 (DP-001)**: Count `COMPLETED`, `SHIPPED`, and `SHIPPED_PART` orders
  as sales. Exclude `PENDING`, `APPROVED`, and `DENIED`. Rationale: pending and
  merely approved orders are not fulfilled sales; partial/fully shipped and
  completed orders represent accepted sales activity.
- **DD-002 (DP-002)**: Use one combined endpoint,
  `GET /api/admin/analytics/sales?startDate=yyyy-MM-dd&endDate=yyyy-MM-dd`.
  The response includes total revenue, total sales count, and per-category
  rows with both metrics. Rationale: the UI needs both charts for the same
  date range, and one response avoids duplicate queries.
- **DD-003 (DP-003)**: Filter by order creation/placement date for the first
  implementation. Rationale: it exists for all migrated orders and matches the
  legacy admin date-range control closely enough for current parity.
- **DD-004 (DP-004)**: Return category id from order line data and category
  display name from current catalog metadata when available. If metadata is
  missing, fall back to the category id.
- **DD-005**: Percentages are calculated as `categoryRevenue / totalRevenue *
  100` and rounded for transport/display to two decimal places. The service
  still keeps decimal arithmetic for tests.
- **DD-006**: Public classes remain one per file. Any constants introduced for
  route names, status sets, precision, or query defaults belong in named
  constants classes rather than hardcoded magic values.

## Project Structure

```text
dotnet/Petstore/
|-- Analytics/
|   |-- IAdminSalesAnalyticsRepository.cs
|   |-- AdminSalesAnalyticsRepository.cs
|   |-- IAdminSalesAnalyticsService.cs
|   `-- AdminSalesAnalyticsService.cs
|-- Controllers/
|   `-- AdminSalesAnalyticsController.cs
|-- Models/
|   |-- AdminSalesAnalyticsDto.cs
|   |-- AdminCategorySalesMetricDto.cs
|   `-- SalesAnalyticsDateRangeRequest.cs
`-- Program.cs

dotnet/Petstore.Tests/
|-- AdminSalesAnalyticsServiceTests.cs
`-- AdminSalesAnalyticsApiContractTests.cs
```

## Data Model

See `specs/015-admin-sales-analytics/data-model.md`.

## API Contract

See `specs/015-admin-sales-analytics/contracts/admin-sales-analytics.md`.

## Quickstart

See `specs/015-admin-sales-analytics/quickstart.md`.

## Post-Design Constitution Check

Still passes. No new infrastructure is introduced, the legacy Docker baseline is
untouched, and backend automated verification is explicitly planned.
