using Godot;
using System.Collections.Generic;

namespace TerrainGenerationApp.Utilities;

public class WorldData : IWorldData
{
    private float[,] _terrainMap;
    private float[,] _slopesMap;
    private float _seaLevel;
    private Dictionary<string, bool[,]> _treesMap;
    private Dictionary<string, Color> _treesColors;



    public float SeaLevel
    {
        get => _seaLevel;
        private set => _seaLevel = value;
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

    public void SetSeaLevel(float value)
    {
        _seaLevel = (float)Mathf.Clamp(value, 0.0, 1.0);
    }


    public void SetTerrain(float[,] terrainMap)
    {
        TerrainMap = terrainMap;
        SlopesMap = MapHelpers.GetSlopes(terrainMap);
        TreeMaps.Clear();
    }

    public void SetTreeMaps(Dictionary<string, bool[,]> maps)
    {
        GD.Print("<TREE>Setting Tree maps! Count: " + maps.Count);

        TreeMaps.Clear();
        foreach (var map in maps)
        {
            TreeMaps[map.Key] = map.Value;
        }
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
