using GreenGenius.Api.Features.Plants.Dtos;
using GreenGenius.Common.Data;
using GreenGenius.Infra.Hosting.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace GreenGenius.Api.Features.Plants.Handlers;

public class ListPlantsHandler(ApplicationDbContext dbContext) : IApiHandler
{
    public async Task<Ok<List<PlantSummaryDto>>> Handle()
    {
        var plants = await dbContext.Plants
            .OrderBy(plant => plant.NameFr)
            .Select(PlantsExtensions.AsSummaryDto)
            .ToListAsync();

        return TypedResults.Ok(plants);
    }
}
