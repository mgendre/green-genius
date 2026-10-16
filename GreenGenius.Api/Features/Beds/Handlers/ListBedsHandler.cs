using GreenGenius.Api.Features.Beds.Dtos;
using GreenGenius.Common.Data;
using GreenGenius.Common.Data.Extensions;
using GreenGenius.Infra.Hosting.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace GreenGenius.Api.Features.Beds.Handlers;

public class ListBedsHandler(ApplicationDbContext dbContext) : IApiHandler
{
    public async Task<Ok<List<BedDto>>> Handle(Guid gardenId)
    {
        await dbContext.Gardens.GetAsync(gardenId);

        var beds = await dbContext.Beds
            .Where(b => b.GardenId == gardenId)
            .OrderBy(b => b.Name)
            .Select(BedsExtensions.AsDto)
            .ToListAsync();

        return TypedResults.Ok(beds);
    }
}
