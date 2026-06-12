# Contract: Admin Sales Charts UI

## Route

`/admin/sales`

### Access

- Admin users can open the route.
- Supplier, customer, and anonymous users must not render sales analytics data.
- Backend `401`/`403` responses must produce safe forbidden or sign-in behavior.

## API Consumption

The UI calls feature 015:

```text
GET /api/admin/analytics/sales?startDate=yyyy-MM-dd&endDate=yyyy-MM-dd
```

Expected response fields:

- `totalRevenue`
- `totalSalesCount`
- `categories[].categoryId`
- `categories[].categoryName`
- `categories[].revenue`
- `categories[].revenuePercent`
- `categories[].salesCount`

## UI States

- Loading: stable dashboard layout with loading state.
- Success with data: date controls, revenue share chart, sales count chart,
  legends/readable values.
- Success without data: no-data state, no fake chart values.
- Validation error: inline date-range feedback, no API request.
- Backend unavailable: existing unavailable/error style.
- Forbidden: no stale chart data rendered.
