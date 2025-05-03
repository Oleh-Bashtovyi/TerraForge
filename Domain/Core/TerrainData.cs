using Godot;
using TerrainGenerationApp.Domain.Enums;
using TerrainGenerationApp.Domain.Extensions;
using TerrainGenerationApp.Domain.Utils;
using TerrainGenerationApp.Domain.Utils.TerrainUtils;

namespace TerrainGenerationApp.Domain.Core;

public class TerrainData
{
    private readonly Logger<TerrainData> _logger = new();
    private float[,] _heightMap = new float[1, 1];
    private float[,] _slopesMap = new float[1, 1];

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

    public bool SlopesGenerated { get; private set; } = true;

    public int TerrainMapHeight => _heightMap.Height();
    public int TerrainMapWidth => _heightMap.Width();

    public Vector2I GetMapSize() => new Vector2I(TerrainMapWidth, TerrainMapHeight);

    public float[,] GetHeightMapCopy() => _heightMap.Copy();

    public float[,] GetSlopesMapCopy() => _slopesMap.Copy();

    public void Clear()
    {
        _heightMap = new float[1, 1];
        _slopesMap = new float[1, 1];
    }

    public void SetTerrain(float[,] terrainMap, bool calculateSlopes = true)
    {
        if (terrainMap == null)
        {
            _logger.LogError("Terrain map is null");
            return;
        }

        HeightMap = terrainMap.Copy();
        SlopesMap = calculateSlopes ? MapHelpers.GetSlopes(terrainMap) : new float[terrainMap.Height(), terrainMap.Width()];
        SlopesGenerated = calculateSlopes;
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

    public float HeightAt(Vector2I position, MapInterpolation mapInterpolation = MapInterpolation.Bilinear)
    {
        return HeightMap.GetValueAt(position, mapInterpolation);
    }

    public float HeightAt(Vector2 position, MapInterpolation mapInterpolation = MapInterpolation.Bilinear)
    {
        return HeightMap.GetValueAt(position, mapInterpolation);
    }

    public float SlopeAt(Vector2I position, MapInterpolation mapInterpolation = MapInterpolation.Bilinear)
    {
        return SlopesMap.GetValueAt(position, mapInterpolation);
    }

    public float SlopeAt(Vector2 position, MapInterpolation mapInterpolation = MapInterpolation.Bilinear)
    {
        return SlopesMap.GetValueAt(position, mapInterpolation);
    }

    public float SlopeAt(int row, int col, MapInterpolation mapInterpolation = MapInterpolation.Bilinear)
    {
        return SlopesMap.GetValueAt(row, col, mapInterpolation);
    }

    public float SlopeAt(float row, float col, MapInterpolation mapInterpolation = MapInterpolation.Bilinear)
    {
        return SlopesMap.GetValueAt(row, col, mapInterpolation);
    }
}

