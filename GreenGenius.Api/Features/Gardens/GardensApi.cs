using GreenGenius.Api.Constants;
using GreenGenius.Api.Features.Gardens.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace GreenGenius.Api.Features.Gardens;

public static class GardensApi
{
    public static void WithGardensApi(this IEndpointRouteBuilder app)
    {
        var gardens = app.MapGroup(RouteConstants.Gardens);
        
        gardens.MapPost("/", ([FromBody] CreateGardenDto dto,
            [FromServices] CreateGardenHandler h) => h.Handle(dto))
            .WithName("CreateGarden");
        
        gardens.MapGet("/", (
            [FromServices] ListGardensHandler h) => h.Handle())
            .WithName("ListGardens");
        
        gardens.MapPut("/{id:guid}", ([FromRoute] Guid id, [FromBody] UpdateGardenDto dto, 
            [FromServices] UpdateGardenHandler h) => h.Handle(id, dto))
            .WithName("UpdateGarden")
            .ProducesProblem(StatusCodes.Status404NotFound);
        
        gardens.MapDelete("/{id:guid}", ([FromRoute] Guid id, 
            [FromServices] DeleteGardenHandler h) => h.Handle(id))
            .WithName("DeleteGarden")
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
