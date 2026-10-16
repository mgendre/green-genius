---
name: task-skills
description: "Which skills a task of a plan names on its `Skills:` line — one row per kind of work, backend and frontend. Read before writing or correcting the tasks of a plan in `.plans/`. The only place where this list lives."
---

# Which skills a task names

Go through the rows for each task and name every skill whose row applies, in this order. Each skill named is a file the developer reads before working: name what the task needs, never a skill "to be safe". A skill that is missing is the first cause of a failed task.

| The task involves | Skill |
|---|---|
| **Backend** | |
| any change of the .NET code | `backend-tests`, in every backend task |
| a route, a handler, a DTO, a mapping | `api-endpoint` |
| an entity, a column, an index, an entity configuration | `ef-entity` and `ef-migrations`, together |
| a migration alone: a rename, a wrong one to remove, a merge | `ef-migrations` |
| a query, a loop over rows, an import, a bulk change | `ef-queries` |
| a log line, a `catch` that reports an error, an import or a service that tells what it did | `dotnet-logging` |
| a `.csproj`, a package, a project reference, a data file copied to the output | `csproj-files` |
| an architecture test, an ADR rule enforced on the compiled code | `architecture-tests` |
| **Frontend** | |
| something a person sees: a page, a card, a list, a form | `ui-quality`; the criteria then say what shows when the data is loading, empty or failed |
| a form, an input field, a validation rule, a submit | `signal-form` |
| state, a store, a method that calls the API | `signal-store` |
| a French text added or changed, even one key | `i18n-fr` |
| a page, a route, a URL with an id | `feature-routes` |
| a component that loads one record from an id | `loading-state` |
| a modal, a create or edit window | `spartan-dialog` |
| a confirmation before a delete or an action that cannot be undone | `spartan-alert-dialog` |
| a search box with suggestions, an autocomplete | `spartan-combobox` |
| one value picked in a closed list, a dropdown | `spartan-select` |
| a message after a save, a delete or an error | `spartan-toast` |
| another Spartan component, one that has no skill of its own | `spartan` |
| SCSS, a class list, a colour, a spacing, a token | `scss-styling` |

A create window with a form names `spartan-dialog`, `signal-form`, `signal-store`, `i18n-fr` and `ui-quality`. A new endpoint on a new column names `api-endpoint`, `ef-entity`, `ef-migrations` and `backend-tests`.

A kind of work no row covers has no skill: never invent a name. The developer then works from the criteria and the `Where:` lines.

A new skill for the developers gets its row here, and nowhere else.
