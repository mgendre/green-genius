using GreenGenius.Common.Data;
using GreenGenius.Common.Data.Entities;
using GreenGenius.Common.Data.Enums;

namespace GreenGenius.App.IntegrationTests.Extensions;

public static class DbContextTestExtensions
{
    extension(ApplicationDbContext dbContext)
    {
        public async Task<Garden> PersistGardenAsync(Guid ownerId, string name = "MyGarden")
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

        public async Task<Bed> PersistBedAsync(Guid gardenId, Guid ownerId, string name = "MyBed")
        {
            var bed = new Bed
            {
                Id = Guid.NewGuid(),
                GardenId = gardenId,
                OwnerId = ownerId,
                Name = name
            };

            dbContext.Add(bed);

            await dbContext.SaveChangesAsync();

            return bed;
        }

        public async Task LinkBedPlantAsync(Guid bedId, Plant? plant = null)
        {
            var linkedPlant = plant ?? await dbContext.PersistPlantAsync();
            var bedPlant = new BedPlant
            {
                BedId = bedId,
                PlantId = linkedPlant.Id
            };

            dbContext.Add(bedPlant);

            await dbContext.SaveChangesAsync();
        }

        public async Task PersistPlantAssociationAsync(Plant plantA,
            Plant plantB,
            Effect effect = Effect.Beneficial,
            Mechanism[]? mechanisms = null,
            Evidence evidence = Evidence.Established,
            string? noteFr = null)
        {
            var id = Guid.NewGuid();
            var association = new PlantAssociation
            {
                Id = id,
                PlantAId = plantA.Id,
                PlantBId = plantB.Id,
                Effect = effect,
                Evidence = evidence,
                NoteFr = noteFr,
                Mechanisms =
                [
                    .. (mechanisms ?? [Mechanism.PestDeterrent])
                    .Select(mechanism => new PlantAssociationMechanism
                    {
                        PlantAssociationId = id,
                        Mechanism = mechanism
                    })
                ]
            };

            dbContext.Add(association);

            await dbContext.SaveChangesAsync();
        }

        public async Task<Plant> PersistPlantAsync(string nameFr = "Tomate",
            PlantFamily family = PlantFamily.Solanaceae,
            PlantLifeCycle lifeCycle = PlantLifeCycle.Annual,
            string? binomialName = null,
            int? frostToleranceC = null)
        {
            var id = Guid.NewGuid();
            var plant = new Plant
            {
                Id = id,
                NameFr = nameFr,
                BinomialName = binomialName,
                Family = family,
                LifeCycle = lifeCycle,
                FrostToleranceC = frostToleranceC,
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