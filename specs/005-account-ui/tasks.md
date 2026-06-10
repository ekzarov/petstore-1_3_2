# Tasks: Account UI

**Input**: `specs/005-account-ui/plan.md`, `specs/005-account-ui/spec.md`

**Tests**: Manual validation per plan (automated UI tests deferred, as in 003).

## Phase 1: Foundational (identity infrastructure)

- [ ] T001 Create identity models (`IdentityState`, sign-in/register DTOs) in `frontend/petstore-ui/src/app/identity/identity.models.ts`
- [ ] T002 Create signal-based `IdentityService` with localStorage persistence (`petstore.token`) and bootstrap restore in `frontend/petstore-ui/src/app/identity/identity.service.ts`
- [ ] T003 Create functional auth interceptor (adds Bearer header, clears identity on 401) in `frontend/petstore-ui/src/app/identity/auth.interceptor.ts` and register it in `frontend/petstore-ui/src/app/app.config.ts`
- [ ] T004 Create `authGuard` with `returnUrl` redirect in `frontend/petstore-ui/src/app/identity/auth.guard.ts`

## Phase 2: User Story 1 - Sign In And Sign Out (P1)

- [ ] T005 [US1] Create sign-in view (user id, password, generic failure message, preserved user id) in `frontend/petstore-ui/src/app/identity/sign-in.component.ts`
- [ ] T006 [US1] Add identity area to the shell header (user id + sign-out, or sign-in link) in `frontend/petstore-ui/src/app/catalog/catalog-shell.component.ts`
- [ ] T007 [US1] Wire `/signin` route in `frontend/petstore-ui/src/app/app.routes.ts`
- [ ] T008 [P] [US1] Add identity/form styles in `frontend/petstore-ui/src/styles.css`
- [ ] T009 [US1] Manually validate sign-in/sign-out with seeded `j2ee` including page-reload identity restore

## Phase 3: User Story 2 - Create An Account (P1)

- [ ] T010 [US2] Create registration view (user id, password, contact fields, duplicate-id and validation feedback, auto sign-in on success) in `frontend/petstore-ui/src/app/identity/register.component.ts`
- [ ] T011 [US2] Wire `/register` route and link from sign-in view in `frontend/petstore-ui/src/app/app.routes.ts`
- [ ] T012 [US2] Manually validate registration happy path, duplicate id, and missing-fields cases

## Phase 4: User Story 3 - View And Edit Account Details (P2)

- [ ] T013 [US3] Create account view (read + edit contact details, save confirmation) in `frontend/petstore-ui/src/app/account/account.component.ts`
- [ ] T014 [US3] Wire guarded `/account` route in `frontend/petstore-ui/src/app/app.routes.ts`
- [ ] T015 [US3] Manually validate read/edit/persist after reload and the anonymous redirect to sign-in

## Phase 5: Polish

- [ ] T016 [P] Document identity flows and storage key in `frontend/petstore-ui/README.md`
- [ ] T017 Run `npx ng build` and full manual quickstart including catalog-stays-anonymous regression

## Dependencies

- Requires feature 004 deployed locally. Phase 1 blocks all stories; US1 → US2 → US3.
