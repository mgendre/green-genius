# 0007. Migrations generated, never edited, applied at startup

Date: 2026-10-16

## Status

Accepted

## Context

The schema is owned by the EF model. Hand-edited migrations drift from the model snapshot, and a separate deployment
step for migrations is one more thing to run.

## Decision

- Migrations are generated with `dotnet ef migrations add` (project `GreenGenius.Common.Data`, startup
  `GreenGenius.App`) and never edited by hand. A wrong migration is removed and generated again.
- `Program.cs` calls `MigrateAsync` at startup in every environment, Production included, and skips it only during
  OpenAPI generation.

## Consequences

- The application needs schema-change rights on its database.
- A model change without its migration fails the tests with `PendingModelChangesWarning`.
- Running migrations at startup in Production is settled and is not reported in reviews.
