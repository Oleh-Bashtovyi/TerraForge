using System;
using Godot;

namespace TerrainGenerationApp.Generators.TreePlacement.TreeRadiusRules;

public class HeightBasedRadiusRule : ITreesRadiusRule
{
    public float BaseRadius { get; }
    public float MinRadius { get; }
    public float MaxRadius { get; }
    public string Description => $"Height-based radius {MinRadius}-{MaxRadius}";

    public HeightBasedRadiusRule(float baseRadius, float minRadius, float maxRadius)
    {
        BaseRadius = baseRadius;
        MinRadius = minRadius;
        MaxRadius = maxRadius;
    }

    public float GetRadius(Vector2 pos, ICurTerrainInfo terrainInfo)
    {
        var map = terrainInfo.CurTerrainMap;
        var h = map[(int)pos.Y, (int)pos.X];

        // Приклад формули: радіус залежить від висоти
        float radius = BaseRadius + h * (MaxRadius - MinRadius);
        return Math.Clamp(radius, MinRadius, MaxRadius);
    }
}