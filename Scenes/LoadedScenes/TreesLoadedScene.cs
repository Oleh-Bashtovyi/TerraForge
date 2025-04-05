using Godot;
using Godot.Collections;

namespace TerrainGenerationApp.Scenes.LoadedScenes;

// ReSharper disable InconsistentNaming
public static class TreesLoadedScene
{
    public const string FOLDER_WITH_TREES =
        "res://Scenes/Objects/Trees/";

    public static readonly PackedScene TREE_SCENE =
        ResourceLoader.Load<PackedScene>($"{FOLDER_WITH_TREES}Tree.tscn");

    public static readonly PackedScene PALM_SCENE =
        ResourceLoader.Load<PackedScene>($"{FOLDER_WITH_TREES}Palm.tscn");

    public static readonly PackedScene PINE_SCENE =
        ResourceLoader.Load<PackedScene>($"{FOLDER_WITH_TREES}Pine.tscn");

    public static readonly PackedScene APPLE_SCENE =
        ResourceLoader.Load<PackedScene>($"{FOLDER_WITH_TREES}Apple.tscn");

    public static readonly PackedScene TRUNK_SCENE =
        ResourceLoader.Load<PackedScene>($"{FOLDER_WITH_TREES}Trunk.tscn");

    public static Dictionary<int, string> GetTreeNames()
    {
        return new Dictionary<int, string>
        {
            {1, "Tree"},
            {2, "Palm"},
            {3, "Pine"},
            {4, "Apple"},
            {5, "Trunk"}
        };
    }

    public static PackedScene GetTreeScene(int treeId)
    {
        return treeId switch
        {
            1 => TREE_SCENE,
            2 => PALM_SCENE,
            3 => PINE_SCENE,
            4 => APPLE_SCENE,
            5 => TRUNK_SCENE,
            _ => TREE_SCENE
        };
    }
}