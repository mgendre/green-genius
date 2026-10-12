---
name: api-endpoint
description: "The ordered steps to add or change a .NET API endpoint here, test first — integration tests, request and output DTOs, IApiHandler handler, AsDto mapping, Minimal API route, then build and tests. Read before adding a route, a handler or a DTO."
---

# API endpoint

Reference: `GreenGenius.Api/Features/Gardens/`. Rules: `GreenGenius.Api/AGENTS.md`. This skill gives the order.

## Steps

1. **Tests first**, in `GreenGenius.App.IntegrationTests/Api/<Feature>ApiIntegrationTest.cs`: `backend-tests` skill.
2. **Entity**, only for a new one or a new column: `ef-entity` skill, then `ef-migrations`.
3. **Output DTO** in `Features/<Feature>/Dtos/<Resource>Dto.cs`.
4. **Mapping** in `Features/<Feature>/<Feature>Extensions.cs`: the `AsDto` expression, the private compiled field, `ToDto()`.
5. **Handler** in `Features/<Feature>/Handlers/<UseCase><Resource>Handler.cs`, implementing `IApiHandler`, with its request DTO in the same file, marked `[UsedImplicitly]`.
6. **Route** in `Features/<Feature>/<Feature>Api.cs`, with `.WithName("<UseCase><Resource>")`. The name becomes the method of the generated TypeScript client.
7. **New feature only**: a constant in `Constants/RouteConstants.cs` and `app.With<Feature>Api();` in `GreenGenius.App/Program.cs`.
8. Build, tests, report. The Debug build of `GreenGenius.App` regenerates the TypeScript client: say so in the report when the contract changed.

## Shape

```csharp
public class UpdateGardenHandler(ApplicationDbContext dbContext) : IApiHandler
{
    public async Task<Ok<GardenDto>> Handle(Guid id, UpdateGardenDto dto)
    {
        var garden = await dbContext.Gardens.GetAsync(id);

        garden.Name = dto.Name;

        await dbContext.SaveChangesAsync();

        return TypedResults.Ok(garden.ToDto());
    }
}

[UsedImplicitly]
public class UpdateGardenDto
{
    public required string Name { get; init; }
}
```

```csharp
gardens.MapPut("/{id:guid}", ([FromRoute] Guid id, [FromBody] UpdateGardenDto dto,
        [FromServices] UpdateGardenHandler h) => h.Handle(id, dto))
    .WithName("UpdateGarden")
    .ProducesProblem(StatusCodes.Status404NotFound);
```

| Use case | Returns | Reads with |
|---|---|---|
| Create | `Task<Created<Dto>>`, `TypedResults.Created((string?)null, dto)` | new entity, `Id = Guid.NewGuid()`, `OwnerId` from `ICurrentUserService` |
| List | `Task<Ok<List<Dto>>>` | `.Select(<Feature>Extensions.AsDto).ToListAsync()` |
| Get | `Task<Ok<Dto>>` | `.Where(e => e.Id == id).Select(<Feature>Extensions.AsDto).FirstOrDefaultAsync()`, then `throw new DataNotFoundException(…)` when null, as `GetPlantHandler` does |
| Update | `Task<Ok<Dto>>` | `GetAsync(id)`, change, `SaveChangesAsync`, `ToDto()` |
| Delete | `Task<NoContent>` | `GetAsync(id)`, `Remove`, `SaveChangesAsync` |

- `GetAsync(id)` throws `DataNotFoundException`, turned into the `404`: never return `NotFound`, never catch it. A route that takes an id declares `.ProducesProblem(StatusCodes.Status404NotFound)`.
- No service interface between the route and the handler.
- An enum is a string in the JSON, already configured (`JsonStringEnumConverter`): put the enum type in the DTO, never a `string` or an `int`.
- A date and time is a `DateTimeOffset`, never a `DateTime`.
- Data access in a handler: `ef-queries` skill.
