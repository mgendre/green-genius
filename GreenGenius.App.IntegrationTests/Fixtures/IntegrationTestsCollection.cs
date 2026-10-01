namespace GreenGenius.App.IntegrationTests.Fixtures;

[CollectionDefinition(Name)]
public sealed class IntegrationTestsCollection : ICollectionFixture<IntegrationTestsApplicationFixture>
{
    public const string Name = "Integration tests";
}
