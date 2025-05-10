using Godot;
using System;
using TerrainGenerationApp.Domain.Enums;
using TerrainGenerationApp.Domain.Extensions;
using TerrainGenerationApp.Domain.Utils.TerrainUtils;

namespace TerrainGenerationApp.Domain.Core;

public class TerrainData
{
    public const float MIN_HEIGHT = 0.0f;
    public const float MAX_HEIGHT = 1.0f;
    public const float MIN_MOISTURE = 0.0f;
    public const float MAX_MOISTURE = 1.0f;
    public const int DEFAULT_MAP_SIZE = 10;

    private float[,] _heightMap = new float[DEFAULT_MAP_SIZE, DEFAULT_MAP_SIZE];
    private float[,] _slopesMap = new float[DEFAULT_MAP_SIZE, DEFAULT_MAP_SIZE];
    private float[,] _moistureMap = new float[DEFAULT_MAP_SIZE, DEFAULT_MAP_SIZE];

    public float[,] HeightMap
    {
        get => _heightMap;
        private set => _heightMap = value;
    }

    private float[,] SlopesMap
    {
        get => _slopesMap;
        set => _slopesMap = value;
    }

    private float[,] MoistureMap
    {
        get => _moistureMap;
        set => _moistureMap = value;
    }

    public bool SlopesGenerated { get; private set; } = true;
    public int TerrainMapHeight => _heightMap.Height();
    public int TerrainMapWidth => _heightMap.Width();

    public Vector2I GetMapSize() => new(TerrainMapWidth, TerrainMapHeight);

    public float[,] GetHeightMapCopy() => _heightMap.Copy();

    public float[,] GetSlopesMapCopy() => _slopesMap.Copy();

    public void Clear()
    {
        _heightMap = new float[DEFAULT_MAP_SIZE, DEFAULT_MAP_SIZE];
        _slopesMap = new float[DEFAULT_MAP_SIZE, DEFAULT_MAP_SIZE];
        _moistureMap = new float[DEFAULT_MAP_SIZE, DEFAULT_MAP_SIZE];
    }

    public void SetTerrain(float[,] terrainMap, bool calculateSlopes = true)
    {
        ArgumentNullException.ThrowIfNull(terrainMap);
        HeightMap = terrainMap.CopyAndClampValues(MIN_HEIGHT, MAX_HEIGHT);
        SlopesMap = calculateSlopes ? MapHelpers.GetSlopes(terrainMap) : new float[terrainMap.Height(), terrainMap.Width()];
        MoistureMap = !MoistureMap.HasSameSizeAs(HeightMap) ? MoistureMap.ScaleTo(HeightMap) : MoistureMap;
        SlopesGenerated = calculateSlopes;
    }

    public void SetMoistureMap(float[,] moistureMap)
    {
        ArgumentNullException.ThrowIfNull(moistureMap);
        MoistureMap = moistureMap.HasSameSizeAs(HeightMap) ? 
            moistureMap.CopyAndClampValues(MIN_MOISTURE, MAX_MOISTURE) : 
            moistureMap.ScaleTo(HeightMap).ClampValues(MIN_MOISTURE, MAX_MOISTURE);
    }

    public void RegenerateSlopesMap()
    {
        if (!SlopesGenerated)
        {
            SlopesMap = MapHelpers.GetSlopes(HeightMap);
            SlopesGenerated = true;
        }
    }

    public float HeightAt(int row, int col, MapInterpolation mapInterpolation = MapInterpolation.Bilinear)
    {
        return HeightMap.GetValueAt(row, col, mapInterpolation);
    }

    public float HeightAt(float row, float col, MapInterpolation mapInterpolation = MapInterpolation.Bilinear)
    {
        return HeightMap.GetValueAt(row, col, mapInterpolation);
    }

    public float HeightAt(Vector2 position, MapInterpolation mapInterpolation = MapInterpolation.Bilinear)
    {
        return HeightMap.GetValueAt(position, mapInterpolation);
    }

    public float SlopeAt(int row, int col, MapInterpolation mapInterpolation = MapInterpolation.Bilinear)
    {
        return SlopesMap.GetValueAt(row, col, mapInterpolation);
    }

    public float SlopeAt(float row, float col, MapInterpolation mapInterpolation = MapInterpolation.Bilinear)
    {
        return SlopesMap.GetValueAt(row, col, mapInterpolation);
    }

    public float SlopeAt(Vector2 position, MapInterpolation mapInterpolation = MapInterpolation.Bilinear)
    {
        return SlopesMap.GetValueAt(position, mapInterpolation);
    }

    public float MoistureAt(int row, int col, MapInterpolation mapInterpolation = MapInterpolation.Bilinear)
    {
        return MoistureMap.GetValueAt(row, col, mapInterpolation);
    }

    public float MoistureAt(float row, float col, MapInterpolation mapInterpolation = MapInterpolation.Bilinear)
    {
        return MoistureMap.GetValueAt(row, col, mapInterpolation);
    }

    public float MoistureAt(Vector2 position, MapInterpolation mapInterpolation = MapInterpolation.Bilinear)
    {
        return MoistureMap.GetValueAt(position, mapInterpolation);
    }

}
