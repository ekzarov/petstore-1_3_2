# Tasks: Supplier Role And Backend Access Split

**Input**: `specs/013-supplier-role-access/plan.md`, `specs/013-supplier-role-access/spec.md`

**Tests**: Required per Constitution VI.

## Phase 1: Foundational (role + seeding)

- [X] T001 Add `Roles.Supplier` to `dotnet/Petstore/Data/AccountModelConstants.cs`
- [X] T002 Seed `supplier`/`supplier` (role `supplier`) idempotently through explicit EF Core migration data
- [X] T003 Register the `SupplierOperations` policy (roles `supplier`, `admin`) in `dotnet/Petstore/Program.cs`

## Phase 2: User Story 1 - Seed And Authenticate Supplier Identity (P1)

- [X] T004 [US1] Add contract tests: supplier sign-in returns role `supplier` (not admin), admin sign-in unchanged, migration seed data is present after `Database.MigrateAsync`, in `dotnet/Petstore.Tests/SupplierRoleApiContractTests.cs` and `dotnet/Petstore.Tests/MigrationSeedDataTests.cs`
- [X] T005 [US1] Verify with `dotnet test dotnet/Petstore/Petstore.slnx`

## Phase 3: User Story 2 - Restrict Order Administration To Admins (P1)

- [X] T006 [US2] Add boundary-matrix contract tests: supplier → 403 on admin order list/detail/approve/deny/transitions; admin → still allowed; customer 403 / anonymous 401 unchanged, in `dotnet/Petstore.Tests/SupplierRoleApiContractTests.cs`
- [X] T007 [US2] Verify with `dotnet test dotnet/Petstore/Petstore.slnx`

## Phase 4: User Story 3 - Supplier Inventory And Fulfillment Operations (P1)

- [X] T008 [US3] Create `dotnet/Petstore/Controllers/SupplierInventoryController.cs` at `api/supplier` with the `SupplierOperations` policy; delete `dotnet/Petstore/Controllers/AdminInventoryController.cs`
- [X] T009 [US3] Retarget and extend inventory contract tests: supplier and admin succeed on `/api/supplier/*`, customer 403, anonymous 401, old `/api/admin/inventory` 404, negative quantity validation, in `dotnet/Petstore.Tests/SupplierInventoryApiContractTests.cs` (renamed from `AdminInventoryApiContractTests.cs`)
- [X] T010 [US3] Retarget the replenishment flow to `/api/supplier/inventory` driven by the supplier identity in `dotnet/Petstore.Tests/FulfillmentIntegrationTests.cs`
- [X] T011 [US3] Verify with `dotnet test dotnet/Petstore/Petstore.slnx`

## Phase 5: Polish

- [X] T012 Run the full suite (customer-facing regression per FR-008) and a manual curl smoke: supplier sign-in → inventory list/set → fulfillment run → forbidden on admin orders

## Dependencies

- Phase 1 blocks all stories. US1 → US2 → US3. Feature 014 consumes the new routes.
