# PetStore Migration Constitution

## Core Principles

### I. Runnable Legacy Baseline
The legacy Java PetStore must remain runnable through the Docker-based Payara setup throughout the migration. Any change that prevents `docker compose up --build -d` from deploying the current EAR stack is non-compliant unless the change explicitly replaces that baseline with an equivalent verified runtime.

### II. Business Flow Parity
The current working order lifecycle is the reference behavior: account creation, catalog browsing, cart, checkout, OPC processing, Supplier fulfillment, and invoice processing. Migration work must preserve or intentionally document changes to this flow before implementation proceeds.

### III. Branch-First, Master-Stable Workflow
All migration, research, and infrastructure changes must happen on feature branches. `master` must stay usable as the integration baseline. Pull requests or merges must keep unrelated refactors, generated churn, and exploratory edits separate from the scoped change.

### IV. Evidence Before Replacement
Legacy components must be understood and documented before they are replaced. Analysis of EAR modules, EJBs, JMS destinations, JDBC datasources, XML schemas, and database tables must be captured in specs, plans, or docs before a .NET equivalent is implemented.

### V. Incremental Migration Over Big Bang
The target .NET system must be introduced through small, verifiable slices. Each slice must have a rollback or isolation story and must be testable independently. Whole-system rewrites without parity checkpoints are not allowed.

## Technical Constraints

- The existing Docker runtime is the source of truth for local reproducibility until a replacement runtime is explicitly approved.
- Current local infrastructure uses Payara Server Full 5.2021.1 and H2 datasources for the legacy Java baseline.
- Existing deployable artifacts are EAR/WAR/EJB modules with JMS, JDBC, JNDI, XML, DTD/XSD, XSLT, and JavaMail dependencies.
- Target .NET architecture decisions must be made in specs/plans before implementation, including web UI/API shape, database choice, messaging replacement, authentication, and deployment model.
- Any source-level fixes that replace Docker-time EAR patching must preserve the proven behavior of the current Docker smoke flow.

## Development Workflow

- Start substantial work with a Spec Kit specification before writing application code.
- Use `/speckit-clarify` when scope, target platform, or migration behavior has multiple reasonable interpretations.
- Use `/speckit-plan` and `/speckit-tasks` before implementation work that changes runtime behavior.
- Verify changes with the narrowest meaningful checks first, then run the Docker smoke flow when Java runtime behavior is affected.
- Record important discoveries in project docs or specs so future migration work does not depend only on chat history.

## Quality Gates

- Legacy runtime changes must verify application deployment and seed data loading.
- Order-flow-impacting changes must verify account creation, catalog access, add-to-cart, checkout, and absence of JMS redelivery/DMQ errors in relevant logs.
- Migration plans must identify affected bounded contexts, data ownership, integration points, and acceptance tests.
- Generated or tool-added files must be reviewed before commit and kept separate from business changes when practical.

## Governance

This constitution guides all PetStore migration specs and plans. If a spec conflicts with this constitution, the spec must either be revised or explicitly propose an amendment. Amendments require a rationale, expected impact, and an update to this file.

**Version**: 1.0.0 | **Ratified**: 2026-06-08 | **Last Amended**: 2026-06-08
