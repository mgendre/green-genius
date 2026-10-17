---
name: backend-tests
description: "Which test to write or adapt for a backend change and how — integration tests through HTTP for an endpoint, unit tests for pure logic (mapping, parsing, a rule), the helpers to reuse, the commands to run. Read at every change of the .NET code, before the first edit."
---

# Backend tests

Every backend change ends with its tests written or adapted, and run: a test that would fail if the change were undone.

## Which test

| What changed | Test | Where |
|---|---|---|
| An endpoint: route, status code, body, who may see what | Integration, through HTTP | `GreenGenius.App.IntegrationTests/Api/<Feature>ApiIntegrationTest.cs` |
| An entity or a column that an endpoint returns or stores | The integration tests of that feature, and its `Persist<Entity>Async` helper | same file, and `GreenGenius.App.IntegrationTests/Extensions/DbContextTestExtensions.cs` |
| Logic with no HTTP and no database: a mapping, a parser, a calculation, a rule with branches | Unit | `GreenGenius.Api.Tests/<Folder>/<Class>Test.cs` |
| A class that takes `ApplicationDbContext` and decides in C#: an import, an upsert rule, a service | Unit, with an in-memory database | `GreenGenius.Api.Tests/<Folder>/<Class>Test.cs` |
| A rename or a removal of a field | No new test: adapt every test that names it | search the old name with `grep` in both test projects |

What an endpoint shows is tested through HTTP only, with no unit test for the same thing.

## Integration test

Reference: `GreenGenius.App.IntegrationTests/Api/GardensApiIntegrationTest.cs`. The class inherits `AbstractApiIntegrationTest` (`GreenGenius.App.IntegrationTests/Infra/AbstractApiIntegrationTest.cs`): one PostgreSQL for the run, emptied before each test, every migration applied, current user `DefaultCurrentUser`.

```csharp
[Fact]
public async Task CreateGarden_ShouldInsert()
{
    const string name = "AnotherGarden";

    var result = await CreateClient().PostAndRead<GardenDto, CreateGardenDto>(
        RouteConstants.Gardens, NewCreateGardenDto(name));

    result.Response.StatusCode.ShouldBe(HttpStatusCode.Created);
    result.Value!.Name.ShouldBe(name);

    await ExecuteInScopeAsync(async ctx =>
    {
        (await ctx.Gardens.AnyAsync(g => g.Name == name)).ShouldBeTrue();
    });
}
```

- Arrange through the database: `ExecuteInScopeAsync(async ctx => …)` with `ctx.PersistGardenAsync(name, ownerId)` or `ctx.PersistPlantAsync(…)`. A new entity gets its `Persist<Entity>Async` in `DbContextTestExtensions.cs`.
- Act through `CreateClient()` and `HttpClientExtensions`: `PostAndRead<TResult, TBody>`, `PutAndRead<TResult, TBody>`, `GetAndRead<TResult>`, `GetAndReadList<TResult>`.
- Assert the status code, then the body, then the database.
- Each endpoint: its happy path, the unknown id (`404`), the row of another owner (`404`). A list returns only the rows of `DefaultCurrentUser`. An entity without owner has no other-owner case: its list test asserts the rows and their order.

## Unit test

Reference: `GreenGenius.Api.Tests/Services/CurrentUserServiceTest.cs`. xUnit, Shouldly, Moq. One test class per class under test, in the folder that mirrors its namespace.

```csharp
public class CurrentUserServiceTest
{
    private readonly CurrentUserService _service = new();

    [Fact]
    public void GetCurrentUserId_ShouldReturnGuid()
    {
        var id = _service.GetCurrentUserId();

        id.ShouldNotBe(Guid.Empty);
    }
}
```

- Build the class under test with `new`. A dependency is a `Mock<IDependency>` whose `.Object` goes to the constructor.
- The happy path that two tests arrange identically goes in the constructor (ADR 0021): readonly fields for its ids,
  `SaveChanges()` through a context of its own; a test arranges only what differs from it.
- One behaviour per test. A rule with branches gets one test per branch, named after the condition.
- No file of the repository, no clock: pass the data in. A database only as the next section says.

## Unit test with an in-memory database

For a class that receives `ApplicationDbContext` (an import, an upsert rule, a service). Read `.opencode/skills/backend-tests/in-memory.md` before writing it: it holds the reference to copy and the rule of one new context per step.

## Shape of every test

Names and layout: `GreenGenius.Api/AGENTS.md`, section **Tests**. Shouldly for the assertions: `ShouldBe`, `ShouldBeTrue`, `ShouldNotBe`, `ShouldBeInOrder`.

## Run

```shell
dotnet test GreenGenius.slnx --filter <TestClass> 2>&1 | tail -20
```

`<TestClass>` is the class name alone: `GardensApiIntegrationTest`. Run the class you wrote or adapted, then those of every feature you touched.

## Never

- Write a test that passes whether the code is right or wrong: no assertion, `x.ShouldBe(x)`, an expected value computed by the code under test.
- Test a mapping or a conversion with an input already in the expected form: every field gets a value of its own, different from the default.
- `Should.Throw<Exception>`: assert the exact exception type.
- A `try/catch` in a test, a `Thread.Sleep`, a `Task.Delay`, a state shared between two tests.
- Three tests that differ by one value: one `[Theory]` with `[InlineData]`.
- `new Mock<DbContext>()`, `new Mock<ApplicationDbContext>()` or a mocked `DbSet`: the in-memory database.
- More mock setup than test: mock only the dependency the behaviour needs, assert the result, not the calls.
- Read the same test file twice: read it once, then write.
- Look inside the database of the integration tests from outside: to know what it holds, assert it in the test.
- Deliver untested when the integration tests cannot start (no container engine): stop and report.
