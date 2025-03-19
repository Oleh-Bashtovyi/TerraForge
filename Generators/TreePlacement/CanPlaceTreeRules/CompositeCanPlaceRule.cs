using System.Collections.Generic;
using System.Linq;
using Godot;

namespace TerrainGenerationApp.Generators.TreePlacement.CanPlaceTreeRules;

public class CompositeCanPlaceRule : ICanPlaceTreeRule
{
    private readonly List<ICanPlaceTreeRule> _rules;
    public string Description { get; }

    public CompositeCanPlaceRule(List<ICanPlaceTreeRule> rules, string description = "Composite Rule")
    {
        _rules = rules;
        Description = description;
    }

    public bool CanPlaceIn(Vector2 pos, ICurTerrainInfo terrainInfo)
    {
        // Перевіряємо всі правила (логічне І)
        return _rules.All(rule => rule.CanPlaceIn(pos, terrainInfo));
    }
}