using Godot;
using TerrainGenerationApp.Domain.Core;

namespace TerrainGenerationApp.Domain.Rules.RadiusRules;

public class ConstantRadiusRule(float radius) : IRadiusRule
{
    public float Radius { get; } = radius;
    public string Description => $"Constant radius {Radius}";

    public float GetRadius(Vector2 pos, IWorldData worldData)
    {
        return Radius;
    }
}
