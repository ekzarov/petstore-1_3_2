# Quickstart: Admin Sales Analytics API

## Prerequisites

- SQL Server test/local database configured as in existing backend features.
- Backend migrations applied.
- Seeded `admin`, `supplier`, and customer users available.

## Automated Verification

From the repository root:

```powershell
dotnet test dotnet/Petstore.Tests/Petstore.Tests.csproj --filter "FullyQualifiedName~AdminSalesAnalytics"
dotnet test dotnet/Petstore.Tests/Petstore.Tests.csproj --filter "Category=DatabaseIntegration"
```

Expected:

- Unit tests verify date validation, aggregation, and percentage math.
- Integration/API tests verify SQL aggregation, empty result, qualifying status
  policy, and role boundaries.

## Manual Smoke

1. Start the backend:

   ```powershell
   dotnet run --project dotnet/Petstore/Petstore.csproj
   ```

2. Sign in as `admin`.
3. Call:

   ```text
   GET http://localhost:5103/api/admin/analytics/sales?startDate=2026-01-01&endDate=2027-12-31
   ```

4. Verify the response contains totals and category rows when qualifying orders
   exist.
5. Sign in as `supplier` and verify the same endpoint returns forbidden.
