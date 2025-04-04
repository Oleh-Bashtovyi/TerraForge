using Godot;
using TerrainGenerationApp.Data.Structure;
using TerrainGenerationApp.Extensions;

namespace TerrainGenerationApp.Rules.PlacementRules;

public class NoiseMapRule(float[,] noiseMap, float noiseThreshold) : IPlacementRule
{
    public bool CanPlaceIn(Vector2 pos, IWorldData worldData)
    {
        var hp = worldData.TerrainData.HeightMap.HeightIndexProgress(pos.Y);
        var wp = worldData.TerrainData.HeightMap.WidthIndexProgress(pos.X);

        var noiseValue = noiseMap.GetValueUsingIndexProgress(hp, wp);

        return noiseValue >= noiseThreshold;
    }

    public string Description => $"Determines placement based on a noise map threshold. Threshold: {noiseThreshold}";
}
