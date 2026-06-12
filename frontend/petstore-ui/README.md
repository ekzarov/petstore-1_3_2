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
| `/cart` | Shopping cart (lines, quantity stepper, remove, empty; works signed out) |
| `/checkout` | Order review + shipping/billing details + placement (requires sign-in) |
| `/orders` | Order history with status badges (requires sign-in) |
| `/orders/:orderId` | Order details: lines, totals, shipping, status (requires sign-in) |
| `/admin` | Admin area (requires the `admin` role; seeded `admin`/`admin`): pending queue with approve/deny (single + bulk), all orders by status with transition history. Suppliers, customers, and anonymous visitors are redirected away; the backend role policy is the enforcement of record |
| `/supplier` | Supplier inventory workspace (roles `supplier` or `admin`; seeded `supplier`/`supplier`): on-hand quantities with inline editing — saving automatically ships waiting approved orders — plus a recovery re-run action |
| `/signin` | Sign in (seeded users: `j2ee`/`j2ee`, `shopper`/`j2ee`, `admin`/`admin`) |
| `/register` | Create an account (auto signs in on success) |
| `/account` | View/edit contact details (requires sign-in; anonymous visitors are redirected) |

## Identity

Sign-in calls `POST /api/auth/signin` and stores the JWT response under the
`localStorage` key `petstore.token`. An HTTP interceptor adds
`Authorization: Bearer <token>` to API calls and treats any 401 outside
`/api/auth/*` as an expired session: it clears the stored identity and
redirects to `/signin` with a `returnUrl`. Sign-out simply discards the
stored token. Passwords are never stored in the browser. Catalog browsing
needs no sign-in.

## Cart

An anonymous cart id (GUID) is generated on first use and stored under the
`localStorage` key `petstore.cartId`; an interceptor sends it as `X-Cart-Id`
on `/api/cart` requests. All amounts and counts come from backend responses —
the UI never computes totals. After sign-in the backend merges the anonymous
cart into the user cart on the first cart call; the `CartService` reloads the
cart whenever the identity changes, so the merge is visible immediately. The
header cart indicator shows the backend item count on every route.

## Checkout & orders

Checkout starts from the cart's Checkout button and requires sign-in (the
guard round-trips through `/signin` with a `returnUrl`). The review step shows
the backend cart and a shipping form prefilled from the account contact; a
"billing same as shipping" toggle controls whether a separate billing block is
sent. The place-order button disables while submitting; the backend's
empty-cart guard is the authoritative double-submit protection. All totals
come from API responses. Order statuses are displayed verbatim
(`PENDING`, `APPROVED`, …) as colored badges.

## Build

```powershell
npm run build
```

## Validation

Manual validation steps live in `specs/003-catalog-angular-ui/quickstart.md`.
Automated UI tests are intentionally deferred for this first slice.
