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
    public int? FrostToleranceC { get; [UsedImplicitly] init; }
    public int? Version { get; [UsedImplicitly] init; }
    public required PlantNeedsJson Needs { get; [UsedImplicitly] init; }
    public required PlantTraitsJson Traits { get; [UsedImplicitly] init; }
}