using GreenGenius.Common.Data.Enums;

namespace GreenGenius.Api.Features.Plants.Dtos;

public class PlantNeedsDto
{
    public SunlightLevel Sunlight { get; set; }
    public WaterNeedLevel WaterNeed { get; set; }
    public RootDepthLevel RootDepth { get; set; }
    public decimal? SoilpHMin { get; set; }
    public decimal? SoilpHMax { get; set; }
    public int SpacingRowCm { get; set; }
    public int SpacingPlantCm { get; set; }
    public int HeightCm { get; set; }
    public int SpreadCm { get; set; }
}
