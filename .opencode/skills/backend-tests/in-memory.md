# Unit test with an in-memory database

For a class that receives `ApplicationDbContext`: give it a real context on the in-memory provider.

Needs `<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="10.0.12" />` in `GreenGenius.Api.Tests.csproj`. If it is missing, stop and report: you do not add a package.

```csharp
public class GardensQueryTest
{
    private static readonly Guid OwnerId = Guid.NewGuid();
    private readonly string _databaseName = Guid.NewGuid().ToString();

    private ApplicationDbContext NewDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options;
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(service => service.GetCurrentUserId()).Returns(OwnerId);

        return new ApplicationDbContext(options, currentUser.Object);
    }

    [Fact]
    public async Task ListGardens_WhenAnotherOwnerHasOne_ShouldReturnOnlyMine()
    {
        await using (var arrange = NewDbContext())
        {
            arrange.Gardens.Add(new Garden { Id = Guid.NewGuid(), OwnerId = OwnerId, Name = "Mine" });
            arrange.Gardens.Add(new Garden { Id = Guid.NewGuid(), OwnerId = Guid.NewGuid(), Name = "Other" });
            await arrange.SaveChangesAsync();
        }

        await using var dbContext = NewDbContext();
        var names = await dbContext.Gardens.Select(garden => garden.Name).ToListAsync();

        names.ShouldBe(["Mine"]);
    }
}
```

- The happy path shared by several tests is built once in the constructor, `SaveChanges()` on a context of its own, its
  ids in `readonly` fields (ADR 0021); each test then arranges only its difference. Never copy the same `arrange` block
  into each test.
- `_databaseName` is a field: xUnit builds the class again for each test, so each test gets an empty database.
- A new context for each step, on the same name: one to arrange, one for **each** call of the class under test, one to assert. Never `var db = NewDbContext();` kept for the whole test: a context remembers what it loaded, so the test passes where the application, which gets a new context at each start, fails. The plant import passed its tests that way and crashed on the second start with `An item with the same key has already been added`.
- Usings: `GreenGenius.Common.Data`, `GreenGenius.Common.Data.Entities`, `GreenGenius.Common.Data.Security`, `Microsoft.EntityFrameworkCore`, `Moq`, `Shouldly`.
- The owner filter works: a row of another owner is not returned.

The in-memory provider is not PostgreSQL. It does not enforce a unique index, a max length or a foreign key, runs no migration, and throws on `ExecuteUpdateAsync`, `ExecuteDeleteAsync` and transactions. A behaviour that depends on one of these is tested through an integration test.
