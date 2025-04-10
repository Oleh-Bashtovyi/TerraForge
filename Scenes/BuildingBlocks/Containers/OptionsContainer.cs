using Godot;
using System;
using TerrainGenerationApp.Scenes.BuildingBlocks.InputLine;

namespace TerrainGenerationApp.Scenes.BuildingBlocks.Containers;

#nullable enable
public partial class OptionsContainer : VBoxContainer
{
    private int _optionsFontSize = 16;

    public event Action? ParametersChanged;

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
        if (node is BaseInputLine inputLine)
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
            if (child is BaseInputLine inputLine)
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
            if (child is BaseInputLine inputLine)
            {
                inputLine.EnableInput();
            }
            else if (child is OptionsContainer optionsContainer)
            {
                optionsContainer.EnableAllOptions();
            }
        }
    }

    public T? FindInputLine<T>(string id, bool recursive = false) where T : BaseInputLine
    {
        foreach (Node child in GetChildren())
        {
            if (child is T inputLine && inputLine.Id == id)
            {
                return inputLine;
            }
            if (recursive && child is OptionsContainer optionsContainer)
            {
                var foundInputLine = optionsContainer.FindInputLine<T>(id);
                if (foundInputLine != null)
                {
                    return foundInputLine;
                }
            }
        }
        return null;
    }

    protected void InvokeParametersChangedEvent()
    {
        ParametersChanged?.Invoke();
    }
}