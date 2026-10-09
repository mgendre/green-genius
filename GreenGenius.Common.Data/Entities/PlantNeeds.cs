using GreenGenius.Common.Data.Enums;

namespace GreenGenius.Common.Data.Entities;

public class PlantNeeds
{
    public Guid PlantId { get; init; }
    public SunlightLevel Sunlight { get; set; }
    public WaterNeedLevel WaterNeed { get; set; }
    public RootDepthLevel RootDepth { get; set; }
    public decimal? SoilPhMin { get; set; }
    public decimal? SoilPhMax { get; set; }
    public int SpacingRowCm { get; set; }
    public int SpacingPlantCm { get; set; }
    public int HeightCm { get; set; }
    public int SpreadCm { get; set; }
    public Plant Plant { get; init; } = null!;
}
