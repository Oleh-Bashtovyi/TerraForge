using Godot;
using TerrainGenerationApp.Domain.Core;

namespace TerrainGenerationApp.Domain.Rules.RadiusRules;

public interface IRadiusRule
{
    float GetRadius(Vector2 pos, IWorldData worldData);
    string Description { get; }
}