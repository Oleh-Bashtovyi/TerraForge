using Godot;
using TerrainGenerationApp.Utilities;

namespace TerrainGenerationApp.RadiusRules;

public interface IRadiusRule
{
    float GetRadius(Vector2 pos, IWorldData worldData);
    string Description { get; }
}