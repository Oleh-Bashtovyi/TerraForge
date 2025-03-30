using System.Collections.Generic;
using Godot;
using TerrainGenerationApp.Enums;

namespace TerrainGenerationApp.Utilities;

public interface IDisplayOptionsProvider
{
    public MapDisplayFormat CurDisplayFormat { get; }
    public float CurSlopeThreshold { get; }
    public Dictionary<string, Color> TreeColors { get; }
}