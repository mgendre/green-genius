namespace GreenGenius.Common.Data.Entities;

public class PlantTraits
{
    public Guid PlantId { get; init; }
    public bool NitrogenFixer { get; set; }
    public bool DynamicAccumulator { get; set; }
    public bool PollinatorFriendly { get; set; }
    public bool DroughtTolerant { get; set; }
    public Plant Plant { get; init; } = null!;
}
