using GreenGenius.Common.Data.Enums;
using JetBrains.Annotations;

namespace GreenGenius.App.Services;

public sealed class PlantAssociationJson
{
    public required string PlantA { get; [UsedImplicitly] init; }
    public required string PlantB { get; [UsedImplicitly] init; }
    public required Effect Effect { get; [UsedImplicitly] init; }
    public required List<Mechanism> Mechanisms { get; [UsedImplicitly] init; }
    public required Evidence Evidence { get; [UsedImplicitly] init; }
    public string? NoteFr { get; [UsedImplicitly] init; }
}