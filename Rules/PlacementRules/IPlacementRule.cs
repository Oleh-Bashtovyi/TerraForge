using Godot;
using TerrainGenerationApp.Data;

namespace TerrainGenerationApp.Rules.PlacementRules;

public interface IPlacementRule
{
    bool CanPlaceIn(Vector2 pos, IWorldData worldData);
    string Description { get; }
}