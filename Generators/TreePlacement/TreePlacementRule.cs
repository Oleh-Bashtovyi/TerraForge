using Godot;
using TerrainGenerationApp.PlacementRules;
using TerrainGenerationApp.RadiusRules;
using TerrainGenerationApp.Utilities;

namespace TerrainGenerationApp.Generators.TreePlacement;

public class TreePlacementRule(string treeId, IPlacementRule placementRule, IRadiusRule radiusRule)
{
    public string TreeId { get; } = treeId;
    public IPlacementRule PlacementRule { get; } = placementRule;
    public IRadiusRule RadiusRule { get; } = radiusRule;

    public bool CanPlace(Vector2 pos, IWorldData worldData)
    {
        return PlacementRule.CanPlaceIn(pos, worldData);
    }

    public float GetRadius(Vector2 pos, IWorldData worldData)
    {
        return RadiusRule.GetRadius(pos, worldData);
    }
}