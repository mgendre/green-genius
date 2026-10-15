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
        var beds = app.MapGroup(RouteConstants.Gardens);

        beds.MapGet("/{gardenId:guid}/beds",
                ([FromRoute] Guid gardenId, [FromServices] ListBedsHandler h)
                    => h.Handle(gardenId))
            .WithName("ListBeds")
            .WithTags("Beds")
            .ProducesProblem(StatusCodes.Status404NotFound);

        beds.MapPost("/{gardenId:guid}/beds",
                ([FromBody] CreateBedDto dto, [FromRoute] Guid gardenId,
                    [FromServices] CreateBedHandler h)
                    => h.Handle(dto, gardenId))
            .WithName("CreateBed")
            .WithTags("Beds")
            .ProducesProblem(StatusCodes.Status404NotFound);

        beds.MapPut("/{gardenId:guid}/beds/{id:guid}",
                ([FromRoute] Guid gardenId, [FromRoute] Guid id,
                    [FromBody] UpdateBedDto dto,
                    [FromServices] UpdateBedHandler h)
                    => h.Handle(gardenId, id, dto))
            .WithName("UpdateBed")
            .WithTags("Beds")
            .ProducesProblem(StatusCodes.Status404NotFound);

        beds.MapDelete("/{gardenId:guid}/beds/{id:guid}",
                ([FromRoute] Guid gardenId, [FromRoute] Guid id,
                    [FromServices] DeleteBedHandler h)
                    => h.Handle(gardenId, id))
            .WithName("DeleteBed")
            .WithTags("Beds")
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
