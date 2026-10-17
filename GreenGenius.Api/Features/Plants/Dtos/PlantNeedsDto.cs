using GreenGenius.Common.Data.Enums;
using JetBrains.Annotations;

namespace GreenGenius.Api.Features.Plants.Dtos;

public class PlantNeedsDto
{
    [UsedImplicitly] public required SunlightLevel Sunlight { get; set; }

    [UsedImplicitly] public required WaterNeedLevel WaterNeed { get; set; }

    [UsedImplicitly] public required RootDepthLevel RootDepth { get; set; }

    [UsedImplicitly] public decimal? SoilPhMin { get; set; }

    [UsedImplicitly] public decimal? SoilPhMax { get; set; }

    [UsedImplicitly] public required int SpacingRowCm { get; set; }

    [UsedImplicitly] public required int SpacingPlantCm { get; set; }

    [UsedImplicitly] public required int HeightCm { get; set; }

    [UsedImplicitly] public required int SpreadCm { get; set; }
}