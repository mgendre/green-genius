using GreenGenius.Api.Features.Gardens;
using GreenGenius.Common.Domain.Extensions;
using GreenGenius.Infra.Hosting.Extensions;

namespace GreenGenius.App.Extensions;

public static class StartupExtensions
{
    public static void ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.WithHostingServices();
        builder.ConfigureDomain();
        
        builder.Services.WithGardensServices();
        
        builder.Services.AddHealthChecks();
        builder.Services.AddOpenApi();
    }
}