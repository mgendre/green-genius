# GreenGenius

Permaculture garden planner: .NET 10 Minimal API on PostgreSQL, Angular 22 front end. Read the conventions of the layer you work on before writing code: `GreenGenius.Api/AGENTS.md` for the backend and its tests, `GreenGenius.Frontend/AGENTS.md` for the front end. An ADR wins over these files when they disagree.

## Rules

- KISS, YAGNI, SRP, DRY: the simplest thing that works, framework built-ins before custom code, nothing "for later", no abstraction with a single implementation.
- Do what the task asks and nothing around it: no review, documentation, commit, push or pull request.
- No comments: names and small methods carry the meaning. Delete dead code. Braces on every control-flow block. Early returns over nesting.
- A warning is an error (`Directory.Build.props`). Never silence a rule to get green.
- A change is done when its check passed: build without warnings and the tests that cover it.

## Commands

```shell
dotnet build GreenGenius.slnx
dotnet test GreenGenius.slnx --filter <Class>                 # integration tests need Docker or podman.socket
dotnet ef migrations add <Name> -p GreenGenius.Common.Data/GreenGenius.Common.Data.csproj -s GreenGenius.App/GreenGenius.App.csproj
npm run build --prefix GreenGenius.Frontend
npm run check --prefix GreenGenius.Frontend                   # build without warnings + frontend conventions, prints check: OK
```

The Debug build of `GreenGenius.App` writes `openapi/GreenGenius.App.json` and regenerates the TypeScript client.

## Layout and dependency direction

```
GreenGenius.App                    composition root: Program.cs, Extensions/StartupExtensions, Security/CurrentUserService
GreenGenius.Api                    Features/<Feature>/ (<Feature>Api, <Feature>Extensions, Dtos/, Handlers/), Constants/RouteConstants
GreenGenius.Common.Data            ApplicationDbContext, Entities/ + configurations, Migrations/, Security/, Exceptions/, Extensions/
GreenGenius.Infra.Hosting          GlobalExceptionHandler, IApiHandler marker and its assembly scan
GreenGenius.App.IntegrationTests   API tests: the main suite
GreenGenius.Api.Tests              unit tests
GreenGenius.Frontend               Angular app
```

`App → Api → Common.Data`, `Api → Infra.Hosting → Common.Data`. `Common.Data` never references `Infra.Hosting` or ASP.NET. Declare a `ProjectReference` only when the build needs it.

## Quality gate

`.github/workflows/ci.yaml` builds, tests with coverage and runs SonarCloud (`mgendre_green-genius`) on pull requests to `main` and pushes to `main`.

