namespace GreenGenius.Common.Data.Entities;

public class BedPlant
{
    public Guid BedId { get; init; }
    public Bed Bed { get; init; } = null!;

    public Guid PlantId { get; init; }
    public Plant Plant { get; init; } = null!;
}
