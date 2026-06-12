# Feature Specification: Supplier UI And Admin Navigation Split

**Feature Branch**: `014-supplier-ui-access-sdd`

**Created**: 2026-06-12

**Status**: Draft

**Input**: User description: "Split the Angular privileged UI so admin users work with order administration and supplier users work with inventory/fulfillment. Supplier must not see admin tabs such as Pending queue and All orders. Decide whether admin may see inventory, but supplier definitely must not see admin order tabs."

## Dependencies

- Requires supplier backend role/access split from `specs/013-supplier-role-access`.
- Builds on the existing Angular app and identity service from `specs/005-account-ui` and current admin area from `specs/012-admin-ui`.

## Legacy Evidence *(Principle IV)*

- Legacy admin UI was a Java Web Start/Swing client launched from `/admin`, focused on orders, approval/denial, and charts.
- Legacy supplier UI was a browser page under `/supplier`, focused on inventory quantity update and supplier fulfillment processing.
- The migrated UI currently places inventory under `/admin`, which is convenient but does not preserve the legacy responsibility boundary.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Supplier Accesses Inventory Workspace (Priority: P1)

As a supplier operator, I want to sign in and see only supplier inventory tools so that I can replenish stock and unblock fulfillment without seeing admin order-management tabs.

**Why this priority**: This is the visible UI expression of the backend role split and the core legacy supplier capability.

**Independent Test**: Sign in as `supplier`, open the privileged area, verify inventory is available, and verify Pending queue and All orders are absent and inaccessible by direct URL.

**Acceptance Scenarios**:

1. **Given** a signed-in supplier, **When** the shell renders, **Then** it offers a supplier/inventory entry point and does not show the Admin order-management entry point.
2. **Given** a signed-in supplier, **When** the inventory page opens, **Then** the supplier sees item ids, on-hand quantities, editable non-negative quantity inputs, and save actions.
3. **Given** a signed-in supplier, **When** it navigates directly to pending queue or all-orders routes, **Then** the UI redirects or shows forbidden feedback without rendering order data.

---

### User Story 2 - Admin Keeps Order Management Workspace (Priority: P1)

As a store administrator, I want my UI to focus on pending orders and order monitoring so that supplier inventory duties do not obscure order decision workflows.

**Why this priority**: Admin order approval is the legacy admin responsibility and should remain easy to find after inventory moves away from the admin tabs.

**Independent Test**: Sign in as `admin`, verify Pending queue and All orders remain available, and verify supplier-only routes follow the plan decision for admin inventory visibility.

**Acceptance Scenarios**:

1. **Given** a signed-in admin, **When** the admin area opens, **Then** Pending queue and All orders are available.
2. **Given** a signed-in admin, **When** an above-threshold order is pending, **Then** approve/deny behavior remains unchanged from feature 012.
3. **Given** a signed-in admin, **When** the admin opens the inventory route, **Then** access follows the decision from 013/014 planning: either allowed as superuser or denied/hidden if inventory is supplier-only.

---

### User Story 3 - Customer And Anonymous Users Cannot Reach Privileged Workspaces (Priority: P1)

As a customer or anonymous visitor, I must not be able to see admin or supplier operations so that privileged operational data remains protected.

**Why this priority**: Route visibility is helpful, but backend authorization remains the enforcement of record; the UI must not mislead users into thinking protected pages are available.

**Independent Test**: Open admin and supplier routes while anonymous and as a customer, and verify redirect/forbidden behavior without rendering protected data.

**Acceptance Scenarios**:

1. **Given** an anonymous visitor, **When** it opens an admin or supplier route, **Then** it is redirected to sign-in with a return URL.
2. **Given** a customer identity, **When** it opens an admin or supplier route, **Then** the UI shows forbidden feedback or redirects to a safe catalog route.
3. **Given** the backend returns 403 for a privileged API call, **When** the UI receives it, **Then** it clears any inappropriate view state and displays a safe forbidden state.

---

### Edge Cases

- A user signs in as `supplier` while already on an admin order route.
- A user signs in as `admin` while already on a supplier inventory route.
- Browser refresh on every privileged route.
- The backend is running an older version without supplier role support.
- Inventory save succeeds but fulfillment processing fails or is unavailable.
- Deep links must not expose stale data from a previously signed-in privileged user after sign-out.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The UI MUST distinguish `admin`, `supplier`, `customer`, and anonymous identity states.
- **FR-002**: Supplier identities MUST have access to the inventory/fulfillment UI.
- **FR-003**: Supplier identities MUST NOT see or access Pending queue, All orders, order detail, approve, deny, or transition-history admin views.
- **FR-004**: Admin identities MUST retain access to Pending queue and All orders views.
- **FR-005**: Admin access to inventory MUST follow the plan decision and be reflected consistently in navigation, route guards, and API error handling.
- **FR-006**: Customer and anonymous identities MUST NOT see privileged navigation links and MUST NOT render privileged views by direct URL.
- **FR-007**: The inventory UI MUST consume the supplier backend API from feature 013 rather than hardcoding admin endpoints.
- **FR-008**: If the selected design runs fulfillment automatically after inventory save, the UI MUST show the resulting state clearly without requiring a separate button for the normal path.
- **FR-009**: If a manual fulfillment action remains, the UI MUST label it as an operational recovery/run action and restrict it to the selected supplier policy.
- **FR-010**: Manual UI validation MUST cover admin, supplier, customer, and anonymous route/access combinations.

### Key Entities *(include if feature involves data)*

- **Privileged Navigation State**: Which operational route links are visible for the signed-in role.
- **Supplier Inventory View State**: Supplier-owned inventory list/edit/fulfillment state.
- **Forbidden View State**: Safe state displayed when a role lacks access to a route or backend operation.

### Decision Points *(must be resolved in plan phase)*

- **DP-001**: Should supplier UI live under `/supplier` as a separate route area, or remain under `/admin/inventory` with supplier-aware guards?
- **DP-002**: Should admin be a superuser for supplier inventory, or should inventory be hidden/forbidden to admin by default?
- **DP-003**: Should inventory save automatically trigger fulfillment in the UI flow, matching legacy supplier submit behavior, or should "Run fulfillment" remain a separate visible action?
- **DP-004**: Should the shell show separate `Admin` and `Supplier` links for users with multiple roles if such users are introduced later?

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Signing in as `supplier` shows an inventory workspace and no admin order tabs.
- **SC-002**: Direct navigation by `supplier` to pending queue or all-orders routes does not render order data.
- **SC-003**: Signing in as `admin` preserves order approval and order browsing flows.
- **SC-004**: Customer and anonymous route checks cannot reveal admin or supplier data.
- **SC-005**: A supplier replenishment flow can complete a waiting approved order according to the selected fulfillment trigger design.

## Assumptions

- This feature is UI-only and must not weaken backend authorization; backend enforcement belongs to feature 013.
- The existing Angular app remains a single app unless planning decides to split supplier/admin into separate apps.
- Visual design should remain consistent with the current modern Angular UI and should not copy the legacy table styling.
- Automated UI tests are not required unless the plan changes the project's current UI testing approach.
