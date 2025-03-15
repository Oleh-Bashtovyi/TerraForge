using System;

namespace TerrainGenerationApp.Utilities;

public static class MapHelpers
{
    private static readonly (int, int)[] DirectionsHvd =
    {
        (-1, 0),
        (1, 0),
        (0, -1),
        (0, 1),
        (-1, -1),
        (1, -1),
        (-1, 1),
        (1, 1)
    };


    public static float[,] SmoothMap(float[,] map)
    {
        var h = map.GetLength(0);
        var w = map.GetLength(1);
        var newMap = new float[h, w];

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                var sum = 0f;
                var count = 0;

                foreach (var (dy, dx) in DirectionsHvd)
                {
                    var nx = x + dx;
                    var ny = y + dy;

                    if (nx >= 0 && nx < w && ny >= 0 && ny < h)
                    {
                        sum += map[ny, nx];
                        count++;
                    }
                }

                newMap[y, x] = count > 0 ? sum / count : map[y, x];
            }
        }
        return newMap;
    }

    public static float[,] GetSlopes(float[,] map)
    {
        int h = map.GetLength(0);
        int w = map.GetLength(1);
        float[,] slopes = new float[h, w];

        float HeightOrDefault(int x, int y) =>
            x >= 0 && x < w && y >= 0 && y < h ? map[y, x] : 0f;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float vecX = 0.5f * (HeightOrDefault(x + 1, y) - HeightOrDefault(x - 1, y));
                float vecY = 0.5f * (HeightOrDefault(x, y + 1) - HeightOrDefault(x, y - 1));
                slopes[y, x] = MathF.Sqrt(vecX * vecX + vecY * vecY);
            }
        }
        return slopes;
    }

    public static void AddHeight(float[,] map, float value, float lowerBound = 0.0f, float upperBound = 1.0f)
    {
        int h = map.GetLength(0);
        int w = map.GetLength(1);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                map[y, x] = Math.Clamp(map[y, x] + value, lowerBound, upperBound);
            }
        }
    }

    public static void MultiplyHeight(float[,] map, float value, float lowerBound = 0.0f, float upperBound = 1.0f)
    {
        int h = map.GetLength(0);
        int w = map.GetLength(1);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                map[y, x] = Math.Clamp(map[y, x] * value, lowerBound, upperBound);
            }
        }
    }
}

