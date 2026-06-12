# Quickstart: Admin Sales Charts UI

## Prerequisites

- Feature 015 backend API is implemented and running.
- Angular dev proxy is configured as in the existing UI.

## Run

From the repository root, start the backend:

```powershell
dotnet run --project dotnet/Petstore/Petstore.csproj
```

Start the Angular UI:

```powershell
cd frontend/petstore-ui
npm start -- --port 4201
```

Open:

```text
http://localhost:4201/admin/sales
```

## Manual Validation

1. Sign in as `admin`.
2. Open `/admin/sales`.
3. Verify both chart sections render for the default range when sales exist:
   revenue share and sales count.
4. Change the date range and refresh analytics.
5. Choose an empty range and verify the no-data state.
6. Sign in as `supplier` and verify sales analytics are not visible.
7. Sign out and verify anonymous access redirects or shows safe forbidden
   behavior without chart data.

## Build Check

```powershell
cd frontend/petstore-ui
npm run build
```
