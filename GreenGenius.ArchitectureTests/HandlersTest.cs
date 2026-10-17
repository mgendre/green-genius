using ArchUnitNET.Fluent;
using ArchUnitNET.xUnit;
using GreenGenius.Infra.Hosting.Handlers;

namespace GreenGenius.ArchitectureTests;

public class HandlersTest
{
    private const string HandlerFullNamePattern =
        @"^GreenGenius\.Api\.Features\.[^.]+\.Handlers\.[A-Za-z0-9_+]*Handler$";

    [Fact]
    public void IApiHandler_ShouldBeImplementedInHandlerNamespaceAndNamedHandler()
    {
        IArchRule rule = ArchRuleDefinition
            .Classes()
            .That().ImplementInterface(typeof(IApiHandler))
            .Should().HaveFullNameMatching(HandlerFullNamePattern)
            .As(
                "ADR 0003: a class implementing IApiHandler lives in the Handlers namespace of a feature and is named <UseCase>Handler");

        rule.Check(Architecture.Loaded);
    }

    [Fact]
    public void HandlerNamespace_ShouldOnlyContainIApiHandlerImplementations()
    {
        IArchRule rule = ArchRuleDefinition
            .Classes()
            .That().HaveFullNameMatching(HandlerFullNamePattern)
            .Should().ImplementInterface(typeof(IApiHandler))
            .As("ADR 0003: a class in the Handlers namespace of a feature implements IApiHandler");

        rule.Check(Architecture.Loaded);
    }
}