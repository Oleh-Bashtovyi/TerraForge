using Godot;

namespace TerrainGenerationApp.Generators.TreePlacement.TreeRadiusRules;

public interface ITreesRadiusRule
{
    float GetRadius(Vector2 pos, ICurTerrainInfo terrainInfo);
    string Description { get; }
}