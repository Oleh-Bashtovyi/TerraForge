using System;
using TerrainGenerationApp.Domain.Extensions;

namespace TerrainGenerationApp.Domain.Visualization;

public class TerrainMeshSettings
{
    private int _gridCellResolution = 6;
    private float _gridCellSize = 1.0f;
    private float _heightScale = 15.0f;
    private MapExtensions.InterpolationType _meshInterpolation = MapExtensions.InterpolationType.Bilinear;

    public int GridCellResolution
    {
        get => _gridCellResolution;
        private set => _gridCellResolution = Math.Max(1, value);
    }
    public float GridCellSize
    {
        get => _gridCellSize;
        private set => _gridCellSize = Math.Max(0.1f, value);
    }
    public float HeightScale
    {
        get => _heightScale;
        private set => _heightScale = Math.Max(0.1f, value);
    }

    public MapExtensions.InterpolationType MeshInterpolation
    {
        get => _meshInterpolation;
        private set => _meshInterpolation = value;
    }

    public void SetGridCellResolution(int resolution)
    {
        GridCellResolution = resolution;
    }

    public void SetGridCellSize(float size)
    {
        GridCellSize = size;
    }

    public void SetHeightScale(float scale)
    {
        HeightScale = scale;
    }

    public void SetInterpolation(MapExtensions.InterpolationType interpolation)
    {
        MeshInterpolation = interpolation;
    }
}