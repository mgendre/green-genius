using GreenGenius.Api.Features.Gardens.Dtos;
using GreenGenius.Api.Features.Gardens.Handlers;
using GreenGenius.Common.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace GreenGenius.Api.Features.Gardens;

public static class GardensExtensions
{
    public static void WithGardensServices(this IServiceCollection services)
    {
        services.AddScoped<CreateGardenHandler>();
        services.AddScoped<ListGardensHandler>();
        services.AddScoped<UpdateGardenHandler>();
    }

    public static GardenDto ToDto(this Garden garden)
    {
        return new GardenDto
        {
            Id = garden.Id,
            Name = garden.Name,
            OwnerId = garden.OwnerId
        };
    }
}
