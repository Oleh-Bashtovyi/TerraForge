using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using TerrainGenerationApp.Scenes.BuildingBlocks.InputLine;

namespace TerrainGenerationApp.Scenes.BuildingBlocks.Containers;

#nullable enable
public partial class OptionsContainer : VBoxContainer
{
    private readonly Dictionary<string, object> _lastUsedOptionValues = new();
    private readonly Dictionary<string, object> _newOptionValues = new();
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
    /// Updates the current value of an option in the _newOptionValues dictionary.  
    /// </summary>  
    /// <param name="id">The identifier of the option.</param>  
    /// <param name="value">The new value.</param>  
    protected void UpdateOptionValue(string id, object value)
    {
        _newOptionValues[id] = value;
    }

    /// <summary>  
    /// Saves the current option values as the last used ones.  
    /// </summary>  
    public void UpdateCurrentOptionsAsLastUsed()
    {
        foreach (var pair in _newOptionValues)
        {
            _lastUsedOptionValues[pair.Key] = pair.Value;
        }
        _newOptionValues.Clear();
    }

    /// <summary>  
    /// Retrieves the dictionary of the last used parameters.  
    /// </summary>  
    /// <returns>A dictionary containing the last used parameters.</returns>  
    public Dictionary<string, object> GetLastUsedOptionValues()
    {
        return new Dictionary<string, object>(_lastUsedOptionValues);
    }

    /// <summary>  
    /// Loads the provided parameters into the container by matching them with the input lines' IDs.  
    /// If a matching input line is found, its value is updated without invoking events.  
    /// Updates the internal dictionary of new option values.  
    /// </summary>  
    /// <param name="parameters">A dictionary containing parameter IDs and their corresponding values.</param>  
    public void LoadParameters(Dictionary<string, object> parameters)
    {
        var options = GetChildren().OfType<BaseInputLine>().ToList();

        foreach (var parameter in parameters)
        {
            var id = parameter.Key;
            var value = parameter.Value;
            var inputLine = options.FirstOrDefault(x => x.Id == id);

            if (inputLine != null)
            {
                if (inputLine.TrySetValue(value, invokeEvent: false))
                {
                    UpdateOptionValue(id, value);
                }
            }
        }
    }

    /// <summary>
    /// Invokes the ParametersChanged event. This should be called whenever the parameters of the options change.
    /// </summary>
    protected void InvokeParametersChangedEvent()
    {
        ParametersChanged?.Invoke();
    }
}