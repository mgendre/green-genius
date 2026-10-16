using GreenGenius.Common.Data;
using GreenGenius.Common.Data.Entities;
using GreenGenius.Common.Data.Enums;

namespace GreenGenius.App.IntegrationTests.Extensions;

public static class DbContextTestExtensions
{
    extension(ApplicationDbContext dbContext)
    {
        public async Task<Garden> PersistGardenAsync(string name, Guid ownerId)
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

        public async Task<Bed> PersistBedAsync(Guid id,
            Guid gardenId,
            string name,
            Guid ownerId)
        {
            var bed = new Bed
            {
                Id = id,
                GardenId = gardenId,
                OwnerId = ownerId,
                Name = name
            };

            dbContext.Add(bed);

            await dbContext.SaveChangesAsync();

            return bed;
        }

        public async Task LinkBedPlantAsync(Guid bedId,
            Plant plant)
        {
            var bedPlant = new BedPlant
            {
                BedId = bedId,
                PlantId = plant.Id
            };

            dbContext.Add(bedPlant);

            await dbContext.SaveChangesAsync();
        }

        public async Task PersistPlantAssociationAsync(Plant plantA,
            Plant plantB,
            Polarity polarity = Polarity.Beneficial,
            Mechanism mechanism = Mechanism.PestDeterrent,
            Evidence evidence = Evidence.Established,
            string? noteFr = null)
        {
            var association = new PlantAssociation
            {
                Id = Guid.NewGuid(),
                PlantAId = plantA.Id,
                PlantBId = plantB.Id,
                Polarity = polarity,
                Mechanism = mechanism,
                Evidence = evidence,
                NoteFr = noteFr
            };

            dbContext.Add(association);

            await dbContext.SaveChangesAsync();
        }

        public async Task<Plant> PersistPlantAsync(string nameFr,
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
}