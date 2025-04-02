using Godot;
using TerrainGenerationApp.Extensions;
using TerrainGenerationApp.Utilities;

namespace TerrainGenerationApp.PlacementRules;

public class SlopeRule(float maxSlope) : IPlacementRule
{
    public float MaxSlope { get; } = maxSlope;
    public string Description => $"Slope less than {MaxSlope}";

    public bool CanPlaceIn(Vector2 pos, IWorldData worldData)
    { 
        var slopesMap = worldData.TerrainSlopesMap;
        var slope = slopesMap.GetValueAt(pos);
        return slope <= MaxSlope;
    }
}