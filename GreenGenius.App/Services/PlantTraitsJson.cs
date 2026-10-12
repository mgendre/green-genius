using JetBrains.Annotations;

namespace GreenGenius.App.Services;

public sealed class PlantTraitsJson
{
    public bool NitrogenFixer { get; [UsedImplicitly] init; }
    public bool DynamicAccumulator { get; [UsedImplicitly] init; }
    public bool PollinatorFriendly { get; [UsedImplicitly] init; }
    public bool DroughtTolerant { get; [UsedImplicitly] init; }
}
