using Godot;
using TerrainGenerationApp.Data.Structure;

namespace TerrainGenerationApp.Rules.PlacementRules;

public class SlopeRule(float minSlope, float maxSlope) : IPlacementRule
{
    public float MinSlope { get; } = minSlope;
    public float MaxSlope { get; } = maxSlope;

    public string Description => $"Slope in range [{MinSlope}; {MaxSlope}]";

    public bool CanPlaceIn(Vector2 pos, IWorldData worldData)
    {
        var slope = worldData.TerrainData.SlopeAt(pos);
        return slope >= MinSlope && slope <= MaxSlope;
    }
}