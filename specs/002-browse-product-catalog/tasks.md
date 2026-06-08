# Tasks: Browse Product Catalog

**Input**: Design documents from `specs/002-browse-product-catalog/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/openapi.yaml, quickstart.md

**Tests**: SQL Server integration tests are included for EF Core catalog seeding and repository behavior so FK/schema/provider issues are caught early. Contract/API tests are included because the feature must remain independently testable and quickstart defines API verification scenarios.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel because the task touches different files and does not depend on incomplete tasks.
- **[Story]**: Which user story this task belongs to.
- Every task includes an exact repository-relative file path.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Prepare the .NET solution for catalog API implementation and tests.

- [x] T001 Add `dotnet/Petstore.Tests/Petstore.Tests.csproj` test project with xUnit, `Microsoft.AspNetCore.Mvc.Testing`, and project reference to `dotnet/Petstore/Petstore.csproj`
- [x] T002 Add `dotnet/Petstore.Tests` to `dotnet/Petstore/Petstore.slnx`
- [x] T003 [P] Add `dotnet/Petstore.Tests/Usings.cs` for shared test usings
- [x] T004 Update `dotnet/Petstore/Petstore.csproj` or `dotnet/Petstore/Program.cs` as needed so ASP.NET Core test hosting can reference the application entry point

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Shared EF Core catalog persistence, seeders, and data access that all catalog endpoints depend on.

**CRITICAL**: No user story endpoint work should begin until this phase is complete.

- [x] T005 [P] Create `dotnet/Petstore/Models/CategoryDto.cs` with id, name, and optional description fields
- [x] T006 [P] Create `dotnet/Petstore/Models/ProductDto.cs` with id, categoryId, name, and optional description fields
- [x] T007 [P] Create `dotnet/Petstore/Models/ItemDto.cs` with id, productId, name, attributes, optional description, price, and currency fields
- [x] T008 [P] Create `dotnet/Petstore/Models/ApiErrorDto.cs` with code and message fields
- [x] T009 Add EF Core SQL Server packages to `dotnet/Petstore/Petstore.csproj` and relational EF Core test packages to `dotnet/Petstore.Tests/Petstore.Tests.csproj`
- [x] T010 Add `ConnectionStrings:PetstoreCatalog` SQL Server connection string to `dotnet/Petstore/appsettings.Development.json`
- [x] T011 [P] Create EF Core entities in `dotnet/Petstore/Data/Entities/CategoryEntity.cs`, `dotnet/Petstore/Data/Entities/ProductEntity.cs`, and `dotnet/Petstore/Data/Entities/ItemEntity.cs`
- [x] T012 Create `dotnet/Petstore/Data/PetstoreCatalogContext.cs` with DbSets and separate EF Core entity configuration classes for keys, relationships, and value mapping for item attributes
- [x] T013 Create `dotnet/Petstore/Catalog/CatalogSeeder.cs` with `CatalogSeeder.Seed(ModelBuilder)` that configures legacy categories, Fish products, Angelfish items, and representative values from `src/apps/petstore/src/docroot/populate/Populate-UTF8.xml` through EF Core `HasData`
- [x] T014 Create `dotnet/Petstore/Catalog/ICatalogRepository.cs` and `dotnet/Petstore/Catalog/CatalogRepository.cs` with read-only async lookup methods for categories, products by category, items by product, and item by id
- [x] T015 Refactor EF Core mapping constants into `dotnet/Petstore/Data/CatalogModelConstants.cs` and verify mapping classes do not hardcode table names, max lengths, or precision values
- [x] T016 Register `PetstoreCatalogContext`, `ICatalogRepository`, and `CatalogRepository` in `dotnet/Petstore/Program.cs`
- [x] T017 Add initial EF Core catalog migration files for the SQL Server schema under `dotnet/Petstore/Data/Migrations/`
- [x] T018 Verify local SQL Server database creation and migration-managed seed data by running `dotnet ef database update --project dotnet/Petstore/Petstore.csproj`
- [x] T019 Add SQL Server test database configuration and reset helper for `PetstoreCatalogTests` in `dotnet/Petstore.Tests/appsettings.json` and `dotnet/Petstore.Tests/PetstoreCatalogTestsFixture.cs`
- [x] T020 [P] Add SQL Server integration tests marked with `Trait("Category", "DatabaseIntegration")` for catalog seeder integrity, unique ids, FK relationships, idempotency, and representative legacy values in `dotnet/Petstore.Tests/CatalogSeederTests.cs`
- [x] T021 [P] Add SQL Server integration tests marked with `Trait("Category", "DatabaseIntegration")` for `CatalogRepository` lookup behavior, empty known collections, and unknown id results in `dotnet/Petstore.Tests/CatalogRepositoryTests.cs`

**Checkpoint**: Catalog DTOs, EF Core schema/context, seeded database data, and read-only repository are available to all stories.

---

## Phase 3: User Story 1 - View Catalog Categories (Priority: P1) MVP

**Goal**: A store visitor can retrieve the seeded pet categories without signing in.

**Independent Test**: Call `GET /api/catalog/categories` and verify that `FISH`, `DOGS`, `REPTILES`, `CATS`, and `BIRDS` are returned with display names.

### Tests for User Story 1

- [x] T022 [P] [US1] Add contract test for `GET /api/catalog/categories` in `dotnet/Petstore.Tests/CatalogApiContractTests.cs`

### Implementation for User Story 1

- [x] T023 [US1] Create `dotnet/Petstore/Controllers/CatalogCategoriesController.cs` with `GET /api/catalog/categories`
- [x] T024 [US1] Verify User Story 1 by running `dotnet test dotnet/Petstore/Petstore.slnx`

**Checkpoint**: User Story 1 is independently functional and testable.

---

## Phase 4: User Story 2 - View Products in a Category (Priority: P1)

**Goal**: A store visitor can retrieve products for a selected category, including Fish products.

**Independent Test**: Call `GET /api/catalog/categories/FISH/products` and verify that `FI-SW-01` Angelfish and `FI-FW-02` Goldfish are returned with descriptions.

### Tests for User Story 2

- [x] T025 [P] [US2] Add contract test for `GET /api/catalog/categories/FISH/products` in `dotnet/Petstore.Tests/CatalogApiContractTests.cs`

### Implementation for User Story 2

- [x] T026 [US2] Extend `dotnet/Petstore/Controllers/CatalogCategoriesController.cs` with `GET /api/catalog/categories/{categoryId}/products`
- [x] T027 [US2] Verify User Story 2 by running `dotnet test dotnet/Petstore/Petstore.slnx`

**Checkpoint**: User Story 2 is independently functional and does not require item endpoints.

---

## Phase 5: User Story 3 - View Items for a Product (Priority: P1)

**Goal**: A store visitor can retrieve sellable items for a product and retrieve a single item by id.

**Independent Test**: Call `GET /api/catalog/products/FI-SW-01/items` and verify `EST-1` Large Angelfish and `EST-2` Small Angelfish. Call `GET /api/catalog/items/EST-1` and verify stable id, product id, price, and currency.

### Tests for User Story 3

- [x] T028 [P] [US3] Add contract test for `GET /api/catalog/products/FI-SW-01/items` in `dotnet/Petstore.Tests/CatalogApiContractTests.cs`
- [x] T029 [P] [US3] Add contract test for `GET /api/catalog/items/EST-1` in `dotnet/Petstore.Tests/CatalogApiContractTests.cs`

### Implementation for User Story 3

- [x] T030 [P] [US3] Create `dotnet/Petstore/Controllers/CatalogProductsController.cs` with `GET /api/catalog/products/{productId}/items`
- [x] T031 [P] [US3] Create `dotnet/Petstore/Controllers/CatalogItemsController.cs` with `GET /api/catalog/items/{itemId}`
- [x] T032 [US3] Verify User Story 3 by running `dotnet test dotnet/Petstore/Petstore.slnx`

**Checkpoint**: User Story 3 is independently functional and provides the item lookup needed by future cart work.

---

## Phase 6: User Story 4 - Preserve Legacy Catalog Parity (Priority: P2)

**Goal**: The migrated API preserves representative legacy catalog identifiers, names, descriptions, and prices needed for future cart parity.

**Independent Test**: Compare the migrated Fish -> Angelfish path against legacy seed data anchors from `src/apps/petstore/src/docroot/populate/Populate-UTF8.xml`.

### Tests for User Story 4

- [x] T033 [P] [US4] Add parity test for Fish -> Angelfish -> EST-1/EST-2 legacy ids and values in `dotnet/Petstore.Tests/CatalogApiContractTests.cs`

### Implementation for User Story 4

- [x] T034 [US4] Review `dotnet/Petstore/Catalog/CatalogSeeder.cs` against `src/apps/petstore/src/docroot/populate/Populate-UTF8.xml` and correct any mismatched representative values
- [x] T035 [US4] Verify User Story 4 by running `dotnet test dotnet/Petstore/Petstore.slnx`

**Checkpoint**: Representative catalog parity path is documented by automated tests.

---

## Phase 7: User Story 5 - Handle Missing Catalog Data (Priority: P3)

**Goal**: API clients receive clear not-found responses for unknown category, product, and item ids.

**Independent Test**: Request unknown category, product, and item ids and verify HTTP `404` with `ApiErrorDto` responses.

### Tests for User Story 5

- [ ] T036 [P] [US5] Add not-found contract tests for unknown category, product, and item ids in `dotnet/Petstore.Tests/CatalogApiContractTests.cs`

### Implementation for User Story 5

- [ ] T037 [US5] Add consistent `ApiErrorDto` not-found responses to `dotnet/Petstore/Controllers/CatalogCategoriesController.cs`
- [ ] T038 [US5] Add consistent `ApiErrorDto` not-found responses to `dotnet/Petstore/Controllers/CatalogProductsController.cs`
- [ ] T039 [US5] Add consistent `ApiErrorDto` not-found responses to `dotnet/Petstore/Controllers/CatalogItemsController.cs`
- [ ] T040 [US5] Verify User Story 5 by running `dotnet test dotnet/Petstore/Petstore.slnx`

**Checkpoint**: Missing catalog data behavior is independently functional and clear.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Remove scaffold noise, align generated API behavior with the planning artifacts, and run final validation.

- [ ] T041 Remove or intentionally retain the WeatherForecast scaffold in `dotnet/Petstore/Controllers/WeatherForecastController.cs`, `dotnet/Petstore/WeatherForecast.cs`, and `dotnet/Petstore/Petstore.http`
- [ ] T042 [P] Update `dotnet/Petstore/Petstore.http` with catalog API sample requests matching `specs/002-browse-product-catalog/quickstart.md`
- [ ] T043 [P] Review `specs/002-browse-product-catalog/contracts/openapi.yaml` against implemented routes and update only if implementation intentionally changes the approved contract
- [ ] T044 Run `dotnet build dotnet/Petstore/Petstore.slnx`
- [ ] T045 Run `dotnet test dotnet/Petstore/Petstore.slnx`
- [ ] T046 Validate quickstart scenarios from `specs/002-browse-product-catalog/quickstart.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies.
- **Foundational (Phase 2)**: Depends on Setup completion and blocks all user stories.
- **User Story 1 (Phase 3)**: Depends on Foundational completion and is the MVP.
- **User Story 2 (Phase 4)**: Depends on Foundational completion; can be implemented after US1 or in parallel by another developer.
- **User Story 3 (Phase 5)**: Depends on Foundational completion; can be implemented after US1/US2 or in parallel by another developer.
- **User Story 4 (Phase 6)**: Depends on US1, US2, and US3 because it verifies the full representative parity path.
- **User Story 5 (Phase 7)**: Depends on endpoint files from US2 and US3.
- **Polish (Phase 8)**: Depends on desired user stories being complete.

