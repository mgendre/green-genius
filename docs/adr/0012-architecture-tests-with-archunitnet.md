# 0012. Architecture tests with ArchUnitNET

Date: 2026-10-16

## Status

Accepted

## Context

Layering, handler and DTO rules lived only in convention files and reviews. A rule that only a reviewer checks drifts.

## Decision

- Project `GreenGenius.ArchitectureTests`, in `GreenGenius.slnx` under `/90_Tests/`, with `TngTech.ArchUnitNET.xUnit`
  and one `ProjectReference` to `GreenGenius.App`. The architecture is loaded once per test run.
- One test class per rule family, one `[Fact]` per rule, named `<Subject>_Should<Rule>`:
    - Layers: the reference rules of ADR 0002, including `Common.Data` not depending on `Microsoft.AspNetCore`.
    - Handlers: a class implementing `IApiHandler` lives in `GreenGenius.Api.Features.*.Handlers` and its name ends with
      `Handler`; a class there whose name ends with `Handler` implements `IApiHandler`.
    - DTOs: no public member of a type in `GreenGenius.Api.Features.*.Dtos` has a type from
      `GreenGenius.Common.Data.Entities`.
- A rule the code breaks is fixed in the code, never silenced or weakened.

## Consequences

- A break of ADR 0002, 0003 or 0004 fails `dotnet test`.
- The suite runs in the existing `dotnet test` step of CI.
