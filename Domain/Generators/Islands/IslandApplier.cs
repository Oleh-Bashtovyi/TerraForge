using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TerrainGenerationApp.Domain.Utils.TerrainUtils;

namespace TerrainGenerationApp.Domain.Generators.Islands;

public class IslandApplier : IIslandsApplier
{
    public enum CenterType
    {
        Single,
        SingleRandomly,
        Many
    }

    public CenterType ApplierType { get; set; } = CenterType.Single;
    public DistanceType DistanceFunction { get; set; } = DistanceType.Euclidean;
    public float RadiusAroundIslands { get; set; } = 55f;
    // Мінімальний фактор впливу (на краях острову)
    public float MinDistanceFactor { get; set; } = 0.0f;
    // Максимальний фактор впливу (в центрі острову)
    public float MaxDistanceFactor { get; set; } = 1.0f;
    // For Many islands type
    public int CentersCount { get; set; } = 1;
    // Щоб острови не знаходились на краях, їх треба здвинути ближче до центра
    // [0; 1] - наприклад, якщо значення буде 0.5, то зліва буде відступ 25% і справа 25%
    public float HorizontalOffsetsToCenter { get; set; } = 0.2f;
    // [0; 1]
    public float VerticalOffsetsToCenter { get; set; } = 0.2f;
    public ulong Seed { get; set; } = 0;
    // Контроль сили змішування (0 = оригінальна карта, 1 = повна заміна на острівну форму)
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
            case CenterType.Single:
                islandCenters = new List<Vector2>() { new(mapCenterX, mapCenterY) };
                break;
            case CenterType.SingleRandomly:
                islandCenters = MapHelpers.GenerateDots(1, xMinBound, xMaxBound, yMinBound, yMaxBound, Seed);
                break;
            case CenterType.Many:
                islandCenters = MapHelpers.GenerateDots(CentersCount, xMinBound, xMaxBound, yMinBound, yMaxBound, Seed);
                break;
        }

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                var dotCenter = new Vector2(x + 0.5f, y + 0.5f);
                var nearestPoint = MapHelpers.FindNearestPoint(dotCenter, islandCenters);

                // Розрахунок відстані до найближчого центру острову
                float distance = Distances.CalculateDistance(dotCenter.X, dotCenter.Y, nearestPoint.X, nearestPoint.Y, DistanceFunction);

                // Нормалізація відстані відносно заданого радіуса
                float normalizedDistance = Math.Min(1.0f, distance / RadiusAroundIslands);

                // Розрахунок фактора впливу острову:
                // В центрі (distance = 0) -> islandInfluenceFactor = MaxDistanceFactor (висока висота, мало змін)
                // На краю (distance = RadiusAroundIslands) -> islandInfluenceFactor = MinDistanceFactor (низька висота)
                // Створюємо цільову висоту для острова (висока в центрі, низька на краях)
                float targetIslandHeight = Mathf.Lerp(MaxDistanceFactor, MinDistanceFactor, normalizedDistance);

                // Змішування оригінальної висоти з цільовою висотою острова
                float originalElevation = map[y, x];
                float newElevation = LinearInterpolation(originalElevation, targetIslandHeight, MixStrength * normalizedDistance);

                // Обмежуємо значення в межах [0, 1]
                newMap[y, x] = Math.Clamp(newElevation, 0.0f, 1.0f);
            }
        }

        return newMap;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float LinearInterpolation(float a, float b, float t) => a * (1.0f - t) + b * t;
}
