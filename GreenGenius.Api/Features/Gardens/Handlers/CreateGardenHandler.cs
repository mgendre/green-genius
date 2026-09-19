using GreenGenius.Api.Services.Interfaces;
using GreenGenius.Common.Domain;
using GreenGenius.Common.Domain.Entities;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace GreenGenius.Api.Features.Gardens.Handlers;

public class CreateGardenHandler(ApplicationDbContext dbContext, ICurrentUserService currentUser)
{
    public async Task<IResult> CreateAsync(CreateGardenDto dto)
    {
        await dbContext.Gardens.AddAsync(new Garden
        {
            Id = Guid.NewGuid(),
            OwnerId = currentUser.GetCurrentUserId(),
            Name = dto.Name
        });
        
        await dbContext.SaveChangesAsync();
        
        return Results.Created();
    }
    
    [UsedImplicitly]
    public class CreateGardenDto
    {
        public required string Name { get; set; }
    }
}
