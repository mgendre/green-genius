using GreenGenius.Api.Features.Plants;
using GreenGenius.Api.Features.Plants.Dtos;
using GreenGenius.Common.Data;
using GreenGenius.Common.Data.Enums;
using GreenGenius.Infra.Hosting.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace GreenGenius.Api.Features.Beds.Handlers;

public class ListBedAssociationsHandler(ApplicationDbContext dbContext) : IApiHandler
{
    public async Task<Ok<List<PlantAssociationDto>>> Handle(Guid gardenId, Guid bedId)
    {
        await dbContext.Beds.ThrowWhenBedMissingAsync(gardenId, bedId);

        var associations = await dbContext.PlantAssociations
            .Where(association =>
                dbContext.BedPlants.Any(link => link.BedId == bedId && link.PlantId == association.PlantAId)
                && dbContext.BedPlants.Any(link => link.BedId == bedId && link.PlantId == association.PlantBId))
            .OrderBy(association => association.Effect != Effect.Antagonistic)
            .ThenBy(association => association.PlantA.NameFr)
            .ThenBy(association => association.PlantB.NameFr)
            .Select(PlantsExtensions.AsAssociationDto)
            .ToListAsync();

        return TypedResults.Ok(associations);
    }
}