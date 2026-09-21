using GreenGenius.Common.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace GreenGenius.Api.Features.Gardens.Handlers;

public class ListGardensHandler(ApplicationDbContext dbContext)
{
    public async Task<IResult> Handle()
    {
        var result = await dbContext.Gardens.OrderBy(garden => garden.Name).ToListAsync();
        
        return Results.Ok(result.Select(g => g.ToDto()));
    }
}
