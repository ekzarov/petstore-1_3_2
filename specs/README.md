# PetStore Migration — Feature Roadmap

Spec-driven migration of legacy Java PetStore 1.3.2 to .NET + Angular.
Governed by `.specify/memory/constitution.md`. Features alternate backend →
UI so every UI slice consumes a verified migrated API.

## Lifecycle Coverage

The legacy reference flow (Constitution Principle II): account creation →
catalog browsing → cart → checkout → OPC processing → Supplier fulfillment →
invoice/completion.

## Features

| # | Feature | Kind | Status | Depends on |
|---|---------|------|--------|------------|
| 001 | [Migration strategy](001-migration-strategy/spec.md) | Docs | Done | — |
| 002 | [Browse product catalog](002-browse-product-catalog/spec.md) | Backend API | Done | 001 |
| 003 | [Catalog Angular UI](003-catalog-angular-ui/spec.md) | UI | Done | 002 |
| 004 | [Customer accounts & sign-in](004-customer-accounts/spec.md) | Backend API | Done | 002 |
| 005 | [Account UI](005-account-ui/spec.md) | UI | Done | 003, 004 |
| 006 | [Shopping cart](006-shopping-cart/spec.md) | Backend API | Done | 002 (anonymous carts), 004 (customer carts) |
| 007 | [Cart UI](007-cart-ui/spec.md) | UI | Done | 003, 006 |
| 008 | [Checkout & orders](008-checkout-orders/spec.md) | Backend API | Done | 004, 006 |
| 009 | [Checkout & order history UI](009-checkout-ui/spec.md) | UI | Planned | 005, 007, 008 |
| 010 | [Order processing & approval](010-order-processing/spec.md) | Backend | Planned | 008 |
| 011 | [Supplier fulfillment](011-supplier-fulfillment/spec.md) | Backend | Planned | 010 |
| 012 | [Admin orders & inventory UI](012-admin-ui/spec.md) | UI | Planned | 010, 011 |

## Per-Feature Workflow

Each feature follows the Spec Kit flow before code:

1. `spec.md` (this roadmap's drafts) — what and why, no implementation choices.
2. `/speckit-clarify` — resolve the spec's Decision Points needing stakeholder input.
3. `/speckit-plan` — architecture, technology choices, test strategy (Constitution VI).
4. `/speckit-tasks` — phased task list; tasks are checked off (`[X]`) as completed.
5. Implementation on a feature branch, PR to `master`.

## Cross-Cutting Decision Points (recur across specs)

- **Authentication mechanism** (004 DP-001) — affects 005–012.
- **Messaging replacement for JMS** (010 DP-001) — affects 010–011; legacy
  used JMS/asyncsender, migration may start synchronous with documented rationale.
- **Payment card data handling** (004 DP-002, 008 DP-003) — legacy stored fake
  plaintext card data; not preserved.
- **Mail notifications** (010 DP-003) — legacy mailer behavior, may be deferred.

## Intentionally Out Of Scope (entire roadmap)

- Localization (legacy `j2ee-ja` Japanese flow) — revisit after 012.
- Legacy "my list"/banner profile preferences (004 DP-003).
- Pet recommendations and search.
- The legacy Swing admin client's visual design.
