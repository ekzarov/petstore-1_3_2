# Tasks: Browse Catalog UI

**Input**: Design documents from `/specs/003-catalog-angular-ui/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, quickstart.md, existing backend API contract at `specs/002-browse-product-catalog/contracts/openapi.yaml`

**Tests**: Automated frontend tests are intentionally deferred for this first UI slice. Each user story includes a manual validation task instead.

**Organization**: Tasks are grouped by user story so each story can be implemented and manually validated as an independent increment.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel after dependencies are satisfied because it touches different files.
- **[Story]**: User story label from `specs/003-catalog-angular-ui/spec.md`.
- Each task includes exact repository-relative file paths.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create the Angular application shell and local development bridge to the existing backend API.

- [X] T001 Create a clean Angular application project under `frontend/petstore-ui/`
- [X] T002 Start the clean Angular application and manually verify the default Angular welcome page renders from `frontend/petstore-ui/`
- [X] T003 Configure Angular dev scripts to run with `frontend/petstore-ui/proxy.conf.json` in `frontend/petstore-ui/package.json`
- [X] T004 Create `/api/*` proxy forwarding to `http://localhost:5103` in `frontend/petstore-ui/proxy.conf.json`
- [X] T005 [P] Add frontend ignore rules for Angular build output and dependencies in `frontend/petstore-ui/.gitignore`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Shared frontend structure required before any catalog user story can be implemented.

**CRITICAL**: No user story work should begin until this phase is complete.

- [X] T006 Configure standalone Angular app providers and HttpClient in `frontend/petstore-ui/src/app/app.config.ts`
- [X] T007 Define catalog route skeletons in `frontend/petstore-ui/src/app/app.routes.ts`
- [X] T008 [P] Define catalog API DTOs and view-state types in `frontend/petstore-ui/src/app/catalog/catalog.models.ts`
- [X] T009 Implement relative `/api/catalog/...` catalog API service in `frontend/petstore-ui/src/app/catalog/catalog-api.service.ts`
- [X] T010 [P] Create reusable loading state component in `frontend/petstore-ui/src/app/shared/loading-state.component.ts`
- [X] T011 [P] Create reusable empty state component in `frontend/petstore-ui/src/app/shared/empty-state.component.ts`
- [X] T012 [P] Create reusable unavailable/error state component in `frontend/petstore-ui/src/app/shared/unavailable-state.component.ts`
- [X] T013 Create catalog shell layout and router outlet in `frontend/petstore-ui/src/app/catalog/catalog-shell.component.ts`

**Checkpoint**: The Angular app can start, route to the catalog shell, and make proxied `/api/*` calls through the API service.

---

## Phase 3: User Story 1 - Browse Categories (Priority: P1) MVP

**Goal**: Show all available catalog categories without sign-in and allow category selection.

**Independent Test**: Start backend and frontend, open the catalog UI, verify Fish, Dogs, Reptiles, Cats, and Birds, then select Fish.

### Implementation for User Story 1

- [X] T014 [US1] Implement category list loading, active-category navigation, and category links in `frontend/petstore-ui/src/app/catalog/category-list.component.ts`
- [X] T015 [US1] Wire default catalog and category routes to the category component in `frontend/petstore-ui/src/app/app.routes.ts`
- [X] T016 [US1] Add responsive category list styles in `frontend/petstore-ui/src/styles.css`
- [ ] T017 [US1] Manually validate category browsing per `specs/003-catalog-angular-ui/quickstart.md`

**Checkpoint**: User Story 1 is fully functional and manually testable independently.

---

## Phase 4: User Story 2 - Browse Products In A Category (Priority: P1)

**Goal**: Show products for a selected category with readable names, descriptions, and empty-state behavior.

**Independent Test**: Select Fish and verify Angelfish and Goldfish appear with meaningful descriptions.

### Implementation for User Story 2

- [ ] T018 [US2] Implement product list loading and known-category empty state in `frontend/petstore-ui/src/app/catalog/product-list.component.ts`
- [ ] T019 [US2] Wire category product route parameters to product loading in `frontend/petstore-ui/src/app/app.routes.ts`
- [ ] T020 [US2] Add product list and card styles in `frontend/petstore-ui/src/styles.css`
- [ ] T021 [US2] Manually validate product browsing per `specs/003-catalog-angular-ui/quickstart.md`

**Checkpoint**: User Stories 1 and 2 work through the same browser flow.

---

## Phase 5: User Story 3 - Browse Items And Item Details (Priority: P1)

**Goal**: Show sellable items for a product and item details with stable ids, attributes, price, and currency.

**Independent Test**: Open Angelfish, verify Large Angelfish and Small Angelfish, then open Large Angelfish and verify `EST-1`, `FI-SW-01`, attributes, description, price, and currency.

### Implementation for User Story 3

- [ ] T022 [US3] Implement product item list loading and empty item state in `frontend/petstore-ui/src/app/catalog/item-list.component.ts`
- [ ] T023 [US3] Implement item details loading and display in `frontend/petstore-ui/src/app/catalog/item-detail.component.ts`
- [ ] T024 [US3] Wire product item and item detail routes in `frontend/petstore-ui/src/app/app.routes.ts`
- [ ] T025 [US3] Add item list and item detail styles in `frontend/petstore-ui/src/styles.css`
- [ ] T026 [US3] Manually validate item browsing and item details per `specs/003-catalog-angular-ui/quickstart.md`

**Checkpoint**: The primary categories-to-item-details flow is complete.

---

## Phase 6: User Story 4 - Handle Missing Catalog Data (Priority: P2)

**Goal**: Show clear not-found and unavailable states for missing category, product, item, or backend failures.

**Independent Test**: Navigate to unknown category, product, and item routes, then stop the backend and verify unavailable feedback.

### Implementation for User Story 4

- [ ] T027 [US4] Normalize 404 and network errors in `frontend/petstore-ui/src/app/catalog/catalog-api.service.ts`
- [ ] T028 [US4] Add route-back actions for missing category states in `frontend/petstore-ui/src/app/catalog/product-list.component.ts`
- [ ] T029 [US4] Add route-back actions for missing product item states in `frontend/petstore-ui/src/app/catalog/item-list.component.ts`
- [ ] T030 [US4] Add route-back actions for missing item detail states in `frontend/petstore-ui/src/app/catalog/item-detail.component.ts`
- [ ] T031 [US4] Manually validate not-found and backend-unavailable scenarios per `specs/003-catalog-angular-ui/quickstart.md`

**Checkpoint**: Missing-data and unavailable-state behavior is complete.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Final UI cleanup and local validation for the complete feature.

- [ ] T032 [P] Document frontend run commands and proxy behavior in `frontend/petstore-ui/README.md`
- [ ] T033 Review visible UI text for clear read-only catalog language in `frontend/petstore-ui/src/app/catalog/catalog-shell.component.ts`, `frontend/petstore-ui/src/app/catalog/category-list.component.ts`, `frontend/petstore-ui/src/app/catalog/product-list.component.ts`, `frontend/petstore-ui/src/app/catalog/item-list.component.ts`, and `frontend/petstore-ui/src/app/catalog/item-detail.component.ts`
- [ ] T034 Verify responsive layout manually at desktop and narrow mobile widths using `frontend/petstore-ui/src/styles.css`
- [ ] T035 Run Angular build validation from `frontend/petstore-ui/package.json`
- [ ] T036 Run full manual quickstart validation from `specs/003-catalog-angular-ui/quickstart.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies.
- **Foundational (Phase 2)**: Depends on Setup completion and blocks all user stories.
- **User Story 1 (Phase 3)**: Depends on Foundational completion and is the MVP.
- **User Story 2 (Phase 4)**: Depends on Foundational completion; normally follows US1 because it extends selected category browsing.
- **User Story 3 (Phase 5)**: Depends on Foundational completion; normally follows US2 because it extends selected product browsing.
- **User Story 4 (Phase 6)**: Depends on the route/component surfaces created by US1-US3.
- **Polish (Phase 7)**: Depends on all desired user stories being complete.

### User Story Dependencies

- **US1 Browse Categories**: Can start after Foundational.
- **US2 Browse Products In A Category**: Can start after Foundational, but integrates naturally after US1.
- **US3 Browse Items And Item Details**: Can start after Foundational, but integrates naturally after US2.
- **US4 Handle Missing Catalog Data**: Should follow the normal browsing views because it adds error states to those surfaces.

### Parallel Opportunities

- T005 can run in parallel with T003-T004 after app creation and default page validation.
- T008 and T010-T012 can run in parallel after app creation.
- Styling tasks T016, T020, and T025 can be handled independently once their components exist.
- T032 and T034 can run in parallel during polish.

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1 Setup.
2. Complete Phase 2 Foundational.
3. Complete Phase 3 User Story 1.
4. Stop and manually validate category browsing.

### Incremental Delivery

1. Setup + Foundation.
2. US1 categories.
3. US2 products.
4. US3 items and item details.
5. US4 missing-data behavior.
6. Polish and full manual quickstart.

### Notes

- Components must call the catalog API service, not hardcode backend URLs.
- The Angular app must use relative `/api/catalog/...` paths so `proxy.conf.json` owns local backend forwarding.
- No automated frontend tests are required in this feature unless the specification changes.
