using GreenGenius.Api.Features.Beds.Dtos;
using GreenGenius.Common.Data;
using GreenGenius.Common.Data.Entities;
using GreenGenius.Common.Data.Extensions;
using GreenGenius.Common.Data.Security;
using GreenGenius.Infra.Hosting.Handlers;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace GreenGenius.Api.Features.Beds.Handlers;

public class CreateBedHandler(ApplicationDbContext dbContext, ICurrentUserService currentUser) : IApiHandler
{
    public async Task<Created<BedDto>> Handle(CreateBedDto dto)
    {
        await dbContext.Gardens.GetAsync(dto.GardenId);

        var bed = new Bed
        {
            Id = Guid.NewGuid(),
            GardenId = dto.GardenId,
            OwnerId = currentUser.GetCurrentUserId(),
            Name = dto.Name
        };

        dbContext.Beds.Add(bed);

        await dbContext.SaveChangesAsync();

        return TypedResults.Created((string?)null, bed.ToDto());
    }
}

[UsedImplicitly]
public class CreateBedDto
{
    public required Guid GardenId { get; init; }
    public required string Name { get; init; } = "";
}
