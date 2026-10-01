using GreenGenius.Common.Data.Security;

namespace GreenGenius.Common.Data.Entities;

public class Garden : IHasOwner
{
    public Guid Id { get; init; }

    public Guid OwnerId { get; init; }
    public required string Name { get; set; }
}
