using Godot;
using TerrainGenerationApp.Domain.Extensions;
using TerrainGenerationApp.Domain.Utils.TerrainUtils;

namespace TerrainGenerationApp.Domain.Core;

public class TerrainData
{
    private float[,] _heightMap;
    private float[,] _slopesMap;

    public float[,] HeightMap
    {
        get => _heightMap;
        private set => _heightMap = value;
    }

    public float[,] SlopesMap
    {
        get => _slopesMap;
        private set => _slopesMap = value;
    }

    public int TerrainMapHeight => _heightMap.Height();
    public int TerrainMapWidth => _heightMap.Width();

    public TerrainData()
    {
        _heightMap = new float[1, 1];
        _slopesMap = new float[1, 1];
    }

    public Vector2I GetMapSize()
    {
        return new Vector2I(TerrainMapWidth, TerrainMapHeight);
    }

    public void Clear()
    {
        _heightMap = new float[1, 1];
        _slopesMap = new float[1, 1];
    }

    public void SetTerrain(float[,] terrainMap)
    {
        HeightMap = terrainMap;
        SlopesMap = MapHelpers.GetSlopes(terrainMap);
    }

    public float HeightAt(int row, int col)
    {
        return HeightMap.GetValueAt(row, col);
    }

    public float HeightAt(float row, float col)
    {
        return HeightMap.GetValueAt(row, col);
    }

    public float HeightAt(Vector2I position)
    {
        return HeightMap.GetValueAt(position);
    }

    public float HeightAt(Vector2 position)
    {
        return HeightMap.GetValueAt(position);
    }

    public float SlopeAt(Vector2I position)
    {
        return SlopesMap.GetValueAt(position);
    }

    public float SlopeAt(Vector2 position)
    {
        return SlopesMap.GetValueAt(position);
    }

    public float SlopeAt(int row, int col)
    {
        return SlopesMap.GetValueAt(row, col);
    }

    public float SlopeAt(float row, float col)
    {
        return SlopesMap.GetValueAt(row, col);
    }
}

