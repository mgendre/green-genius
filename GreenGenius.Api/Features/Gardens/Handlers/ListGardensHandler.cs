using GreenGenius.Api.Features.Gardens.Dtos;
using GreenGenius.Common.Data;
using GreenGenius.Infra.Hosting.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace GreenGenius.Api.Features.Gardens.Handlers;

public class ListGardensHandler(ApplicationDbContext dbContext) : IApiHandler
{
    public async Task<Ok<IEnumerable<GardenDto>>> Handle()
    {
        var result = await dbContext.Gardens.OrderBy(garden => garden.Name).ToListAsync();
        
        return TypedResults.Ok(result.Select(g => g.ToDto()));
    }
}
