using Godot;
using TerrainGenerationApp.Data;

namespace TerrainGenerationApp.PlacementRules;

public class SlopeRule(float maxSlope) : IPlacementRule
{
    public float MaxSlope { get; } = maxSlope;
    public string Description => $"Slope less than {MaxSlope}";

    public bool CanPlaceIn(Vector2 pos, IWorldData worldData)
    { 
        var slope = worldData.TerrainData.SlopeAt(pos);
        return slope <= MaxSlope;
    }
}