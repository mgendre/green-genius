using GreenGenius.Api.Features.Gardens.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace GreenGenius.Api.Features.Gardens;

public static class GardensExtensions
{
    public static void WithGardensServices(this IServiceCollection services)
    {
        services.AddScoped<CreateGardenHandler>();
    }
}
