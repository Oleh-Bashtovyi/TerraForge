using System;

namespace TerrainGenerationApp.Domain.Visualization;

public class TerrainMeshSettings
{
    private int _gridCellResolution = 1;
    private float _gridCellSize = 1.0f;
    private float _heightScale = 1.0f;

    public int GridCellResolution
    {
        get => _gridCellResolution;
        set
        {
            _gridCellResolution = Math.Max(1, value);
        }
    }
    public float GridCellSize
    {
        get => _gridCellSize;
        set
        {
            _gridCellSize = Math.Max(0.1f, value);
        }
    }
    public float HeightScale
    {
        get => _heightScale;
        set
        {
            _heightScale = Math.Max(0.1f, value);
        }
    }
}