### User Story Dependencies

- **US1 View Catalog Categories (P1)**: No dependency on other user stories after foundation.
- **US2 View Products in a Category (P1)**: Uses shared EF Core repository and the categories controller; no dependency on item endpoints.
- **US3 View Items for a Product (P1)**: Uses shared EF Core repository; no dependency on category endpoint implementation.
- **US4 Preserve Legacy Catalog Parity (P2)**: Depends on US1-US3 to verify category-to-product-to-item parity.
- **US5 Handle Missing Catalog Data (P3)**: Depends on endpoint implementations created for US2 and US3.

### Parallel Opportunities

- T003 can run after T001 while T002/T004 are being prepared.
- T005-T008 can run in parallel because each creates a separate DTO file.
- T011 can run after T009/T010 because entities depend on EF package setup and configured persistence decisions.
- T016 must run before T017-T018 because migrations and local database verification require the runtime EF Core registration.
- T017-T018 must run before T019-T021 so local schema and seeding are proven before integration test infrastructure is added.
- T019 must run before T020-T021 because both integration test groups require the SQL Server test database helper.
- T020 and T021 can be written after T019 to lock seeder/repository behavior against local SQL Server before controllers.
- T022 can be written before T023 as a failing contract test.
- T028 and T029 can run in parallel because they target different endpoint behaviors.
- T030 and T031 can run in parallel because they create separate controller files.
- T037-T039 can be worked in parallel after the corresponding controller files exist.
- T042 and T043 can run in parallel during polish.

