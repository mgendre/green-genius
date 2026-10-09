using System.Text.Json;
using System.Text.Json.Serialization;
using GreenGenius.Common.Data;
using GreenGenius.Common.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GreenGenius.App.Services;

public sealed class PlantImporter(ApplicationDbContext dbContext, ILogger<PlantImporter> logger, string plantsDirectory = null!)
{
    private readonly string _plantsDirectory = plantsDirectory ?? Path.Combine(AppContext.BaseDirectory, "Data", "Plants");

    public async Task<int> ImportAsync(CancellationToken ct = default)
    {
        var options = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() }
        };

        var files = Directory.GetFiles(_plantsDirectory, "*.json");
        var keys = files.Select(file => Path.GetFileNameWithoutExtension(file)).ToList();

        var plants = await dbContext.Plants
            .Include(plant => plant.Needs)
            .Include(plant => plant.Traits)
            .Where(plant => plant.Key != null && keys.Contains(plant.Key))
            .ToDictionaryAsync(plant => plant.Key!, ct);

        var imported = 0;
        foreach (var file in files)
        {
            var key = Path.GetFileNameWithoutExtension(file);
            var json = await File.ReadAllTextAsync(file, ct);

            PlantJson? plantJson;
            try
            {
                plantJson = JsonSerializer.Deserialize<PlantJson>(json, options);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Plant {Key} failed to import", key);
                continue;
            }

            if (plantJson is null)
            {
                logger.LogWarning("Plant {Key} skipped: empty or invalid JSON", key);
                continue;
            }

            try
            {
                if (ImportOne(dbContext, plants, key, plantJson))
                {
                    imported++;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Plant {Key} failed to import", key);
            }
        }

        await dbContext.SaveChangesAsync(ct);

        return imported;
    }

    private static bool ImportOne(ApplicationDbContext dbContext, Dictionary<string, Plant> plants, string key, PlantJson plantJson)
    {
        if (!plants.TryGetValue(key, out var plant))
        {
            plant = new Plant
            {
                Id = Guid.NewGuid(),
                Key = key,
                NameFr = plantJson.NameFr,
                Needs = new PlantNeeds(),
                Traits = new PlantTraits()
            };
            dbContext.Plants.Add(plant);
        }
        else if (plant.Version >= (plantJson.Version ?? 0))
        {
            return false;
        }

        Map(plant, plantJson);
        return true;
    }

    private static void Map(Plant plant, PlantJson plantJson)
    {
        plant.NameFr = plantJson.NameFr;
        plant.DescriptionFr = plantJson.DescriptionFr;
        plant.BinomialName = plantJson.BinomialName;
        plant.Family = plantJson.Family;
        plant.LifeCycle = plantJson.LifeCycle;
        plant.DaysToMaturity = plantJson.DaysToMaturity;
        plant.TemperatureMinC = plantJson.TemperatureMinC;
        plant.Version = plantJson.Version ?? 0;

        plant.Needs.Sunlight = plantJson.Needs.Sunlight;
        plant.Needs.WaterNeed = plantJson.Needs.WaterNeed;
        plant.Needs.RootDepth = plantJson.Needs.RootDepth;
        plant.Needs.SoilPhMin = plantJson.Needs.SoilPhMin;
        plant.Needs.SoilPhMax = plantJson.Needs.SoilPhMax;
        plant.Needs.SpacingRowCm = plantJson.Needs.SpacingRowCm;
        plant.Needs.SpacingPlantCm = plantJson.Needs.SpacingPlantCm;
        plant.Needs.HeightCm = plantJson.Needs.HeightCm;
        plant.Needs.SpreadCm = plantJson.Needs.SpreadCm;

        plant.Traits.NitrogenFixer = plantJson.Traits.NitrogenFixer;
        plant.Traits.DynamicAccumulator = plantJson.Traits.DynamicAccumulator;
        plant.Traits.PollinatorFriendly = plantJson.Traits.PollinatorFriendly;
        plant.Traits.DroughtTolerant = plantJson.Traits.DroughtTolerant;
    }
}
