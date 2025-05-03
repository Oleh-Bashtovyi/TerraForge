using System;
using Godot;
using TerrainGenerationApp.Domain.Generators;

namespace TerrainGenerationApp.Domain.Generators.DomainWarping;

public class DomainWarpingApplier : IDomainWarpingApplier
{
    private readonly PerlinNoiseGenerator xNoise;
    private readonly PerlinNoiseGenerator yNoise;
    private float warpingStrength = 1.0f;

    public float WarpingStrength
    {
        get => warpingStrength;
        set => warpingStrength = value;
    }

    public PerlinNoiseGenerator XNoise
    {
        get => xNoise;
    }

    public PerlinNoiseGenerator YNoise
    {
        get => yNoise;
    }



    public DomainWarpingApplier()
    {
        xNoise = new PerlinNoiseGenerator { Octaves = 1, Offset = new Vector2(120, 40), Frequency = 0.125f };
        yNoise = new PerlinNoiseGenerator { Octaves = 1, Offset = new Vector2(3479, 9823), Frequency = 0.125f };
    }


    public float[,] ApplyWarping(float[,] map)
    {
        var height = map.GetLength(0);
        var width = map.GetLength(1);
        var xOffsets = xNoise.GenerateMap(height, width);
        var yOffsets = yNoise.GenerateMap(height, width);
        return Sample(map, xOffsets, yOffsets);
    }

    private float[,] Sample(float[,] map, float[,] xOffsets, float[,] yOffsets)
    {
        var height = map.GetLength(0);
        var width = map.GetLength(1);
        var newMap = new float[height, width];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float xSample = x + (xOffsets[y, x] * 2 - 1) * warpingStrength;
                float ySample = y + (yOffsets[y, x] * 2 - 1) * warpingStrength;

                int x0 = (int)Math.Floor(xSample);
                int y0 = (int)Math.Floor(ySample);
                int x1 = x0 + 1;
                int y1 = y0 + 1;

                x0 = (x0 % width + width) % width;
                y0 = (y0 % height + height) % height;
                x1 = (x1 % width + width) % width;
                y1 = (y1 % height + height) % height;

                float fx = xSample - (float)Math.Floor(xSample);
                float fy = ySample - (float)Math.Floor(ySample);

                float v00 = map[y0, x0];
                float v10 = map[y0, x1];
                float v01 = map[y1, x0];
                float v11 = map[y1, x1];

                newMap[y, x] = Lerp(Lerp(v00, v10, fx), Lerp(v01, v11, fx), fy);
            }
        }

        return newMap;
    }

    private static float Lerp(float a, float b, float t)
    {
        return a + t * (b - a);
    }


}