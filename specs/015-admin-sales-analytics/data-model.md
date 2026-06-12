# Data Model: Admin Sales Analytics API

## SalesAnalyticsDateRangeRequest

- `startDate`: Inclusive start date in local date format accepted by the API.
- `endDate`: Inclusive end date.

Validation:

- Both dates are required.
- `startDate` must be less than or equal to `endDate`.

## AdminSalesAnalyticsDto

- `startDate`: Echoed inclusive start date.
- `endDate`: Echoed inclusive end date.
- `totalRevenue`: Sum of qualifying order-line revenue.
- `totalSalesCount`: Sum of qualifying order-line quantities.
- `categories`: Ordered category metric rows.

Validation:

- Empty result uses `totalRevenue = 0`, `totalSalesCount = 0`, and an empty
  `categories` collection.

## AdminCategorySalesMetricDto

- `categoryId`: Legacy category id such as `CATS`, `FISH`, or `BIRDS`.
- `categoryName`: Display label from current catalog metadata, falling back to
  `categoryId`.
- `revenue`: Sum of `quantity * unitPrice` for qualifying lines in the category.
- `revenuePercent`: Category revenue divided by total revenue, rounded to two
  decimal places.
- `salesCount`: Sum of sold quantities for qualifying lines in the category.

Relationships:

- Metrics are grouped by category id from order lines.
- Category display name is optional metadata from catalog tables.

## Qualifying Order Policy

Included statuses:

- `SHIPPED_PART`
- `SHIPPED`
- `COMPLETED`

Excluded statuses:

- `PENDING`
- `APPROVED`
- `DENIED`
