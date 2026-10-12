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
    public async Task<Ok<BedDto>> Handle(Guid id, UpdateBedDto dto)
    {
        var bed = await dbContext.Beds.GetAsync(id);

        bed.Name = dto.Name;

        var uniquePlantIds = dto.PlantIds.Distinct().ToArray();

        var existingPlantIds = await dbContext.Plants
            .Where(p => uniquePlantIds.Contains(p.Id))
            .Select(p => p.Id)
            .ToListAsync();

        if (existingPlantIds.Count != uniquePlantIds.Length)
        {
            var missingPlantId = uniquePlantIds.First(pid => !existingPlantIds.Contains(pid));
            throw new DataNotFoundException($"Plant not found: {missingPlantId}");
        }

        var existingLinks = dbContext.BedPlants
            .Where(bp => bp.BedId == id)
            .ToList();

        foreach (var link in existingLinks)
        {
            dbContext.BedPlants.Remove(link);
        }

        foreach (var plantId in uniquePlantIds)
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
