using Godot;

namespace TerrainGenerationApp.Generators.TreePlacement;

public interface ICanTreePlacementRuleUIOption
{
    public TreesPlacementRule Rule { get; }
    public Color TreeColor { get; }
}
