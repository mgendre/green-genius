using JetBrains.Annotations;

namespace GreenGenius.Api.Features.Plants.Dtos;

public class PlantTraitsDto
{
    [UsedImplicitly] public required bool NitrogenFixer { get; set; }

    [UsedImplicitly] public required bool DynamicAccumulator { get; set; }

    [UsedImplicitly] public required bool PollinatorFriendly { get; set; }

    [UsedImplicitly] public required bool DroughtTolerant { get; set; }
}