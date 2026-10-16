---
name: ef-migrations
description: "How to make an EF Core migration here — the commands, a short clear name, reading what was generated, the rename trap, removing a wrong one, merging the migrations of a branch or all migrations into one. Read after changing an entity or its configuration, and before running any `dotnet ef` command."
---

# EF Core migrations

Migrations live in `GreenGenius.Common.Data/Migrations/`. They are generated, never edited by hand, and applied at startup.

Every command runs from the repository root with the same two projects, written `-p … -s …` below:

```shell
-p GreenGenius.Common.Data/GreenGenius.Common.Data.csproj -s GreenGenius.App/GreenGenius.App.csproj
```

If `dotnet ef` is not found: `dotnet tool restore`, once.

## Commands

| Need | Command |
|---|---|
| Does the model differ from the last migration? | `dotnet ef migrations has-pending-model-changes -p … -s …` |
| Create a migration | `dotnet ef migrations add <Name> -p … -s …` |
| List the migrations | `dotnet ef migrations list --no-connect -p … -s …` |
| Read the SQL a migration runs | `dotnet ef migrations script <PreviousName> <Name> -p … -s …` |
| Drop the last migration, not applied yet | `dotnet ef migrations remove -p … -s …`, never with a name: it always drops the last one |

## When the model changes

1. Change the entity and its configuration (`ef-entity` skill).
2. `dotnet build GreenGenius.slnx -v q --nologo 2>&1 | tail -20`: the build must be clean first, the tool builds the startup project.
3. `dotnet ef migrations add <Name> -p … -s …`. One migration per change, short name, see below.
4. Read the generated `<timestamp>_<Name>.cs`: what you changed and nothing else, with the types, lengths, nullability and table names you meant. Every `<Entity>Id` column has its `ForeignKey` to the table of that entity: a missing one is a relationship missing in the configuration (`ef-entity` skill). An empty `Up`: your change did not reach the database model. Remove that migration, never keep an empty one. An unexpected `DropColumn`, `DropTable` or `RenameColumn`: `dotnet ef migrations remove -p … -s …`, fix the model, add it again. Never fix the migration file.
5. `dotnet ef migrations has-pending-model-changes -p … -s …` must report no pending change.
6. Run the integration tests of the feature: they apply every migration to a fresh PostgreSQL. Every test failing with `PendingModelChangesWarning` means the model and the last migration differ: you changed an entity or a configuration after step 3.

One task, one migration. The model changes again after step 3, or goes back: remove the migration of this task and add it again, see below. Never a second migration that corrects the first.

## Name

Short and clear: a verb, the table, what changes. Two to four words in PascalCase, 40 characters at most.

| Change | Name |
|---|---|
| A new table | `AddPlants` |
| A column added | `AddPlantKey` |
| Two or three columns added to one table | `AddPlantKeyAndVersion` |
| A column renamed | `RenamePlantLatinName` |
| A column removed | `RemovePlantLatinName` |
| An index | `AddPlantKeyIndex` |

Never a list of everything inside: `RemoveNameLatinAddKeyBinomialNameAndVersion` is two migrations with two short names.

## The rename trap

A column removed and another of the same type added in one migration become a `RenameColumn`: the old values land in the new column. `NameLatin` removed and `Key` added became `RenameColumn("NameLatin", newName: "Key")`.

- A rename (the data stays): rename the property alone, add the migration, check its single `RenameColumn`. Other columns go in a second migration.
- A removal and an addition: remove the property, add the migration, check its `DropColumn`. Then the new property and a second migration.

## When a migration is wrong

- Not committed, not applied: `dotnet ef migrations remove -p … -s …`, fix the model, add it again. Never delete the files by hand.
- `remove` reads the local database to know what is applied. It prints `Failed to connect to 127.0.0.1:5432`: run `podman start db-postgres`, then `remove` again. It still fails: stop and report. Never finish its work by hand.
- Applied to your local database: `remove` refuses. Stop and report.
- Committed on `main` or shared: never remove it; add a migration that corrects it.
- Committed on the current branch only, and the task asks for it: merge the migrations of the branch, see below.

## Merging the migrations of the branch into one

Only when the task asks for it. `<Previous>` is the last migration that is also on `main`; `<Name>` is the name of the merged migration (see **Name**). The files are deleted by hand and the snapshot is taken back from `main`: no `remove`, no database.

1. `git show main:GreenGenius.Common.Data/Migrations/ --name-only`: the migrations of `main`. The migrations of the branch are the other `<timestamp>_<Name>.cs` files of the folder, all newer than `<Previous>`. One of the migrations to merge is on `main`, or older than `<Previous>`: stop and report.
2. Delete the two files of each migration of the branch, one `rm <path>` per file: `<timestamp>_<Name>.cs` and `<timestamp>_<Name>.Designer.cs`.
3. `git show main:GreenGenius.Common.Data/Migrations/ApplicationDbContextModelSnapshot.cs > GreenGenius.Common.Data/Migrations/ApplicationDbContextModelSnapshot.cs`: the snapshot as `main` left it. It is the one case where the snapshot is written by a command other than `dotnet ef`.
4. `dotnet ef migrations list --no-connect -p … -s …` ends with `<Previous>`.
5. `dotnet build GreenGenius.slnx -v q --nologo 2>&1 | tail -20`, then `dotnet ef migrations add <Name> -p … -s …`.
6. Read the generated migration: the final state of the branch in one go. A table created by the branch is one `CreateTable` with all its columns and indexes: no `AddColumn`, `DropIndex` or `defaultValue: new Guid("00000000-…")` on it. An empty `Up`: step 3 did not happen.
7. `has-pending-model-changes`, then the integration tests.
8. Report under `Not done`: a local database that applied the deleted migrations fails at the next start; the user takes it back before or drops it.

## Merging all migrations into one

Only when the task asks for it. The snapshot is what EF compares the model with: as long as it is there, a new migration is empty. So the snapshot goes too, and EF writes it again.

1. Delete **every** file of `GreenGenius.Common.Data/Migrations/`, `ApplicationDbContextModelSnapshot.cs` included, one `rm <path>` per file. It is the one case where files of that folder are deleted by hand.
2. `dotnet build GreenGenius.slnx -v q --nologo 2>&1 | tail -20`.
3. `dotnet ef migrations add Initial -p … -s …`.
4. Read the generated migration: one `CreateTable` for each table of the model, no `AlterColumn`, no `RenameColumn`. The snapshot is back, written by EF.
5. `has-pending-model-changes`, then the integration tests.
6. Report under `Not done`: a database created with the old migrations must be dropped by the user before the next start.

An empty migration after `add` means the snapshot was still there: delete the three files of the folder and start again at step 2. Never write the snapshot yourself to an older state.

## Never

- Edit a migration, its `.Designer.cs` or `ApplicationDbContextModelSnapshot.cs`.
- Run `dotnet ef database update` or `dotnet ef database drop` without being asked.
