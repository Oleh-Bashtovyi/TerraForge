using Godot;
using System;
using System.Collections.Generic;

namespace TerrainGenerationApp.Utilities;

/// <summary>
/// Provides helper methods for map manipulation and terrain generation.
/// </summary>
public static class MapHelpers
{
    private static readonly (int, int)[] DirectionsHorizontalVerticalDiagonals =
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

    /// <summary>
    /// Smooths the given map by averaging the values of neighboring cells.
    /// </summary>
    /// <param name="map">The input map to be smoothed.</param>
    /// <returns>A new smoothed map.</returns>
    public static float[,] SmoothMap(float[,] map)
    {
        var h = map.Height();
        var w = map.Width();
        var newMap = new float[h, w];

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                var sum = 0f;
                var count = 0;

                foreach (var (dy, dx) in DirectionsHorizontalVerticalDiagonals)
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

    /// <summary>
    /// Calculates the slopes of the given map.
    /// </summary>
    /// <param name="map">The input map to calculate slopes for.</param>
    /// <returns>A new map containing the slopes.</returns>
    public static float[,] GetSlopes(float[,] map)
    {
        var h = map.Height();
        var w = map.Width();
        var slopes = new float[h, w];

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

    /// <summary>
    /// Adds a specified value to the height of each cell in the map, clamping the result within the given bounds.
    /// </summary>
    /// <param name="map">The input map to modify.</param>
    /// <param name="value">The value to add to each cell.</param>
    /// <param name="lowerBound">The lower bound for clamping.</param>
    /// <param name="upperBound">The upper bound for clamping.</param>
    public static void AddHeight(float[,] map, float value, float lowerBound = 0.0f, float upperBound = 1.0f)
    {
        var h = map.Height();
        var w = map.Width();

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                map[y, x] = Math.Clamp(map[y, x] + value, lowerBound, upperBound);
            }
        }
    }

    /// <summary>
    /// Multiplies the height of each cell in the map by a specified value, clamping the result within the given bounds.
    /// </summary>
    /// <param name="map">The input map to modify.</param>
    /// <param name="value">The value to multiply each cell by.</param>
    /// <param name="lowerBound">The lower bound for clamping.</param>
    /// <param name="upperBound">The upper bound for clamping.</param>
    public static void MultiplyHeight(float[,] map, float value, float lowerBound = 0.0f, float upperBound = 1.0f)
    {
        var h = map.Height();
        var w = map.Width();

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                map[y, x] = Math.Clamp(map[y, x] * value, lowerBound, upperBound);
            }
        }
    }

    /// <summary>
    /// Generates a list of random dots within the specified bounds.
    /// </summary>
    /// <param name="count">The number of dots to generate.</param>
    /// <param name="minX">The minimum x-coordinate.</param>
    /// <param name="maxX">The maximum x-coordinate.</param>
    /// <param name="minY">The minimum y-coordinate.</param>
    /// <param name="maxY">The maximum y-coordinate.</param>
    /// <param name="seed">The seed for the random number generator.</param>
    /// <returns>A list of generated dots.</returns>
    public static List<Vector2> GenerateDots(int count, float minX, float maxX, float minY, float maxY, ulong seed)
    {
        var rng = new RandomNumberGenerator();
        var dots = new List<Vector2>(count);
        rng.Seed = seed;

        for (int i = 0; i < count; i++)
        {
            var x = rng.RandfRange(minX, maxX);
            var y = rng.RandfRange(minY, maxY);
            dots.Add(new Vector2(x, y));
        }

        return dots;
    }

    /// <summary>
    /// Finds the nearest point to the specified point from a list of dots.
    /// </summary>
    /// <param name="from">The point to find the nearest point to.</param>
    /// <param name="dots">The list of dots to search.</param>
    /// <returns>The nearest point to the specified point.</returns>
    public static Vector2 FindNearestPoint(Vector2 from, List<Vector2> dots)
    {
        if (dots == null || dots.Count == 0)
        {
            return from;
        }

        var nearest = dots[0];
        var minDist = float.MaxValue;

        foreach (var dot in dots)
        {
            var dist = Distances.Euclidean(from, dot);

            if (dist < minDist)
            {
                minDist = dist;
                nearest = dot;
            }
        }

        return nearest;
    }
}

