using Godot;
using TerrainGenerationApp.Data;

namespace TerrainGenerationApp.Rules.PlacementRules;

public class AboveSeaLevelRule(float minAboveWater, float maxAboveWater) : IPlacementRule
{
    public float MinAboveWater { get; } = minAboveWater;
    public float MaxAboveWater { get; } = maxAboveWater;
    public string Description => $"Above water level by {MinAboveWater} to {MaxAboveWater}";

    public bool CanPlaceIn(Vector2 pos, IWorldData worldData)
    {
        var waterLevel = worldData.SeaLevel;
        var h = worldData.TerrainData.HeightAt(pos);

        if (h < waterLevel)
            return false;

        float aboveWater = h - waterLevel;
        return aboveWater >= MinAboveWater && aboveWater <= MaxAboveWater;
    }
}
