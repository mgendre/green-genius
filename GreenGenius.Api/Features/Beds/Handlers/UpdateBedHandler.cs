using GreenGenius.Api.Features.Beds.Dtos;
using GreenGenius.Common.Data;
using GreenGenius.Common.Data.Entities;
using GreenGenius.Common.Data.Extensions;
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
        var bed = await dbContext.Beds
            .Where(b => b.Id == id && b.GardenId == gardenId)
            .FirstOrDefaultAsync();

        if (bed is null)
        {
            throw new DataNotFoundException($"Bed {id} not found in garden {gardenId}");
        }

        bed.Name = dto.Name;

        var uniquePlantIds = dto.PlantIds.Distinct().ToHashSet();

        var existingPlantIds = await dbContext.Plants
            .Where(p => uniquePlantIds.Contains(p.Id))
            .Select(p => p.Id)
            .ToListAsync();

        if (existingPlantIds.Count != uniquePlantIds.Count)
        {
            var missingPlantId = uniquePlantIds.First(pid => !existingPlantIds.Contains(pid));
            throw new DataNotFoundException($"Plant not found: {missingPlantId}");
        }

        var existingPlantIdsOnBed = await dbContext.BedPlants
            .Where(bp => bp.BedId == id)
            .Select(bp => bp.PlantId)
            .ToListAsync();

        var existingSet = new HashSet<Guid>(existingPlantIdsOnBed);

        var plantIdsToRemove = existingPlantIdsOnBed.Where(pid => !uniquePlantIds.Contains(pid)).ToList();
        var plantIdsToAdd = uniquePlantIds.Where(pid => !existingSet.Contains(pid)).ToArray();

        foreach (var plantId in plantIdsToRemove)
        {
            var link = dbContext.BedPlants.Single(bp => bp.BedId == id && bp.PlantId == plantId);
            dbContext.BedPlants.Remove(link);
        }

        foreach (var plantId in plantIdsToAdd)
        {
            dbContext.BedPlants.Add(new BedPlant { BedId = id, PlantId = plantId });
        }

        await dbContext.SaveChangesAsync();

        var updatedBed = await dbContext.Beds
            .Include("BedPlants.Plant")
            .AsNoTracking()
            .SingleAsync(b => b.Id == id);

        return TypedResults.Ok(updatedBed.ToDto());
    }
}

[UsedImplicitly]
public class UpdateBedDto
{
    public required string Name { get; init; } = "";
    public required Guid[] PlantIds { get; init; } = [];
}
