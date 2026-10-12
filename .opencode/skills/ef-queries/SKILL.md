---
name: ef-queries
description: "How to read and write rows with EF Core here — DTO projection, loading an entity to change it, many rows in one query instead of one per loop turn, one save. Read before writing a query, a loop over rows, an import or a bulk change. Not for entities, columns or migrations."
---

# EF Core queries

Reference: `GreenGenius.Api/Features/Plants/Handlers/GetPlantHandler.cs`, `GreenGenius.Api/Features/Gardens/Handlers/`. A user-scoped entity is filtered to the current user by its query filter: never add `Where(x => x.OwnerId == …)`.

## Which read

| You need | Write |
|---|---|
| A DTO, one row | `.Where(plant => plant.Id == id).Select(PlantsExtensions.AsDto).FirstOrDefaultAsync()`, then `DataNotFoundException` when null |
| A DTO list | `.OrderBy(…).Select(GardensExtensions.AsDto).ToListAsync()` |
| One entity to change or delete | `await dbContext.Gardens.GetAsync(id)`: it throws the `404` |
| An entity to change with its 1:1 dependents (`Plant.Needs`, `Plant.Traits`) | `.Include(plant => plant.Needs).Include(plant => plant.Traits)` on the tracked query |
| Does a row exist | `AnyAsync(…)`, never `CountAsync(…) > 0` or a `ToListAsync` |
| Entities read and not changed, not projected | `.AsNoTracking()` |

A projection needs no `Include` and no `AsNoTracking`: EF turns the navigations read by `AsDto` into joins.

## The context

A class that queries receives `ApplicationDbContext` in its primary constructor and uses its sets: `dbContext.Plants`, `dbContext.Gardens`. Never the base `DbContext`, never `Set<Plant>()`.

```csharp
public sealed class PlantImporter(ApplicationDbContext dbContext)
```

## Requests on a plant

Does a plant exist:

```csharp
var exists = await dbContext.Plants.AnyAsync(plant => plant.Key == key);
```

One plant to change, with its needs and traits:

```csharp
var plant = await dbContext.Plants
    .Include(plant => plant.Needs)
    .Include(plant => plant.Traits)
    .FirstOrDefaultAsync(plant => plant.Key == key);
```

A list, filtered, sorted and projected in the database:

```csharp
var names = await dbContext.Plants
    .Where(plant => plant.Family == family)
    .OrderBy(plant => plant.NameFr)
    .Select(plant => plant.NameFr)
    .ToListAsync();
```

## Many rows: one query, then the loop, one save

A query inside a loop runs once per turn. Load what the loop needs before it, change in memory, save once after it. The import of plants:

```csharp
var keys = files.Select(file => file.Key).ToList();
var plants = await dbContext.Plants
    .Include(plant => plant.Needs)
    .Include(plant => plant.Traits)
    .Where(plant => plant.Key != null && keys.Contains(plant.Key))
    .ToDictionaryAsync(plant => plant.Key!);

foreach (var file in files)
{
    if (!plants.TryGetValue(file.Key, out var plant))
    {
        plant = new Plant
        {
            Id = Guid.NewGuid(),
            Key = file.Key,
            NameFr = file.NameFr,
            Needs = new PlantNeeds(),
            Traits = new PlantTraits()
        };
        dbContext.Plants.Add(plant);
    }
    else if (file.Version <= plant.Version)
    {
        continue;
    }

    plant.NameFr = file.NameFr;
    plant.Version = file.Version;
    plant.Needs.HeightCm = file.HeightCm;
    plant.Traits.DroughtTolerant = file.DroughtTolerant;
}

await dbContext.SaveChangesAsync();
```

- A new plant gets its `Needs` and `Traits` objects in the same `Add`; EF sets their `PlantId`.
- An existing plant keeps its `Needs` and `Traits` objects: change their properties. `plant.Needs = new PlantNeeds { … }` on a loaded plant inserts a second row for the same plant and the save fails.
- The same change on every matching row, with no rule in C#: `ExecuteUpdateAsync` or `ExecuteDeleteAsync`, one statement, no entity loaded.

## Rules

- Filter, sort and project before `ToListAsync`: a `ToList()` or `AsEnumerable()` before `Where` or `Select` loads the whole table.
- Compare the bare column: `plant.Key == key`, never `plant.Key.ToLower() == key` or `plant.Version.ToString()`. A function on the column defeats its index.
- A column used in `Where` or `OrderBy` on a table that grows has an index: `ef-entity` skill.
- Two collection navigations included in one query: add `.AsSplitQuery()`.
- Always the async method, awaited: `ToListAsync`, `FirstOrDefaultAsync`, `AnyAsync`, `SaveChangesAsync`.
- No lazy loading, no `virtual` navigation, no SQL by hand.
- Braces `{ }` on every `if`, `else`, `for`, `foreach` and `while`, also around a single line: never `if (x) return;`.

## Mistakes already made here

- `private readonly DbContext _db` and `_db.Set<Plant>()`: `ApplicationDbContext dbContext` and `dbContext.Plants`.
- `FirstOrDefaultAsync` inside the `foreach` to find the existing plant: the dictionary above, one query.
- A plant loaded without `Include`, then `plant.Needs = new PlantNeeds { … }`: include, then change the properties.
- `plant.Version = 0` whatever the file says: compare, then copy the version of the file.
- `SaveChangesAsync` inside the loop: once, after it.
- `.Select(e => e.ToDto())`: it loads the entity. Project with `.Select(<Feature>Extensions.AsDto)`.
