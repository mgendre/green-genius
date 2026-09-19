using GreenGenius.Api.Services;
using GreenGenius.Api.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace GreenGenius.Api.Extensions;

public static class ApiExtensions
{
    public static void ConfigureApiServices(this IServiceCollection services)
    {
        services.AddScoped<ICurrentUserService, CurrentUserService>();
    }
}