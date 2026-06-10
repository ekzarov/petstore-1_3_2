# Feature Specification: Customer Accounts And Sign-In API

**Feature Branch**: `004-customer-accounts-sdd`

**Created**: 2026-06-10

**Status**: Draft

**Input**: User description: "Migrate the legacy PetStore customer account behavior to a backend API slice: create an account, sign in, sign out, and view/update account contact details, preserving parity with the legacy signon and customer components so the future cart and checkout slices can attach orders to a known customer."

## Legacy Evidence *(Principle IV)*

- Legacy components: `src/components/signon` (user id + password authentication), `src/components/customer` (Account, Profile), `src/components/contactinfo`, `src/components/address`, `src/components/creditcard`.
- Seeded users in `src/apps/petstore/src/docroot/populate/Populate-UTF8.xml`: `j2ee`/`j2ee`, `j2ee-ja`/`j2ee`, `shopper`/`j2ee`.
- Legacy customer record: ContactInfo (family name, given name, two street lines, city, state, zip, country, email, phone), CreditCard (number, type, expiry), Profile (preferred language, favorite category, my-list preference, banner preference).

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Create An Account (Priority: P1)

As a store visitor, I want to create an account with a user id and password so that I can later check out orders under my identity.

**Why this priority**: Account creation is the entry point of the legacy order lifecycle and is required by checkout parity.

**Independent Test**: Call the account-creation operation with a new user id, password, and contact details, then verify the account can sign in and its contact details can be read back.

**Acceptance Scenarios**:

1. **Given** no account exists for a user id, **When** a visitor submits user id, password, and required contact details, **Then** the account is created and the response confirms the new account identity.
2. **Given** an account already exists for a user id, **When** a visitor submits the same user id again, **Then** the API reports a duplicate-account conflict and does not change the existing account.
3. **Given** a request is missing required fields, **When** it is submitted, **Then** the API reports a validation error naming the missing fields.

---

### User Story 2 - Sign In And Sign Out (Priority: P1)

As a returning customer, I want to sign in with my user id and password and later sign out so that my browsing session can act on my behalf only while I allow it.

**Why this priority**: Sign-in gates checkout and account management; legacy parity requires the seeded `j2ee` user to authenticate.

**Independent Test**: Sign in with seeded credentials, call an authenticated account operation, sign out, and verify the same operation is rejected afterwards.

**Acceptance Scenarios**:

1. **Given** a seeded account `j2ee` with password `j2ee` exists, **When** the customer signs in with those credentials, **Then** the API establishes an authenticated session or token usable by subsequent requests.
2. **Given** a wrong password, **When** sign-in is attempted, **Then** the API rejects the attempt without revealing whether the user id exists.
3. **Given** a signed-in customer, **When** the customer signs out, **Then** previously issued session state is invalidated for authenticated operations.

---

### User Story 3 - View And Update Account Details (Priority: P2)

As a signed-in customer, I want to view and update my contact information so that future orders ship to the correct address.

**Why this priority**: Required for checkout correctness but only after create/sign-in works.

**Independent Test**: Sign in, read account contact details, change the address, and read it back.

**Acceptance Scenarios**:

1. **Given** a signed-in customer, **When** the customer requests account details, **Then** the API returns contact info fields matching the legacy customer structure.
2. **Given** a signed-in customer, **When** the customer updates contact info with valid values, **Then** subsequent reads return the updated values.
3. **Given** an unauthenticated caller, **When** account details are requested, **Then** the API rejects the request as unauthorized.

---

### Edge Cases

- Sign-in attempts for a non-existent user id must behave like a wrong password (no account enumeration).
- Passwords must never be returned by any API response.
- Concurrent duplicate account creation for the same user id must result in exactly one account.
- Legacy seeded users must be able to authenticate in the migrated slice for parity verification.
- Profile preferences (language, favorite category) may be deferred; if deferred this must be recorded as an explicit assumption.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The API MUST allow creating an account with a unique user id, a password, and contact information.
- **FR-002**: The API MUST reject duplicate user ids with a conflict result that does not modify the existing account.
- **FR-003**: The API MUST authenticate a user id and password and establish a session or token for subsequent authenticated calls.
- **FR-004**: The API MUST allow a signed-in customer to sign out, invalidating the session state used for authenticated operations.
- **FR-005**: The API MUST allow a signed-in customer to read their own account contact details.
- **FR-006**: The API MUST allow a signed-in customer to update their own contact details with validation.
- **FR-007**: The API MUST store passwords only in a non-reversible hashed form.
- **FR-008**: The API MUST NOT disclose whether a user id exists on failed sign-in.
- **FR-009**: The API MUST seed the legacy parity users (`j2ee`, `j2ee-ja`, `shopper`) with deterministic migration-time credentials so parity checks can run.
- **FR-010**: The API MUST keep catalog browsing operations accessible without authentication (no regression of feature 002/003).
- **FR-011**: Authenticated operations MUST reject unauthenticated callers with a consistent error contract.

### Key Entities *(include if feature involves data)*

- **User Credential**: User id and hashed password used only for authentication.
- **Customer Account**: The customer identity owning contact information and (later) orders; keyed by the same user id.
- **Contact Info**: Family name, given name, street lines, city, state, zip, country, email, phone.

### Decision Points *(must be resolved in plan phase)*

- **DP-001**: Session mechanism — cookie session vs token (e.g. JWT) for the Angular UI; must work through the dev proxy.
- **DP-002**: Whether credit card data is stored in this slice or deferred to checkout; storing real card data has compliance weight, and the legacy values are fake.
- **DP-003**: Whether legacy Profile preferences (language, favorite category, my-list, banner) are migrated now or deferred.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A new visitor can create an account and sign in with it in one flow.
- **SC-002**: Seeded user `j2ee` can sign in with the migration-time parity password.
- **SC-003**: A wrong password and an unknown user id produce indistinguishable sign-in failures.
- **SC-004**: A signed-in customer can read and update contact details and see the change persisted.
- **SC-005**: Catalog endpoints remain accessible without authentication.
- **SC-006**: No API response ever contains a password or password hash.

## Assumptions

- The migrated catalog API slice (002) and its database remain in place; account data joins the same migrated datastore unless the plan decides otherwise.
- Authentication is for the storefront customer only; admin authentication is a separate later feature.
- The legacy plaintext password storage is intentionally NOT preserved; hashing is a documented intentional behavior change.
- Automated backend tests are required per Constitution Principle VI; the plan must define unit tests for validation/hashing logic and integration/contract tests for the API surface.
