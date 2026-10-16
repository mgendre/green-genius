using System.Linq.Expressions;
using GreenGenius.Api.Features.Plants.Dtos;
using GreenGenius.Common.Data.Entities;

namespace GreenGenius.Api.Features.Plants;

public static class PlantsExtensions
{
    public static readonly Expression<Func<Plant, PlantSummaryDto>> AsSummaryDto = plant => new PlantSummaryDto
    {
        Id = plant.Id,
        NameFr = plant.NameFr,
        BinomialName = plant.BinomialName,
        Emoji = plant.Emoji,
        Family = plant.Family
    };

    public static Expression<Func<PlantAssociation, PlantAssociationDto>> AsAssociationDto(Guid plantId) =>
        association => new PlantAssociationDto
        {
            Plant = new PlantSummaryDto
            {
                Id = association.PlantAId == plantId ? association.PlantB.Id : association.PlantA.Id,
                NameFr = association.PlantAId == plantId ? association.PlantB.NameFr : association.PlantA.NameFr,
                BinomialName = association.PlantAId == plantId ? association.PlantB.BinomialName : association.PlantA.BinomialName,
                Emoji = association.PlantAId == plantId ? association.PlantB.Emoji : association.PlantA.Emoji,
                Family = association.PlantAId == plantId ? association.PlantB.Family : association.PlantA.Family
            },
            Polarity = association.Polarity,
            Mechanism = association.Mechanism,
            Evidence = association.Evidence,
            NoteFr = association.NoteFr
        };

    public static readonly Expression<Func<Plant, PlantDto>> AsDto = plant => new PlantDto
    {
        Id = plant.Id,
        NameFr = plant.NameFr,
        BinomialName = plant.BinomialName,
        Emoji = plant.Emoji,
        DescriptionFr = plant.DescriptionFr,
        Family = plant.Family,
        LifeCycle = plant.LifeCycle,
        DaysToMaturity = plant.DaysToMaturity,
        TemperatureMinC = plant.TemperatureMinC,
        Needs = new PlantNeedsDto
        {
            Sunlight = plant.Needs.Sunlight,
            WaterNeed = plant.Needs.WaterNeed,
            RootDepth = plant.Needs.RootDepth,
            SoilPhMin = plant.Needs.SoilPhMin,
            SoilPhMax = plant.Needs.SoilPhMax,
            SpacingRowCm = plant.Needs.SpacingRowCm,
            SpacingPlantCm = plant.Needs.SpacingPlantCm,
            HeightCm = plant.Needs.HeightCm,
            SpreadCm = plant.Needs.SpreadCm
        },
        Traits = new PlantTraitsDto
        {
            NitrogenFixer = plant.Traits.NitrogenFixer,
            DynamicAccumulator = plant.Traits.DynamicAccumulator,
            PollinatorFriendly = plant.Traits.PollinatorFriendly,
            DroughtTolerant = plant.Traits.DroughtTolerant
        }
    };
}
