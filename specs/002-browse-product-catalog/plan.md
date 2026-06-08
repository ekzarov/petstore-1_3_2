# Implementation Plan: Browse Product Catalog

**Branch**: `spec-browse-product-catalog` | **Date**: 2026-06-08 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/002-browse-product-catalog/spec.md`

## Summary

Implement the first migrated catalog slice as a read-only ASP.NET Core Web API in the existing `dotnet/Petstore` solution. The API exposes categories, products by category, items by product, and item by id using hardcoded in-process seed data that preserves stable legacy PetStore identifiers for later cart and checkout migration.

## Technical Context

**Language/Version**: C# on the existing `net10.0` ASP.NET Core scaffold.

**Primary Dependencies**: ASP.NET Core controllers and existing `Microsoft.AspNetCore.OpenApi`; no additional runtime dependency is planned for this slice.

**Storage**: In-process hardcoded catalog seed data held in a static class/provider. Database persistence is intentionally deferred.

**Testing**: .NET test project to be added under `dotnet/Petstore.Tests`, using xUnit or the repo-approved .NET test default during task generation. Contract tests should exercise the API through ASP.NET Core test hosting.

**Target Platform**: Local developer machine and container-friendly ASP.NET Core service; legacy Docker/Payara remains the parity reference but is not changed by this feature.

**Project Type**: Web service API inside the existing `dotnet/Petstore` solution.

**Performance Goals**: Catalog reads should complete within ordinary local API latency for static in-memory data; no external I/O is allowed in this slice.

**Constraints**: Read-only API; no authentication; no database; no JMS; no cart, checkout, OPC, Supplier, invoice, search, localization, admin inventory editing, or personalized catalog behavior.

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
|   |-- CatalogSeedData.cs
|   `-- CatalogStore.cs
|-- Models/
|   |-- CategoryDto.cs
|   |-- ProductDto.cs
|   |-- ItemDto.cs
|   `-- ApiErrorDto.cs
|-- Program.cs
|-- Petstore.csproj
`-- Petstore.slnx

dotnet/Petstore.Tests/
|-- CatalogApiContractTests.cs
`-- Petstore.Tests.csproj
```

**Structure Decision**: Keep the first slice in the existing ASP.NET Core project to minimize migration surface area. Add a narrow test project next to it so the API contract can be verified without touching the legacy Java runtime.

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
- Evidence Before Replacement: Pass. The planned implementation uses documented parity anchors from the legacy catalog data.
- Incremental Migration Over Big Bang: Pass. No cart/checkout/order processing is included.

## Complexity Tracking

No constitution violations require justification.
