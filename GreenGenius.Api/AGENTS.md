# GreenGenius backend

.NET 10 Minimal API, EF Core on PostgreSQL. Layout, dependency direction, commands and quality gate: root `AGENTS.md`.

## Backend conventions

`Features/Gardens/` is the reference: read it before writing a new feature.

- **Types**: one public type per file, no nested types. The one exception is the request DTO of a handler, which lives in the handler's file. No tuples across a boundary you define: a named `sealed record` instead.
- **Endpoints**: one static `<Feature>Api` with `With<Feature>Api(this IEndpointRouteBuilder)`, one `MapGroup(RouteConstants.<Feature>)` with `.WithTags`, routes `"/"` and `"/{id:guid}"`. The lambda binds the input and calls the handler, nothing else. Every parameter carries `[FromRoute]`, `[FromBody]` or `[FromServices]` explicitly.
- **Handlers**: one per use case, implementing `IApiHandler` (registered scoped by the assembly scan, never by hand). One public `Handle` returning `TypedResults`; its signature lists only what it returns (`Task<Created<GardenDto>>`, `Task<Ok<GardenDto>>`, `Task<NoContent>`), never `NotFound`.
- **DTOs**: request DTOs next to their handler, output DTOs in `Dtos/` named after the resource. Entities are never exposed.
- **Mapping**: in `<Feature>Extensions`, a `public static readonly Expression<Func<Entity, EntityDto>> AsDto` (plain member reads only), a private compiled field, and `ToDto()` delegating to it. Queries project with `.Select(<Feature>Extensions.AsDto)`; `ToDto()` is for an entity already in memory. Never `.Select(e => e.ToDto())`.
- **Not found**: `dbSet.GetAsync(id)` throws `DataNotFoundException`; `GlobalExceptionHandler` writes the `404` `ProblemDetails`. Unknown and foreign resources are both `404`. Routes that can answer it declare `.ProducesProblem(StatusCodes.Status404NotFound)`. A new persistence error is a new exception in `Common.Data/Exceptions/`, never a BCL type.
- **Created**: `TypedResults.Created((string?)null, dto)`; a `Location` only when a `GET /{id}` exists. Never add a `GET` to fill it.
- **Entities**: `Guid Id` set in code, `ValueGeneratedNever()`. One `IEntityTypeConfiguration<T>` next to the entity, lowercase plural table name, every string with a max length from `EntitiesConstants`. No `IsRequired()` when the type already says it (non-nullable value type, `required` non-nullable reference).
- **Ownership**: user-scoped entities implement `IHasOwner` and call `ConfigureOwnerIdQueryFilter`. `OwnerId` comes from `ICurrentUserService`, never from the request.
- **Reads**: a read that returns a DTO projects in the database, by id as for a list: `.Where(e => e.Id == id).Select(<Feature>Extensions.AsDto).FirstOrDefaultAsync()`, then `DataNotFoundException` when null. No `Include`, no `AsNoTracking` there. An unprojected, unmodified entity read uses `AsNoTracking()`. Never load related data in a loop.
- **Migrations**: generated with `dotnet ef migrations add`, never edited. Applied at startup in every environment.
- **Decided, not to be revisited or reported**: no `CancellationToken` in handlers, no input validation layer, migrations at startup in Production.

## Tests

The `backend-tests` skill holds the references, the helpers and the command.

- **Integration first**: every endpoint behaviour through HTTP in `GreenGenius.App.IntegrationTests/Api/<Feature>ApiIntegrationTest`, inheriting `AbstractApiIntegrationTest` (one Testcontainers PostgreSQL per run, Respawn reset before each test, current user `DefaultCurrentUser`).
- Every endpoint: happy path, unknown-id `404`, not-mine `404`; a list returns only the current user's rows.
- `GreenGenius.Api.Tests` (unit, Moq) only for logic an API test covers badly.
- xUnit, Shouldly. `<UseCase>_Should<Outcome>`, `<UseCase>_When<Condition>_Should<Outcome>`. Arrange / Act / Assert separated by a blank line.
