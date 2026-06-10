# Implementation Plan: Customer Accounts And Sign-In API

**Branch**: `004-customer-accounts-sdd` | **Date**: 2026-06-10 | **Spec**: `specs/004-customer-accounts/spec.md`

## Summary

Add user accounts to the existing ASP.NET backend: a users table with per-user
salt + PBKDF2 password hash and a role column, JWT bearer authentication,
registration, sign-in, and authenticated read/update of contact details.
Catalog endpoints stay anonymous. Seed legacy parity users.

## Technical Context

**Language/Version**: C# / .NET 10, existing `dotnet/Petstore` ASP.NET Core project.

**Primary Dependencies**: `Microsoft.AspNetCore.Authentication.JwtBearer`; PBKDF2 via `System.Security.Cryptography.Rfc2898DeriveBytes` (no third-party hashing packages).

**Storage**: Same SQL Server database as catalog; new tables via EF Core migration in the existing `PetstoreCatalogContext` migrations chain.

**Testing**: xUnit in `dotnet/Petstore.Tests`; unit tests for hashing/validation, integration/contract tests for the API (Constitution VI). Database tests use the existing `DatabaseIntegration` trait convention.

**Constraints**: No e-mail confirmation, password recovery, or lockout. Sign-out is client-side token discard (documented FR-004 simplification, token lifetime 8 hours).

## Constitution Check

- **Runnable Legacy Baseline**: Pass — Docker/Payara untouched.
- **Business Flow Parity**: Pass — account creation/sign-in migrated; plaintext password storage intentionally replaced by salted hashes (documented change).
- **Branch-First**: Pass — feature branch.
- **Evidence Before Replacement**: Pass — legacy evidence in spec.
- **Incremental**: Pass — independent slice; catalog unaffected.
- **Automated Backend Verification**: Pass — test strategy below.

## Design Decisions

- **DD-001 (DP-001)**: JWT bearer tokens. `POST /api/auth/signin` returns `{ token, userId, role, expiresAt }`. Symmetric signing key from configuration (`Jwt:Key`, dev value in `appsettings.Development.json`). Token carries `sub` (user id) and `role` claims. No refresh tokens in this slice.
- **DD-002**: Users table: `UserId` (PK, string, legacy ids preserved), `PasswordHash`, `PasswordSalt` (random 16 bytes per user), `Role` (`customer`/`admin`), `CreatedAt`. PBKDF2-SHA256, 100_000 iterations.
- **DD-003 (DP-002)**: No credit card data stored anywhere in this slice.
- **DD-004 (DP-003)**: Profile preferences deferred.
- **DD-005**: Contact info lives in a `CustomerContacts` table keyed by `UserId` (family name, given name, street1, street2 nullable, city, state, zip, country, email, phone).
- **DD-006**: Seed users `j2ee`, `j2ee-ja`, `shopper` (password `j2ee`, role `customer`) and `admin` (password `admin`, role `admin`) via idempotent startup/migration seeding with precomputed salt+hash values.
- **DD-007**: API surface:
  - `POST /api/account` — register (user id, password, contact info) → 201 or 409 conflict.
  - `POST /api/auth/signin` — credentials → token or 401 (identical for unknown user / wrong password).
  - `GET /api/account` — authenticated; own contact details.
  - `PUT /api/account/contact` — authenticated; update contact details.
  - Errors use the existing `ApiErrorDto` contract; validation errors name fields.
- **DD-008**: Authorization via standard `[Authorize]`; catalog controllers remain anonymous.

## Project Structure

```text
dotnet/Petstore/
|-- Accounts/
|   |-- IPasswordHasher.cs / Pbkdf2PasswordHasher.cs
|   |-- IAccountRepository.cs / AccountRepository.cs
|   `-- AccountSeeder.cs
|-- Controllers/
|   |-- AccountController.cs
|   `-- AuthController.cs
|-- Data/Entities/UserEntity.cs, CustomerContactEntity.cs
|-- Data/Configurations/UserEntityConfiguration.cs, CustomerContactEntityConfiguration.cs
|-- Models/RegisterRequestDto.cs, SignInRequestDto.cs, SignInResponseDto.cs, ContactInfoDto.cs, AccountDto.cs
`-- Data/Migrations/<new migration>

dotnet/Petstore.Tests/
|-- Pbkdf2PasswordHasherTests.cs          (unit)
|-- AccountValidationTests.cs             (unit)
|-- AccountApiContractTests.cs            (contract/integration)
`-- AuthApiContractTests.cs               (contract/integration)
```

## Test Strategy (Constitution VI)

- Unit: PBKDF2 hasher (round-trip verify, distinct salts, wrong password fails), request validation rules.
- Contract/integration: register happy path, duplicate 409, validation 400 with field names; sign-in success, wrong password vs unknown user indistinguishable 401; authenticated GET/PUT account; 401 for anonymous; catalog endpoints anonymous regression check; no password/hash in any response.
- Database integration (`DatabaseIntegration` trait): seeding idempotency, parity users present.
