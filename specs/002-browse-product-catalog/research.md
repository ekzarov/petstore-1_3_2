# Research: Browse Product Catalog

## Decision: Build the first slice as a read-only ASP.NET Core Web API

**Rationale**: The current .NET scaffold is already an ASP.NET Core Web API project under `dotnet/Petstore`. A read-only API gives a small, independently testable migration slice and avoids coupling the first feature to UI choices.

**Alternatives considered**:

- Migrated server-rendered UI: rejected because the user explicitly chose Web API for the first slice.
- Full frontend + backend catalog flow: rejected because it expands scope before catalog parity is proven.

## Decision: Use EF Core with a configured relational database for catalog storage

**Rationale**: The implementation should exercise a real persistence boundary before controllers are built. EF Core gives the migrated .NET slice a normal data access path while preserving the same public API contract. Connection strings come from `appsettings` so local and future environments can differ without code changes.

**Alternatives considered**:

- Hardcoded static class: rejected because the feature should now validate real data access, seeders, and context wiring before API implementation continues.
- Read directly from legacy H2: rejected because it couples the new API to the legacy runtime and database setup.
- Parse `Populate-UTF8.xml` at runtime: rejected for this slice because deterministic application seeders are simpler and easier to test.

## Decision: Use SQL Server as the local EF Core provider

**Rationale**: The developer already has local SQL Server available with Windows Authentication. Using SQL Server now validates the same provider family likely to be used beyond the first catalog slice while still keeping the API contract independent from storage details.

**Alternatives considered**:

- SQLite: rejected because local SQL Server is already available and gives more realistic provider behavior for the migrated .NET application.
- EF Core InMemory provider: rejected because it does not behave like a relational database and would miss relationship/schema issues.
- Legacy H2: rejected because it keeps the migrated API tied to Java-era infrastructure.

## Decision: Preserve legacy ids in API responses

**Rationale**: Future cart and checkout work will need stable category, product, and item identifiers. The legacy seed file provides canonical ids such as `FISH`, `FI-SW-01`, `FI-FW-02`, `EST-1`, and `EST-2`.

**Alternatives considered**:

- Generate new ids: rejected because it would create a mapping problem before cart migration.
- Use display names as ids: rejected because display names are localized and not stable.

## Decision: API endpoint scope

**Rationale**: The selected scope is categories, products by category, items by product, and item by id. This matches catalog browsing and provides the item lookup needed by future add-to-cart work.

**Alternatives considered**:

- Categories only: rejected as too small to validate product/item parity.
- Single aggregate catalog endpoint: rejected because it hides the navigation boundaries used by the legacy flow.
- Add search/localization/admin endpoints: rejected as outside this feature.

## Decision: Error and empty result behavior

**Rationale**: Unknown identifiers should return `404 Not Found`; known categories/products with no children should return `200 OK` with an empty array. This separates invalid links from valid empty catalog state and is straightforward to test.

**Alternatives considered**:

- Always return `200 OK`: rejected because clients cannot distinguish bad ids from empty data.
- Always return `404` for empty results: rejected because valid empty categories/products are listed edge cases.

## Decision: OpenAPI contract as planning artifact

**Rationale**: The scaffold already includes OpenAPI support, and a written contract gives tasks and tests an exact target without requiring implementation code in the spec.

**Alternatives considered**:

- Informal endpoint list only: rejected because it leaves response shape ambiguous.
- Generate contract after implementation: rejected because tasks should be contract-driven.

## Decision: Seed catalog data through application seeders

**Rationale**: Seeders keep the first migrated catalog deterministic while using EF Core storage. They can preserve known legacy ids such as `FISH`, `FI-SW-01`, and `EST-1`, and can later be replaced by migrations/import tooling if the migration strategy changes.

**Alternatives considered**:

- Manual database setup: rejected because local startup and tests should be reproducible.
- Runtime XML import: deferred until a broader data migration slice.
