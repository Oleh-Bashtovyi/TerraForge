using Godot;
using System;
using TerrainGenerationApp.Scenes.BuildingBlocks;

namespace TerrainGenerationApp.Scenes.GenerationOptions;

public partial class OptionsContainer : VBoxContainer
{
    private int _optionsFontSize = 16;

    public event Action ParametersChanged;

    /// <summary>
    /// The font size for the options in the container.
    /// </summary>
    [Export(PropertyHint.Range, "1,30,1")]
    public int OptionsFontSize
    {
        get => _optionsFontSize;
        set
        {
            _optionsFontSize = value;
            SetOptionsFontSizeForChildren();
        }
    }

    public override void _Ready()
    {
        ChildEnteredTree += OnChildEnteredTree;
        SetOptionsFontSizeForChildren();
    }

    private void OnChildEnteredTree(Node node)
    {
        SetFontSize(node);
    }

    private void SetOptionsFontSizeForChildren()
    {
        foreach (var child in GetChildren())
        {
            SetFontSize(child);
        }
    }

    private void SetFontSize(Node node)
    {
        if (node is InputLineBase inputLine)
        {
            inputLine.SetFontSize(OptionsFontSize);
        }
        else if (node is OptionsContainer optionsContainer)
        {
            optionsContainer.OptionsFontSize = OptionsFontSize;
        }
    }

    public void DisableAllOptions()
    {
        foreach (Node child in GetChildren())
        {
            if (child is InputLineBase inputLine)
            {
                inputLine.DisableInput();
            }
            else if (child is OptionsContainer optionsContainer)
            {
                optionsContainer.DisableAllOptions();
            }
        }
    }

    public void EnableAllOptions()
    {
        foreach (Node child in GetChildren())
        {
            if (child is InputLineBase inputLine)
            {
                inputLine.EnableInput();
            }
            else if (child is OptionsContainer optionsContainer)
            {
                optionsContainer.EnableAllOptions();
            }
        }
    }

    protected void InvokeParametersChangedEvent()
    {
        ParametersChanged?.Invoke();
    }
}