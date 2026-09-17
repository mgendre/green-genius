using GreenGenius.Common.Domain.Extensions;

namespace GreenGenius.App.Extensions;

public static class StartupExtensions
{
    public static void ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.ConfigureDomain();
        
        builder.Services.AddHealthChecks();
        builder.Services.AddOpenApi();
    }
}