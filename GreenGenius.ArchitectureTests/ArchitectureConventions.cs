using ArchUnitNET.Domain;
using ArchUnitNET.Domain.Extensions;

namespace GreenGenius.ArchitectureTests;

internal static class ArchitectureConventions
{
    private const string HandleNamePattern = @"^Handle\(";

    internal const string FeatureNamespacePattern = @"^GreenGenius\.Api\.Features\.[^.]+$";

    internal static bool IsStatic(Class candidate) =>
        candidate is { IsAbstract: true, IsSealed: true };

    internal static IEnumerable<MethodMember> PublicHandleMethods(Class handler) =>
        handler.Members
            .OfType<MethodMember>()
            .Where(method =>
                method.NameMatches(HandleNamePattern) && method.Visibility == Visibility.Public);
}