using GreenGenius.Common.Data;
using GreenGenius.Common.Data.Extensions;
using GreenGenius.Infra.Hosting.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace GreenGenius.Api.Features.Gardens.Handlers;

public class DeleteGardenHandler(ApplicationDbContext dbContext) : IApiHandler
{
    public async Task<Results<NoContent, NotFound>> Handle(Guid id)
    {
        var gardenToDelete = await dbContext.Gardens.GetAsync(id);

        dbContext.Gardens.Remove(gardenToDelete);

        await dbContext.SaveChangesAsync();

        return TypedResults.NoContent();
    }
}
