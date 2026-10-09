using GreenGenius.Common.Data.Enums;

namespace GreenGenius.App.Services;

public sealed class PlantJson
{
    public required string NameFr { get; init; }
    public string? DescriptionFr { get; init; }
    public string? BinomialName { get; init; }
    public required PlantFamily Family { get; init; }
    public PlantLifeCycle LifeCycle { get; init; }
    public int? DaysToMaturity { get; init; }
    public int? TemperatureMinC { get; init; }
    public int? Version { get; init; }
    public required PlantNeedsJson Needs { get; init; }
    public required PlantTraitsJson Traits { get; init; }
}

public sealed class PlantNeedsJson
{
    public SunlightLevel Sunlight { get; init; }
    public WaterNeedLevel WaterNeed { get; init; }
    public RootDepthLevel RootDepth { get; init; }
    public decimal? SoilPhMin { get; init; }
    public decimal? SoilPhMax { get; init; }
    public int SpacingRowCm { get; init; }
    public int SpacingPlantCm { get; init; }
    public int HeightCm { get; init; }
    public int SpreadCm { get; init; }
}

public sealed class PlantTraitsJson
{
    public bool NitrogenFixer { get; init; }
    public bool DynamicAccumulator { get; init; }
    public bool PollinatorFriendly { get; init; }
    public bool DroughtTolerant { get; init; }
}
