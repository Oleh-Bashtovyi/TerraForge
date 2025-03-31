
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
    public float GetHeightAt(Vector2I position) => TerrainHeightMap[position.Y, position.X];
    public float GetHeightAt(int row, int col) => TerrainHeightMap[row, col];
    public float GetSlopeAt(Vector2I position) => TerrainSlopesMap[position.Y, position.X];
    public float GetSlopeAt(int row, int col) => TerrainSlopesMap[row, col];
}