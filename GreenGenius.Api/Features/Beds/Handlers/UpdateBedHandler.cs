using GreenGenius.Api.Features.Beds.Dtos;
using GreenGenius.Common.Data;
using GreenGenius.Common.Data.Entities;
using GreenGenius.Common.Data.Exceptions;
using GreenGenius.Infra.Hosting.Handlers;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace GreenGenius.Api.Features.Beds.Handlers;

public class UpdateBedHandler(ApplicationDbContext dbContext) : IApiHandler
{
    public async Task<Ok<BedDto>> Handle(Guid gardenId, Guid id, UpdateBedDto dto)
    {
        var bed = await dbContext.Beds.GetBedWithPlantsAsync(gardenId, id);

        bed.Name = dto.Name;

        var plantIds = dto.PlantIds.Distinct().ToArray();

        await EnsurePlantsExistAsync(plantIds);
        ApplyPlants(bed, plantIds);

        await dbContext.SaveChangesAsync();

        return TypedResults.Ok(await GetBedDtoAsync(gardenId, id));
    }

    private async Task EnsurePlantsExistAsync(Guid[] plantIds)
    {
        if (plantIds.Length == 0)
        {
            return;
        }

        var foundIds = await dbContext.Plants
            .Where(p => plantIds.Contains(p.Id))
            .Select(p => p.Id)
            .ToListAsync();

        var missingPlantId = plantIds.FirstOrDefault(plantId => !foundIds.Contains(plantId));

        if (missingPlantId != Guid.Empty)
        {
            throw new DataNotFoundException($"Plant not found: {missingPlantId}");
        }
    }

    private static void ApplyPlants(Bed bed, Guid[] plantIds)
    {
        var requestedPlantIds = plantIds.ToHashSet();
        var linkedPlantIds = bed.BedPlants.Select(link => link.PlantId).ToHashSet();

        foreach (var link in bed.BedPlants.Where(link => !requestedPlantIds.Contains(link.PlantId)).ToList())
        {
            bed.BedPlants.Remove(link);
        }

        foreach (var plantId in plantIds.Where(plantId => !linkedPlantIds.Contains(plantId)))
        {
            bed.BedPlants.Add(new BedPlant { BedId = bed.Id, PlantId = plantId });
        }
    }

    private async Task<BedDto> GetBedDtoAsync(Guid gardenId, Guid bedId)
    {
        var bed = await dbContext.Beds
            .Where(b => b.Id == bedId && b.GardenId == gardenId)
            .Select(BedsExtensions.AsDto)
            .FirstOrDefaultAsync();

        return bed!;
    }
}

[UsedImplicitly]
public class UpdateBedDto
{
    public required string Name { get; init; } = "";
    public required Guid[] PlantIds { get; init; } = [];
}