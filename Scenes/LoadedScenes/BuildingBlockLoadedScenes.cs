using Godot;

namespace TerrainGenerationApp.Scenes.LoadedScenes;

// ReSharper disable InconsistentNaming
public class BuildingBlockLoadedScenes
{
    public static string INPUT_LINE_FOLDER = "res://Scenes/BuildingBlocks/InputLine/";

    public static string SEPARATORS_FOLDER = "res://Scenes/BuildingBlocks/Separators/";

    public static readonly PackedScene INPUT_LINE_SLIDER = 
        GD.Load<PackedScene>($"{INPUT_LINE_FOLDER}InputLineSlider.tscn");

    public static readonly PackedScene INPUT_LINE_COMBOBOX =
        GD.Load<PackedScene>($"{INPUT_LINE_FOLDER}InputLineCombobox.tscn");

    public static readonly PackedScene INPUT_LINE_CHECKBOX =
        GD.Load<PackedScene>($"{INPUT_LINE_FOLDER}InputLineCheckbox.tscn");

    public static readonly PackedScene INPUT_LINE_TEXT =
        GD.Load<PackedScene>($"{INPUT_LINE_FOLDER}InputLineText.tscn");

    public static readonly PackedScene OPTIONS_CATEGORY_TITLE =
        GD.Load<PackedScene>($"{SEPARATORS_FOLDER}OptionsCategoryTitle.tscn");
}