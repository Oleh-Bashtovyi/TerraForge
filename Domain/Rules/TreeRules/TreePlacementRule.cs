using Godot;
using TerrainGenerationApp.Domain.Core;
using TerrainGenerationApp.Domain.Rules.PlacementRules;
using TerrainGenerationApp.Domain.Rules.RadiusRules;

namespace TerrainGenerationApp.Domain.Rules.TreeRules;

public class TreePlacementRule(string treeId, IPlacementRule placementRule, IRadiusRule radiusRule, bool overwriteLayers)
{
    public string TreeId { get; } = treeId;
    public bool OverwriteLayers { get; } = overwriteLayers;
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
