using Godot;
using System.Collections.Generic;

namespace TerrainGenerationApp.Utilities;

public class WorldData : IWorldData
{
    private float[,] _terrainMap;
    private float[,] _slopesMap;
    private float _waterLevel;
    private Dictionary<string, bool[,]> _treesMap;
    private Dictionary<string, Color> _treesColors;



    public float WaterLevel
    {
        get => _waterLevel;
        private set => _waterLevel = value;
    }

    public float[,] TerrainMap
    {
        get => _terrainMap;
        private set => _terrainMap = value;
    }

    public float[,] SlopesMap
    {
        get => _slopesMap;
        private set => _slopesMap = value;
    }

    public Dictionary<string, bool[,]> TreeMaps
    {
        get => _treesMap;
        private set => _treesMap = value;
    }

    public Dictionary<string, Color> TreeColors
    {
        get => _treesColors;
        private set => _treesColors = value;
    }

    public int MapHeight => _terrainMap.GetLength(0);
    public int MapWidth => _terrainMap.GetLength(1);


    public WorldData()
    {
        _treesMap = new Dictionary<string, bool[,]>();
        _treesColors = new Dictionary<string, Color>();
        _terrainMap = new float[1, 1];
        _slopesMap = new float[1, 1];
    }


    public void SetTerrain(float[,] terrainMap)
    {
        TerrainMap = terrainMap;
        SlopesMap = MapHelpers.GetSlopes(terrainMap);
        TreeMaps.Clear();
    }

    public float HeightAt(int row, int col)
    {
        return TerrainMap[row, col];
    }

    public float SlopeAt(int row, int col)
    {
        return SlopesMap[row, col];
    }
}
