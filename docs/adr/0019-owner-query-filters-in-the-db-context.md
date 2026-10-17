# 0019. Owner query filters in the DbContext

Date: 2026-10-16

## Status

Accepted

Supersedes 0006

## Context

ADR 0006 put the owner filter in each entity configuration, which took the `ApplicationDbContext` in its constructor.
That made `Entities` and `Extensions` depend on the context and the context on them: a namespace cycle that ADR 0018
forbids.

## Decision

- A user-scoped entity implements `IHasOwner`.
- Its owner query filter is declared in `ApplicationDbContext.OnModelCreating`, reading the context's `CurrentUserId`:
  `modelBuilder.Entity<Garden>().HasQueryFilter(garden => garden.OwnerId == CurrentUserId)`. A dependent without
  `OwnerId` filters through its owner (`BedPlant` through `Bed`).
- Entity configurations have a parameterless constructor and never reference `ApplicationDbContext`; the context
  registers them with `ApplyConfigurationsFromAssembly`.
- `OwnerId` is set from `ICurrentUserService`, never read from the request.
- A resource owned by another user is invisible and answers `404`, exactly like an unknown id.
- Every endpoint has an integration test for the unknown-id `404` and the not-mine `404`; a list returns only the
  current user's rows.

## Consequences

- A new user-scoped entity needs its filter line in `OnModelCreating`; its not-mine tests catch a forgotten one.
- Handlers do not filter by owner themselves.
- Reference data (plants, associations) is not user-scoped and carries no owner.
