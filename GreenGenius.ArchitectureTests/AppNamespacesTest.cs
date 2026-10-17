using ArchUnitNET.Fluent.Slices;

namespace GreenGenius.ArchitectureTests;

public class AppNamespacesTest
{
    [Fact]
    public void AppNamespaces_ShouldHaveNoCycle()
    {
        var rule = SliceRuleDefinition
            .Slices()
            .Matching("GreenGenius.App.(*)")
            .Should()
            .BeFreeOfCycles();

        Assert.True(rule.HasNoViolations(Architecture.Loaded),
            "ADR 0020: no cycle between the namespaces of GreenGenius.App");
    }
}