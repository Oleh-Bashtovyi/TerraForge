using Godot;
using System.Collections.Generic;
using System.Linq;
using TerrainGenerationApp.Utilities;

namespace TerrainGenerationApp.PlacementRules;

public class CompositePlacementRule(List<IPlacementRule> rules, string description = "Composite Rule") : IPlacementRule
{
    public string Description { get; } = description;

    public CompositePlacementRule(string description = "Composite Rule") : this(new List<IPlacementRule>(), description)
    {
    }

    public bool CanPlaceIn(Vector2 pos, IWorldData worldData)
    {
        if (!rules.Any())
        {
            return false;
        }

        return rules.All(rule => rule.CanPlaceIn(pos, worldData));
    }
}
