# Tasks: Admin Sales Analytics API

**Input**: `specs/015-admin-sales-analytics/plan.md`, `specs/015-admin-sales-analytics/spec.md`

**Tests**: Automated backend unit and integration/API contract tests are required by Constitution Principle VI.

## Phase 1: Setup

- [X] T001 Review existing order, order-line, category, and admin authorization code in `dotnet/Petstore/Data/Entities/OrderEntity.cs`, `dotnet/Petstore/Data/Entities/OrderLineEntity.cs`, `dotnet/Petstore/Controllers/AdminOrdersController.cs`, and `dotnet/Petstore/Program.cs`
- [X] T002 Create analytics folder structure with one public class per file under `dotnet/Petstore/Analytics/`

## Phase 2: Foundational

- [X] T003 Add sales analytics DTO files in `dotnet/Petstore/Models/AdminSalesAnalyticsDto.cs`, `dotnet/Petstore/Models/AdminCategorySalesMetricDto.cs`, and `dotnet/Petstore/Models/SalesAnalyticsDateRangeRequest.cs`
- [X] T004 Add analytics constants for included statuses, rounding, and route names in `dotnet/Petstore/Analytics/SalesAnalyticsConstants.cs`
- [X] T005 Add repository/service interfaces in `dotnet/Petstore/Analytics/IAdminSalesAnalyticsRepository.cs` and `dotnet/Petstore/Analytics/IAdminSalesAnalyticsService.cs`

## Phase 3: User Story 1 - Revenue Share By Category (P1) MVP

**Goal**: Admin can retrieve category revenue totals and percentages for a date range.

**Independent Test**: Known order lines across categories produce expected total revenue and percentages.

### Tests for User Story 1

- [X] T006 [P] [US1] Add unit tests for date range validation and revenue percentage math in `dotnet/Petstore.Tests/AdminSalesAnalyticsServiceTests.cs`
- [X] T007 [P] [US1] Add SQL Server integration test for revenue aggregation by category and date filtering in `dotnet/Petstore.Tests/AdminSalesAnalyticsApiContractTests.cs`

### Implementation for User Story 1

- [X] T008 [US1] Implement EF revenue aggregation query in `dotnet/Petstore/Analytics/AdminSalesAnalyticsRepository.cs`
- [X] T009 [US1] Implement date validation, total revenue, and revenue percentage calculation in `dotnet/Petstore/Analytics/AdminSalesAnalyticsService.cs`
- [X] T010 [US1] Register analytics repository/service in `dotnet/Petstore/Program.cs`

## Phase 4: User Story 2 - Sales Count By Category (P1)

**Goal**: Admin can retrieve sold quantity counts by category for the same date range.

**Independent Test**: Known order lines with quantities greater than one produce expected category counts.

### Tests for User Story 2

- [X] T011 [P] [US2] Add unit tests for sales count totals and empty result behavior in `dotnet/Petstore.Tests/AdminSalesAnalyticsServiceTests.cs`
- [X] T012 [P] [US2] Add SQL Server integration test for quantity-based category counts and qualifying status policy in `dotnet/Petstore.Tests/AdminSalesAnalyticsApiContractTests.cs`

### Implementation for User Story 2

- [X] T013 [US2] Extend `dotnet/Petstore/Analytics/AdminSalesAnalyticsRepository.cs` to return sales counts with the revenue rows
- [X] T014 [US2] Extend `dotnet/Petstore/Analytics/AdminSalesAnalyticsService.cs` to populate total sales count and per-category sales count

## Phase 5: User Story 3 - Admin-Only Access (P1)

**Goal**: Sales analytics are available only to admin users.

**Independent Test**: Admin receives analytics, while supplier, customer, and anonymous callers are rejected.

### Tests for User Story 3

- [X] T015 [P] [US3] Add API contract tests for admin success and supplier/customer/anonymous denial in `dotnet/Petstore.Tests/AdminSalesAnalyticsApiContractTests.cs`

### Implementation for User Story 3

- [X] T016 [US3] Add `AdminSalesAnalyticsController` with admin authorization and validation responses in `dotnet/Petstore/Controllers/AdminSalesAnalyticsController.cs`
- [X] T017 [US3] Verify API error shape follows existing `ApiErrorDto` conventions in `dotnet/Petstore/Controllers/AdminSalesAnalyticsController.cs`

## Phase 6: Polish & Cross-Cutting

- [X] T018 [P] Update backend API examples in `dotnet/Petstore/Petstore.http`
- [X] T019 Run `dotnet test dotnet/Petstore.Tests/Petstore.Tests.csproj --filter "FullyQualifiedName~AdminSalesAnalytics"` and database integration tests for `dotnet/Petstore.Tests/Petstore.Tests.csproj`
- [X] T020 Run manual quickstart from `specs/015-admin-sales-analytics/quickstart.md`

## Dependencies

- Phase 1 -> Phase 2 -> US1.
- US1 should complete before US2 because the combined response and repository query start with revenue rows.
- US3 can start after Phase 2 but should be completed before exposing the UI.
- Feature 016 depends on this feature's API contract.

## Parallel Opportunities

- T006 and T007 can run in parallel.
- T011 and T012 can run in parallel.
- T015 can run while US1/US2 service work is finishing, after controller shape is known.

## Implementation Strategy

1. Complete setup and foundational DTO/interface tasks.
2. Deliver US1 as MVP: revenue share endpoint backed by tests.
3. Add US2 count metrics into the same response.
4. Add and verify US3 authorization boundaries.
5. Run quickstart and full relevant backend tests before moving to UI feature 016.
