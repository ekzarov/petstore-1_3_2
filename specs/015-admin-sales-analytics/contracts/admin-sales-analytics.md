# Contract: Admin Sales Analytics API

## GET `/api/admin/analytics/sales`

Returns category-level sales analytics for the selected date range.

### Authorization

- Requires `admin` role.
- `supplier`, `customer`, and anonymous callers must not receive analytics data.

### Query Parameters

- `startDate` - required, inclusive, `yyyy-MM-dd`.
- `endDate` - required, inclusive, `yyyy-MM-dd`.

### Success Response: `200 OK`

```json
{
  "startDate": "2026-06-01",
  "endDate": "2026-06-30",
  "totalRevenue": 193.00,
  "totalSalesCount": 6,
  "categories": [
    {
      "categoryId": "FISH",
      "categoryName": "Fish",
      "revenue": 120.00,
      "revenuePercent": 62.18,
      "salesCount": 4
    },
    {
      "categoryId": "BIRDS",
      "categoryName": "Birds",
      "revenue": 73.00,
      "revenuePercent": 37.82,
      "salesCount": 2
    }
  ]
}
```

### Empty Response: `200 OK`

```json
{
  "startDate": "2026-06-01",
  "endDate": "2026-06-30",
  "totalRevenue": 0,
  "totalSalesCount": 0,
  "categories": []
}
```

### Validation Error: `400 Bad Request`

Returned when required dates are missing, malformed, or `startDate` is after
`endDate`.

### Forbidden: `403 Forbidden`

Returned for authenticated non-admin callers.

### Unauthorized: `401 Unauthorized`

Returned for anonymous callers.
