# Implementation Plan: Checkout And Order History UI

**Branch**: `009-checkout-ui-sdd` | **Date**: 2026-06-10 | **Spec**: `specs/009-checkout-ui/spec.md`

## Summary

Complete the storefront journey in Angular: a guarded checkout route showing
the cart review with contact details prefilled from the account, order
placement with single-submit protection, a confirmation view, and order
history (list + details) routes.

## Technical Context

**Language/Version**: TypeScript, Angular 21 (zoneless, signals) in `frontend/petstore-ui`.

**Primary Dependencies**: Orders API (008), cart service (007), identity service/guard (005).

**Testing**: Manual validation (consistent deferral).

## Constitution Check

All principles pass; frontend-only slice.

## Design Decisions

- **DD-001**: Routes: `/checkout` (authGuard), `/orders` (authGuard), `/orders/:orderId` (authGuard). Checkout button in the cart view (007 placeholder) activates and navigates with `returnUrl` handling for anonymous users.
- **DD-002**: Checkout review loads cart (read-only lines + totals) and account contact info into an editable shipping block; billing optional ("same as shipping" checkbox, no card fields per 008 DD-003).
- **DD-003**: Single-submit: the place-order button disables on click until the response arrives; the backend's empty-cart guard is the authoritative double-submit protection.
- **DD-004**: Confirmation is a view state of the checkout component showing order id, status, and links to order details and catalog; cart indicator refreshes from the emptied cart.
- **DD-005**: `OrderApiService` mirrors the 008 contract; statuses displayed verbatim (`PENDING` etc.) with a CSS badge per status.
- **DD-006**: Session expiry mid-checkout: the 401 interceptor (005) redirects to sign-in; the anonymous cart id remains in localStorage so the cart survives.

## Project Structure

```text
frontend/petstore-ui/src/app/
|-- orders/
|   |-- order-api.service.ts
|   |-- order.models.ts
|   |-- checkout.component.ts
|   |-- order-list.component.ts
|   `-- order-detail.component.ts
|-- cart/cart.component.ts   (activate checkout button)
`-- app.routes.ts            (three guarded routes)
```

## Manual Validation

Full journey signed-in and via anonymous→sign-in redirect; double-click placement; refresh on review/confirmation/orders/details; foreign and unknown order ids; backend stopped at each step.
