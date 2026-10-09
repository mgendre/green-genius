using GreenGenius.Common.Data.Enums;
using JetBrains.Annotations;

namespace GreenGenius.Api.Features.Plants.Dtos;

public class PlantDto
{
    public Guid Id { get; set; }
    public required string NameFr { get; set; }
    public string? BinomialName { [UsedImplicitly] get; set; }
    public string? DescriptionFr { [UsedImplicitly] get; set; }
    public PlantFamily Family { [UsedImplicitly] get; set; }
    public PlantLifeCycle LifeCycle { [UsedImplicitly] get; set; }
    public int? DaysToMaturity { [UsedImplicitly] get; set; }
    public int? TemperatureMinC { [UsedImplicitly] get; set; }
    public required PlantNeedsDto Needs { get; set; }
    public required PlantTraitsDto Traits { get; set; }
}
