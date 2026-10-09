using GreenGenius.Common.Data;
using GreenGenius.Common.Data.Entities;
using GreenGenius.Common.Data.Enums;

namespace GreenGenius.App.IntegrationTests.Extensions;

public static class DbContextTestExtensions
{
    public static async Task<Garden> PersistGardenAsync(this ApplicationDbContext dbContext, string name, Guid ownerId)
    {
        var garden = new Garden
        {
            Id = Guid.NewGuid(),
            OwnerId = ownerId,
            Name = name
        };

        dbContext.Add(garden);
        
        await dbContext.SaveChangesAsync();

        return garden;
    }

    public static async Task<Plant> PersistPlantAsync(
        this ApplicationDbContext dbContext,
        string nameFr,
        PlantFamily family = PlantFamily.Solanaceae,
        PlantLifeCycle lifeCycle = PlantLifeCycle.Annual,
        string? binomialName = null)
    {
        var id = Guid.NewGuid();
        var plant = new Plant
        {
            Id = id,
            NameFr = nameFr,
            BinomialName = binomialName,
            Family = family,
            LifeCycle = lifeCycle,
            Needs = new PlantNeeds
            {
                PlantId = id,
                Sunlight = SunlightLevel.Medium,
                WaterNeed = WaterNeedLevel.Medium,
                RootDepth = RootDepthLevel.Medium,
                SoilPhMin = 6.0m,
                SoilPhMax = 7.0m,
                SpacingRowCm = 50,
                SpacingPlantCm = 30,
                HeightCm = 100,
                SpreadCm = 80
            },
            Traits = new PlantTraits
            {
                PlantId = id,
                NitrogenFixer = false,
                DynamicAccumulator = false,
                PollinatorFriendly = false,
                DroughtTolerant = false
            }
        };

        dbContext.Add(plant);

        await dbContext.SaveChangesAsync();

        return plant;
    }
}