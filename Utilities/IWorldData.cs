
using System.Collections.Generic;
using Godot;

namespace TerrainGenerationApp.Utilities;

public interface IWorldData
{
    public float[,] TerrainHeightMap { get; }
    public float[,] TerrainSlopesMap { get; }
    public int MapHeight { get; }
    public int MapWidth { get; }
    public float SeaLevel { get; }
    public Dictionary<string, bool[,]> TreeMaps { get; }

    public Vector2I GetMapSize() => new (MapWidth, MapHeight);
}