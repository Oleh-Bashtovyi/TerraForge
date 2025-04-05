using Godot;
using System;
using TerrainGenerationApp.Scenes.BuildingBlocks;

namespace TerrainGenerationApp.Scenes.GenerationOptions;

/// <summary>
/// Base class for generator options, inheriting from VBoxContainer.
/// </summary>
public partial class BaseGeneratorOptions : VBoxContainer
{
    public event Action ParametersChanged;

    public override void _Ready()
    {
        InputLineManager.CreateInputLinesForObject(this, this);
    }

    public virtual float[,] GenerateMap()
    {
        return new float[1, 1];
    }

    public void DisableAllOptions()
    {
        foreach (Node child in GetChildren())
        {
            if (child is InputLine inputLine)
            {
                inputLine.DisableInput();
            }
        }
    }

    public void EnableAllOptions()
    {
        foreach (Node child in GetChildren())
        {
            if (child is InputLine inputLine)
            {
                inputLine.EnableInput();
            }
        }
    }

    protected void InvokeParametersChangedEvent()
    {
        ParametersChanged?.Invoke();
    }
}
