# Implementation Plan: Account UI

**Branch**: `005-account-ui-sdd` | **Date**: 2026-06-10 | **Spec**: `specs/005-account-ui/spec.md`

## Summary

Extend the Angular app with identity: sign-in and registration views, an
identity service holding the JWT, an HTTP interceptor adding the
Authorization header, a route guard for account-only routes, and an account
view for reading/updating contact details. Catalog stays anonymous.

## Technical Context

**Language/Version**: TypeScript, Angular 21 (zoneless, signals) in `frontend/petstore-ui`.

**Primary Dependencies**: Angular Router, HttpClient + functional interceptor, accounts API from feature 004 through the existing `/api` dev proxy.

**Testing**: Manual validation (deferral re-confirmed, consistent with 003); quickstart checklist in this folder.

**Constraints**: JWT stored in `localStorage` (key `petstore.token`); passwords never stored; token expiry handled by treating 401 as signed-out.

## Constitution Check

All principles pass; no backend or legacy runtime changes. Automated Backend Verification not applicable (frontend-only; backend tests live in 004).

## Design Decisions

- **DD-001**: `IdentityService` (signal-based): holds `{ userId, role, token }` or anonymous; restores from `localStorage` on bootstrap; exposes `signIn`, `signOut`, `isSignedIn`, `isAdmin`.
- **DD-002**: Functional HTTP interceptor adds `Authorization: Bearer <token>` to `/api/*` requests when signed in; on 401 response clears identity and redirects to sign-in.
- **DD-003**: Routes: `/signin`, `/register`, `/account` (guarded). Guard redirects anonymous users to `/signin` with `returnUrl` query param.
- **DD-004**: Registration success signs the user in immediately (calls signin after register).
- **DD-005**: Shell header shows identity state (user id + sign-out button, or sign-in link) on all routes.
- **DD-006**: Reuse shared loading/unavailable components and `ApiErrorDto` handling patterns from 003.

## Project Structure

```text
frontend/petstore-ui/src/app/
|-- identity/
|   |-- identity.service.ts
|   |-- auth.interceptor.ts
|   |-- auth.guard.ts
|   |-- identity.models.ts
|   |-- sign-in.component.ts
|   `-- register.component.ts
|-- account/
|   `-- account.component.ts
|-- app.config.ts        (interceptor registration)
|-- app.routes.ts        (new routes + guard)
`-- catalog/catalog-shell.component.ts (identity header area)
```

## Manual Validation

Quickstart: sign in as `j2ee`, register a new user, edit contact, reload page (identity restored), sign out, guard redirect check, catalog regression while signed out.
