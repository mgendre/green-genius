using GreenGenius.Common.Domain;
using GreenGenius.Infra.Database.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace GreenGenius.Api.Features.Gardens.Handlers;

public class UpdateGardenHandler(ApplicationDbContext dbContext)
{
    public async Task<IResult> Handle(Guid id, UpdateGardenDto dto)
    {
        var result = await dbContext.Gardens.GetAsync(id);

        result.Name = dto.Name;
        
        await dbContext.SaveChangesAsync();
        
        return Results.Ok(result.ToDto());
    }
}

public class UpdateGardenDto
{
    public required string Name { get; init; }
}