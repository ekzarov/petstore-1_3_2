# PetStore Catalog UI

Read-only Angular catalog browsing UI for the migrated PetStore catalog API
(`specs/003-catalog-angular-ui`). Covers categories, products, sellable items,
item details, and loading/empty/not-found/unavailable states. No cart,
checkout, sign-in, search, or admin features in this slice.

## Prerequisites

- Node.js with npm
- The ASP.NET catalog backend from `dotnet/Petstore` (see
  `specs/003-catalog-angular-ui/quickstart.md`)

## Run locally

Start the backend first:

```powershell
dotnet run --project dotnet/Petstore/Petstore.csproj
```

The backend listens on `http://localhost:5103`.

Then start the frontend:

```powershell
cd frontend/petstore-ui
npm install
npm start
```

Open `http://localhost:4200`. The root route redirects to `/catalog`.

## Dev proxy

The app calls relative `/api/catalog/...` URLs only. During `ng serve`,
`proxy.conf.json` forwards `/api` requests to `http://localhost:5103`, so the
browser talks to a single origin and the backend needs no development CORS
policy.

Notes for Angular 21 (Vite dev server):

- The proxy context key must be `/api` — Vite does not support webpack-style
  glob keys like `/api/*`.
- `proxyConfig` is set in `angular.json` under the `serve` target, so the
  proxy applies to any `ng serve` invocation, not just `npm start`.

## Routes

| Route | View |
|---|---|
| `/catalog` | Category list |
| `/catalog/categories/:categoryId` | Products in a category |
| `/catalog/products/:productId` | Sellable items for a product (`?category=` preserves the back link) |
| `/catalog/items/:itemId` | Item details |

## Build

```powershell
npm run build
```

## Validation

Manual validation steps live in `specs/003-catalog-angular-ui/quickstart.md`.
Automated UI tests are intentionally deferred for this first slice.
