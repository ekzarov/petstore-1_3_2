# Feature Specification: Browse Catalog UI

**Feature Branch**: `003-catalog-angular-ui-sdd`

**Created**: 2026-06-08

**Status**: Draft

**Input**: User description: "Create a separate frontend catalog browsing UI that shows the catalog behavior already covered by the backend: categories, products in a category, items for a product, item details, and missing-data states. Keep the first version minimal and modern rather than copying the legacy Java UI. Run the frontend separately from the ASP.NET backend during development."

## Clarifications

### Session 2026-06-08

- Q: Should the catalog UI be a separate frontend application? -> A: Yes, place it under a separate frontend area and run it independently from the ASP.NET backend.
- Q: Should the first UI copy the legacy Java PetStore visual design? -> A: No, build a simple modern catalog browsing UI first and improve styling later.
- Q: Should the first UI implementation include automated UI tests? -> A: No, validate manually for the first pass.
- Q: How should the frontend reach the backend during local development? -> A: Use the Angular development proxy for `/api/*` so the browser talks to the frontend origin while the dev server forwards API calls to the ASP.NET backend.
- Q: What catalog data should the UI expect from the backend? -> A: The backend seed mirrors the full English legacy catalog, so all five categories can expose products and items rather than only Fish.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Browse Categories (Priority: P1)

As a store visitor, I want to see available pet categories in a browser so that I can choose where to start browsing the catalog.

**Why this priority**: Categories are the first visible entry point into the catalog and the smallest useful UI slice.

**Independent Test**: Start the backend and frontend, open the catalog UI, and verify that the category list appears without signing in.

**Acceptance Scenarios**:

1. **Given** the catalog backend is available with migrated catalog data, **When** a visitor opens the catalog UI, **Then** the visitor sees Fish, Dogs, Reptiles, Cats, and Birds.
2. **Given** the visitor is viewing categories, **When** the visitor selects Fish, **Then** the UI shows that Fish is the active browsing context and loads products for Fish.

---

### User Story 2 - Browse Products In A Category (Priority: P1)

As a store visitor, I want to view products in a selected category so that I can choose a product family to inspect.

**Why this priority**: Product browsing is the core continuation from category selection and proves the UI can use the existing catalog API beyond the landing state.

**Independent Test**: Select Fish and verify that Angelfish and Goldfish appear with meaningful descriptions.

**Acceptance Scenarios**:

1. **Given** the visitor selected Fish, **When** the product list loads, **Then** Angelfish and Goldfish are visible.
2. **Given** the visitor selected Dogs, Reptiles, Cats, or Birds, **When** the product list loads, **Then** products from that legacy category are visible.
3. **Given** a known category has no products in a future or test dataset, **When** the visitor opens that category, **Then** the UI shows an empty state without treating it as an error.

---

### User Story 3 - Browse Items And Item Details (Priority: P1)

As a store visitor, I want to open a product and inspect sellable items so that I can understand which item variants exist before future cart work is added.

**Why this priority**: Items are the sellable catalog units needed by the future cart and checkout migration.

**Independent Test**: Open Angelfish, verify Large Angelfish and Small Angelfish, then open Large Angelfish and verify its stable id, attributes, price, and currency.

**Acceptance Scenarios**:

1. **Given** the visitor is viewing Angelfish, **When** item data loads, **Then** Large Angelfish and Small Angelfish are visible with prices.
2. **Given** the visitor selects Large Angelfish, **When** the item details are shown, **Then** the UI displays `EST-1`, product id `FI-SW-01`, attributes, description, price, and currency.

---

### User Story 4 - Handle Missing Catalog Data (Priority: P2)

As a store visitor, I want clear feedback when a category, product, or item cannot be found so that I do not see a broken or misleading page.

**Why this priority**: Missing data is less common than normal browsing but is important for robust navigation and manual validation.

**Independent Test**: Navigate to unknown category, product, and item states and verify clear not-found feedback with a route back to normal browsing.

