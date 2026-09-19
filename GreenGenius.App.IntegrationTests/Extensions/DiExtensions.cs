using Microsoft.Extensions.DependencyInjection;

namespace GreenGenius.App.IntegrationTests.Extensions;

public static class DiExtensions
{
    public static void Replace(this IServiceCollection services, Type toReplace, object replacement)
    {
        var descriptor = services.Single(d => 
            d.ServiceType == toReplace);
            
        services.Remove(descriptor);
        services.AddScoped(toReplace, _ => replacement);
    }
}
