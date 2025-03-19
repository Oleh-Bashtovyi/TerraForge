using Godot;
using TerrainGenerationApp.Utilities;

namespace TerrainGenerationApp.PlacementRules;

public class HeightRangeRule(float minHeight, float maxHeight) : IPlacementRule
{
    public float MinHeight { get; } = minHeight;
    public float MaxHeight { get; } = maxHeight;
    public string Description => $"Height between {MinHeight} and {MaxHeight}";

    public bool CanPlaceIn(Vector2 pos, IWorldData worldData)
    {
        var map = worldData.TerrainMap;
        var h = map[(int)pos.Y, (int)pos.X];
        return h >= MinHeight && h <= MaxHeight;
    }
}