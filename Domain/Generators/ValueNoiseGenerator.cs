using System;

namespace TerrainGenerationApp.Domain.Generators;

public class ValueNoiseGenerator : NoiseMapGenerator
{
    private int _seed;
    private Random _random;
    private float[,,] _noiseMap;
    private const int NoiseMapSize = 256;

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

    public ValueNoiseGenerator()
    {
        _random = new Random(_seed);
        GenerateNoiseMap();
    }

    private void GenerateNoiseMap()
    {
        _noiseMap = new float[NoiseMapSize, NoiseMapSize, NoiseMapSize];

        // Fill the noise map with random values  
        for (int y = 0; y < NoiseMapSize; y++)
        {
            for (int x = 0; x < NoiseMapSize; x++)
            {
                for (int z = 0; z < NoiseMapSize; z++)
                {
                    _noiseMap[z, x, y] = (float)_random.NextDouble();
                }
            }
        }
    }
    public override float Noise1D(float x)
    {
        // Determine integer coordinates for interpolation  
        int x0 = (int)Math.Floor(x) & (NoiseMapSize - 1);
        int x1 = (x0 + 1) & (NoiseMapSize - 1);

        // Local coordinates for interpolation  
        float dx = x - (float)Math.Floor(x);

        // Smooth local coordinates  
        float u = Fade(dx);

        // Get noise values at the corresponding points  
        float v0 = _noiseMap[0, x0, 0];
        float v1 = _noiseMap[0, x1, 0];

        // Interpolate between values  
        return Lerp(v0, v1, u);
    }

    public override float Noise2D(float x, float y)
    {
        // Determine integer coordinates for interpolation  
        int x0 = (int)Math.Floor(x) & (NoiseMapSize - 1);
        int y0 = (int)Math.Floor(y) & (NoiseMapSize - 1);
        int x1 = (x0 + 1) & (NoiseMapSize - 1);
        int y1 = (y0 + 1) & (NoiseMapSize - 1);

        // Local coordinates for interpolation  
        float dx = x - (float)Math.Floor(x);
        float dy = y - (float)Math.Floor(y);

        // Smooth local coordinates  
        float u = Fade(dx);
        float v = Fade(dy);

        // Get noise values at the corners of the square  
        float v00 = _noiseMap[0, x0, y0];
        float v10 = _noiseMap[0, x1, y0];
        float v01 = _noiseMap[0, x0, y1];
        float v11 = _noiseMap[0, x1, y1];

        // Bilinear interpolation  
        float nx0 = Lerp(v00, v10, u);
        float nx1 = Lerp(v01, v11, u);
        return Lerp(nx0, nx1, v);
    }

    public override float Noise3D(float x, float y, float z)
    {
        // Determine integer coordinates for interpolation  
        int x0 = (int)Math.Floor(x) & (NoiseMapSize - 1);
        int y0 = (int)Math.Floor(y) & (NoiseMapSize - 1);
        int z0 = (int)Math.Floor(z) & (NoiseMapSize - 1);
        int x1 = (x0 + 1) & (NoiseMapSize - 1);
        int y1 = (y0 + 1) & (NoiseMapSize - 1);
        int z1 = (z0 + 1) & (NoiseMapSize - 1);
        // Local coordinates for interpolation  
        float dx = x - (float)Math.Floor(x);
        float dy = y - (float)Math.Floor(y);
        float dz = z - (float)Math.Floor(z);
        // Smooth local coordinates  
        float u = Fade(dx);
        float v = Fade(dy);
        float w = Fade(dz);
        // Get noise values at the corners of the cube  
        float v000 = _noiseMap[x0, y0, z0];
        float v100 = _noiseMap[x1, y0, z0];
        float v010 = _noiseMap[x0, y1, z0];
        float v110 = _noiseMap[x1, y1, z0];
        float v001 = _noiseMap[x0, y0, z1];
        float v101 = _noiseMap[x1, y0, z1];
        float v011 = _noiseMap[x0, y1, z1];
        float v111 = _noiseMap[x1, y1, z1];
        // Trilinear interpolation  
        return Lerp(
            Lerp(
                Lerp(v000, v100, u), 
                Lerp(v010, v110, u), 
                v), 
            Lerp(
                Lerp(v001, v101, u), 
                Lerp(v011, v111, u), 
                v),
            w);
    }
}
