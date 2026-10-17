using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.xUnit;

namespace GreenGenius.ArchitectureTests;

public class LayersTest
{
    private static readonly string[] CommonDataUpperLayers =
    [
        "GreenGenius.Infra.Hosting",
        "GreenGenius.Api",
        "GreenGenius.App",
        "Microsoft.AspNetCore",
    ];

    [Fact]
    public void CommonData_ShouldDependOnNoUpperLayer()
    {
        var rule = RuleForLayer(
            @"^GreenGenius\.Common\.Data",
            CommonDataUpperLayers,
            "ADR 0002: Common.Data depends on no upper layer nor on ASP.NET Core");

        rule.Check(Architecture.Loaded);
    }

    [Fact]
    public void InfraHosting_ShouldDependOnNeitherApiNorApp()
    {
        var rule = RuleForLayer(
            @"^GreenGenius\.Infra\.Hosting",
            ["GreenGenius.Api", "GreenGenius.App"],
            "ADR 0002: Infra.Hosting depends on neither Api nor App");

        rule.Check(Architecture.Loaded);
    }

    [Fact]
    public void Api_ShouldNotDependOnApp()
    {
        var rule = RuleForLayer(
            @"^GreenGenius\.Api",
            ["GreenGenius.App"],
            "ADR 0002: Api does not depend on App");

        rule.Check(Architecture.Loaded);
    }

    private static IArchRule RuleForLayer(string subjectPattern, string[] forbiddenNamespaces, string adrRule)
    {
        IType[] forbiddenTypes =
        [
            .. Architecture.Loaded
                .Types
                .Where(type => forbiddenNamespaces.Any(ns => type.FullName.StartsWith(ns, StringComparison.Ordinal)))
        ];

        return ArchRuleDefinition
            .Types()
            .That().ResideInNamespaceMatching(subjectPattern)
            .Should().NotDependOnAny(forbiddenTypes)
            .As(adrRule);
    }
}