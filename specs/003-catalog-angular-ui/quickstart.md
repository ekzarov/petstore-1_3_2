# Quickstart: Browse Catalog UI

## Prerequisites

- SQL Server catalog database from the backend catalog slice is migrated and seeded.
- ASP.NET backend from `dotnet/Petstore` is runnable locally.
- Node.js and Angular CLI-compatible tooling are available before implementation starts.

## Run Backend

```powershell
dotnet run --project dotnet/Petstore/Petstore.csproj --urls http://localhost:5103
```

Expected outcome: backend exposes the catalog API documented in `specs/002-browse-product-catalog/contracts/openapi.yaml`.

## Run Frontend

After the Angular app is implemented:

```powershell
cd frontend/petstore-ui
npm install
npm start
```

Expected outcome: Angular dev server starts and uses `proxy.conf.json` to forward `/api/*` to `http://localhost:5103`.

## Manual Validation

1. Open the Angular dev server URL.
2. Verify Fish, Dogs, Reptiles, Cats, and Birds are visible without signing in.
3. Select Fish and verify Angelfish, Tiger Shark, Koi, and Goldfish appear.
4. Select Dogs and verify Bulldog, Poodle, Dalmation, Golden Retriever, Labrador Retriever, and Chihuahua appear.
5. Open Angelfish and verify Large Angelfish and Small Angelfish appear with prices and currency.
6. Open Bulldog and verify Male Adult Bulldog and Female Puppy Bulldog appear with prices and currency.
7. Open Large Angelfish details and verify `EST-1`, `FI-SW-01`, attributes, description, price, and currency.
8. Refresh on a category route, product item route, and item detail route; verify the same state reloads.
9. Navigate to unknown category, product, and item routes; verify not-found states and links back to normal browsing.
10. Stop the backend while the frontend is running and verify the unavailable state appears for catalog API calls.

## Out of Scope for This Quickstart

- Automated Angular tests.
- Production hosting.
- Cart, checkout, account, search, localization, recommendations, or admin catalog editing.
