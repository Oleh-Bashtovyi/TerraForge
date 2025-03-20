using Godot;

namespace TerrainGenerationApp.Utilities;

// ReSharper disable InconsistentNaming
public static class LoadedScenes
{
    public static readonly PackedScene TREE_PLACEMENT_RULE_ITEM_SCENE =
        ResourceLoader.Load<PackedScene>(
            "res://Scenes/GenerationOptions/TreePlacementOptions/TreePlacementRuleItem.tscn");

    public static readonly PackedScene ABOVE_SEA_LEVEL_PLACEMENT_RULE_ITEM_SCENE =
        ResourceLoader.Load<PackedScene>(
        "res://Scenes/GenerationOptions/TreePlacementOptions/PlacementRuleItems/AboveSeaLevelRuleItem.tscn");
}