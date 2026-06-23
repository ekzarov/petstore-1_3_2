# Legacy User Flows Workbook Instructions

Use `analysis/legacy_user_flows_template.xlsx` to reverse-engineer legacy
behavior, plan the target migration through SDD, and later verify that the
new implementation preserves the intended behavior.

This workbook is not an implementation task list. It is an evidence and parity
matrix. Agents must not start coding from this workbook alone. Implementation
starts only after the user approves the relevant SDD/spec/plan/tasks.

## Required Workflow

1. **Collect legacy evidence from code first**
   - Inspect only the legacy source, deployable descriptors, configuration,
     database scripts, UI pages, controllers/servlets/actions, services, EJBs,
     jobs, messaging handlers, reports, and tests.
   - Record concrete file/class/page evidence in the workbook.
   - Prefer behavior that is visible to a user, operator, or external system.
   - Do not invent business requirements from names alone. If behavior is
     inferred, mark it as inferred in the notes/evidence.

2. **Write or update SDD before implementation**
   - Convert the discovered user-visible behavior into feature specs,
     clarifications, plans, and tasks.
   - Mark whether each workbook row is covered, deferred, or missed by SDD.
   - Do not implement code until the user explicitly approves the next
     implementation step.

3. **Implement only after user approval**
   - Work from approved SDD artifacts, not directly from the workbook.
   - Keep changes scoped to the approved feature/user story.
   - Add tests required by the project constitution and the feature plan.

4. **Verify target parity after implementation**
   - Revisit the workbook after the new code exists.
   - Fill destination columns with implemented status, notes, and code evidence.
   - Use the workbook to identify gaps between legacy behavior, SDD, and the
     target implementation.

## Workbook Structure

The workbook has one sheet: `User Flows`.

Rows 1-4 are title, notes, and color legend. Rows 5-6 are section headers and
column headers. Data entry starts at row 7.

Keep one row per atomic functional step. A broad use case can span multiple
rows, but each row should describe one checkable behavior.

## Columns

### Source / Legacy Code

`Use Case ID`
: Stable identifier for the use case, for example `UF-001`. Reuse the same ID
for multiple rows that belong to the same use case.

`Use Case`
: Short business name, for example `User login`, `Checkout`, `Approve order`,
or `Supplier inventory update`.

`Scenario`
: Row classification inside the use case. Examples: `Happy path`,
`Validation`, `Authorization`, `Error handling`, `Edge case`,
`Not Passed - Missed`, `Deferred`, `Operator flow`.

`Functional requirement / step`
: A precise, testable behavior. Write it as a requirement, not as code.
Example: `Returning customer can sign in with valid credentials.`

`Business-readable description`
: Plain-language explanation of why the behavior exists and what it means for
the business/user flow.

`Expected user-visible result`
: What a user, admin, supplier, operator, or external caller can observe.
Examples: page shown, status changed, validation message displayed, order
queued, inventory updated, report refreshed.

`Source implemented?`
: Whether the legacy code implements this behavior. Use `Yes`, `No`,
`Partial`, or `Inferred`.

`Source code evidence`
: Concrete references to legacy evidence. Include file names, classes, pages,
routes, descriptors, database tables, queues/topics, or config keys. Use enough
detail that another agent can find the evidence without chat history.

### Destination / Target Implementation

`Destination implemented?`
: Whether the migrated/target system implements this behavior. Use `Yes`,
`No`, `Partial`, or `N/A`.

`Destination notes`
: Short explanation of the implementation state, intentional behavior changes,
known gaps, or test observations.

`Destination code evidence`
: Concrete target references: controller/component/service/test file names,
routes, migrations, database tables, UI paths, or test names.

### SDD Coverage

`Covered in SDD?`
: Whether the behavior is covered by an SDD artifact. Use `Yes`, `No`,
`Partial`, or `N/A`.

`Deferred in SDD?`
: Whether SDD explicitly defers the behavior. Use `Yes` only when a spec/plan
clearly marks it as out of scope, future work, or an intentional decision.

`SDD evidence`
: Reference the spec/plan/tasks file and, when useful, the requirement,
decision, user story, or task ID.

## Color Legend

Use row coloring to make review fast.

`Green`
: Fully covered. Legacy behavior exists, SDD covers it, target implementation
exists, and no unresolved parity gap remains.

`Red`
: Missed. Legacy behavior exists, but the target does not implement it and SDD
does not cover or defer it. These rows usually require a new SDD feature or a
user decision.

`Orange`
: Known gap, partial behavior, or planned/deferred behavior. Use this when the
gap is intentional, already captured in SDD, blocked by a decision, or only
partially implemented.

`White`
: Neutral/unclassified. Use for newly discovered rows before SDD/target review
is complete.

## Quality Rules

- Do not claim `Yes` without concrete evidence.
- Do not treat documentation as source truth when code contradicts it.
- Prefer small rows over giant bundled requirements.
- Preserve legacy vocabulary where it matters, but explain it in modern
business language.
- If the legacy behavior is unclear, write `Inferred` or `Partial` and explain
the uncertainty.
- If SDD intentionally changes behavior, mark the destination as implemented
only when the target implements the approved SDD behavior, and document the
legacy difference in notes.
- If a row is red, do not fix it directly in code. First create or update SDD
and ask the user for approval.

## Review Checklist

Before handing the workbook back:

- Every legacy behavior row has source evidence.
- Every destination `Yes` or `Partial` has target evidence.
- Every `Deferred in SDD? = Yes` has SDD evidence.
- Red rows are actionable and not vague.
- Orange rows explain the reason for partial/deferred status.
- The workbook can be understood without reading the chat history.
