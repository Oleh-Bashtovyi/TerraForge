using Godot;
using System;

namespace TerrainGenerationApp.Domain.Generators;

public class ValueNoiseGenerator
{
    private int _seed = 0;
    private float _frequency = 0.01f;
    private int _octaves = 2;
    private Vector2 _offset = Vector2.Zero;
    private float _persistence = 0.5f;
    private float _lacunarity = 2.0f;
    private float _warpingStrength = 1.0f;
    private float _warpingSize = 1.0f;
    private bool _enableWarping = true;
    private Random _random;

    // Two-dimensional array for storing generated random values  
    private float[,] _noiseMap;
    private readonly int _noiseMapSize = 256;

    public int Seed
    {
        get => _seed;
        set
        {
            _seed = value;
            _random = new Random(_seed);
            GenerateNoiseMap();
        }
    }

    public float Frequency
    {
        get => _frequency;
        set => _frequency = value;
    }

    public Vector2 Offset
    {
        get => _offset;
        set => _offset = value;
    }

    public int Octaves
    {
        get => _octaves;
        set => _octaves = Mathf.Clamp(value, 1, 10);
    }

    public float Persistence
    {
        get => _persistence;
        set => _persistence = value;
    }

    public float Lacunarity
    {
        get => _lacunarity;
        set => _lacunarity = value;
    }

    public float WarpingStrength
    {
        get => _warpingStrength;
        set => _warpingStrength = value;
    }

    public float WarpingSize
    {
        get => _warpingSize;
        set => _warpingSize = value;
    }

    public bool EnableWarping
    {
        get => _enableWarping;
        set => _enableWarping = value;
    }

    public ValueNoiseGenerator()
    {
        _random = new Random(_seed);
        GenerateNoiseMap();
    }

    private void GenerateNoiseMap()
    {
        _noiseMap = new float[_noiseMapSize, _noiseMapSize];

        // Fill the noise map with random values  
        for (int y = 0; y < _noiseMapSize; y++)
        {
            for (int x = 0; x < _noiseMapSize; x++)
            {
                _noiseMap[x, y] = (float)_random.NextDouble();
            }
        }
    }

    public float[,] GenerateMap(int mapHeight, int mapWidth)
    {
        var map = new float[mapHeight, mapWidth];
        var wstr = _warpingStrength;
        var wsize = _warpingSize;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float xSample = (x + _offset.X) * _frequency;
                float ySample = (y + _offset.Y) * _frequency;
                float height = 0.0f;

                if (_enableWarping)
                {
                    // Apply domain warping using base value noise  
                    float warpX = Value2D(xSample * wsize, ySample * wsize) * wstr;
                    map[y, x] = Value2DOctaves(xSample + warpX, ySample + warpX);
                }
                else
                {
                    map[y, x] = Value2DOctaves(xSample, ySample);
                }
            }
        }
        return map;
    }

    public float Value1DOctaves(float x)
    {
        float value = 0.0f;
        float amplitude = 1.0f;
        float frequency = 1.0f;
        float maxValue = 0.0f;

        for (int i = 0; i < _octaves; i++)
        {
            value += Value1D(x * frequency) * amplitude;
            maxValue += amplitude;
            amplitude *= _persistence;
            frequency *= _lacunarity;
        }

        // Normalize to the range [0, 1]  
        return value / maxValue;
    }

    public float Value2DOctaves(float x, float y)
    {
        float value = 0.0f;
        float amplitude = 1.0f;
        float frequency = 1.0f;
        float maxValue = 0.0f;

        for (int i = 0; i < _octaves; i++)
        {
            value += Value2D(x * frequency, y * frequency) * amplitude;
            maxValue += amplitude;
            amplitude *= _persistence;
            frequency *= _lacunarity;
        }

        // Normalize to the range [0, 1]  
        return value / maxValue;
    }

    public float Value1D(float x)
    {
        // Determine integer coordinates for interpolation  
        int x0 = (int)Math.Floor(x) & (_noiseMapSize - 1);
        int x1 = (x0 + 1) & (_noiseMapSize - 1);

        // Local coordinates for interpolation  
        float dx = x - (float)Math.Floor(x);

        // Smooth local coordinates  
        float u = Fade(dx);

        // Get noise values at the corresponding points  
        float v0 = _noiseMap[x0, 0];
        float v1 = _noiseMap[x1, 0];

        // Interpolate between values  
        return Lerp(v0, v1, u);
    }

    public float Value2D(float x, float y)
    {
        // Determine integer coordinates for interpolation  
        int x0 = (int)Math.Floor(x) & (_noiseMapSize - 1);
        int y0 = (int)Math.Floor(y) & (_noiseMapSize - 1);
        int x1 = (x0 + 1) & (_noiseMapSize - 1);
        int y1 = (y0 + 1) & (_noiseMapSize - 1);

        // Local coordinates for interpolation  
        float dx = x - (float)Math.Floor(x);
        float dy = y - (float)Math.Floor(y);

        // Smooth local coordinates  
        float u = Fade(dx);
        float v = Fade(dy);

        // Get noise values at the corners of the square  
        float v00 = _noiseMap[x0, y0];
        float v10 = _noiseMap[x1, y0];
        float v01 = _noiseMap[x0, y1];
        float v11 = _noiseMap[x1, y1];

        // Bilinear interpolation  
        float nx0 = Lerp(v00, v10, u);
        float nx1 = Lerp(v01, v11, u);
        return Lerp(nx0, nx1, v);
    }

    protected float Fade(float t)
    {
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    protected float Lerp(float a, float b, float t)
    {
        return a + t * (b - a);
    }
}
