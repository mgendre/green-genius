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

    public static readonly Expression<Func<PlantAssociation, BedAssociationDto>> AsAssociationDto =
        association => new BedAssociationDto
        {
            PlantA = new PlantSummaryDto
            {
                Id = association.PlantA.Id,
                NameFr = association.PlantA.NameFr,
                BinomialName = association.PlantA.BinomialName,
                Emoji = association.PlantA.Emoji,
                Family = association.PlantA.Family
            },
            PlantB = new PlantSummaryDto
            {
                Id = association.PlantB.Id,
                NameFr = association.PlantB.NameFr,
                BinomialName = association.PlantB.BinomialName,
                Emoji = association.PlantB.Emoji,
                Family = association.PlantB.Family
            },
            Polarity = association.Polarity,
            Mechanism = association.Mechanism,
            Evidence = association.Evidence,
            NoteFr = association.NoteFr
        };

    private static readonly Func<Bed, BedDto> AsDtoCompiled = AsDto.Compile();

    public static BedDto ToDto(this Bed bed)
    {
        return AsDtoCompiled(bed);
    }
}
