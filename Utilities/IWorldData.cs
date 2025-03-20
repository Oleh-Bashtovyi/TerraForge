
using System.Collections.Generic;

namespace TerrainGenerationApp.Utilities;

public interface IWorldData
{
    public float[,] TerrainMap { get; }
    public float[,] SlopesMap { get; }
    public int MapHeight { get; }
    public int MapWidth { get; }
    public float SeaLevel { get; }
    public Dictionary<string, bool[,]> TreeMaps { get; }
}