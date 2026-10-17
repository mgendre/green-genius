using GreenGenius.Api.Features.Gardens;
using GreenGenius.App.Json;
using GreenGenius.App.Security;
using GreenGenius.App.Services;
using GreenGenius.Common.Data.Extensions;
using GreenGenius.Common.Data.Security;
using GreenGenius.Infra.Hosting.Extensions;
using GreenGenius.Infra.Hosting.Handlers;

namespace GreenGenius.App.Extensions;

public static class StartupExtensions
{
    extension(WebApplicationBuilder builder)
    {
        public void ConfigureServices()
        {
            builder.WithHostingServices();
            builder.RegisterApiHandlers(typeof(GardensApi).Assembly);
            builder.ConfigurePersistence();

            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
            builder.Services.AddScoped<PlantImporter>();
            builder.Services.AddScoped<PlantAssociationImporter>();

            builder.Services.AddHealthChecks();
            builder.Services.AddOpenApi();

            builder.ConfigureJsonOptions();
        }

        private void ConfigureJsonOptions()
        {
            builder.Services.ConfigureHttpJsonOptions(options => { options.SerializerOptions.UseAppJsonDefaults(); });
        }
    }

    extension(WebApplication app)
    {
        public async Task ImportDataAsync()
        {
            using var scope = app.Services.CreateScope();
            var plantImporter = scope.ServiceProvider.GetRequiredService<PlantImporter>();
            await plantImporter.ImportAsync();
            var associationImporter = scope.ServiceProvider.GetRequiredService<PlantAssociationImporter>();
            await associationImporter.ImportAsync();
        }
    }
}