# 0018. No namespace cycles

Date: 2026-10-16

## Status

Superseded by 0020

## Context

SonarCloud reported a cycle inside `GreenGenius.App`: `Extensions` used `Services`, and `Services` used a helper of
`Extensions`. A cycle ties namespaces together so neither can change or move alone.

## Decision

- No dependency cycle between the namespaces of a project: `GreenGenius.App.(*)`, `GreenGenius.Api.Features.(*)` and the
  namespaces of `GreenGenius.Common.Data` form a directed graph.
- A helper used by two namespaces lives in a namespace of its own (`GreenGenius.App.Json`), never in one of its callers.
- `GreenGenius.ArchitectureTests` enforces it with ArchUnitNET slice rules (`BeFreeOfCycles`).

## Consequences

- A new dependency that closes a loop fails the build's tests, before SonarCloud sees it.
- Shared helpers get small namespaces of their own instead of joining the first caller's.
