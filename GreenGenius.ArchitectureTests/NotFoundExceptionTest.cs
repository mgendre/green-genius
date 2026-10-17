using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.xUnit;
using GreenGenius.Infra.Hosting.Handlers;

namespace GreenGenius.ArchitectureTests;

public class NotFoundExceptionTest
{
    private static readonly string[] BclNotFoundFullNames =
    [
        "System.Collections.Generic.KeyNotFoundException",
        "System.IO.FileNotFoundException",
        "System.IO.DirectoryNotFoundException",
    ];

    [Fact]
    public void CommonDataException_ShouldBePublicInExceptionsNamespace()
    {
        IArchRule rule = ArchRuleDefinition
            .Classes()
            .That().ResideInNamespaceMatching(@"^GreenGenius\.Common\.Data")
            .And().AreAssignableTo(typeof(Exception))
            .Should().ResideInNamespaceMatching(@"^GreenGenius\.Common\.Data\.Exceptions$")
            .AndShould().BePublic()
            .As("ADR 0005: a new persistence error is a public exception type in Common.Data/Exceptions/");

        rule.Check(Architecture.Loaded);
    }

    [Fact]
    public void Handler_ShouldNotDependOnBclNotFoundException()
    {
        IArchRule rule = ArchRuleDefinition
            .Classes()
            .That().ImplementInterface(typeof(IApiHandler))
            .Should().FollowCustomCondition(
                handler => !DependsOnBclNotFoundException(handler),
                "not depend on a BCL not-found exception",
                "depends on a BCL not-found exception")
            .As("ADR 0005: a handler never depends on a BCL not-found exception");

        rule.Check(Architecture.Loaded);
    }

    private static bool DependsOnBclNotFoundException(Class handler) =>
        handler.Dependencies
            .Any(dependency => BclNotFoundFullNames.Contains(dependency.Target.FullName));
}