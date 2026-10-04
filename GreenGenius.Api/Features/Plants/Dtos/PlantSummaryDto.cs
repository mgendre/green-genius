namespace GreenGenius.Api.Features.Plants.Dtos;

public class PlantSummaryDto
{
    public Guid Id { get; set; }
    public required string NameFr { get; set; }
    public string? NameLatin { get; set; }
    public required string Family { get; set; }
}
