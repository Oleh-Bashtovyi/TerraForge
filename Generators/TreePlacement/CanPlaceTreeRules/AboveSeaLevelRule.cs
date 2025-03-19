using Godot;

namespace TerrainGenerationApp.Generators.TreePlacement.CanPlaceTreeRules;

public class AboveSeaLevelRule : ICanPlaceTreeRule
{
    public float MinAboveWater { get; }
    public float MaxAboveWater { get; }
    public string Description => $"Above water level by {MinAboveWater} to {MaxAboveWater}";

    public AboveSeaLevelRule(float minAboveWater, float maxAboveWater)
    {
        MinAboveWater = minAboveWater;
        MaxAboveWater = maxAboveWater;
    }

    public bool CanPlaceIn(Vector2 pos, ICurTerrainInfo terrainInfo)
    {
        var map = terrainInfo.CurTerrainMap;
        var waterLevel = terrainInfo.CurWaterLevel;
        var h = map[(int)pos.Y, (int)pos.X];

        if (h < waterLevel)
            return false;

        float aboveWater = h - waterLevel;
        return aboveWater >= MinAboveWater && aboveWater <= MaxAboveWater;
    }
}
