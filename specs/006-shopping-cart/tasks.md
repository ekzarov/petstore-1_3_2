# Tasks: Shopping Cart API

**Input**: `specs/006-shopping-cart/plan.md`, `specs/006-shopping-cart/spec.md`

**Tests**: Required per Constitution VI.

## Phase 1: Foundational (data + identity)

- [X] T001 [P] Create `dotnet/Petstore/Data/Entities/CartEntity.cs` and `CartLineEntity.cs` with configurations in `dotnet/Petstore/Data/Configurations/`; add DbSets to `PetstoreCatalogContext`
- [X] T002 Add EF Core migration for cart tables under `dotnet/Petstore/Data/Migrations/` and verify `dotnet ef database update`
- [X] T003 Create `dotnet/Petstore/Cart/CartIdentityResolver.cs` (JWT user key, `X-Cart-Id` anonymous key, neither → empty/400 behavior)
- [X] T004 Create `dotnet/Petstore/Cart/ICartRepository.cs` and `CartRepository.cs` (get-or-create, add/merge line, set quantity, remove, empty, delete cart)
- [X] T005 Create `dotnet/Petstore/Cart/CartViewBuilder.cs` joining catalog data for names/prices/totals, flagging unavailable items
- [X] T006 [P] Add unit tests in `dotnet/Petstore.Tests/CartRulesTests.cs` (quantity bounds 1..99, 0 removes, totals, merge summing)

## Phase 2: User Story 1 - Add Items To A Cart (P1)

- [X] T007 [P] [US1] Create DTOs `CartDto`, `CartLineDto`, `AddCartItemRequestDto` in `dotnet/Petstore/Models/`
- [X] T008 [US1] Add contract tests for `POST /api/cart/items` (add, duplicate-add merges, unknown item 404 leaves cart unchanged) in `dotnet/Petstore.Tests/CartApiContractTests.cs`
- [X] T009 [US1] Implement `POST /api/cart/items` in `dotnet/Petstore/Controllers/CartController.cs`
- [X] T010 [US1] Verify with `dotnet test dotnet/Petstore/Petstore.slnx`

## Phase 3: User Story 2 - View Cart With Totals (P1)

- [X] T011 [US2] Add contract tests for `GET /api/cart` (line fields, subtotals, total, empty cart not an error, unknown identity yields empty cart) in `dotnet/Petstore.Tests/CartApiContractTests.cs`
- [X] T012 [US2] Implement `GET /api/cart` in `dotnet/Petstore/Controllers/CartController.cs`
- [X] T013 [US2] Verify with `dotnet test dotnet/Petstore/Petstore.slnx`

## Phase 4: User Story 3 - Update Quantities And Remove Items (P1)

- [X] T014 [P] [US3] Create `SetCartQuantityRequestDto` in `dotnet/Petstore/Models/`
- [X] T015 [US3] Add contract tests for `PUT /api/cart/items/{itemId}` (set, 0 removes, negative/>99 rejected), `DELETE /api/cart/items/{itemId}`, and `DELETE /api/cart` in `dotnet/Petstore.Tests/CartApiContractTests.cs`
- [X] T016 [US3] Implement quantity update, line removal, and empty-cart endpoints in `dotnet/Petstore/Controllers/CartController.cs`
- [X] T017 [US3] Verify with `dotnet test dotnet/Petstore/Petstore.slnx`

## Phase 5: User Story 4 - Keep A Cart Across Requests (P2)

- [X] T018 [US4] Add integration tests for identity persistence and anonymous→user merge-once semantics in `dotnet/Petstore.Tests/CartMergeTests.cs`
- [X] T019 [US4] Implement merge-on-authenticated-request in `dotnet/Petstore/Cart/CartRepository.cs` / `CartIdentityResolver.cs`
- [X] T020 [US4] Verify with `dotnet test dotnet/Petstore/Petstore.slnx`

## Phase 6: Polish

- [X] T021 [P] Add unavailable-item flagging test (item removed from catalog after add) in `dotnet/Petstore.Tests/CartApiContractTests.cs`
- [X] T022 Run full suite and manual smoke with curl: add EST-1 and EST-2, set quantity 3, remove, empty

## Dependencies

- Phase 1 blocks all; US1 → US2 → US3 → US4. Requires 002 (catalog data); 004 needed only for the authenticated path (US4 merge tests).
