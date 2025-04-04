using Godot;
using TerrainGenerationApp.Data.Structure;

namespace TerrainGenerationApp.Rules.RadiusRules;

public interface IRadiusRule
{
    float GetRadius(Vector2 pos, IWorldData worldData);
    string Description { get; }
}