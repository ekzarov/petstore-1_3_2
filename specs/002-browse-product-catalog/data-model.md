# Data Model: Browse Product Catalog

## Category

Represents a top-level catalog group.

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| id | string | Yes | Stable legacy id, for example `FISH`. |
| name | string | Yes | English display name for this slice, for example `Fish`. |
| description | string | No | Optional short description if available later. |

**Relationships**: One category has zero or more products.

**Persistence**: Stored as an EF Core entity with `id` as the primary key.

**Validation rules**:

- `id` is unique across categories.
- `id` is stable and must not be derived from display name.
- Seed set includes `FISH`, `DOGS`, `REPTILES`, `CATS`, and `BIRDS` from the legacy catalog XML.

## Product

Represents a product family inside a category.

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| id | string | Yes | Stable legacy product id, for example `FI-SW-01`. |
| categoryId | string | Yes | Existing category id. |
| name | string | Yes | English display name, for example `Angelfish`. |
| description | string | No | English legacy description when available. |

**Relationships**: One product belongs to one category and has zero or more sellable items.

**Persistence**: Stored as an EF Core entity with `id` as the primary key and `categoryId` as a required foreign key to Category.

**Validation rules**:

- `id` is unique across products.
- `categoryId` must refer to an existing category in the seed data.
- Seeded products must cover every English legacy product from `Populate-UTF8.xml`: 16 products across Fish, Dogs, Reptiles, Cats, and Birds.
- Representative Fish products include `FI-SW-01` Angelfish, `FI-SW-02` Tiger Shark, `FI-FW-01` Koi, and `FI-FW-02` Goldfish.

## Item

Represents a sellable catalog variant.

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| id | string | Yes | Stable legacy item id, for example `EST-1`. |
| productId | string | Yes | Existing product id. |
| name | string | Yes | Display name suitable for API clients. |
| attributes | string[] | No | Variant attributes such as `Large` or `Small`. |
| description | string | No | English legacy item description when available. |
| price | decimal | Yes | List price from legacy seed data. |
| currency | string | Yes | `USD` for this slice. |

**Relationships**: One item belongs to one product.

**Persistence**: Stored as an EF Core entity with `id` as the primary key and `productId` as a required foreign key to Product. `attributes` may be persisted as owned child rows or a provider-appropriate serialized value; the API response remains an array of strings.

**Validation rules**:

- `id` is unique across items.
- `productId` must refer to an existing product in the seed data.
- Price must be non-negative.
- Seeded items must cover every English legacy item from `Populate-UTF8.xml`: `EST-1` through `EST-28`.
- Representative Angelfish items include `EST-1` Large Angelfish and `EST-2` Small Angelfish.
- Representative non-Fish items include `EST-6` Male Adult Bulldog, `EST-13` Green Adult Iguana, `EST-14` Tailless Manx, and `EST-18` Adult Male Amazon Parrot.

## ApiError

Represents a simple non-success API response.

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| code | string | Yes | Stable machine-readable error code. |
| message | string | Yes | Human-readable error summary. |

**Validation rules**:

- Unknown category, product, or item ids return an `ApiError` with HTTP `404`.

## PetstoreCatalogContext

EF Core context for the migrated catalog slice.

| DbSet | Entity | Notes |
|-------|--------|-------|
| Categories | Category | Top-level seeded categories. |
| Products | Product | Product families linked to categories. |
| Items | Item | Sellable variants linked to products. |

**Configuration rules**:

- Reads connection string `PetstoreCatalog` from application configuration.
- Local development uses SQL Server with Windows Authentication unless a later plan selects another provider.
- Schema must enforce required ids and relationships needed for catalog parity.
- Entity mapping rules live in separate EF Core `IEntityTypeConfiguration<T>` classes.
- Table names, max lengths, precision, and similar mapping literals are centralized in catalog model constants.

## Catalog Seeder

Deterministic seeder that inserts or updates the full English legacy catalog data needed by browsing and future cart parity.

**Seed rules**:

- Must preserve stable legacy ids.
- Must seed at least `FISH`, `DOGS`, `REPTILES`, `CATS`, `BIRDS`.
- Must seed all 16 legacy products from Fish, Dogs, Reptiles, Cats, and Birds.
- Must seed all 28 legacy sellable items, `EST-1` through `EST-28`.
- Must seed Fish products including `FI-SW-01` Angelfish, `FI-SW-02` Tiger Shark, `FI-FW-01` Koi, and `FI-FW-02` Goldfish.
- Must seed Angelfish items including `EST-1` Large Angelfish and `EST-2` Small Angelfish, plus sellable items for Dogs, Reptiles, Cats, and Birds.
- Must be safe to run repeatedly without duplicating rows.
