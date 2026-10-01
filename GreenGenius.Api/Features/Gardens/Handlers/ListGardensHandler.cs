using GreenGenius.Api.Features.Gardens.Dtos;
using GreenGenius.Common.Data;
using GreenGenius.Infra.Hosting.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace GreenGenius.Api.Features.Gardens.Handlers;

public class ListGardensHandler(ApplicationDbContext dbContext) : IApiHandler
{
    public async Task<Ok<List<GardenDto>>> Handle()
    {
        var gardens = await dbContext.Gardens
            .OrderBy(garden => garden.Name)
            .Select(GardensExtensions.AsDto)
            .ToListAsync();

        return TypedResults.Ok(gardens);
    }
}
