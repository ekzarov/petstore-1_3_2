# Implementation Plan: Browse Product Catalog

**Branch**: `spec-browse-product-catalog` | **Date**: 2026-06-08 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/002-browse-product-catalog/spec.md`

## Summary

Implement the first migrated catalog slice as a read-only ASP.NET Core Web API in the existing `dotnet/Petstore` solution. The API exposes categories, products by category, items by product, and item by id using EF Core with seeded relational catalog data that preserves stable legacy PetStore identifiers for later cart and checkout migration.

## Technical Context

**Language/Version**: C# on the existing `net10.0` ASP.NET Core scaffold.

**Primary Dependencies**: ASP.NET Core controllers, existing `Microsoft.AspNetCore.OpenApi`, EF Core, and the EF Core SQL Server provider for local development.

**Storage**: EF Core relational database. Local development uses SQL Server with Windows Authentication configured by `ConnectionStrings:PetstoreCatalog` in `appsettings`; the provider/connection string can be changed later without changing the public API contract.

**Testing**: .NET test project to be added under `dotnet/Petstore.Tests`, using xUnit or the repo-approved .NET test default during task generation. Contract tests should exercise the API through ASP.NET Core test hosting.

**Target Platform**: Local developer machine and container-friendly ASP.NET Core service; legacy Docker/Payara remains the parity reference but is not changed by this feature.

**Project Type**: Web service API inside the existing `dotnet/Petstore` solution.

**Performance Goals**: Catalog reads should complete within ordinary local API latency for a small relational seed dataset.

**Constraints**: Read-only API; no authentication; no legacy H2 datasource; no JMS; no cart, checkout, OPC, Supplier, invoice, search, localization, admin inventory editing, or personalized catalog behavior.

**Scale/Scope**: Small seed dataset matching the representative legacy catalog categories/products/items needed for browse parity and future cart integration.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- Runnable Legacy Baseline: Pass. This feature does not alter Docker/Payara runtime files or legacy EAR patching.
- Business Flow Parity: Pass. The plan preserves legacy catalog ids and documents parity anchors before implementation.
- Branch-First, Master-Stable Workflow: Pass. Work is isolated on `spec-browse-product-catalog`.
- Evidence Before Replacement: Pass. Legacy seed data source is identified as `src/apps/petstore/src/docroot/populate/Populate-UTF8.xml`.
- Incremental Migration Over Big Bang: Pass. Scope is one read-only catalog API slice, independently testable.

## Project Structure

### Documentation (this feature)

```text
specs/002-browse-product-catalog/
|-- plan.md
|-- research.md
|-- data-model.md
|-- quickstart.md
|-- contracts/
|   `-- openapi.yaml
|-- checklists/
|   `-- requirements.md
`-- spec.md
```

### Source Code (repository root)

```text
dotnet/Petstore/
|-- Controllers/
|   |-- CatalogCategoriesController.cs
|   |-- CatalogProductsController.cs
|   `-- CatalogItemsController.cs
|-- Catalog/
|   |-- CatalogSeeder.cs
|   `-- CatalogRepository.cs
|-- Data/
|   |-- PetstoreCatalogContext.cs
|   |-- CatalogModelConstants.cs
|   |-- Configurations/
|   |   |-- CategoryEntityConfiguration.cs
|   |   |-- ProductEntityConfiguration.cs
|   |   `-- ItemEntityConfiguration.cs
|   `-- Entities/
|       |-- CategoryEntity.cs
|       |-- ProductEntity.cs
|       `-- ItemEntity.cs
|-- Models/
|   |-- CategoryDto.cs
|   |-- ProductDto.cs
|   |-- ItemDto.cs
|   `-- ApiErrorDto.cs
|-- Program.cs
|-- Petstore.csproj
`-- Petstore.slnx

dotnet/Petstore.Tests/
|-- CatalogSeederTests.cs
|-- CatalogRepositoryTests.cs
|-- CatalogApiContractTests.cs
`-- Petstore.Tests.csproj
```

**Structure Decision**: Keep the first slice in the existing ASP.NET Core project to minimize migration surface area. Add EF Core data access inside the same project for now, backed by a local SQL Server database configured through `appsettings`. Add a narrow test project next to it so repository behavior and API contracts can be verified without touching the legacy Java runtime.

**EF Core Mapping Decision**: Keep `PetstoreCatalogContext` focused on DbSets and configuration registration. Entity mapping must live in separate `IEntityTypeConfiguration<T>` classes under `dotnet/Petstore/Data/Configurations/`. Table names, string length limits, precision values, and other repeated model constants must be centralized in `dotnet/Petstore/Data/CatalogModelConstants.cs`; mappings must reference constants rather than hardcoded literals.

**EF Core Seed Data Decision**: Deterministic reference catalog data must be configured through `OnModelCreating` with separate public static seeder classes, for example `CatalogSeeder.Seed(modelBuilder)`. Do not seed reference data from hosted services or application startup side effects; seed data should be captured by migrations through `HasData`.

**.NET Naming Decision**: New .NET classes, interfaces, fixtures, and test types must use PascalCase names without underscores, for example `PetstoreCatalogTestsFixture` rather than `PetstoreCatalog_TestsFixture`. Database names and connection string values should follow the same convention unless an external provider requires otherwise.

**.NET File Organization Decision**: Every public class, interface, record, enum, and fixture must live in its own file named after the public type.

## Phase 0: Research

See [research.md](research.md).

## Phase 1: Design & Contracts

- Data model: [data-model.md](data-model.md)
- API contract: [contracts/openapi.yaml](contracts/openapi.yaml)
- Validation guide: [quickstart.md](quickstart.md)

## Post-Design Constitution Check

- Runnable Legacy Baseline: Pass. Planned files are confined to `dotnet/` and feature documentation.
- Business Flow Parity: Pass. Stable ids from the legacy seed file remain API ids.
- Branch-First, Master-Stable Workflow: Pass. No direct `master` changes.
- Evidence Before Replacement: Pass. The planned implementation uses documented parity anchors from the legacy catalog data and seeds them into the new EF Core catalog store.
- Incremental Migration Over Big Bang: Pass. No cart/checkout/order processing is included.

## Complexity Tracking

No constitution violations require justification.
