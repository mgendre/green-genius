using GreenGenius.Common.Data.Enums;
using JetBrains.Annotations;

namespace GreenGenius.Api.Features.Plants.Dtos;

public class PlantAssociationDto
{
    public required PlantSummaryDto PlantA { get; init; }
    public required PlantSummaryDto PlantB { get; init; }
    public required Effect Effect { get; init; }
    public required List<Mechanism> Mechanisms { get; init; }
    public required Evidence Evidence { get; init; }
    public string? NoteFr { [UsedImplicitly] get; init; }
}