using GreenGenius.Common.Data.Enums;
using JetBrains.Annotations;

namespace GreenGenius.Api.Features.Plants.Dtos;

public class PlantSummaryDto
{
    public required Guid Id { get; init; }
    public required string NameFr { get; init; }
    public string? BinomialName { [UsedImplicitly] get; init; }
    public string? Emoji { [UsedImplicitly] get; init; }
    public required PlantFamily Family { get; init; }
}