# Data Model: Browse Catalog UI

This feature does not create new persistent data. The frontend consumes the existing catalog API contract from `specs/002-browse-product-catalog/contracts/openapi.yaml`.

## CatalogCategory

**Purpose**: Top-level category shown as the entry point into catalog browsing.

**Fields**:

- `id`: Stable legacy category id, for example `FISH`.
- `name`: Display name, for example `Fish`.
- `description`: Optional category description.

**Validation/Display Rules**:

- `id` is required and must be preserved in route links.
- `name` is required and is the primary visible label.
- Missing `description` must not break the category list.

## CatalogProduct

**Purpose**: Product family listed for a selected category.

**Fields**:

- `id`: Stable legacy product id, for example `FI-SW-01`.
- `categoryId`: Stable category id that owns the product.
- `name`: Display name, for example `Angelfish`.
- `description`: Optional product description.

**Relationships**:

- Belongs to one `CatalogCategory`.
- Has zero or more `CatalogItem` records.

**Validation/Display Rules**:

- `id` and `categoryId` must be preserved in route links and service calls.
- Empty product lists for known categories are shown as empty states, not errors.

## CatalogItem

**Purpose**: Sellable catalog variant shown under a product and in item details.

**Fields**:

- `id`: Stable legacy item id, for example `EST-1`.
- `productId`: Stable product id, for example `FI-SW-01`.
- `name`: Display name, for example `Large Angelfish`.
- `attributes`: Optional attribute list.
- `description`: Optional item description.
- `price`: Numeric price.
- `currency`: Currency code, for example `USD`.

**Relationships**:

- Belongs to one `CatalogProduct`.

**Validation/Display Rules**:

- `id`, `productId`, `price`, and `currency` must be visible or inspectable where required by the specification.
- Missing `attributes` or `description` must be rendered as absent/empty detail, not as a broken page.

## CatalogViewState

**Purpose**: UI state for each catalog route.

**States**:

- `loading`: The route has started an API request and is waiting for data.
- `ready`: Data was loaded and can be displayed.
- `empty`: Known category/product exists but has no products/items.
- `notFound`: Backend returned a missing category, product, or item response.
- `unavailable`: Backend cannot be reached or returned an unexpected error.

**State Transitions**:

- Route activation starts in `loading`.
- Successful non-empty response transitions to `ready`.
- Successful empty array response transitions to `empty`.
- `404` transitions to `notFound`.
- Network and unexpected errors transition to `unavailable`.
