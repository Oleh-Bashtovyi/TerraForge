using Godot;
using System;

namespace TerrainGenerationApp.Scenes.GenerationOptions;

/// <summary>
/// Base class for generator options, inheriting from VBoxContainer.
/// </summary>
public partial class BaseGeneratorOptions : VBoxContainer
{
    /// <summary>
    /// Event triggered when parameters are changed.
    /// </summary>
    public event Action ParametersChanged;

    /// <summary>
    /// Generates a map with default implementation.
    /// </summary>
    /// <returns>A 2D array of floats representing the map.</returns>
    public virtual float[,] GenerateMap()
    {
        return new float[1, 1];
    }

    /// <summary>
    /// Disables all options. To be overridden in derived classes.
    /// </summary>
    public virtual void DisableAllOptions()
    {
    }

    /// <summary>
    /// Enables all options. To be overridden in derived classes.
    /// </summary>
    public virtual void EnableAllOptions()
    {
    }

    /// <summary>
    /// Invokes the ParametersChanged event.
    /// </summary>
    protected void InvokeParametersChangedEvent()
    {
        ParametersChanged?.Invoke();
    }
}
