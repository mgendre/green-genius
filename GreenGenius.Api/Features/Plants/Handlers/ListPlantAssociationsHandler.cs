using GreenGenius.Api.Features.Plants.Dtos;
using GreenGenius.Common.Data;
using GreenGenius.Common.Data.Exceptions;
using GreenGenius.Infra.Hosting.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace GreenGenius.Api.Features.Plants.Handlers;

public class ListPlantAssociationsHandler(ApplicationDbContext dbContext) : IApiHandler
{
    public async Task<Ok<List<PlantAssociationDto>>> Handle(Guid plantId)
    {
        await ThrowWhenPlantMissing(plantId);

        var associations = await dbContext.PlantAssociations
            .Where(association => association.PlantAId == plantId || association.PlantBId == plantId)
            .OrderBy(association => association.PlantAId == plantId
                ? association.PlantB.NameFr
                : association.PlantA.NameFr)
            .Select(PlantsExtensions.AsAssociationDto)
            .ToListAsync();

        return TypedResults.Ok(associations);
    }

    private async Task ThrowWhenPlantMissing(Guid plantId)
    {
        if (!await dbContext.Plants.AnyAsync(plant => plant.Id == plantId))
        {
            throw new DataNotFoundException($"Plant {plantId} not found");
        }
    }
}