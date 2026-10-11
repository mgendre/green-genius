using GreenGenius.Common.Data.Enums;
using JetBrains.Annotations;

namespace GreenGenius.App.Services;

public sealed class PlantJson
{
    public required string NameFr { get; [UsedImplicitly] init; }
    public string? DescriptionFr { get; [UsedImplicitly] init; }
    public string? BinomialName { get; [UsedImplicitly] init; }
    public string? Emoji { get; [UsedImplicitly] init; }
    public required PlantFamily Family { get; [UsedImplicitly] init; }
    public PlantLifeCycle LifeCycle { get; [UsedImplicitly] init; }
    public int? DaysToMaturity { get; [UsedImplicitly] init; }
    public int? TemperatureMinC { get; [UsedImplicitly] init; }
    public int? Version { get; [UsedImplicitly] init; }
    public required PlantNeedsJson Needs { get; [UsedImplicitly] init; }
    public required PlantTraitsJson Traits { get; [UsedImplicitly] init; }
}

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

public sealed class PlantTraitsJson
{
    public bool NitrogenFixer { get; [UsedImplicitly] init; }
    public bool DynamicAccumulator { get; [UsedImplicitly] init; }
    public bool PollinatorFriendly { get; [UsedImplicitly] init; }
    public bool DroughtTolerant { get; [UsedImplicitly] init; }
}
