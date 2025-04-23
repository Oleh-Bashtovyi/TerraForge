using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using TerrainGenerationApp.Scenes.BuildingBlocks.InputLine;

namespace TerrainGenerationApp.Scenes.BuildingBlocks.Containers;

#nullable enable
public partial class OptionsContainer : VBoxContainer
{
    private readonly LastUsedInputLineValueTracker _lastUsedValuesTracker = new();
    private int _optionsFontSize = 16;

    public event Action? ParametersChanged;

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
        SetInputLineTrackerForChildren();
    }

    private void OnChildEnteredTree(Node node)
    {
        SetFontSize(node);
        SetInputLineTracker(node);
    }

    /// <summary>
    /// Disables all options in the container and its children.
    /// </summary>
    public void DisableAllOptions()
    {
        foreach (Node child in GetChildren())
        {
            if (child is IInputLine inputLine)
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
            if (child is IInputLine inputLine)
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
    public T? FindInputLine<T>(string id, bool recursive = false) where T : IInputLine
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
        return default;
    }

    /// <summary>  
    /// Saves the current option values as the last used ones.  
    /// </summary>  
    public void UpdateCurrentOptionsAsLastUsed()
    {
        _lastUsedValuesTracker.UpdateLastUsedValues();
    }

    /// <summary>  
    /// Retrieves the dictionary of the last used parameters for generation or similar things.
    /// Used to export configuration to files. 
    /// </summary>  
    /// <returns>A dictionary containing the last used parameters.</returns>  
    public IReadOnlyDictionary<string, object> GetLastUsedInputLineValues()
    {
        return _lastUsedValuesTracker.GetLastUsedValues();
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
                inputLine.TrySetValue(value, invokeEvent: false);
            }
        }
    }

    private void SetInputLineTrackerForChildren()
    {
        foreach (var child in GetChildren())
        {
            SetInputLineTracker(child);
        }
    }

    private void SetInputLineTracker(Node node)
    {
        if (node is IInputLine inputLine)
        {
            inputLine.SetValueTracker(_lastUsedValuesTracker.ValueTracker);
        }
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

    protected void SetAndInvokeParametersChangedEvent<T>(ref T property, T value)
    {
        property = value;
        InvokeParametersChangedEvent();
    }

    /// <summary>
    /// Invokes the ParametersChanged event. This should be called whenever the parameters of the options change.
    /// </summary>
    protected void InvokeParametersChangedEvent()
    {
        ParametersChanged?.Invoke();
    }
}