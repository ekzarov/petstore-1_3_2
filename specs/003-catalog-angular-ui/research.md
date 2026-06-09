# Research: Browse Catalog UI

## Decision: Use a Separate Angular App Under `frontend/petstore-ui`

**Rationale**: The frontend should evolve independently from the ASP.NET backend and the legacy Java runtime. A separate Angular app keeps migration slices isolated and avoids coupling early UI experiments to backend hosting decisions.

**Alternatives considered**:

- Host Angular output from ASP.NET immediately: rejected because production hosting is not part of this first UI slice.
- Modify the legacy Java UI: rejected because this feature is for the new migrated UI and should not destabilize the legacy baseline.

## Decision: Use Angular Router for Catalog Navigation

**Rationale**: Category, product, item list, and item details views need refresh-safe and back/forward-safe navigation. Router-based deep links make manual validation repeatable and preserve stable catalog ids in URLs for future cart work.

**Alternatives considered**:

- Single component with in-memory selection only: rejected because browser refresh and direct URL navigation are explicit edge cases.

## Decision: Use Angular Dev Proxy for `/api/*`

**Rationale**: The frontend and backend remain separate local processes, but browser requests stay same-origin from the Angular dev server's point of view. This avoids adding development-only CORS configuration to the ASP.NET backend while preserving a clean API boundary.

**Alternatives considered**:

- Direct backend host and port from Angular: rejected because browser CORS would require backend changes that are not needed for this first UI slice.
- Hardcoded API data in the frontend: rejected because the UI must consume migrated backend catalog behavior.

## Decision: Keep the First UI Slice Manually Validated

**Rationale**: The user explicitly chose no automated UI tests for the first pass. Manual validation is enough for the initial catalog browser because backend API behavior is already covered by integration tests from `specs/002-browse-product-catalog`.

**Alternatives considered**:

- Add Angular unit tests and e2e tests now: rejected to keep the first UI iteration small.

## Decision: Minimal Modern UI, Not Legacy Visual Clone

**Rationale**: The first UI should prove migrated catalog behavior in a browser without copying the old Java PetStore look. Styling should be clean, readable, responsive, and restrained, leaving richer product styling for later refinement.

**Alternatives considered**:

- Recreate legacy Java PetStore pages: rejected because it would spend effort on old visual conventions before validating the migrated frontend flow.
