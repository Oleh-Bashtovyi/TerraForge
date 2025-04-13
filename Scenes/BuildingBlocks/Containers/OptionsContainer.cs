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

    /// <summary>
    /// Sets the font size for the given node. This is used to set the font size for input lines and options containers.
    /// </summary>
    /// <param name="node"></param>
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

    /// <summary>
    /// Disables all options in the container and its children.
    /// </summary>
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

    /// <summary>
    /// Enables all options in the container and its children.
    /// </summary>
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

    /// <summary>
    /// Finds an input line by its ID. If recursive is true, it will search through all child options containers.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="id"></param>
    /// <param name="recursive"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Invokes the ParametersChanged event. This should be called whenever the parameters of the options change.
    /// </summary>
    protected void InvokeParametersChangedEvent()
    {
        ParametersChanged?.Invoke();
    }
}