using GreenGenius.Common.Data.Enums;

namespace GreenGenius.Common.Data.Entities;

public class PlantAssociationMechanism
{
    public Guid PlantAssociationId { get; init; }
    public PlantAssociation PlantAssociation { get; init; } = null!;

    public Mechanism Mechanism { get; init; }
}