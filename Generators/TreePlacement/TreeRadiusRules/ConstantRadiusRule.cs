using Godot;

namespace TerrainGenerationApp.Generators.TreePlacement.TreeRadiusRules;

public class ConstantRadiusRule : ITreesRadiusRule
{
    public float Radius { get; }
    public string Description => $"Constant radius {Radius}";

    public ConstantRadiusRule(float radius)
    {
        Radius = radius;
    }

    public float GetRadius(Vector2 pos, ICurTerrainInfo terrainInfo)
    {
        return Radius;
    }
}
