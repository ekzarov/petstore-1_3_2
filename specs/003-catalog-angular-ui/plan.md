# Implementation Plan: Browse Catalog UI

**Branch**: `003-catalog-angular-ui-sdd` | **Date**: 2026-06-08 | **Spec**: `specs/003-catalog-angular-ui/spec.md`

**Input**: Feature specification from `/specs/003-catalog-angular-ui/spec.md`

**Note**: This template is filled in by the `/speckit-plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

Create a separate Angular catalog browsing UI under `frontend/petstore-ui` that consumes the existing migrated ASP.NET catalog API from `specs/002-browse-product-catalog`. The first slice is read-only and supports categories, products, product items, item details, loading/empty/error/not-found states, and browser navigation. During local development, the Angular dev server will use a proxy for `/api/*` requests so the frontend and backend can remain separate processes without adding development-only CORS policy to the backend.

## Technical Context

**Language/Version**: TypeScript with Angular CLI-selected Angular version at implementation time; .NET backend remains the existing ASP.NET catalog API.

**Primary Dependencies**: Angular, Angular Router, Angular HttpClient, existing ASP.NET catalog API.

**Storage**: N/A for frontend; catalog data remains owned by SQL Server through the existing backend API.

**Testing**: Manual browser validation for this first UI slice; no automated UI/unit/e2e tests in this feature.

**Target Platform**: Local desktop browser during development.

**Project Type**: Separate frontend web app plus existing backend web API.

**Performance Goals**: Primary catalog browsing flow should complete in under two minutes manually on a local developer machine; route changes should keep perceived waiting states explicit.

**Constraints**: Read-only catalog UI; no cart, checkout, login, admin editing, search, localization, or production deployment integration in this feature.

**Scale/Scope**: One Angular app under `frontend/petstore-ui`, four catalog browsing user stories, and shared local dev proxy configuration.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**Runnable Legacy Baseline**: Pass. This feature does not alter Docker Payara runtime or legacy EAR deployment.

**Business Flow Parity**: Pass. Scope is read-only catalog browsing and preserves stable legacy catalog ids through API consumption; order flow is not changed.

**Branch-First, Master-Stable Workflow**: Pass. Work is on feature branch `003-catalog-angular-ui-sdd`.

**Evidence Before Replacement**: Pass. UI consumes the documented API contract from `specs/002-browse-product-catalog/contracts/openapi.yaml` rather than inventing hardcoded catalog data.

**Incremental Migration Over Big Bang**: Pass. Separate UI slice can be validated independently and does not replace the full legacy Java frontend.

## Project Structure

### Documentation (this feature)

```text
specs/003-catalog-angular-ui/
|-- plan.md
|-- research.md
|-- data-model.md
|-- quickstart.md
|-- tasks.md
`-- checklists/
    `-- requirements.md
```

### Source Code (repository root)

```text
dotnet/
|-- Petstore/
|   `-- existing ASP.NET catalog API
`-- Petstore.Tests/
    `-- existing backend tests

frontend/
`-- petstore-ui/
    |-- angular.json
    |-- package.json
    |-- proxy.conf.json
    `-- src/
        |-- app/
        |   |-- app.config.ts
        |   |-- app.routes.ts
        |   |-- catalog/
        |   |   |-- catalog-api.service.ts
        |   |   |-- catalog.models.ts
        |   |   |-- catalog-shell.component.ts
        |   |   |-- category-list.component.ts
        |   |   |-- product-list.component.ts
        |   |   |-- item-list.component.ts
        |   |   `-- item-detail.component.ts
        |   `-- shared/
        |       |-- empty-state.component.ts
        |       |-- loading-state.component.ts
        |       `-- unavailable-state.component.ts
        `-- styles.css
```

**Structure Decision**: Add a separate Angular application in `frontend/petstore-ui`. The app consumes relative `/api/catalog/...` URLs and relies on `frontend/petstore-ui/proxy.conf.json` during local development. Backend source remains in `dotnet/Petstore` and should not be modified for CORS as part of this UI slice.

## Design Decisions

- Use Angular Router for deep links to category, product item list, and item detail views so refresh and back/forward navigation are part of the first slice.
- Use Angular HttpClient through a single catalog API service; components must not hardcode backend hosts, ports, or raw endpoint strings outside the service.
- Use a minimal modern catalog layout: clear navigation, readable lists, repeated product/item cards where useful, explicit loading/empty/error states, and responsive behavior for desktop and mobile widths.
- Do not copy the legacy Java PetStore UI. Legacy behavior parity comes from data and navigation outcomes, not visual styling.
- Defer automated frontend tests, cart, checkout, login, search, localization, admin, and production hosting decisions.

## Complexity Tracking

> Fill ONLY if Constitution Check has violations that must be justified.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | N/A | N/A |
