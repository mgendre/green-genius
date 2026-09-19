using GreenGenius.Api.Features.Gardens.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace GreenGenius.Api.Features.Gardens;

public static class GardensApi
{
    public const string BasePattern = "/gardens";
    public static void WithGardensApi(this IEndpointRouteBuilder app)
    {
        app.MapPost(BasePattern, (CreateGardenHandler.CreateGardenDto dto, CreateGardenHandler handler) 
            => handler.CreateAsync(dto));
    }
}
