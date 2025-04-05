using Godot;
using TerrainGenerationApp.Domain.Core;

namespace TerrainGenerationApp.Domain.Rules.PlacementRules;

public interface IPlacementRule
{
    bool CanPlaceIn(Vector2 pos, IWorldData worldData);
    string Description { get; }
}