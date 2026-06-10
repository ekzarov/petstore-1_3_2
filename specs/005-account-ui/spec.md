# Feature Specification: Account UI

**Feature Branch**: `005-account-ui-sdd`

**Created**: 2026-06-10

**Status**: Draft

**Input**: User description: "Extend the Angular catalog UI with account features backed by the customer accounts API (004): create an account, sign in, sign out, and view/update contact details, keeping catalog browsing available without sign-in."

## Dependencies

- Requires the customer accounts API from `specs/004-customer-accounts` to be implemented and reachable through the existing `/api` dev proxy.
- Builds on the Angular app from `specs/003-catalog-angular-ui` (zoneless Angular 21, signal-based view state, shared loading/empty/unavailable components).

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Sign In And Sign Out From The Storefront (Priority: P1)

As a returning customer, I want to sign in and out from the catalog UI so that the store can act on my identity when I am ready to order.

**Why this priority**: Sign-in is the smallest UI slice that exercises the new accounts API and unblocks cart/checkout UI work.

**Independent Test**: Open the UI, sign in as `j2ee`, verify the header shows the signed-in identity, sign out, and verify the header returns to the anonymous state.

**Acceptance Scenarios**:

1. **Given** the catalog UI is open, **When** the visitor opens the sign-in view and submits valid credentials, **Then** the UI shows the signed-in user and offers sign-out.
2. **Given** invalid credentials, **When** sign-in is submitted, **Then** the UI shows a generic failure message and stays on the form with entered user id preserved.
3. **Given** a signed-in customer, **When** the customer signs out, **Then** the UI returns to the anonymous state and authenticated views are no longer accessible.

---

### User Story 2 - Create An Account (Priority: P1)

As a new visitor, I want to register an account from the UI so that I can sign in and later place orders.

**Why this priority**: Account creation pairs with sign-in to complete the identity entry point.

**Independent Test**: Open the registration view, submit a new user id, password, and contact details, and verify the UI lands in the signed-in state.

**Acceptance Scenarios**:

1. **Given** the registration form is filled with valid values, **When** it is submitted, **Then** the account is created and the visitor ends up signed in (or on the sign-in view, as decided in the plan).
2. **Given** a duplicate user id, **When** the form is submitted, **Then** the UI shows a duplicate-id message and preserves the entered contact details.
3. **Given** missing required fields, **When** the form is submitted, **Then** the UI marks the invalid fields without calling the backend or after backend validation, consistently.

---

### User Story 3 - View And Edit Account Details (Priority: P2)

As a signed-in customer, I want to view and edit my contact details in the UI so that my future orders use correct shipping data.

**Why this priority**: Needed before checkout UI but after the identity flows work.

**Independent Test**: Sign in, open the account view, change the address, save, reload the page, and verify the change persisted.

**Acceptance Scenarios**:

1. **Given** a signed-in customer, **When** the account view opens, **Then** contact details from the API are displayed.
2. **Given** edited valid contact details, **When** the customer saves, **Then** the UI confirms the save and shows the updated values after reload.
3. **Given** an anonymous visitor navigating to the account route, **When** the route activates, **Then** the UI redirects to sign-in instead of showing the account view.

---

### Edge Cases

- Session expiry or backend-invalidated session while the UI still believes it is signed in.
- Backend unavailable during sign-in, registration, or account loading (reuse the unavailable state pattern).
- Browser refresh on the account route must restore the signed-in state or redirect to sign-in.
- Password fields must never be pre-filled or echoed back.
- Catalog browsing must remain fully usable while signed out.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The UI MUST provide a sign-in view with user id and password fields.
- **FR-002**: The UI MUST show the current identity state (anonymous vs signed-in user id) in the catalog shell on all catalog routes.
- **FR-003**: The UI MUST provide sign-out available from any catalog route while signed in.
- **FR-004**: The UI MUST provide an account registration view collecting user id, password, and the contact fields required by the accounts API.
- **FR-005**: The UI MUST provide an account view to read and update contact details for the signed-in customer.
- **FR-006**: The UI MUST guard account-only routes and redirect anonymous visitors to sign-in.
- **FR-007**: The UI MUST keep all catalog browsing from feature 003 accessible without sign-in.
- **FR-008**: The UI MUST show clear loading, validation-error, failure, and backend-unavailable states for all account operations.
- **FR-009**: The UI MUST restore identity state on full page reload using the session mechanism chosen in feature 004 (DP-001).
- **FR-010**: The UI MUST NOT store passwords in browser storage.

### Key Entities *(include if feature involves data)*

- **Identity State**: Anonymous or signed-in user id, shared across catalog and account views.
- **Account Form State**: Registration and edit form values, validation errors, submission state.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A visitor can register, sign in, and sign out entirely through the UI against the running backend.
- **SC-002**: Seeded user `j2ee` can sign in through the UI.
- **SC-003**: A signed-in customer can edit contact details and the change survives a browser reload.
- **SC-004**: An anonymous visitor opening the account route is redirected to sign-in.
- **SC-005**: All catalog flows from feature 003 still pass their manual quickstart while signed out.

## Assumptions

- Manual validation remains the test approach for the UI unless the plan introduces automated UI tests; this deferral follows the precedent of feature 003 and must be re-confirmed at plan time.
- Visual design follows the minimal modern style of feature 003; no legacy visual parity is intended.
- Profile preferences are out of scope unless feature 004 resolves DP-003 to include them.
