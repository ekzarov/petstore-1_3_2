# Feature Specification: Supplier Role And Backend Access Split

**Feature Branch**: `013-supplier-role-access-sdd`

**Created**: 2026-06-12

**Status**: Draft

**Input**: User description: "Separate the migrated admin and supplier responsibilities. Admin works with order approval and order monitoring. Supplier works with inventory and fulfillment. Add the supplier role and seeded supplier user, enforce backend access boundaries with tests, and stop treating inventory operations as ordinary admin operations unless explicitly decided."

## Legacy Evidence *(Principle IV)*

- Legacy admin app: `src/apps/admin` exposed a Java Web Start/Swing client launched from `/admin/AdminRequestProcessor`. Its business capabilities were pending-order approval/denial, non-pending order viewing, and sales/order charts.
- Legacy supplier app: `src/apps/supplier` exposed `/supplier/RcvrRequestProcessor` with `supplier`/`supplier` credentials. Its browser UI displayed supplier inventory, accepted quantity updates, and then processed pending supplier orders/invoices after inventory changes.
- Both legacy admin and supplier web modules declared the Java EE role name `administrator`, but they used different principals and different application boundaries: `jps_admin` for admin, `supplier` for supplier. In the migrated single backend this boundary must become explicit authorization policy rather than relying on separate EARs/context roots.
- Current migrated behavior from features 011/012 placed inventory and fulfillment controls under the `admin` role. This feature corrects that role model for closer legacy responsibility parity.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Seed And Authenticate Supplier Identity (Priority: P1)

As a supplier operator, I want to sign in with a dedicated supplier identity so that inventory operations are not performed through the store administrator account.

**Why this priority**: Role separation cannot be verified until a supplier identity exists and carries the correct authorization role.

**Independent Test**: Sign in as seeded `supplier` and verify the returned identity has role `supplier`, while seeded `admin` still has role `admin`.

**Acceptance Scenarios**:

1. **Given** the application starts against an existing database, **When** seeding runs, **Then** a deterministic `supplier` user exists with role `supplier` and password `supplier`.
2. **Given** seeded `supplier` credentials, **When** sign-in succeeds, **Then** the token contains a supplier role claim and not an admin role claim.
3. **Given** seeded `admin` credentials, **When** sign-in succeeds, **Then** the token remains admin-only and is not converted into a supplier identity.

---

### User Story 2 - Restrict Order Administration To Admins (Priority: P1)

As a store administrator, I want order decision and order-monitoring endpoints to remain admin-only so that supplier operators cannot approve, deny, or browse customer order details.

**Why this priority**: This preserves the legacy admin responsibility boundary and prevents the supplier role from becoming a superuser.

**Independent Test**: Sign in as `supplier` and verify all admin order endpoints reject access; sign in as `admin` and verify the same endpoints still work.

**Acceptance Scenarios**:

1. **Given** a supplier identity, **When** it calls pending-order list, approve, deny, status-filtered order list, order detail, or transition-history endpoints, **Then** the API rejects the call as forbidden.
2. **Given** an admin identity, **When** it calls order administration endpoints, **Then** the existing feature-010/012 behavior remains available.
3. **Given** a customer or anonymous caller, **When** it calls order administration endpoints, **Then** access remains rejected as before.

---

### User Story 3 - Restrict Supplier Inventory And Fulfillment Operations (Priority: P1)

As a supplier operator, I want to view and adjust supplier inventory and trigger fulfillment processing so that backordered approved orders can be completed without granting me order-approval privileges.

**Why this priority**: Inventory adjustment is the supplier application's core legacy capability.

**Independent Test**: Set inventory for an item to zero, create an approved order that waits for stock, sign in as `supplier`, increase inventory, trigger fulfillment if required by the selected design, and verify the order completes without supplier access to admin order tabs.

**Acceptance Scenarios**:

1. **Given** a supplier identity, **When** it lists supplier inventory, **Then** the API returns item ids and on-hand quantities.
2. **Given** a supplier identity, **When** it sets a non-negative inventory quantity, **Then** the API persists the new value and returns the confirmed value.
3. **Given** a supplier identity, **When** it triggers fulfillment processing, **Then** eligible `APPROVED` or `SHIPPED_PART` orders are processed according to feature 011 rules.
4. **Given** a customer identity, **When** it calls inventory or fulfillment endpoints, **Then** the API rejects the call.

---

### Edge Cases

- Existing databases may already have an `admin` user and no `supplier`; seeding must be idempotent and must not reset passwords for existing users unexpectedly.
- Existing APIs may currently use a broad admin policy for inventory; changing it must not accidentally expose inventory to customers or anonymous callers.
- If an order is already `COMPLETED`, supplier fulfillment re-runs must remain idempotent and must not create duplicate shipments.
- If inventory is raised for one item, only affected waiting orders should be processed automatically if that design is selected.
- Forbidden responses must not leak customer order details to supplier identities.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The backend MUST seed a deterministic `supplier` user with role `supplier` and password `supplier` for local parity verification.
- **FR-002**: The authentication response and JWT claims MUST represent the supplier role distinctly from `admin` and `customer`.
- **FR-003**: Order administration endpoints MUST require the `admin` role and MUST reject `supplier`, `customer`, and anonymous callers.
- **FR-004**: Supplier inventory list/update endpoints MUST require the supplier authorization policy decided in this feature.
- **FR-005**: Fulfillment trigger endpoints MUST require the supplier authorization policy decided in this feature.
- **FR-006**: Backend tests MUST cover supplier sign-in, admin-vs-supplier authorization boundaries, and customer/anonymous denial.
- **FR-007**: Backend tests MUST cover the inventory replenishment flow that unblocks an approved or partially shipped order.
- **FR-008**: The API MUST keep existing customer-facing catalog, cart, checkout, and order-history behavior unchanged.

### Key Entities *(include if feature involves data)*

- **Supplier Identity**: A privileged non-customer user that can operate supplier inventory and fulfillment but cannot decide orders.
- **Admin Identity**: A privileged user that can approve/deny and browse orders but is not automatically assumed to be a supplier operator unless explicitly decided.
- **Supplier Inventory Operation**: List, set quantity, and fulfillment trigger operations from feature 011.

### Decision Points *(must be resolved in plan phase)*

- **DP-001**: Should `admin` retain access to supplier inventory as a superuser, or should inventory be strictly `supplier`-only?
- **DP-002**: Should fulfillment run automatically after a successful inventory update, matching the legacy supplier submit flow, or remain an explicit backend operation?
- **DP-003**: Should the current `/api/admin/inventory` route be kept for compatibility, moved to `/api/supplier/inventory`, or aliased during migration?
- **DP-004**: Should a supplier be allowed to read minimal waiting-order references affected by inventory, or only operate inventory and run fulfillment blindly?

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: `supplier` can sign in and call supplier inventory operations locally.
- **SC-002**: `supplier` receives forbidden responses for every order approval and order browsing endpoint.
- **SC-003**: `admin` can still approve and deny pending orders.
- **SC-004**: A zero-stock approved order completes after supplier replenishment and fulfillment according to the selected design.
- **SC-005**: Authorization contract tests fail if supplier and admin permissions collapse back into one broad role accidentally.

## Assumptions

- This feature changes backend authorization and seeding only; Angular route and tab visibility are handled by feature 014.
- The existing `customer` and `admin` roles remain valid.
- Legacy Java EE role naming is not preserved literally; the migrated role names should represent business responsibilities.
- Automated backend tests are required per Constitution Principle VI.
