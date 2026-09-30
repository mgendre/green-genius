using GreenGenius.Api.Features.Gardens.Dtos;
using GreenGenius.Common.Data.Entities;

namespace GreenGenius.Api.Features.Gardens;

public static class GardensExtensions
{
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
