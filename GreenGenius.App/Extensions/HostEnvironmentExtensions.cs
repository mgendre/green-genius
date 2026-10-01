using System.Reflection;

namespace GreenGenius.App.Extensions;

public static class HostEnvironmentExtensions
{
    private const string OpenApiGeneratorAssembly = "GetDocument.Insider";

    public static bool IsOpenApiGeneration(this IHostEnvironment _)
    {
        return Assembly.GetEntryAssembly()?.GetName().Name == OpenApiGeneratorAssembly;
    }
}
