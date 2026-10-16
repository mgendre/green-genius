using GreenGenius.Common.Data.Enums;
using JetBrains.Annotations;

namespace GreenGenius.Api.Features.Plants.Dtos;

public class PlantAssociationDto
{
    public required PlantSummaryDto Plant { get; init; }
    public required Polarity Polarity { get; init; }
    public required Mechanism Mechanism { get; init; }
    public required Evidence Evidence { get; init; }
    public string? NoteFr { [UsedImplicitly] get; init; }
}
