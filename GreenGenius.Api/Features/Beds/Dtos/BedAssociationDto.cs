using GreenGenius.Api.Features.Plants.Dtos;
using GreenGenius.Common.Data.Enums;
using JetBrains.Annotations;

namespace GreenGenius.Api.Features.Beds.Dtos;

public class BedAssociationDto
{
    public required PlantSummaryDto PlantA { get; init; }
    public required PlantSummaryDto PlantB { get; init; }
    public required Polarity Polarity { get; init; }
    public required Mechanism Mechanism { get; init; }
    public required Evidence Evidence { get; init; }
    public string? NoteFr { [UsedImplicitly] get; init; }
}
