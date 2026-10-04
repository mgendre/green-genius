using GreenGenius.Common.Data.Enums;

namespace GreenGenius.Api.Features.Plants.Dtos;

public class PlantDto
{
    public Guid Id { get; set; }
    public required string NameFr { get; set; }
    public string? NameLatin { get; set; }
    public string? DescriptionFr { get; set; }
    public PlantFamily Family { get; set; }
    public PlantLifeCycle LifeCycle { get; set; }
    public int? DaysToMaturity { get; set; }
    public int? TemperatureMinC { get; set; }
    public required PlantNeedsDto Needs { get; set; }
    public required PlantTraitsDto Traits { get; set; }
}
