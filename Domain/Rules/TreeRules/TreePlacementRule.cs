using Godot;
using TerrainGenerationApp.Domain.Core;
using TerrainGenerationApp.Domain.Rules.PlacementRules;
using TerrainGenerationApp.Domain.Rules.RadiusRules;

namespace TerrainGenerationApp.Domain.Rules.TreeRules;

public class TreePlacementRule(
    string treeId,
    string layerName,
    IPlacementRule placementRule,
    IRadiusRule radiusRule,
    bool overwriteLayers)
{
    public string TreeId { get; } = treeId;
    public string LayerName { get; } = string.IsNullOrEmpty(layerName) ? treeId : layerName;
    public bool OverwriteLayers { get; } = overwriteLayers;
    public IPlacementRule PlacementRule { get; } = placementRule;
    public IRadiusRule RadiusRule { get; } = radiusRule;

    // Constructor that defaults LayerName to TreeId for backward compatibility
    public TreePlacementRule(
        string treeId,
        IPlacementRule placementRule,
        IRadiusRule radiusRule,
        bool overwriteLayers)
        : this(treeId, treeId, placementRule, radiusRule, overwriteLayers)
    {
    }

    public bool CanPlace(Vector2 pos, IWorldData worldData)
    {
        return PlacementRule.CanPlaceIn(pos, worldData);
    }

    public float GetRadius(Vector2 pos, IWorldData worldData)
    {
        return RadiusRule.GetRadius(pos, worldData);
    }
}
