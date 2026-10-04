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
        NameLatin = plant.NameLatin,
        Family = plant.Family.ToString()
    };

    public static readonly Expression<Func<Plant, PlantDto>> AsDto = plant => new PlantDto
    {
        Id = plant.Id,
        NameFr = plant.NameFr,
        NameLatin = plant.NameLatin,
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
            SoilpHMin = plant.Needs.SoilpHMin,
            SoilpHMax = plant.Needs.SoilpHMax,
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
