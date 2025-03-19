using Godot;

namespace TerrainGenerationApp.Generators.TreePlacement.CanPlaceTreeRules;

public class HeightRangeRule : ICanPlaceTreeRule
{
    public float MinHeight { get; }
    public float MaxHeight { get; }
    public string Description => $"Height between {MinHeight} and {MaxHeight}";

    public HeightRangeRule(float minHeight, float maxHeight)
    {
        MinHeight = minHeight;
        MaxHeight = maxHeight;
    }

    public bool CanPlaceIn(Vector2 pos, ICurTerrainInfo terrainInfo)
    {
        var map = terrainInfo.CurTerrainMap;
        var h = map[(int)pos.Y, (int)pos.X];
        return h >= MinHeight && h <= MaxHeight;
    }
}