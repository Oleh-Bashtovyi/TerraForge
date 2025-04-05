using Godot;

namespace TerrainGenerationApp.Scenes.LoadedScenes;

// ReSharper disable InconsistentNaming
public static class TreePlacementRuleLoadedScenes
{
    public const string FOLDER_WITH_RADIUS_RULES =
        "res://Scenes/GenerationOptions/TreePlacement/RadiusRuleItems/";

    public const string FOLDER_WITH_TREE_PLACEMENT_RULES =
        "res://Scenes/GenerationOptions/TreePlacement/PlacementRuleItems/";

    public static readonly PackedScene TREE_PLACEMENT_RULE_ITEM_SCENE =
        ResourceLoader.Load<PackedScene>(
            "res://Scenes/GenerationOptions/TreePlacement/TreePlacementRuleItem.tscn");

    public static readonly PackedScene ABOVE_SEA_LEVEL_PLACEMENT_RULE_ITEM_SCENE =
        ResourceLoader.Load<PackedScene>(
            $"{FOLDER_WITH_TREE_PLACEMENT_RULES}AboveSeaLevelRuleItem.tscn");

    public static readonly PackedScene SLOPE_PLACEMENT_RULE_ITEM_SCENE =
        ResourceLoader.Load<PackedScene>(
            $"{FOLDER_WITH_TREE_PLACEMENT_RULES}SlopeRuleItem.tscn");

    public static readonly PackedScene CONSTANT_RADIUS_RULE_ITEM_SCENE =
        ResourceLoader.Load<PackedScene>(
            $"{FOLDER_WITH_RADIUS_RULES}ConstantRadiusRuleItem.tscn");

    public static readonly PackedScene NOISE_MAP_PLACEMENT_RULE_ITEM_SCENE =
        ResourceLoader.Load<PackedScene>(
            $"{FOLDER_WITH_TREE_PLACEMENT_RULES}NoiseMapRuleItem.tscn");
}