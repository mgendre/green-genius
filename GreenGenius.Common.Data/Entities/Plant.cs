using GreenGenius.Common.Data.Enums;

namespace GreenGenius.Common.Data.Entities;

public class Plant
{
    public Guid Id { get; init; }
    public required string NameFr { get; set; }
    public string? Key { get; init; }
    public string? DescriptionFr { get; set; }
    public string? BinomialName { get; set; }
    public PlantFamily Family { get; set; }
    public PlantLifeCycle LifeCycle { get; set; }
    public int? DaysToMaturity { get; set; }
    public int? TemperatureMinC { get; set; }
    public int Version { get; set; }
    public required PlantNeeds Needs { get; init; }
    public required PlantTraits Traits { get; init; }
}
