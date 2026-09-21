using GreenGenius.Common.Domain.Security;

namespace GreenGenius.Api.Features.Gardens.Dtos;

public class GardenDto : IHasOwner
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    
    public Guid OwnerId { get; set; }
}