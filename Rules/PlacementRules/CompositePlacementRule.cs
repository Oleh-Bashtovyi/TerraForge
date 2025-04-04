using Godot;
using System.Collections.Generic;
using System.Linq;
using TerrainGenerationApp.Data;

namespace TerrainGenerationApp.Rules.PlacementRules;

public class CompositePlacementRule : IPlacementRule
{
    private readonly List<IPlacementRule> _rules;
    public string Description { get; }


    public CompositePlacementRule(List<IPlacementRule> rules, string description = "Composite Rule")
    {
        _rules = rules;
        Description = description;
    }

    public bool CanPlaceIn(Vector2 pos, IWorldData worldData)
    {
        if (!_rules.Any())
        {
            return false;
        }

        return _rules.All(rule => rule.CanPlaceIn(pos, worldData));
    }
}
