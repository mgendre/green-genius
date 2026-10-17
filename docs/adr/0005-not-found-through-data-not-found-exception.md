# 0005. Not found through DataNotFoundException

Date: 2026-10-16

## Status

Accepted

## Context

Every read by id must answer `404` when the row is missing. Returning `NotFound` from each handler repeats the same
branch and widens every handler signature.

## Decision

- `dbSet.GetAsync(id)` throws `DataNotFoundException` when the row is missing; a projected read throws it when
  `FirstOrDefaultAsync` returns null.
- `GlobalExceptionHandler` in `Infra.Hosting` turns it into a `404` `ProblemDetails`.
- A handler signature never lists `NotFound`. A route that can answer `404` declares
  `.ProducesProblem(StatusCodes.Status404NotFound)`.
- A new persistence error is a new exception type in `Common.Data/Exceptions/`, never a BCL exception.

## Consequences

- Unknown and foreign resources are answered the same way (ADR 0006).
- Error mapping has one place; handlers keep only the happy path.
