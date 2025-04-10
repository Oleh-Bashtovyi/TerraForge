using Godot;

namespace TerrainGenerationApp.Scenes.BuildingBlocks.InputLine;

public partial class BaseInputLine : HBoxContainer
{
    private Label _descriptionLabel;
    private string _id;

    protected Label DescriptionLabel
    {
        get
        {
            _descriptionLabel ??= GetNode<Label>("%DescriptionLabel");
            return _descriptionLabel;
        }
    }

    public string Id => _id;

    public void SetId(string id)
    {
        _id = id;
    }

    public void SetDescription(string description)
    {
        DescriptionLabel.Text = description;
    }

    public virtual void SetFontSize(int size)
    {
        DescriptionLabel.AddThemeFontSizeOverride("font_size", size);
    }

    public virtual void EnableInput()
    {
    }

    public virtual void DisableInput()
    {
    }
}