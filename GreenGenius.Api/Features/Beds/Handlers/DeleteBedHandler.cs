using GreenGenius.Common.Data;
using GreenGenius.Common.Data.Exceptions;
using GreenGenius.Infra.Hosting.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace GreenGenius.Api.Features.Beds.Handlers;

public class DeleteBedHandler(ApplicationDbContext dbContext) : IApiHandler
{
    public async Task<NoContent> Handle(Guid gardenId, Guid id)
    {
        var bed = await dbContext.Beds
            .Where(b => b.Id == id && b.GardenId == gardenId)
            .FirstOrDefaultAsync();

        if (bed is null)
        {
            throw new DataNotFoundException($"Bed {id} not found in garden {gardenId}");
        }

        dbContext.Beds.Remove(bed);

        await dbContext.SaveChangesAsync();

        return TypedResults.NoContent();
    }
}
