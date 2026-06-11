# Claude Code SDD Review Hook

This project-level Claude Code setup adds a non-blocking SDD compliance review
that can run when a Claude Code task finishes.

## How To Trigger A Review

Create `.claude/run-sdd-review` before Claude stops.

The file may be empty. If it contains a feature directory on the first line, the
reviewer uses that directory, for example:

```text
specs/012-admin-ui
```

If the file is empty, the hook falls back to `.specify/feature.json`.

## What Happens

When Claude Code reaches the `Stop` lifecycle event:

1. `.claude/hooks/review-sdd-after-stop.ps1` checks for `.claude/run-sdd-review`.
2. If the flag is absent, nothing happens.
3. If the flag exists, the hook prepares git diff context against `origin/master`.
4. The hook asks the `sdd-compliance-reviewer` agent to compare the diff with the
   active SDD artifacts.
5. The report is written to `.claude/reviews/sdd-review-last.md`.
6. The flag is removed to avoid repeated reviews.

The hook is intentionally non-blocking. If the Claude CLI is missing or the
review fails, it writes a diagnostic report and exits successfully so the main
Claude session is not trapped in a Stop-hook loop.

## Suggested Prompt

```text
Work on the current SDD feature. When implementation and checks are complete,
write specs/012-admin-ui into .claude/run-sdd-review so the Stop hook runs the
SDD compliance reviewer.
```
