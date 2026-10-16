using System.Linq.Expressions;
using GreenGenius.Api.Features.Beds.Dtos;
using GreenGenius.Api.Features.Plants.Dtos;
using GreenGenius.Common.Data.Entities;

namespace GreenGenius.Api.Features.Beds;

public static class BedsExtensions
{
    public static readonly Expression<Func<Bed, BedDto>> AsDto = bed => new BedDto
    {
        Id = bed.Id,
        Name = bed.Name,
        GardenId = bed.GardenId,
        Plants = bed.BedPlants
            .OrderBy(bp => bp.Plant.NameFr)
            .Select(bp => new PlantSummaryDto
            {
                Id = bp.Plant.Id,
                NameFr = bp.Plant.NameFr,
                BinomialName = bp.Plant.BinomialName,
                Emoji = bp.Plant.Emoji,
                Family = bp.Plant.Family
            })
            .ToList()
    };

    private static readonly Func<Bed, BedDto> AsDtoCompiled = AsDto.Compile();

    public static BedDto ToDto(this Bed bed)
    {
        return AsDtoCompiled(bed);
    }
}
