using Godot;

namespace TerrainGenerationApp.Scenes.BuildingBlocks;

public partial class InputLineBase : HBoxContainer
{
    private Label _descriptionLabel;

    protected Label DescriptionLabel
    {
        get
        {
            _descriptionLabel ??= GetNode<Label>("%DescriptionLabel");
            return _descriptionLabel;
        }
    }

    public void SetDescription(string description)
    {
        DescriptionLabel.Text = description;
    }

    public virtual void SetFontSize(int size)
    {
        GD.Print($"SETTING FONT SIZE! {size}");
        DescriptionLabel.AddThemeFontSizeOverride("font_size", size);
    }

    public virtual void EnableInput()
    {
    }

    public virtual void DisableInput()
    {
    }
}