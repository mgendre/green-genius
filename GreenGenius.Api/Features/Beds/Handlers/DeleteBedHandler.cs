using GreenGenius.Common.Data;
using GreenGenius.Infra.Hosting.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace GreenGenius.Api.Features.Beds.Handlers;

public class DeleteBedHandler(ApplicationDbContext dbContext) : IApiHandler
{
    public async Task<NoContent> Handle(Guid gardenId, Guid id)
    {
        var bed = await dbContext.Beds.GetBedWithoutPlantsAsync(gardenId, id);

        dbContext.Beds.Remove(bed);

        await dbContext.SaveChangesAsync();

        return TypedResults.NoContent();
    }
}