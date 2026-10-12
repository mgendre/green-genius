using GreenGenius.Api.Constants;
using GreenGenius.Api.Features.Beds.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace GreenGenius.Api.Features.Beds;

public static class BedsApi
{
    public static void WithBedsApi(this IEndpointRouteBuilder app)
    {
        var beds = app.MapGroup(RouteConstants.Beds)
            .WithTags("Beds");

        beds.MapGet("/", ([FromQuery] Guid gardenId, 
            [FromServices] ListBedsHandler h) => h.Handle(gardenId))
            .WithName("ListBeds");

        beds.MapPost("/", ([FromBody] CreateBedDto dto, 
            [FromServices] CreateBedHandler h) => h.Handle(dto))
            .WithName("CreateBed")
            .ProducesProblem(StatusCodes.Status404NotFound);

        beds.MapPut("/{id:guid}", ([FromRoute] Guid id, [FromBody] UpdateBedDto dto, 
            [FromServices] UpdateBedHandler h) => h.Handle(id, dto))
            .WithName("UpdateBed")
            .ProducesProblem(StatusCodes.Status404NotFound);

        beds.MapDelete("/{id:guid}", ([FromRoute] Guid id, 
            [FromServices] DeleteBedHandler h) => h.Handle(id))
            .WithName("DeleteBed")
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
