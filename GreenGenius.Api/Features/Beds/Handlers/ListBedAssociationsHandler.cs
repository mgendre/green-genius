using GreenGenius.Api.Features.Beds.Dtos;
using GreenGenius.Common.Data;
using GreenGenius.Common.Data.Enums;
using GreenGenius.Common.Data.Exceptions;
using GreenGenius.Common.Data.Extensions;
using GreenGenius.Infra.Hosting.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace GreenGenius.Api.Features.Beds.Handlers;

public class ListBedAssociationsHandler(ApplicationDbContext dbContext) : IApiHandler
{
    public async Task<Ok<List<BedAssociationDto>>> Handle(Guid gardenId, Guid bedId)
    {
        await ThrowWhenBedMissing(gardenId, bedId);

        var associations = await dbContext.PlantAssociations
            .Where(association =>
                dbContext.BedPlants.Any(link => link.BedId == bedId && link.PlantId == association.PlantAId)
                && dbContext.BedPlants.Any(link => link.BedId == bedId && link.PlantId == association.PlantBId))
            .OrderBy(association => association.Polarity != Polarity.Antagonistic)
            .ThenBy(association => association.PlantA.NameFr)
            .ThenBy(association => association.PlantB.NameFr)
            .Select(BedsExtensions.AsAssociationDto)
            .ToListAsync();

        return TypedResults.Ok(associations);
    }

    private async Task ThrowWhenBedMissing(Guid gardenId, Guid bedId)
    {
        await dbContext.Gardens.GetAsync(gardenId);

        if (!await dbContext.Beds.AnyAsync(bed => bed.Id == bedId && bed.GardenId == gardenId))
        {
            throw new DataNotFoundException($"Bed {bedId} not found in garden {gardenId}");
        }
    }
}
