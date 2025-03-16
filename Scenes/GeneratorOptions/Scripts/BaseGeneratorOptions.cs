using System;
using Godot;

namespace TerrainGenerationApp.Scenes.GeneratorOptions.Scripts;

public partial class BaseGeneratorOptions : VBoxContainer
{
    public event Action ParametersChanged;

    public virtual float[,] GenerateMap()
    {
        return new float[1, 1];
    }

    protected void InvokeParametersChangedEvent()
    {
        ParametersChanged?.Invoke();
    }
}