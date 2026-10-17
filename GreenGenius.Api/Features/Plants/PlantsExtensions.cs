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

    public static readonly Expression<Func<PlantAssociation, PlantAssociationDto>> AsAssociationDto =
        association => new PlantAssociationDto
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
            Effect = association.Effect,
            Mechanisms = association.Mechanisms
                .OrderBy(mechanism => mechanism.Mechanism)
                .Select(mechanism => mechanism.Mechanism)
                .ToList(),
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
        FrostToleranceC = plant.FrostToleranceC,
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