---

## Parallel Example: User Story 3

```text
Task: "T028 [P] [US3] Add contract test for GET /api/catalog/products/FI-SW-01/items in dotnet/Petstore.Tests/CatalogApiContractTests.cs"
Task: "T029 [P] [US3] Add contract test for GET /api/catalog/items/EST-1 in dotnet/Petstore.Tests/CatalogApiContractTests.cs"
Task: "T030 [P] [US3] Create dotnet/Petstore/Controllers/CatalogProductsController.cs with GET /api/catalog/products/{productId}/items"
Task: "T031 [P] [US3] Create dotnet/Petstore/Controllers/CatalogItemsController.cs with GET /api/catalog/items/{itemId}"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1 setup.
2. Complete Phase 2 foundation.
3. Complete Phase 3 User Story 1.
4. Stop and validate `GET /api/catalog/categories` through tests and quickstart.

### Incremental Delivery

1. Setup + foundation create the reusable EF Core catalog context, seeder, and repository.
2. US1 exposes categories as the first demonstrable migrated API.
3. US2 adds products by category.
4. US3 adds product items and item lookup.
5. US4 locks representative legacy parity.
6. US5 hardens missing data behavior.

### Team Parallel Strategy

After Phase 2, one developer can work on categories/products while another works on item endpoints. Parity and not-found hardening should happen after the relevant endpoint files exist.

## Notes

- Keep this slice read-only at the API level; database writes are limited to schema creation/migration and deterministic seeders.
- Do not introduce legacy H2 connectivity, JMS, cart, checkout, OPC, Supplier, invoice, search, localization, or admin catalog editing work in this feature.
- Preserve legacy ids exactly because future cart migration will depend on item ids.
- Commit implementation in small logical groups at story checkpoints.
