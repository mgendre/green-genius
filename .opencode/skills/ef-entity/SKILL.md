---
name: ef-entity
description: "How to add or change an EF Core entity here — the entity class, its IEntityTypeConfiguration, table and column rules, the DbSet, ownership. Read before adding an entity, a column or an index, then follow with the ef-migrations skill."
---

# Entity and configuration

Entities and configurations live side by side in `GreenGenius.Common.Data/Entities/`. Reference: `Garden.cs`, `GardenConfiguration.cs`.

```csharp
public class Garden : IHasOwner
{
    public Guid Id { get; init; }
    public Guid OwnerId { get; init; }
    public required string Name { get; set; }
}
```

```csharp
public sealed class GardenConfiguration(ApplicationDbContext dbContext) : IEntityTypeConfiguration<Garden>
{
    public void Configure(EntityTypeBuilder<Garden> builder)
    {
        builder.ToTable("gardens");
        builder.HasKey(garden => garden.Id);
        builder.Property(garden => garden.Id).ValueGeneratedNever();
        builder.Property(garden => garden.Name)
            .HasMaxLength(EntitiesConstants.MediumTextMaxLength);
        builder.HasIndex(garden => new { garden.OwnerId, garden.Name });
        builder.ConfigureOwnerIdQueryFilter(dbContext);
    }
}
```

## Steps

1. Entity: one public class per file. `Guid Id` set in code. A column that must have a value is `required` and non-nullable; an optional one is nullable (`string?`).
2. Configuration: one `IEntityTypeConfiguration<T>` next to the entity.
3. A new entity: its `ApplyConfiguration(new <Entity>Configuration())` line in `ApplicationDbContext`, and a `DbSet` only when it is queried on its own.
4. Then the `ef-migrations` skill.

## Rules

- Table name: lowercase plural, words separated by `_`: `plant_needs`, never `plantNeeds`.
- Every string has `HasMaxLength` with a constant of `EntitiesConstants` (`MediumTextMaxLength` is 250). An enum stored with `HasConversion<string>()` too.
- `Id`: `ValueGeneratedNever()`.
- Configure only what EF does not infer: no `IsRequired()` on a non-nullable value type (`int`, `bool`, `Guid`, enum) or on a `required` non-nullable reference, no `HasDefaultValue` equal to the CLR default.
- A unique value: `builder.HasIndex(entity => entity.Key).IsUnique()`.
- A user-scoped entity implements `IHasOwner` and calls `ConfigureOwnerIdQueryFilter(dbContext)`. `OwnerId` comes from `ICurrentUserService`, never from a request.
- A dependent reached only through its principal's navigation gets no `DbSet`.
- An entity split into a principal and 1:1 dependents sharing its key is a decided design: do not merge them.
- Braces `{ }` on every `if`, `else`, `for`, `foreach` and `while`, also around a single line: never `if (x) return;`.

## Renaming or replacing a column

A rename is the property renamed, alone, in its own migration. A removal and an addition of the same type never go in one migration: see the rename trap in `ef-migrations`.
