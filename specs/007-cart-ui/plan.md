# Implementation Plan: Cart UI

**Branch**: `007-cart-ui-sdd` | **Date**: 2026-06-10 | **Spec**: `specs/007-cart-ui/spec.md`

## Summary

Add cart interactions to the Angular app: a `CartService` owning the cart
state signal and the anonymous `X-Cart-Id`, add-to-cart buttons on item views,
a shell cart indicator, and a cart route with quantity editing, removal, and
empty-cart.

## Technical Context

**Language/Version**: TypeScript, Angular 21 (zoneless, signals) in `frontend/petstore-ui`.

**Primary Dependencies**: Cart API (006) via `/api` proxy; identity service (005) only for the signed-in case — all cart UI works anonymously.

**Testing**: Manual validation (deferral consistent with 003/005).

## Constitution Check

All principles pass; frontend-only slice.

## Design Decisions

- **DD-001**: `CartService` (signal-based): holds the latest `CartDto`; generates and persists the anonymous cart id in `localStorage` (key `petstore.cartId`); an HTTP interceptor (or the service itself) adds `X-Cart-Id` to `/api/cart` requests. After sign-in the backend merge makes the header harmless; the service refreshes the cart on identity changes.
- **DD-002**: All mutations go through the API and replace local state with the backend response (backend is the source of truth; satisfies FR-005).
- **DD-003**: Cart indicator in the shell header reads the `CartService` item-count signal; loaded on app start.
- **DD-004**: Cart route `/cart` with line table, quantity stepper (1..99), remove per line, empty-cart action, empty state linking to the catalog. Checkout button placeholder disabled until feature 009.
- **DD-005**: Add-to-cart buttons on item cards (item list) and item details; brief non-blocking confirmation; indicator updates from the API response.

## Project Structure

```text
frontend/petstore-ui/src/app/
|-- cart/
|   |-- cart.service.ts
|   |-- cart.models.ts
|   |-- cart-id.interceptor.ts
|   `-- cart.component.ts
|-- catalog/item-list.component.ts     (add-to-cart button)
|-- catalog/item-detail.component.ts   (add-to-cart button)
|-- catalog/catalog-shell.component.ts (cart indicator)
|-- app.config.ts                      (interceptor)
`-- app.routes.ts                      (/cart route)
```

## Manual Validation

Add from details and list, indicator math across routes and reload, quantity edit/remove/empty, anonymous flow end to end, unavailable backend feedback, deep link and refresh on `/cart`.
