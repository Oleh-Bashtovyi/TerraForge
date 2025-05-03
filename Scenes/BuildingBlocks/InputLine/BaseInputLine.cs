#nullable enable
using Godot;

namespace TerrainGenerationApp.Scenes.BuildingBlocks.InputLine;

public partial class BaseInputLine : HBoxContainer, IInputLine
{
    private Label _descriptionLabel;
    private IInputLineValueTracker? _valueTracker;
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

    public void SetValueTracker(IInputLineValueTracker tracker)
    {
        _valueTracker = tracker;
        TrackCurrentValue();
    }

    protected void TrackCurrentValue()
    {
        _valueTracker?.TrackValue(this);
    }

    public virtual void EnableInput()
    {
    }

    public virtual void DisableInput()
    {
    }

    public virtual bool TrySetValue(object? value, bool invokeEvent = true)
    {
        return false;
    }

    public virtual object? GetValueAsObject()
    {
        return null;
    }
}