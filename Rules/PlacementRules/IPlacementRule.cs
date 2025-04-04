using Godot;
using TerrainGenerationApp.Data.Structure;

namespace TerrainGenerationApp.Rules.PlacementRules;

public interface IPlacementRule
{
    bool CanPlaceIn(Vector2 pos, IWorldData worldData);
    string Description { get; }
}