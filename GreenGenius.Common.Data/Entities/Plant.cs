using GreenGenius.Common.Data.Enums;

namespace GreenGenius.Common.Data.Entities;

public class Plant
{
    public Guid Id { get; init; }
    public required string NameFr { get; set; }
    public string? NameLatin { get; set; }
    public string? DescriptionFr { get; set; }
    public PlantFamily Family { get; set; }
    public PlantLifeCycle LifeCycle { get; set; }
    public int? DaysToMaturity { get; set; }
    public int? TemperatureMinC { get; set; }
    public PlantNeeds Needs { get; set; } = null!;
    public PlantTraits Traits { get; set; } = null!;
}
