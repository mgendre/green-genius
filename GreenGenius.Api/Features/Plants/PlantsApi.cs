using GreenGenius.Api.Constants;
using GreenGenius.Api.Features.Plants.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace GreenGenius.Api.Features.Plants;

public static class PlantsApi
{
    public static void WithPlantsApi(this IEndpointRouteBuilder app)
    {
        var plants = app.MapGroup(RouteConstants.Plants)
            .WithTags("Plants");

        plants.MapGet("/", (
            [FromServices] ListPlantsHandler h) => h.Handle())
            .WithName("ListPlants");

        plants.MapGet("/{id:guid}", ([FromRoute] Guid id,
            [FromServices] GetPlantHandler h) => h.Handle(id))
            .WithName("GetPlant")
            .ProducesProblem(StatusCodes.Status404NotFound);

        plants.MapGet("/{id:guid}/associations", ([FromRoute] Guid id,
            [FromServices] ListPlantAssociationsHandler h) => h.Handle(id))
            .WithName("ListPlantAssociations")
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
