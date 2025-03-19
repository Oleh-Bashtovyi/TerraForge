using Godot;
using TerrainGenerationApp.Utilities;

namespace TerrainGenerationApp.PlacementRules;

public interface IPlacementRule
{
    bool CanPlaceIn(Vector2 pos, IWorldData worldData);
    string Description { get; }
}