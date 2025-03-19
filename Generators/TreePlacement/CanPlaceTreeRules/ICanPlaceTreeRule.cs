using Godot;

namespace TerrainGenerationApp.Generators.TreePlacement.CanPlaceTreeRules;

public interface ICanPlaceTreeRule
{
    bool CanPlaceIn(Vector2 pos, ICurTerrainInfo terrainInfo);
    string Description { get; }
}