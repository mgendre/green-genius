using GreenGenius.Api.Features.Gardens;
using GreenGenius.App.Security;
using GreenGenius.Common.Data.Extensions;
using GreenGenius.Common.Data.Security;
using GreenGenius.Infra.Hosting.Extensions;
using GreenGenius.Infra.Hosting.Handlers;

namespace GreenGenius.App.Extensions;

public static class StartupExtensions
{
    public static void ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.WithHostingServices();
        builder.RegisterApiHandlers(typeof(GardensApi).Assembly);
        builder.ConfigurePersistence();
        
        builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
        
        builder.Services.AddHealthChecks();
        builder.Services.AddOpenApi();
    }
}