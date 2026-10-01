using GreenGenius.Api.Features.Gardens.Dtos;
using GreenGenius.Common.Data;
using GreenGenius.Common.Data.Entities;
using GreenGenius.Common.Data.Security;
using GreenGenius.Infra.Hosting.Handlers;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace GreenGenius.Api.Features.Gardens.Handlers;

public class CreateGardenHandler(ApplicationDbContext dbContext, ICurrentUserService currentUser) : IApiHandler
{
    public async Task<Created<GardenDto>> Handle(CreateGardenDto dto)
    {
        var garden = new Garden
        {
            Id = Guid.NewGuid(),
            OwnerId = currentUser.GetCurrentUserId(),
            Name = dto.Name
        };

        dbContext.Gardens.Add(garden);

        await dbContext.SaveChangesAsync();

        return TypedResults.Created((string?)null, garden.ToDto());
    }
}

[UsedImplicitly]
public class CreateGardenDto
{
    public required string Name { get; init; }
}