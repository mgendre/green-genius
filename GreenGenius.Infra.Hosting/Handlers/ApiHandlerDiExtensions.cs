using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace GreenGenius.Infra.Hosting.Handlers;

public static class ApiHandlerDiExtensions
{
    public static void RegisterApiHandlers(this WebApplicationBuilder builder, Assembly assembly)
    {
        var handlers = FindHandlers(assembly);
        foreach (var handler in handlers)
        {
            builder.Services.AddScoped(handler);
        }
    }

    private static IEnumerable<Type> FindHandlers(Assembly assembly)
    {
        return assembly
            .GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false }
                        && typeof(IApiHandler).IsAssignableFrom(t));
    }
}
