using ArchUnitNET.Fluent.Slices;

namespace GreenGenius.ArchitectureTests;

public class CommonDataNamespacesTest
{
    [Fact]
    public void CommonDataNamespaces_ShouldHaveNoCycle()
    {
        var rule = SliceRuleDefinition
            .Slices()
            .Matching("GreenGenius.Common.(**)")
            .Should()
            .BeFreeOfCycles();

        Assert.True(rule.HasNoViolations(Architecture.Loaded),
            "ADR 0020: no cycle between the namespaces of GreenGenius.Common.Data");
    }
}