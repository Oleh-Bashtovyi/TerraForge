using Godot;

namespace TerrainGenerationApp.Scenes.LoadedScenes;

public class InputLineScenes
{
    public static string INPUT_LINE_FOLDER = "res://Scenes/BuildingBlocks/";

    public static readonly PackedScene InputLineSliderScene = 
        GD.Load<PackedScene>($"{INPUT_LINE_FOLDER}InputLineSlider.tscn");

    public static readonly PackedScene InputLineComboboxScene =
        GD.Load<PackedScene>($"{INPUT_LINE_FOLDER}InputLineCombobox.tscn");

    public static readonly PackedScene InputLineCheckboxScene =
        GD.Load<PackedScene>($"{INPUT_LINE_FOLDER}InputLineCheckbox.tscn");

    public static readonly PackedScene OptionsCategoryTitleScene =
        GD.Load<PackedScene>($"{INPUT_LINE_FOLDER}OptionsCategoryTitle.tscn");
}