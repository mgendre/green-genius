using GreenGenius.Common.Data.Security;

namespace GreenGenius.Common.Data.Entities;

public class Bed : IHasOwner
{
    public Guid Id { get; init; }
    public Guid GardenId { get; init; }
    public Guid OwnerId { get; init; }
    public required string Name { get; set; }

    public List<BedPlant> BedPlants { get; init; } = [];
}
