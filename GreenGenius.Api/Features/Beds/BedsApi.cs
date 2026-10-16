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
        var beds = app.MapGroup(RouteConstants.Gardens + "/{gardenId:guid}" + RouteConstants.Beds)
            .WithTags("Beds");

        beds.MapGet("/",
                ([FromRoute] Guid gardenId, [FromServices] ListBedsHandler h)
                    => h.Handle(gardenId))
            .WithName("ListBeds")
            .ProducesProblem(StatusCodes.Status404NotFound);

        beds.MapGet("/{id:guid}/associations",
                ([FromRoute] Guid gardenId, [FromRoute] Guid id,
                    [FromServices] ListBedAssociationsHandler h)
                    => h.Handle(gardenId, id))
            .WithName("ListBedAssociations")
            .ProducesProblem(StatusCodes.Status404NotFound);

        beds.MapPost("/",
                ([FromBody] CreateBedDto dto, [FromRoute] Guid gardenId,
                    [FromServices] CreateBedHandler h)
                    => h.Handle(dto, gardenId))
            .WithName("CreateBed")
            .ProducesProblem(StatusCodes.Status404NotFound);

        beds.MapPut("/{id:guid}",
                ([FromRoute] Guid gardenId, [FromRoute] Guid id,
                    [FromBody] UpdateBedDto dto,
                    [FromServices] UpdateBedHandler h)
                    => h.Handle(gardenId, id, dto))
            .WithName("UpdateBed")
            .ProducesProblem(StatusCodes.Status404NotFound);

        beds.MapDelete("/{id:guid}",
                ([FromRoute] Guid gardenId, [FromRoute] Guid id,
                    [FromServices] DeleteBedHandler h)
                    => h.Handle(gardenId, id))
            .WithName("DeleteBed")
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
