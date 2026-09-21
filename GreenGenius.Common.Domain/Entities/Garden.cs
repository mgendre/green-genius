using GreenGenius.Common.Domain.Security;

namespace GreenGenius.Common.Domain.Entities;

public class Garden : IHasOwner
{
    public Guid Id { get; init; }

    public Guid OwnerId { get; init; }
    public required string Name { get; init; }
}
