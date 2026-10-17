# 0002. Layered solution and dependency direction

Date: 2026-10-16

## Status

Accepted

## Context

The backend is a .NET 10 Minimal API on PostgreSQL. Persistence, hosting infrastructure, endpoints and composition must
stay separable so that the data layer never depends on the web stack.

## Decision

- `GreenGenius.App`: composition root (`Program.cs`, `Extensions/StartupExtensions`, `Security/CurrentUserService`, data
  importers).
- `GreenGenius.Api`: endpoints, one folder per feature `Features/<Feature>/` with `<Feature>Api`, `<Feature>Extensions`,
  `Dtos/`, `Handlers/`; routes in `Constants/RouteConstants`.
- `GreenGenius.Common.Data`: `ApplicationDbContext`, `Entities/` with their configurations, `Migrations/`, `Security/`,
  `Exceptions/`, `Extensions/`.
- `GreenGenius.Infra.Hosting`: `GlobalExceptionHandler`, the `IApiHandler` marker and its assembly scan.
- Allowed references: `App → Api → Common.Data` and `Api → Infra.Hosting → Common.Data`. `Common.Data` references
  neither `Infra.Hosting`, `Api`, `App` nor ASP.NET Core. `Infra.Hosting` references neither `Api` nor `App`. `Api` does
  not reference `App`.
- A `ProjectReference` is declared only when the build needs it.

## Consequences

- `Common.Data` can be used by the importers and the tests without the web stack.
- The direction is enforced by the architecture tests (ADR 0012).
