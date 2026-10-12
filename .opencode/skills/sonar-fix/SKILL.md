---
name: sonar-fix
description: "How the dispatcher fixes the open SonarCloud issues of GreenGenius by priority — list them with a script, turn them into a plan, hand them to the developers. Read when the user asks to fix, clear or look at the Sonar issues. Not for a red build or a failing test."
---

# Fix the Sonar issues by priority

For the dispatcher. The issues come from the last analysis of `main` on SonarCloud (project `mgendre_green-genius`, public: no token). Sonar runs in CI only: nothing here can run it again.

## 1. List

```
bash .opencode/skills/sonar-fix/issues.sh
```

For a pull request, its number: `bash .opencode/skills/sonar-fix/issues.sh 42`.

One line per issue, highest priority first:

```
FIX  MAJOR CODE_SMELL javascript:S3358  GreenGenius.Frontend/scripts/check.mjs:132  Extract this nested ternary operation into an independent statement.
SKIP MINOR CODE_SMELL typescript:S7744  GreenGenius.Frontend/src/app/shared/spartan/helm/dialog/src/lib/hlm-dialog.service.ts:30  The empty object is useless.
5 open
```

The order of the lines is the order of the work: never sort them again. `0 open`: answer that nothing is open and stop.

## 2. Sort out

| The line | What to do |
|---|---|
| `SKIP` | Generated or vendored code (Helm components, migrations, generated client). No task. Copy the line under `## Notes`. |
| `FIX`, and the file at that line still shows what the message says | A task. |
| `FIX`, and the line no longer shows it | Already fixed since the analysis. No task. Copy the line under `## Notes` with `already fixed`. |
| `HOTSPOT` | A task like the others, first in the plan. |

Read each `FIX` file once, at the line given, to tell the last two cases apart.

## 3. Plan

A plan as for any request, with these rules:

- One task per file, in the order of the first line of that file in the list. Three issues at most per task; a file with more is cut in two tasks.
- Layer: a path under `GreenGenius.Frontend/` is frontend, anything else is backend.
- Title: `No Sonar issue left in <file name> — frontend`.
- `Where:` the full path and the line of each issue.
- `Criteria:` one per issue, the message turned into what is true once fixed, with the rule key: `No nested ternary at line 132 (javascript:S3358).`
- `Out of scope:` any other change in the file, any behaviour change.
- `Check:` the check of the layer. Backend: the test class that covers the file, or `dotnet build GreenGenius.slnx -v q --nologo 2>&1 | tail -20` when no test covers it.
- `Skills:` only what the fix needs; a backend task names `backend-tests`.

Then hand over and verify as usual. A criterion is met when the diff shows the construct gone and the check passes.

## Never

- Silence an issue: `// NOSONAR`, `#pragma warning disable`, `[SuppressMessage]`, `eslint-disable`, a rule turned off, a file excluded.
- Mark an issue as false positive or accepted: tell the user under `## Notes`, they decide on SonarCloud.
- Touch a `SKIP` file.
- Write that the Sonar issues are closed: write that the fixes are in the code and that the next CI analysis will tell.

## Report

The usual report, plus: the number of issues listed, fixed, skipped and already fixed.
