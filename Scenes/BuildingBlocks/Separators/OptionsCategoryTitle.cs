using Godot;

namespace TerrainGenerationApp.Scenes.BuildingBlocks.Separators;

public partial class OptionsCategoryTitle : Label
{
    public void SetTitle(string title)
    {
        Text = title;
    }

    public void SetFontSize(int size)
    {
        AddThemeFontSizeOverride("font_size", size);
    }
}