# Tasks: Admin Sales Charts UI

**Input**: `specs/016-admin-sales-charts-ui/plan.md`, `specs/016-admin-sales-charts-ui/spec.md`

**Tests**: Manual UI validation per plan. Backend aggregation and authorization tests are covered by feature 015.

## Phase 1: Setup

- [X] T001 Review existing admin shell, admin API service, identity service, shared states, and routes in `frontend/petstore-ui/src/app/admin/admin-shell.component.ts`, `frontend/petstore-ui/src/app/admin/admin-api.service.ts`, `frontend/petstore-ui/src/app/identity/identity.service.ts`, and `frontend/petstore-ui/src/app/app.routes.ts`
- [X] T002 Confirm feature 015 API contract is available in `specs/015-admin-sales-analytics/contracts/admin-sales-analytics.md`

## Phase 2: Foundational

- [X] T003 Add sales analytics models to `frontend/petstore-ui/src/app/admin/admin.models.ts`
- [X] T004 Extend `AdminApiService` with `getSalesAnalytics(startDate, endDate)` in `frontend/petstore-ui/src/app/admin/admin-api.service.ts`
- [X] T005 Add `/admin/sales` route guarded by existing admin guard in `frontend/petstore-ui/src/app/app.routes.ts`
- [X] T006 Add Sales navigation link in `frontend/petstore-ui/src/app/admin/admin-shell.component.ts`

## Phase 3: User Story 1 - View Revenue Share Pie Chart (P1) MVP

**Goal**: Admin sees revenue share by category for the selected date range.

**Independent Test**: Known backend analytics render category labels, amounts, and percentages.

### Implementation for User Story 1

- [X] T007 [US1] Create `SalesDashboardComponent` with date controls, loading/error/empty states, and analytics load flow in `frontend/petstore-ui/src/app/admin/sales-dashboard.component.ts`
- [X] T008 [US1] Implement custom accessible SVG/CSS revenue share donut chart in `frontend/petstore-ui/src/app/admin/sales-dashboard.component.ts`
- [X] T009 [P] [US1] Add sales dashboard and chart styles in `frontend/petstore-ui/src/styles.css`
- [X] T010 [US1] Manually validate admin revenue chart against backend response values using `specs/016-admin-sales-charts-ui/quickstart.md`

## Phase 4: User Story 2 - View Sales Count Bar Chart (P1)

**Goal**: Admin sees sold quantity counts by category for the same selected date range.

**Independent Test**: Known backend analytics render bar values matching category `salesCount`.

### Implementation for User Story 2

- [X] T011 [US2] Add custom accessible bar chart section to `frontend/petstore-ui/src/app/admin/sales-dashboard.component.ts`
- [X] T012 [US2] Add readable legend/summary rows for category counts in `frontend/petstore-ui/src/app/admin/sales-dashboard.component.ts`
- [X] T013 [US2] Manually validate date range changes update both revenue and count charts without page refresh using `specs/016-admin-sales-charts-ui/quickstart.md`

## Phase 5: User Story 3 - Handle No Data, Loading, And Access Boundaries (P1)

**Goal**: The dashboard behaves clearly for empty, loading, error, and unauthorized states.

**Independent Test**: Empty range, unavailable backend, supplier, customer, and anonymous users never see misleading or protected chart data.

### Implementation for User Story 3

- [X] T014 [US3] Add client-side date validation and no-data rendering in `frontend/petstore-ui/src/app/admin/sales-dashboard.component.ts`
- [X] T015 [US3] Ensure forbidden/unavailable responses clear stale analytics state in `frontend/petstore-ui/src/app/admin/sales-dashboard.component.ts`
- [X] T016 [US3] Manually validate supplier/customer/anonymous denial and backend unavailable behavior using `specs/016-admin-sales-charts-ui/quickstart.md`

## Phase 6: Polish & Cross-Cutting

- [X] T017 [P] Document the sales dashboard in `frontend/petstore-ui/README.md`
- [X] T018 Run `npm run build` from `frontend/petstore-ui` using `frontend/petstore-ui/package.json`
- [X] T019 Run the full manual quickstart in `specs/016-admin-sales-charts-ui/quickstart.md`

## Dependencies

- Feature 015 must be implemented before this feature can be fully validated.
- Phase 1 -> Phase 2 -> US1.
- US1 and US2 share the dashboard component and should be implemented sequentially.
- US3 can be completed after the dashboard load flow exists.

## Parallel Opportunities

- T009 can run after component class/template structure is visible.
- T017 can run after route and UX behavior are settled.

## Implementation Strategy

1. Wire the API service, route, and navigation.
2. Deliver US1 as MVP with revenue share and date controls.
3. Add US2 count bar chart to the same dashboard.
4. Harden US3 states and access behavior.
5. Run build and manual quickstart.
