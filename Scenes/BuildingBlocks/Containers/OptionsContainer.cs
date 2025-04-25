#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using TerrainGenerationApp.Scenes.BuildingBlocks.InputLine;

namespace TerrainGenerationApp.Scenes.BuildingBlocks.Containers;

/// <summary>
/// A container for managing UI input options that handles value tracking, parameter changes, and configuration loading.
/// Provides functionality for nested input elements, font size management, and bidirectional data binding.
/// </summary>
public partial class OptionsContainer : VBoxContainer, ILastUsedConfigProvider, IOptionsToggleable
{
    private readonly LastUsedInputLineValueTracker _lastUsedValuesTracker = new();
    private int _optionsFontSize = 16;

    /// <summary>
    /// Event triggered when any parameter within the container changes.
    /// This event is suppressed during configuration loading.
    /// </summary>
    public event Action? ParametersChanged;

    /// <summary>
    /// Indicates whether the container is currently loading values from a configuration.
    /// When true, <see cref="ParametersChanged"/> event is suppressed.
    /// </summary>
    public bool IsLoading { get; private set; } = false;

    /// <summary>
    /// Controls the font size for all options in this container and its children.
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
        SetInputLineTrackerForChildren();
    }

    /// <summary>
    /// Handles the event when a child node is added to the container.
    /// Configures the new child with appropriate font size and value tracking.
    /// </summary>
    /// <param name="node">The node that was added to the container.</param>
    private void OnChildEnteredTree(Node node)
    {
        SetFontSize(node);
        SetInputLineTracker(node);
    }

    /// <summary>
    /// Disables all input options in the container and its children.
    /// Recursively applies to nested <see cref="OptionsContainer"/> instances.
    /// </summary>
    public void DisableOptions()
    {
        foreach (Node child in GetChildren())
        {
            if (child is IInputLine inputLine)
            {
                inputLine.DisableInput();
            }
            else if (child is OptionsContainer optionsContainer)
            {
                optionsContainer.DisableOptions();
            }
        }
    }

    /// <summary>
    /// Enables all input options in the container and its children.
    /// Recursively applies to nested <see cref="OptionsContainer"/> instances.
    /// </summary>
    public void EnableOptions()
    {
        foreach (Node child in GetChildren())
        {
            if (child is IInputLine inputLine)
            {
                inputLine.EnableInput();
            }
            else if (child is OptionsContainer optionsContainer)
            {
                optionsContainer.EnableOptions();
            }
        }
    }

    /// <summary>
    /// Finds an input line by its ID within this container or its children.
    /// </summary>
    /// <typeparam name="T">The type of input line to find.</typeparam>
    /// <param name="id">The unique identifier of the input line.</param>
    /// <param name="recursive">If true, searches through all child containers recursively.</param>
    /// <returns>The input line if found, otherwise null.</returns>
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
    /// Useful for preserving state between operations or exporting as dictionary.
    /// </summary>  
    public void UpdateCurrentConfigAsLastUsed()
    {
        _lastUsedValuesTracker.UpdateLastUsedValues();
    }

    /// <summary>  
    /// Retrieves the dictionary of the last used parameters.
    /// Used to export configuration to files or save state.
    /// </summary>  
    /// <returns>A dictionary containing the last used parameter IDs and their values.</returns>  
    public Dictionary<string, object> GetLastUsedConfig()
    {
        return _lastUsedValuesTracker.GetLastUsedValues();
    }

    /// <summary>  
    /// Loads the provided parameters into the container by matching them with the input lines' IDs.
    /// During loading, the <see cref="IsLoading"/> flag is set to true to prevent <see cref="ParametersChanged"/> events from firing.
    /// After loading, the <see cref="LoadCallback"/> method is called to allow derived classes to perform additional actions.
    /// </summary>  
    /// <param name="config">A dictionary containing parameter IDs and their corresponding values.</param>  
    public void LoadConfigFrom(Dictionary<string, object> config)
    {
        try
        {
            IsLoading = true;

            var options = GetChildren().OfType<IInputLine>().ToList();

            foreach (var parameter in config)
            {
                var id = parameter.Key;
                var value = parameter.Value;
                var inputLine = options.FirstOrDefault(x => x.Id == id);

                if (inputLine != null)
                {
                    inputLine.TrySetValue(value);
                }
            }

            LoadCallback();
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Sets the value tracker for all child input lines.
    /// </summary>
    private void SetInputLineTrackerForChildren()
    {
        foreach (var child in GetChildren())
        {
            SetInputLineTracker(child);
        }
    }

    /// <summary>
    /// Sets the value tracker for a specific node if it implements <see cref="IInputLine"/>.
    /// </summary>
    /// <param name="node">The node to set the value tracker for.</param>
    private void SetInputLineTracker(Node node)
    {
        if (node is IInputLine inputLine)
        {
            inputLine.SetValueTracker(_lastUsedValuesTracker.ValueTracker);
        }
    }

    /// <summary>
    /// Sets the font size for all child elements.
    /// </summary>
    private void SetOptionsFontSizeForChildren()
    {
        foreach (var child in GetChildren())
        {
            SetFontSize(child);
        }
    }

    /// <summary>
    /// Sets font size for  <see cref="IInputLine"/> elements and  <see cref="OptionsContainer"/> containers.
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
    /// Virtual method called after loading values from configuration.
    /// Override this method in derived classes to perform additional actions after loading,
    /// like value assignment for properties.
    /// </summary>
    protected virtual void LoadCallback()
    {
    }

    /// <summary>
    /// Sets a property value and invokes the <see cref="ParametersChanged"/> event.
    /// The event is only triggered if the container is not currently loading values.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="property">Reference to the property to set.</param>
    /// <param name="value">The new value to assign to the property.</param>
    protected void SetAndInvokeParametersChangedEvent<T>(ref T property, T value)
    {
        property = value;
        InvokeParametersChangedEvent();
    }

    /// <summary>
    /// Invokes the <see cref="ParametersChanged"/> event. This should be called whenever the parameters of the options change.
    /// </summary>
    protected void InvokeParametersChangedEvent()
    {
        if (!IsLoading)
        {
            ParametersChanged?.Invoke();
        }
    }
}