# Tasks: Customer Accounts And Sign-In API

**Input**: `specs/004-customer-accounts/plan.md`, `specs/004-customer-accounts/spec.md`

**Tests**: Required per Constitution VI (unit + contract/integration as defined in the plan).

## Phase 1: Setup

- [X] T001 Add `Microsoft.AspNetCore.Authentication.JwtBearer` to `dotnet/Petstore/Petstore.csproj` and JWT settings (`Jwt:Key`, `Jwt:Issuer`, `Jwt:ExpiryHours`) to `dotnet/Petstore/appsettings.Development.json`
- [X] T002 Register JWT bearer authentication and authorization middleware in `dotnet/Petstore/Program.cs` (catalog endpoints stay anonymous)

## Phase 2: Foundational (data + hashing)

- [X] T003 [P] Create `dotnet/Petstore/Data/Entities/UserEntity.cs` (UserId PK, PasswordHash, PasswordSalt, Role, CreatedAt) and `dotnet/Petstore/Data/Entities/CustomerContactEntity.cs`
- [X] T004 [P] Create entity configurations in `dotnet/Petstore/Data/Configurations/UserEntityConfiguration.cs` and `CustomerContactEntityConfiguration.cs`; add DbSets to `dotnet/Petstore/Data/PetstoreCatalogContext.cs`
- [X] T005 Create `dotnet/Petstore/Accounts/IPasswordHasher.cs` and `Pbkdf2PasswordHasher.cs` (PBKDF2-SHA256, 100k iterations, 16-byte random salt)
- [X] T006 [P] Add unit tests in `dotnet/Petstore.Tests/Pbkdf2PasswordHasherTests.cs` (verify round-trip, distinct salts, wrong password fails)
- [X] T007 Create `dotnet/Petstore/Accounts/AccountSeeder.cs` seeding `j2ee`, `j2ee-ja`, `shopper` (password `j2ee`, role customer) and `admin` (password `admin`, role admin), idempotent
- [X] T008 Add EF Core migration for users and contacts tables under `dotnet/Petstore/Data/Migrations/` and verify `dotnet ef database update`
- [X] T009 Create `dotnet/Petstore/Accounts/IAccountRepository.cs` and `AccountRepository.cs` (create account, find user, get/update contact)

## Phase 3: User Story 1 - Create An Account (P1)

- [X] T010 [P] [US1] Create DTOs `RegisterRequestDto`, `ContactInfoDto`, `AccountDto` in `dotnet/Petstore/Models/`
- [X] T011 [US1] Add contract tests for `POST /api/account` (201, duplicate 409, validation 400 with field names, no password in response) in `dotnet/Petstore.Tests/AccountApiContractTests.cs`
- [X] T012 [US1] Implement `POST /api/account` in `dotnet/Petstore/Controllers/AccountController.cs`
- [X] T013 [US1] Verify with `dotnet test dotnet/Petstore/Petstore.slnx`

## Phase 4: User Story 2 - Sign In And Sign Out (P1)

- [X] T014 [P] [US2] Create DTOs `SignInRequestDto`, `SignInResponseDto` in `dotnet/Petstore/Models/`
- [X] T015 [US2] Add contract tests for `POST /api/auth/signin` (token issued for seeded `j2ee`; wrong password and unknown user produce identical 401; token grants access to an authenticated endpoint) in `dotnet/Petstore.Tests/AuthApiContractTests.cs`
- [X] T016 [US2] Implement `POST /api/auth/signin` with JWT issuance in `dotnet/Petstore/Controllers/AuthController.cs`
- [X] T017 [US2] Verify with `dotnet test dotnet/Petstore/Petstore.slnx`

## Phase 5: User Story 3 - View And Update Account Details (P2)

- [X] T018 [US3] Add contract tests for `GET /api/account` and `PUT /api/account/contact` (authenticated read/update, 401 anonymous, validation 400) in `dotnet/Petstore.Tests/AccountApiContractTests.cs`
- [X] T019 [US3] Implement `GET /api/account` and `PUT /api/account/contact` in `dotnet/Petstore/Controllers/AccountController.cs`
- [X] T020 [US3] Verify with `dotnet test dotnet/Petstore/Petstore.slnx`

## Phase 6: Polish

- [X] T021 [P] Add catalog-stays-anonymous regression test in `dotnet/Petstore.Tests/AuthApiContractTests.cs`
- [X] T022 [P] Add database integration test (`DatabaseIntegration` trait) for seeder idempotency and parity users in `dotnet/Petstore.Tests/AccountSeederTests.cs`
- [X] T023 Run full test suite and manual smoke: register → sign in → read account → update contact via HTTP file or curl

## Dependencies

- Phase 2 blocks all user stories; US1 → US2 → US3 is the natural order; T006 parallel with T007-T009.
