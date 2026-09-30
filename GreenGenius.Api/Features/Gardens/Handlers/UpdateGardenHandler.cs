using GreenGenius.Api.Features.Gardens.Dtos;
using GreenGenius.Common.Data;
using GreenGenius.Common.Data.Extensions;
using GreenGenius.Infra.Hosting.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace GreenGenius.Api.Features.Gardens.Handlers;

public class UpdateGardenHandler(ApplicationDbContext dbContext) : IApiHandler
{
    public async Task<Results<Ok<GardenDto>, NotFound>> Handle(Guid id, UpdateGardenDto dto)
    {
        var result = await dbContext.Gardens.GetAsync(id);

        result.Name = dto.Name;
        
        await dbContext.SaveChangesAsync();
        
        return TypedResults.Ok(result.ToDto());
    }
}

public class UpdateGardenDto
{
    public required string Name { get; init; }
}