using Godot;
using System;
using TerrainGenerationApp.Utilities;

namespace TerrainGenerationApp.RadiusRules;

public class HeightBasedRadiusRule(float baseRadius, float minRadius, float maxRadius) : IRadiusRule
{
    public float BaseRadius { get; } = baseRadius;
    public float MinRadius { get; } = minRadius;
    public float MaxRadius { get; } = maxRadius;
    public string Description => $"Height-based radius {MinRadius}-{MaxRadius}";

    public float GetRadius(Vector2 pos, IWorldData worldData)
    {
        var map = worldData.TerrainHeightMap;
        var h = map[(int)pos.Y, (int)pos.X];

        float radius = BaseRadius + h * (MaxRadius - MinRadius);
        return Math.Clamp(radius, MinRadius, MaxRadius);
    }
}