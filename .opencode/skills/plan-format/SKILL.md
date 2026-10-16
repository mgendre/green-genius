---
name: plan-format
description: "The shape of a plan file in `.plans/` — its name and number, the example, what each line holds, how large a task is. Read before writing a plan or adding tasks to one. The only place where this shape lives."
---

# The shape of a plan

One file per request: `.plans/<NN>_<name>.md`, a two-digit number then snake_case, as `.plans/01_add_encyclopedia.md`. The number is the highest one in `.plans/`, `done_` files included, plus one: list them with `ls .plans`. A new request is a new file. Keep it short: a developer reads all of it at every task. No section and no line other than these.

```
# Garden notes

Request: let the user add a free-text note to a garden and see it in the list.
Status: in progress

Existing:
- `GreenGenius.Api/Features/Gardens/GardensApi.cs`: `PUT /gardens/{id}` updates the name only; `GardenDto` has no note.
- `GreenGenius.Frontend/src/app/shared/stores/gardens.store.ts`: the store has `updateGarden`.

## 1. [ ] The API stores and returns the note of a garden — backend

Skills: api-endpoint, ef-entity, ef-migrations, backend-tests
Where:
- `GreenGenius.Api/Features/Gardens/Handlers/UpdateGardenHandler.cs`, the `Handle` method.
- `GreenGenius.Common.Data/Entities/Garden.cs`, after `Name`.
Criteria:
- `PUT /gardens/{id}` accepts a `note` of 4000 characters at most and returns it.
- A garden of another user still answers `404`.
Out of scope: the frontend.
Check: `dotnet test GreenGenius.slnx --filter GardensApiIntegrationTest 2>&1 | tail -20`
Result:

## 2. [ ] The user edits the note of a garden and sees it in the list — frontend

Skills: signal-form, signal-store, i18n-fr
Where:
- `GreenGenius.Frontend/src/app/shared/stores/gardens.store.ts`, the `updateGarden` method.
- A new component, next to `planner-gardens-list.component.ts`.
Criteria:
- The edit window of a garden has a field labelled "Note", prefilled with the current note.
- A note longer than 4000 characters shows "La note ne peut pas dépasser 4000 caractères." and cannot be saved.
Out of scope: the backend, the layout of the list.
Check: `npm run check --prefix GreenGenius.Frontend`
Result:

## Notes
```

| Line | What it holds |
|---|---|
| `Request:` | What the user asked. A plan that follows another has `Follows: .plans/done_<NN>_<name>.md` under it. |
| `Status:` | `in progress` when written. |
| `Existing:` | Two to five facts you read, each starting with the full path of its file. A cause the user gave is copied here as is. |
| Title | The outcome: what is true for the user or the API once done, then `— backend` or `— frontend`. The box is `[ ]`. |
| `Skills:` | From the table of `.opencode/skills/task-skills/SKILL.md`, read with the read tool once per plan. Never a name that is not in it, never from memory. |
| `Where:` | One to three lines: an existing file by its full path with a landmark you read in it, or the folder of a new file. Never a guess, never the name of something to create. |
| `Criteria:` | Three at most, one behaviour each: a criterion that needs two tests is two criteria. What a person sees, does or receives: a behaviour, a French text as shown, a limit, a status code, a response field. Never the name of a component, class, selector, signal, i18n key, import or method. |
| `Out of scope:` | What sits next to the task and stays untouched. |
| `Check:` | Backend `dotnet test GreenGenius.slnx --filter <TestClass> 2>&1 | tail -20`, frontend `npm run check --prefix GreenGenius.Frontend`. |
| `Result:` | Empty: the dispatcher fills it. |
| `## Notes` | What no task covers, a question for the user, one line `Decided: …` per decision a developer cannot read in a criterion. |

A task ends with something a person can check by using the app or calling the API; building a piece and plugging it in is one task. More than three criteria, or two layers: several tasks, backend before frontend.

A task never asks a developer to change a file their tool refuses: `angular.json`, `package.json`, `package-lock.json`, `shared/spartan/helm/`, `api-client.generated.ts`, a migration by hand, `compose.yaml`, a `Dockerfile`, `start.sh`, `.github/`, `dotnet-tools.json`. Such a change goes under `## Notes` as a question for the user.
