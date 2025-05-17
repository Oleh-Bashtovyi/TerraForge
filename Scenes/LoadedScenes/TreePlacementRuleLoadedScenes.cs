using Godot;

namespace TerrainGenerationApp.Scenes.LoadedScenes;

// ReSharper disable InconsistentNaming
public static class TreePlacementRuleLoadedScenes
{
    public const string FOLDER_WITH_RADIUS_RULES =
        "res://Scenes/FeatureOptions/TreePlacement/RadiusRuleItems/";

    public const string FOLDER_WITH_TREE_PLACEMENT_RULES =
        "res://Scenes/FeatureOptions/TreePlacement/PlacementRuleItems/";

    public static readonly PackedScene TREE_PLACEMENT_RULE_ITEM_SCENE =
        ResourceLoader.Load<PackedScene>(
            "res://Scenes/FeatureOptions/TreePlacement/TreePlacementRuleItem.tscn");

    public static readonly PackedScene CONSTANT_RADIUS_RULE_ITEM_SCENE =
        ResourceLoader.Load<PackedScene>(
            $"{FOLDER_WITH_RADIUS_RULES}ConstantRadiusRuleItem.tscn");

    public static readonly PackedScene ABOVE_SEA_LEVEL_PLACEMENT_RULE_ITEM_SCENE =
        ResourceLoader.Load<PackedScene>(
            $"{FOLDER_WITH_TREE_PLACEMENT_RULES}AboveSeaLevelRuleItem.tscn");

    public static readonly PackedScene SLOPE_PLACEMENT_RULE_ITEM_SCENE =
        ResourceLoader.Load<PackedScene>(
            $"{FOLDER_WITH_TREE_PLACEMENT_RULES}SlopeRuleItem.tscn");

    public static readonly PackedScene NOISE_MAP_PLACEMENT_RULE_ITEM_SCENE =
        ResourceLoader.Load<PackedScene>(
            $"{FOLDER_WITH_TREE_PLACEMENT_RULES}NoiseMapRuleItem.tscn");

    public static readonly PackedScene NO_TREE_LAYER_IN_RADIUS_RULE_ITEM_SCENE =
        ResourceLoader.Load<PackedScene>(
            $"{FOLDER_WITH_TREE_PLACEMENT_RULES}NoTreeLayersInRadiusRuleItem.tscn");

    public static readonly PackedScene WATER_IN_RADIUS_RULE_ITEM_SCENE =
        ResourceLoader.Load<PackedScene>(
            $"{FOLDER_WITH_TREE_PLACEMENT_RULES}WaterInRadiusRuleItem.tscn");

    public static readonly PackedScene MOISTURE_RULE_ITEM_SCENE =
        ResourceLoader.Load<PackedScene>(
            $"{FOLDER_WITH_TREE_PLACEMENT_RULES}MoistureRuleItem.tscn");
}