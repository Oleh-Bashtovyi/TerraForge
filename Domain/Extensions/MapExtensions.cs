using Godot;
using System;
using System.Collections.Generic;
using TerrainGenerationApp.Domain.Enums;
using TerrainGenerationApp.Domain.Utils.TerrainUtils;

namespace TerrainGenerationApp.Domain.Extensions;

public static class MapExtensions
{
    public static int Height<T>(this T[,] map) => map.GetLength(0);

    public static int Width<T>(this T[,] map) => map.GetLength(1);

    public static T[,] Copy<T>(this T[,] map)
    {
        var h = map.Height();
        var w = map.Width();
        var copy = new T[h, w];
        
        for (int i = 0; i < h; i++)
        {
            for (int j = 0; j < w; j++)
            {
                copy[i, j] = map[i, j];
            }
        }
        
        return copy;
    }

    public static float HeightProgress<T>(this T[,] map, float height) => height / map.Height();

    public static float WidthProgress<T>(this T[,] map, float width) => width / map.Width();

    public static float HeightIndexProgress<T>(this T[,] map, float rowIndex) => rowIndex / (map.Height() - 1);

    public static float WidthIndexProgress<T>(this T[,] map, float colIndex) => colIndex / (map.Width() - 1);

    public static T GetValueAt<T>(this T[,] map, Vector2I position) => map.GetValueAt(position.Y, position.X);

    public static T GetValueAt<T>(this T[,] map, int row, int col)
    {
        row = Mathf.Clamp(row, 0, map.Height() - 1);
        col = Mathf.Clamp(col, 0, map.Width() - 1);
        return map[row, col];
    }

    public static float GetValueAt(this float[,] map, 
        Vector2 position, 
        MapInterpolation mapInterpolation = MapInterpolation.Bilinear) => map.GetValueAt(position.Y, position.X, mapInterpolation);

    public static float GetValueAt(this float[,] map,
        float row, 
        float col, 
        MapInterpolation mapInterpolation = MapInterpolation.Bilinear)
    {
        row = Mathf.Clamp(row, 0f, map.Height() - 1);
        col = Mathf.Clamp(col, 0f, map.Width() - 1);

        if (mapInterpolation == MapInterpolation.Bilinear)
        {
            var row1 = Mathf.FloorToInt(row);
            var row2 = Mathf.CeilToInt(row);
            var col1 = Mathf.FloorToInt(col);
            var col2 = Mathf.CeilToInt(col);
            var v1 = Mathf.Lerp(map[row1, col1], map[row1, col2], col - col1);
            var v2 = Mathf.Lerp(map[row2, col1], map[row2, col2], col - col1);
            return Mathf.Lerp(v1, v2, row - row1);
        }
        if (mapInterpolation == MapInterpolation.SmoothStep)
        {
            var row1 = Mathf.FloorToInt(row);
            var row2 = Mathf.CeilToInt(row);
            var col1 = Mathf.FloorToInt(col);
            var col2 = Mathf.CeilToInt(col);
            var t1 = Interpolations.SmoothStep(0, 1, col - col1);
            var t2 = Interpolations.SmoothStep(0, 1, row - row1);
            var v1 = Mathf.Lerp(map[row1, col1], map[row1, col2], t1);
            var v2 = Mathf.Lerp(map[row2, col1], map[row2, col2], t1);
            return Mathf.Lerp(v1, v2, t2);
        }

        return map[Mathf.FloorToInt(row), Mathf.FloorToInt(col)];
    }

    public static float GetValueUsingIndexProgress(this float[,] map, 
        float rowProgress, 
        float colProgress, 
        MapInterpolation mapInterpolation = MapInterpolation.Bilinear)
    {
        var row = rowProgress * (map.Height() - 1);
        var col = colProgress * (map.Width() - 1);
        return map.GetValueAt(row, col, mapInterpolation);
    }

    public static float GetValueAtCenter(this float[,] map,
        Vector2I position) => map.GetValueAtCenter(position.Y, position.X);

    public static float GetValueAtCenter(this float[,] map, 
        int row, 
        int col, 
        MapInterpolation mapInterpolation = MapInterpolation.Bilinear) => map.GetValueAt(row + 0.5f, col + 0.5f, mapInterpolation);

    public static void SetValueAt<T>(this T[,] map, Vector2I position, T value) => map.SetValueAt(position.Y, position.X, value);

    public static void SetValueAt<T>(this T[,] map, int row, int col, T value)
    {
        row = Mathf.Clamp(row, 0, map.Height() - 1);
        col = Mathf.Clamp(col, 0, map.Width() - 1);
        map[row, col] = value;
    }

    public static void SetValueAt<T>(this T[,] map, Vector2 position, T value) => map.SetValueAt(position.Y, position.X, value);

    public static void SetValueAt<T>(this T[,] map, float row, float col, T value)
    {
        var r = Mathf.Clamp(Mathf.RoundToInt(row), 0, map.Height() - 1);
        var c = Mathf.Clamp(Mathf.RoundToInt(col), 0, map.Width() - 1);
        map[r, c] = value;
    }

    public static T[][] ToJaggedArray<T>(this T[,] twoDimensionalArray)
    {
        var h = twoDimensionalArray.Height();
        var w = twoDimensionalArray.Width();
        var jaggedArray = new T[h][];

        for (int i = 0; i < h; i++)
        {
            jaggedArray[i] = new T[w];

            for (int j = 0; j < w; j++)
            {
                jaggedArray[i][j] = twoDimensionalArray[i, j];
            }
        }

        return jaggedArray;
    }

    public static T[] ToOneDimensionArray<T>(this T[,] arr)
    {
        var h = arr.Height();
        var w = arr.Width();
        var result = new T[h * w];

        for (int row = 0; row < h; row++)
        {
            for (int col = 0; col < w; col++)
            {
                result[row * w + col] = arr[row, col];
            }
        }

        return result;
    }

    public static T[,] ToTwoDimensionArray<T>(T[] arr, int height, int width)
    {
        if (height * width != arr.Length)
        {
            throw new ArgumentException("The length of the array does not match the specified dimensions.");
        }

        var result = new T[height, width];

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                result[i, j] = arr[i * width + j];
            }
        }

        return result;
    }

    public static T[,] ToTwoDimensionArray<T>(this IEnumerable<T> arr, int height, int width)
    {
        var result = new T[height, width];
        var row = 0;
        var col = 0;

        foreach (var value in arr)
        {
            result[row, col] = value;

            col++;
            if (col == width)
            {
                col = 0;
                row++;
                if (row == height)
                {
                    break;
                }
            }
        }

        return result;
    }
}
