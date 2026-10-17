# 0004. Entities never exposed, DTOs projected in the database

Date: 2026-10-16

## Status

Accepted

## Context

Returning EF entities couples the API contract and the generated TypeScript client to the database schema, and loading
entities only to map them wastes queries.

## Decision

- Output DTOs live in `Features/<Feature>/Dtos/`, named after the resource. A request DTO lives in its handler's file.
  No public member of a DTO has a type from `GreenGenius.Common.Data.Entities`.
- Every non-nullable DTO member is `required`; nullable members stay optional.
- Mapping lives in `<Feature>Extensions`: a `public static readonly Expression<Func<Entity, EntityDto>> AsDto` made of
  plain member reads, a private compiled field, and `ToDto()` delegating to it.
- A read that returns a DTO projects in the database with `.Select(<Feature>Extensions.AsDto)`, by id as for a list,
  without `Include` or `AsNoTracking`. `ToDto()` is only for an entity already in memory. `.Select(e => e.ToDto())` is
  forbidden.

## Consequences

- The OpenAPI document, and so the generated client, types required members as non-optional.
- A schema change does not leak into the contract unless the projection changes.
- The "entities never exposed" rule is enforced by the architecture tests (ADR 0012).
