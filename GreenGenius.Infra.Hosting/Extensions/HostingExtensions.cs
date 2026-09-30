using GreenGenius.Infra.Hosting.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace GreenGenius.Infra.Hosting.Extensions;

public static class HostingExtensions
{
    public static void WithHostingServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddProblemDetails();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    }
}