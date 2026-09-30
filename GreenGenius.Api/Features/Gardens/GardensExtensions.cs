using System.Linq.Expressions;
using GreenGenius.Api.Features.Gardens.Dtos;
using GreenGenius.Common.Data.Entities;

namespace GreenGenius.Api.Features.Gardens;

public static class GardensExtensions
{
    public static readonly Expression<Func<Garden, GardenDto>> AsDto = garden => new GardenDto
    {
        Id = garden.Id,
        Name = garden.Name
    };

    private static readonly Func<Garden, GardenDto> AsDtoCompiled = AsDto.Compile();

    public static GardenDto ToDto(this Garden garden)
    {
        return AsDtoCompiled(garden);
    }
}
