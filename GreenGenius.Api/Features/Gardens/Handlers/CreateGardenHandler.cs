using GreenGenius.Api.Constants;
using GreenGenius.Common.Domain;
using GreenGenius.Common.Domain.Entities;
using GreenGenius.Common.Domain.Services.Interfaces;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace GreenGenius.Api.Features.Gardens.Handlers;

public class CreateGardenHandler(ApplicationDbContext dbContext, ICurrentUserService currentUser)
{
    public async Task<IResult> Handle(CreateGardenDto dto)
    {
        var garden = new Garden
        {
            Id = Guid.NewGuid(),
            OwnerId = currentUser.GetCurrentUserId(),
            Name = dto.Name
        };
        await dbContext.Gardens.AddAsync(garden);
        
        await dbContext.SaveChangesAsync();
        
        return Results.Created(RouteConstants.Gardens + "/" + garden.Id, garden.ToDto());
    }
}

[UsedImplicitly]
public class CreateGardenDto
{
    public required string Name { get; set; }
}