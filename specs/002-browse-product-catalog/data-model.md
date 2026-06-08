# Data Model: Browse Product Catalog

## Category

Represents a top-level catalog group.

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| id | string | Yes | Stable legacy id, for example `FISH`. |
| name | string | Yes | English display name for this slice, for example `Fish`. |
| description | string | No | Optional short description if available later. |

**Relationships**: One category has zero or more products.

**Validation rules**:

- `id` is unique across categories.
- `id` is stable and must not be derived from display name.
- First seed set includes `FISH`, `DOGS`, `REPTILES`, `CATS`, and `BIRDS`.

## Product

Represents a product family inside a category.

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| id | string | Yes | Stable legacy product id, for example `FI-SW-01`. |
| categoryId | string | Yes | Existing category id. |
| name | string | Yes | English display name, for example `Angelfish`. |
| description | string | No | English legacy description when available. |

**Relationships**: One product belongs to one category and has zero or more sellable items.

**Validation rules**:

- `id` is unique across products.
- `categoryId` must refer to an existing category in the seed data.
- Representative Fish products include `FI-SW-01` Angelfish and `FI-FW-02` Goldfish.

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

**Validation rules**:

- `id` is unique across items.
- `productId` must refer to an existing product in the seed data.
- Price must be non-negative.
- Representative Angelfish items include `EST-1` Large Angelfish and `EST-2` Small Angelfish.

## ApiError

Represents a simple non-success API response.

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| code | string | Yes | Stable machine-readable error code. |
| message | string | Yes | Human-readable error summary. |

**Validation rules**:

- Unknown category, product, or item ids return an `ApiError` with HTTP `404`.
