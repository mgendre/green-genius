using JetBrains.Annotations;

namespace GreenGenius.Api.Features.Plants.Dtos;

public class PlantSummaryDto
{
    public Guid Id { get; init; }
    public required string NameFr { get; init; }
    public string? BinomialName { [UsedImplicitly] get; init; }
    public required string Family { get; init; }
}
