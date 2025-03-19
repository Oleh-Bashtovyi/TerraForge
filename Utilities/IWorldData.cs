
using System.Collections.Generic;

namespace TerrainGenerationApp.Utilities;

public interface IWorldData
{
    public float[,] TerrainMap { get; }
    public float[,] SlopesMap { get; }
    public float WaterLevel { get; }
    public Dictionary<string, bool[,]> TreeMaps { get; }
}