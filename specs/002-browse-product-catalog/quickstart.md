# Quickstart: Browse Product Catalog

This guide validates the first migrated catalog API slice after implementation.

## Prerequisites

- .NET SDK capable of building `dotnet/Petstore/Petstore.csproj`.
- Repository checked out on the feature branch.
- `ConnectionStrings:PetstoreCatalog` configured in `dotnet/Petstore/appsettings.Development.json`.
- Optional parity reference: legacy Payara stack running from the existing Docker setup.

Local development uses SQL Server with Windows Authentication by default. Example development connection string:

```json
{
  "ConnectionStrings": {
    "PetstoreCatalog": "Server=.;Database=PetstoreCatalog;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True"
  }
}
```

The database is created and seeded through EF Core migrations. Apply migrations before serving catalog requests:

```powershell
dotnet ef database update --project dotnet/Petstore/Petstore.csproj --startup-project dotnet/Petstore/Petstore.csproj
```

## Build

```powershell
dotnet build dotnet/Petstore/Petstore.slnx
```

## Run API

```powershell
dotnet run --project dotnet/Petstore/Petstore.csproj
```

Use the local URL printed by ASP.NET Core. In development, OpenAPI should be available at `/openapi/v1.json` if the existing scaffold behavior is retained.

Before serving catalog requests, ensure the EF Core catalog database exists and contains migration-managed category, product, and item seed rows.

## Contract Checks

### List categories

```powershell
Invoke-RestMethod http://localhost:5000/api/catalog/categories
```

Expected result includes category ids and names:

- `FISH` / `Fish`
- `DOGS` / `Dogs`
- `REPTILES` / `Reptiles`
- `CATS` / `Cats`
- `BIRDS` / `Birds`

### List Fish products

```powershell
Invoke-RestMethod http://localhost:5000/api/catalog/categories/FISH/products
```

Expected result includes:

- `FI-SW-01` / `Angelfish`
- `FI-SW-02` / `Tiger Shark`
- `FI-FW-01` / `Koi`
- `FI-FW-02` / `Goldfish`

### List Dogs products

```powershell
Invoke-RestMethod http://localhost:5000/api/catalog/categories/DOGS/products
```

Expected result includes:

- `K9-BD-01` / `Bulldog`
- `K9-PO-02` / `Poodle`
- `K9-DL-01` / `Dalmation`
- `K9-RT-01` / `Golden Retriever`
- `K9-RT-02` / `Labrador Retriever`
- `K9-CW-01` / `Chihuahua`

### List Angelfish items

```powershell
Invoke-RestMethod http://localhost:5000/api/catalog/products/FI-SW-01/items
```

Expected result includes:

- `EST-1` / `Large Angelfish` / `16.50` / `USD`
- `EST-2` / `Small Angelfish` / `16.50` / `USD`

### List Bulldog items

```powershell
Invoke-RestMethod http://localhost:5000/api/catalog/products/K9-BD-01/items
```

Expected result includes:

- `EST-6` / `Male Adult Bulldog` / `18.50` / `USD`
- `EST-7` / `Female Puppy Bulldog` / `18.50` / `USD`

### Get item by id

```powershell
Invoke-RestMethod http://localhost:5000/api/catalog/items/EST-1
```

Expected result has `id = EST-1`, `productId = FI-SW-01`, and stable price/currency fields.

### Unknown id behavior

```powershell
Invoke-WebRequest http://localhost:5000/api/catalog/categories/UNKNOWN/products
```

Expected result is HTTP `404` with an `ApiError` response.

## Automated Tests

After tasks are generated and implemented, run:

```powershell
dotnet test dotnet/Petstore/Petstore.slnx
```

Tests should cover the OpenAPI contract paths, representative legacy parity ids, not-found behavior, EF Core schema/seed integrity, and repository behavior.

Unit tests should cover pure backend logic when it exists outside EF Core/API integration boundaries. Integration tests should cover:

- catalog seeder idempotency
- stable legacy ids and required relationships for all 5 categories, 16 products, and 28 items
- repository lookup behavior for known and unknown ids