**Acceptance Scenarios**:

1. **Given** a visitor opens a route for an unknown category, **When** the backend reports that it is missing, **Then** the UI shows a category-not-found state and offers a path back to the category list.
2. **Given** a visitor opens a route for an unknown product or item, **When** the backend reports that it is missing, **Then** the UI shows a clear not-found state and does not display unrelated catalog data.

---

### Edge Cases

- The backend is not running or cannot be reached.
- The backend returns an error while loading categories, products, items, or item details.
- A known category has no products.
- A known product has no sellable items.
- Catalog descriptions or item attributes are missing.
- A user refreshes the browser on a category, product, item list, or item details route.
- A user navigates back and forward between catalog levels.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The UI MUST show the migrated catalog categories Fish, Dogs, Reptiles, Cats, and Birds when catalog data is available.
- **FR-002**: The UI MUST allow a visitor to select a category and view products for that category.
- **FR-003**: The UI MUST show product names and descriptions when product data is available.
- **FR-004**: The UI MUST allow a visitor to select a product and view sellable items for that product.
- **FR-005**: The UI MUST show item names, stable item ids, prices, and currency for sellable items.
- **FR-006**: The UI MUST allow a visitor to open an item details view.
- **FR-007**: The item details view MUST show item id, product id, name, attributes, description, price, and currency when available.
- **FR-008**: The UI MUST preserve stable legacy ids in visible or inspectable item and navigation states where they are needed for future cart work.
- **FR-009**: The UI MUST NOT require sign-in for catalog browsing.
- **FR-010**: The UI MUST provide clear loading, empty, backend-unavailable, and not-found states.
- **FR-011**: The UI MUST provide a way to return from deeper catalog views to broader catalog views.
- **FR-012**: The first UI slice MUST remain read-only and MUST NOT include cart, checkout, account, search, localization, recommendations, or admin catalog editing.
- **FR-013**: The UI MUST consume the existing migrated catalog API behavior rather than hardcoded catalog data.
- **FR-014**: The UI MUST support manual validation of all primary browsing paths during the first pass.
- **FR-015**: The UI MUST support browsing products and items in every seeded legacy category, including Dogs, Reptiles, Cats, and Birds, not only Fish.

### Key Entities *(include if feature involves data)*

- **Catalog Category**: A top-level catalog group with a stable id and display name.
- **Catalog Product**: A product family in a category with a stable id, category id, display name, and optional description.
- **Catalog Item**: A sellable product variant with a stable id, product id, display name, optional attributes, optional description, price, and currency.
- **Catalog View State**: The user's current browsing level, selected category, selected product, selected item, loading state, empty state, or error state.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A visitor can open the UI and see all five seeded category names without signing in.
- **SC-002**: A visitor can select Fish and see Angelfish and Goldfish in one browsing flow.
- **SC-002a**: A visitor can select a non-Fish category such as Dogs and see its legacy products in the same browsing flow.
- **SC-003**: A visitor can open Angelfish and see Large Angelfish and Small Angelfish with prices and currency.
- **SC-004**: A visitor can open Large Angelfish details and verify `EST-1`, `FI-SW-01`, attributes, description, price, and currency.
- **SC-005**: Unknown category, product, and item states show clear not-found feedback instead of unrelated catalog data.
- **SC-006**: If the backend is unavailable, the UI shows a clear unavailable state during manual validation.
- **SC-007**: Manual validation can complete the primary categories-to-item-details flow in under two minutes on a local developer machine.

## Assumptions

- The catalog API slice from `specs/002-browse-product-catalog` is available and remains the source of catalog data.
- The first UI pass is browser-only and read-only.
- The frontend and backend run as separate local development processes, with Angular dev proxy forwarding `/api/*` requests to the backend during local development.
- The first pass favors functional clarity over final visual polish.
- Automated UI tests are deferred until a later refinement unless the plan phase changes this decision.
- Cart, checkout, login, account creation, search, localization, recommendations, and admin catalog editing are outside this feature.
