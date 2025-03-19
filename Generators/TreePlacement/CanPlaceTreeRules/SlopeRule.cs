using Godot;

namespace TerrainGenerationApp.Generators.TreePlacement.CanPlaceTreeRules;

public class SlopeRule : ICanPlaceTreeRule
{
    public float MaxSlope { get; }
    public string Description => $"Slope less than {MaxSlope}";

    public SlopeRule(float maxSlope)
    {
        MaxSlope = maxSlope;
    }

    public bool CanPlaceIn(Vector2 pos, ICurTerrainInfo terrainInfo)
    {
        var slopesMap = terrainInfo.CurSlopesMap;
        var slope = slopesMap[(int)pos.Y, (int)pos.X];
        return slope <= MaxSlope;
    }
}