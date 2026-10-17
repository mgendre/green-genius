using GreenGenius.Api.Features.Plants.Dtos;
using JetBrains.Annotations;

namespace GreenGenius.Api.Features.Beds.Dtos;

public class BedDto
{
    public required Guid Id { get; [UsedImplicitly] set; }
    public required string Name { get; [UsedImplicitly] set; }
    public required Guid GardenId { get; [UsedImplicitly] set; }
    public required List<PlantSummaryDto> Plants { get; [UsedImplicitly] set; } = [];
}