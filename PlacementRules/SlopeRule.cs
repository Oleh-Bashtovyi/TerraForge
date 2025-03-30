using Godot;
using TerrainGenerationApp.Utilities;

namespace TerrainGenerationApp.PlacementRules;

public class SlopeRule(float maxSlope) : IPlacementRule
{
    public float MaxSlope { get; } = maxSlope;
    public string Description => $"Slope less than {MaxSlope}";

    public bool CanPlaceIn(Vector2 pos, IWorldData worldData)
    {
        var slopesMap = worldData.TerrainSlopesMap;
        var slope = slopesMap[(int)pos.Y, (int)pos.X];
        return slope <= MaxSlope;
    }
}