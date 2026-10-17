using ArchUnitNET.Domain;
using ArchUnitNET.Domain.Extensions;
using ArchUnitNET.Fluent;
using ArchUnitNET.xUnit;
using GreenGenius.Common.Data.Security;
using GreenGenius.Infra.Hosting.Handlers;

namespace GreenGenius.ArchitectureTests;

public class OwnershipAndHandlerSignatureTest
{
    private const string EntityNamespacePattern = @"^GreenGenius\.Common\.Data\.Entities$";
    private const string OwnerIdPropertyName = "OwnerId";
    private const string HasOwnerFullName = "GreenGenius.Common.Data.Security.IHasOwner";
    private const string CancellationTokenFullName = "System.Threading.CancellationToken";

    [Fact]
    public void EntityWithOwnerId_ShouldImplementIHasOwner()
    {
        IArchRule rule = ArchRuleDefinition
            .Classes()
            .That().ResideInNamespaceMatching(EntityNamespacePattern)
            .Should().FollowCustomCondition(
                ImplementsIHasOwnerWhenItHasOwnerId,
                $"implement {nameof(IHasOwner)} when it has a property named {OwnerIdPropertyName}",
                $"has a property named {OwnerIdPropertyName} without implementing {nameof(IHasOwner)}")
            .As("ADR 0019: a user-scoped entity with an OwnerId property implements IHasOwner");

        rule.Check(Architecture.Loaded);
    }

    [Fact]
    public void HandlerHandle_ShouldNotTakeCancellationToken()
    {
        IArchRule rule = ArchRuleDefinition
            .Classes()
            .That().ImplementInterface(typeof(IApiHandler))
            .Should().FollowCustomCondition(
                HandleHasNoCancellationTokenParameter,
                "have no Handle parameter of type CancellationToken",
                "has a Handle parameter of type CancellationToken")
            .As("ADR 0008: a Handle method of an IApiHandler class takes no CancellationToken");

        rule.Check(Architecture.Loaded);
    }

    private static bool ImplementsIHasOwnerWhenItHasOwnerId(Class entity) =>
        !HasOwnerIdProperty(entity) || entity.ImplementsInterface(HasOwnerFullName);

    private static bool HasOwnerIdProperty(Class entity) =>
        entity.Members
            .OfType<PropertyMember>()
            .Any(property => property.Name == OwnerIdPropertyName);

    private static bool HandleHasNoCancellationTokenParameter(Class handler) =>
        ArchitectureConventions.PublicHandleMethods(handler)
            .All(method => method.Parameters.All(parameter => parameter.FullName != CancellationTokenFullName));
}