using GreenGenius.Common.Data.Enums;

namespace GreenGenius.Common.Data.Entities;

public class PlantAssociation
{
    public Guid Id { get; init; }

    public Guid PlantAId { get; init; }
    public Plant PlantA { get; init; } = null!;

    public Guid PlantBId { get; init; }
    public Plant PlantB { get; init; } = null!;

    public Polarity Polarity { get; set; }
    public Mechanism Mechanism { get; set; }
    public Evidence Evidence { get; set; }
    public string? NoteFr { get; set; }
}
