using GreenGenius.Api.Features.Plants.Dtos;
using GreenGenius.Common.Data;
using GreenGenius.Common.Data.Exceptions;
using GreenGenius.Infra.Hosting.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace GreenGenius.Api.Features.Plants.Handlers;

public class GetPlantHandler(ApplicationDbContext dbContext) : IApiHandler
{
    public async Task<Ok<PlantDto>> Handle(Guid id)
    {
        var plant = await dbContext.Plants
            .Where(p => p.Id == id)
            .Select(PlantsExtensions.AsDto)
            .FirstOrDefaultAsync();

        return plant is null ? throw new DataNotFoundException($"Plant not found") : TypedResults.Ok(plant);
    }
}
