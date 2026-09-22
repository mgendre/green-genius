using GreenGenius.Api.Constants;
using GreenGenius.Api.Features.Gardens.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace GreenGenius.Api.Features.Gardens;

public static class GardensApi
{
    public static void WithGardensApi(this IEndpointRouteBuilder app)
    {
        app.MapPost(RouteConstants.Gardens, (CreateGardenDto dto, [FromServices] CreateGardenHandler h) => h.Handle(dto));
        app.MapGet(RouteConstants.Gardens, ([FromServices] ListGardensHandler h) => h.Handle());
        app.MapPut(RouteConstants.Gardens + "/{id:guid}", ([FromRoute] Guid id, [FromBody] UpdateGardenDto dto, 
            [FromServices] UpdateGardenHandler h) => h.Handle(id, dto));
    }
}
