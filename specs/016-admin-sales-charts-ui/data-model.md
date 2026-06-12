# Data Model: Admin Sales Charts UI

## SalesDashboardViewState

- `startDate`: Date input value.
- `endDate`: Date input value.
- `loading`: Whether analytics are currently loading.
- `error`: Optional user-facing error state.
- `analytics`: Latest successful analytics response.

Validation:

- Both date fields are required before requesting analytics.
- Start date must be less than or equal to end date.

## SalesAnalyticsResponse

Mirrors feature 015 response:

- `startDate`
- `endDate`
- `totalRevenue`
- `totalSalesCount`
- `categories`

## CategorySalesMetric

- `categoryId`
- `categoryName`
- `revenue`
- `revenuePercent`
- `salesCount`
- `color`: UI-assigned stable chart color.

## ChartDisplayRow

- `label`: Category name or id.
- `value`: Revenue, percentage, or count depending on chart section.
- `color`: Swatch used by the chart and legend.
- `rank`: Display ordering.

Validation:

- Missing category name falls back to category id.
- Empty categories produce no chart segments/bars and show an empty state.
