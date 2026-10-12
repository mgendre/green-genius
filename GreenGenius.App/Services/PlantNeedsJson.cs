using GreenGenius.Common.Data.Enums;
using JetBrains.Annotations;

namespace GreenGenius.App.Services;

public sealed class PlantNeedsJson
{
    public SunlightLevel Sunlight { get; init; }
    public WaterNeedLevel WaterNeed { get; init; }
    public RootDepthLevel RootDepth { get; init; }
    public decimal? SoilPhMin { get; [UsedImplicitly] init; }
    public decimal? SoilPhMax { get; [UsedImplicitly] init; }
    public int SpacingRowCm { get; [UsedImplicitly] init; }
    public int SpacingPlantCm { get; [UsedImplicitly] init; }
    public int HeightCm { get; [UsedImplicitly] init; }
    public int SpreadCm { get; [UsedImplicitly] init; }
}
