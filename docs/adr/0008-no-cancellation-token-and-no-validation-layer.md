# 0008. No CancellationToken in handlers, no input validation layer

Date: 2026-10-16

## Status

Accepted

## Context

Both are common .NET recommendations. For this application they add parameters and a layer without a current need, and
the user decided against them.

## Decision

- `Handle` methods take no `CancellationToken`.
- There is no input validation layer: no validation library, no endpoint filter, no validator classes.
- These two points are not to be revisited nor reported in reviews.

## Consequences

- Handler signatures stay limited to their input and their services.
- Proposing either one requires a new ADR superseding this one.
