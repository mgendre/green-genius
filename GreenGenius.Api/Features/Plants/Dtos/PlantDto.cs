using GreenGenius.Common.Data.Enums;
using JetBrains.Annotations;

namespace GreenGenius.Api.Features.Plants.Dtos;

public class PlantDto
{
    public Guid Id { get; [UsedImplicitly] set; }
    public required string NameFr { get; [UsedImplicitly] set; }
    public string? BinomialName { [UsedImplicitly] get; [UsedImplicitly] set; }
    public string? Emoji { [UsedImplicitly] get; [UsedImplicitly] set; }
    public string? DescriptionFr { [UsedImplicitly] get; [UsedImplicitly] set; }
    public PlantFamily Family { [UsedImplicitly] get; [UsedImplicitly] set; }
    public PlantLifeCycle LifeCycle { [UsedImplicitly] get; [UsedImplicitly] set; }
    public int? DaysToMaturity { [UsedImplicitly] get; [UsedImplicitly] set; }
    public int? TemperatureMinC { [UsedImplicitly] get; [UsedImplicitly] set; }
    public required PlantNeedsDto Needs { get; [UsedImplicitly] set; }
    public required PlantTraitsDto Traits { get; [UsedImplicitly] set; }
}
