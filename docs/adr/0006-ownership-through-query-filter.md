# 0006. Ownership through IHasOwner and a query filter

Date: 2026-10-16

## Status

Superseded by 0019

## Context

Gardens, beds and their plants belong to one user. Filtering by owner in every query is easy to forget and is a data
leak when forgotten.

## Decision

- A user-scoped entity implements `IHasOwner` and its configuration calls `ConfigureOwnerIdQueryFilter`.
- `OwnerId` is set from `ICurrentUserService`, never read from the request.
- A resource owned by another user is invisible and answers `404`, exactly like an unknown id.
- Every endpoint has an integration test for the unknown-id `404` and the not-mine `404`; a list returns only the
  current user's rows.

## Consequences

- Handlers do not filter by owner themselves.
- Reference data (plants, associations) is not user-scoped and carries no owner.
