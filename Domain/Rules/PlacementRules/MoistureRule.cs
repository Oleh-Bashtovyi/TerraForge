using Godot;
using TerrainGenerationApp.Domain.Core;

namespace TerrainGenerationApp.Domain.Rules.PlacementRules;

public class MoistureRule(float minMoisture, float maxMoisture) : IPlacementRule
{
    public string Description => $"Moisture in range [{minMoisture}, {maxMoisture}]";

    public bool CanPlaceIn(Vector2 pos, IWorldData worldData)
    {
        var moisture = worldData.TerrainData.MoistureAt(pos);
        return moisture >= minMoisture && moisture <= maxMoisture;
    }
}