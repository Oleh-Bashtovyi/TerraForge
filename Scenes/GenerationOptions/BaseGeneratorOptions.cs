using System;
using Godot;

namespace TerrainGenerationApp.Scenes.GenerationOptions;

public partial class BaseGeneratorOptions : VBoxContainer
{
	public event Action ParametersChanged;

	public virtual float[,] GenerateMap()
	{
		return new float[1, 1];
	}

    public virtual void DisableAllOptions()
    {
    }

    public virtual void EnableAllOptions()
    {
    }

    protected void InvokeParametersChangedEvent()
	{
		ParametersChanged?.Invoke();
	}
}
