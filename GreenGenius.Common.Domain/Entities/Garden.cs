namespace GreenGenius.Common.Domain.Entities;

public class Garden
{
    public Guid Id { get; init; }

    public Guid OwnerId { get; init; }
    public required string Name { get; init; }
}
