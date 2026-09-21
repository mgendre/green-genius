using GreenGenius.Api.Constants;
using GreenGenius.Api.Features.Gardens.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace GreenGenius.Api.Features.Gardens;

public static class GardensApi
{
    public static void WithGardensApi(this IEndpointRouteBuilder app)
    {
        app.MapPost(RouteConstants.Gardens, (CreateGardenDto dto, CreateGardenHandler h) => h.Handle(dto));
        app.MapGet(RouteConstants.Gardens, (ListGardensHandler h) => h.Handle());
    }
}
