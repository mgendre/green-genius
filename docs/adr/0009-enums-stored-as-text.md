# 0009. Enums stored as text by one EF convention

Date: 2026-10-16

## Status

Accepted

## Context

Enum columns were each configured with `.HasConversion<string>().HasMaxLength(...)`, the same block repeated in several
configurations.

## Decision

- `ApplicationDbContext.ConfigureConventions` declares
  `Properties<Enum>().HaveConversion<string>().HaveMaxLength(EntitiesConstants.ShortTextMaxLength)`.
- No entity configuration calls `HasConversion<string>()` on an enum property.

## Consequences

- Every enum, present and future, is stored as its member name, readable in the database.
- Renaming an enum member changes stored values and needs a data migration.
