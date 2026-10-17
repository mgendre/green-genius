using GreenGenius.Common.Data.Enums;

namespace GreenGenius.Common.Data.Entities;

public class PlantAssociation
{
    public Guid Id { get; init; }

    public Guid PlantAId { get; init; }
    public Plant PlantA { get; init; } = null!;

    public Guid PlantBId { get; init; }
    public Plant PlantB { get; init; } = null!;

    public Effect Effect { get; set; }
    public Evidence Evidence { get; set; }
    public string? NoteFr { get; set; }

    public List<PlantAssociationMechanism> Mechanisms { get; init; } = [];
}