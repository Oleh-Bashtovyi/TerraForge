using System;
using System.Collections.Generic;
using TerrainGenerationApp.Domain.Utils.TerrainUtils;

namespace TerrainGenerationApp.Domain.Generators;

public static class WorleyNoise
{
    // Worley Noise (also known as Cellular Noise) algorithm generates
    // organic-looking textures based on the distance to randomly placed points.
    // The resulting pattern resembles cells or voronoi diagrams and is useful for terrain features,
    // textures like stone, cracked surfaces, or cellular structures.
    //
    // Algorithm:
    // 1) Generate random "feature points" across the map
    // 2) For each pixel in the map, find the distance to the nearest feature point
    // 3) Normalize the distance values to create the noise map
    // 4) Optionally invert the values (closer points will be brighter instead of darker)

    public static float[,] GenerateMap(
        int mapHeight,
        int mapWidth,
        int dotsCount,
        float maxDistance = 100.0f,
        bool inverse = true,
        int seed = 0,
        DistanceType distanceType = DistanceType.Euclidean)
    {
        var map = new float[mapHeight, mapWidth];

        // Step 1: Generate random feature points
        var dots = GenerateDots(dotsCount, mapHeight, mapWidth, seed);

        // Step 2 & 3: Calculate distance field and normalize values
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                // Add 0.5 to get the center of the cell
                var cellX = x + 0.5f;
                var cellY = y + 0.5f;

                // Find distance to nearest feature point
                var minDistance = FindNearestDistance(cellX, cellY, dots, maxDistance, distanceType);

                // Normalize the distance value between 0 and 1
                map[y, x] = Math.Min(minDistance, maxDistance) / maxDistance;

                // Step 4: Optionally invert the values
                if (inverse)
                {
                    map[y, x] = 1.0f - map[y, x];
                }
            }
        }

        return map;
    }

    // Finds the distance to the nearest feature point from the given coordinates
    private static float FindNearestDistance(
        float x, float y,
        List<(float, float)> dots, 
        float maxDistance, 
        DistanceType distanceType = DistanceType.Euclidean)
    {
        var minDist = maxDistance;

        // Check distance to each feature point
        foreach (var (dotX, dotY) in dots)
        {
            // Calculate Euclidean distance
            //var dist = MathF.Sqrt((x - dotX) * (x - dotX) + (y - dotY) * (y - dotY));
            var dist = Distances.CalculateDistance(x, y, dotX, dotY, distanceType);

            // Keep track of minimum distance found
            if (dist < minDist)
            {
                minDist = dist;
            }
        }

        return minDist;
    }

    // Generates random feature points within the map dimensions
    private static List<(float, float)> GenerateDots(int count, int height, int width, int seed)
    {
        var rng = new Random(seed);
        var dots = new List<(float, float)>(count);

        // Generate the specified number of random points
        for (int i = 0; i < count; i++)
        {
            var x = (float)rng.NextDouble() * width;
            var y = (float)rng.NextDouble() * height;
            dots.Add((x, y));
        }

        return dots;
    }
}