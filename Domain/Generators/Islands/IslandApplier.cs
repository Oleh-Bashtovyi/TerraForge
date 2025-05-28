using Godot;
using System;
using System.Collections.Generic;
using TerrainGenerationApp.Domain.Utils.TerrainUtils;

namespace TerrainGenerationApp.Domain.Generators.Islands;

public class IslandApplier : IIslandsApplier
{
    public enum IslandType
    {
        SingleAtCenter,
        SingleRandomly,
        Many
    }

    public enum DistanceType
    {
        Euclidean,
        EuclideanSquared,
        Manhattan,
        Diagonal,
        Hyperboloid,
        Blob,
        SquareBump
    }

    public IslandType ApplierType { get; set; } = IslandType.SingleAtCenter;
    public DistanceType DistanceFunction { get; set; } = DistanceType.Euclidean;
    public float RadiusAroundIslands { get; set; } = 55f;
    // Далі радіуса фактор не буде менший за MinDistanceFactor, щоб поза островом не було нульових висот
    public float MinDistanceFactor { get; set; } = 0.2f;
    // For Many islands type
    public int IslandsCount { get; set; } = 1;
    // Щоб острови не знаходились на краях, їх треба здвинути ближче до центра
    // [0; 1] - наприклад, якщо значення буде 0.5, то зліва буде відступ 25% і справа 25%
    public float HorizontalOffsetsToCenter { get; set; } = 0.2f;
    // [0; 1]
    public float VerticalOffsetsToCenter { get; set; } = 0.2f;
    public ulong Seed { get; set; } = 0;
    // Контроль сили змішування
    public float MixStrength { get; set; } = 0.8f;


    public float[,] ApplyIslands(float[,] map)
    {
        var h = map.GetLength(0);
        var w = map.GetLength(1);
        var newMap = new float[h, w];
        var mapCenterX = w / 2.0f;
        var mapCenterY = h / 2.0f;
        float xOffset = w * HorizontalOffsetsToCenter / 2.0f;
        float yOffset = h * VerticalOffsetsToCenter / 2.0f;
        float xMinBound = 0 + xOffset;
        float xMaxBound = w - xOffset;
        float yMinBound = 0 + yOffset;
        float yMaxBound = h - yOffset;

        List<Vector2> islandCenters = new();
        switch (ApplierType)
        {
            case IslandType.SingleAtCenter:
                islandCenters = new List<Vector2>() { new(mapCenterX, mapCenterY) };
                break;
            case IslandType.SingleRandomly:
                islandCenters = MapHelpers.GenerateDots(1, xMinBound, xMaxBound, yMinBound, yMaxBound, Seed);
                break;
            case IslandType.Many:
                islandCenters = MapHelpers.GenerateDots(IslandsCount, xMinBound, xMaxBound, yMinBound, yMaxBound, Seed);
                break;
        }

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                var dotCenter = new Vector2(x + 0.5f, y + 0.5f);
                var nearestPoint = MapHelpers.FindNearestPoint(dotCenter, islandCenters);

                // Розрахунок відстані до найближчого центру острову
                float distance = CalculateDistance(dotCenter.X, dotCenter.Y, nearestPoint.X, nearestPoint.Y);

                // Нормалізація відстані відносно заданого радіуса
                float normalizedDistance = Math.Min(1.0f, distance / RadiusAroundIslands);

                /*                // Застосування мінімального фактора на відстані 
                                float distanceFactor = Math.Max(MinDistanceFactor, normalizedDistance);

                                // Інвертування фактора для острова (центр = високий, край = низький)
                                float islandShapeFactor = 1.0f - distanceFactor;*/


                // Інвертування фактора для острова (центр = високий, край = низький)
                float islandShapeFactor = Math.Max(1.0f - normalizedDistance, MinDistanceFactor);




                // Змішування карти з фактором острова
                float originalElevation = map[y, x];
                float newElevation = LinearInterpolation(originalElevation, islandShapeFactor, MixStrength);

                newMap[y, x] = newElevation;
            }
        }

        return newMap;
    }

    private float CalculateDistance(float x1, float y1, float x2, float y2)
    {
        return DistanceFunction switch
        {
            DistanceType.Euclidean => Distances.Euclidean(x1, y1, x2, y2),
            DistanceType.EuclideanSquared => Distances.EuclideanSquared(x1, y1, x2, y2),
            DistanceType.Manhattan => Distances.Manhattan(x1, y1, x2, y2),
            DistanceType.Blob => Distances.Blob(x1, y1, x2, y2),
            DistanceType.Diagonal => Distances.Diagonal(x1, y1, x2, y2),
            DistanceType.Hyperboloid => Distances.Hyperboloid(x1, y1, x2, y2),
            DistanceType.SquareBump => Distances.SquareBump(x1, y1, x2, y2),
            _ => Distances.Euclidean(x1, y1, x2, y2)
        };
    }

    private float LinearInterpolation(float a, float b, float t)
    {
        return a * (1.0f - t) + b * t;
    }
}

