using Godot;
using TerrainGenerationApp.Data;

namespace TerrainGenerationApp.PlacementRules;

public interface IPlacementRule
{
    bool CanPlaceIn(Vector2 pos, IWorldData worldData);
    string Description { get; }
}