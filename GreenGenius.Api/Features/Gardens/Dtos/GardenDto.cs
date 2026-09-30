using GreenGenius.Common.Data.Security;

namespace GreenGenius.Api.Features.Gardens.Dtos;

public class GardenDto : IHasOwner
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    
    public Guid OwnerId { get; set; }
}