using Godot;
using TerrainGenerationApp.Domain.Utils.TerrainUtils;

namespace TerrainGenerationApp.Domain.Extensions;

public static class MapExtensions
{
    public enum InterpolationType
    {
        None,
        Bilinear,
        SmoothStep,
    }
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
        InterpolationType interpolation = InterpolationType.Bilinear) => map.GetValueAt(position.Y, position.X, interpolation);

    public static float GetValueAt(this float[,] map,
        float row, 
        float col, 
        InterpolationType interpolation = InterpolationType.Bilinear)
    {
        row = Mathf.Clamp(row, 0f, map.Height() - 1);
        col = Mathf.Clamp(col, 0f, map.Width() - 1);

        if (interpolation == InterpolationType.Bilinear)
        {
            var row1 = Mathf.FloorToInt(row);
            var row2 = Mathf.CeilToInt(row);
            var col1 = Mathf.FloorToInt(col);
            var col2 = Mathf.CeilToInt(col);
            var v1 = Mathf.Lerp(map[row1, col1], map[row1, col2], col - col1);
            var v2 = Mathf.Lerp(map[row2, col1], map[row2, col2], col - col1);
            return Mathf.Lerp(v1, v2, row - row1);
        }
        if (interpolation == InterpolationType.SmoothStep)
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
        InterpolationType interpolation = InterpolationType.Bilinear)
    {
        var row = rowProgress * (map.Height() - 1);
        var col = colProgress * (map.Width() - 1);
        return map.GetValueAt(row, col, interpolation);
    }

    public static float GetValueAtCenter(this float[,] map,
        Vector2I position) => map.GetValueAtCenter(position.Y, position.X);

    public static float GetValueAtCenter(this float[,] map, 
        int row, 
        int col, 
        InterpolationType interpolation = InterpolationType.Bilinear) => map.GetValueAt(row + 0.5f, col + 0.5f, interpolation);

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
}
