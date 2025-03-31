using Godot;

namespace TerrainGenerationApp.Utilities;

public static class MapExtensions
{
    /// <summary>
    /// Gets the height of the map.
    /// </summary>
    /// <param name="map">The map array.</param>
    /// <returns>The height of the map.</returns>
    public static int Height<T>(this T[,] map)
    {
        return map.GetLength(0);
    }

    /// <summary>
    /// Gets the width of the map.
    /// </summary>
    /// <param name="map">The map array.</param>
    /// <returns>The width of the map.</returns>
    public static int Width<T>(this T[,] map)
    {
        return map.GetLength(1);
    }

    /// <summary>
    /// Gets the value at the specified position in the map without throwing index out of range exceptions.
    /// </summary>
    /// <param name="map">The map array.</param>
    /// <param name="position">The position as a Vector2I.</param>
    /// <returns>The value at the specified position.</returns>
    public static T GetValueAt<T>(this T[,] map, Vector2I position)
    {
        return map.GetValueAt(position.Y, position.X);
    }

    /// <summary>
    /// Gets the value at the specified row and column in the map without throwing index out of range exceptions.
    /// </summary>
    /// <param name="map">The map array.</param>
    /// <param name="row">The row index.</param>
    /// <param name="col">The column index.</param>
    /// <returns>The value at the specified row and column.</returns>
    public static T GetValueAt<T>(this T[,] map, int row, int col)
    {
        var h = map.GetLength(0);
        var w = map.GetLength(1);
        row = Mathf.Clamp(row, 0, h - 1);
        col = Mathf.Clamp(col, 0, w - 1);
        return map[row, col];
    }

    /// <summary>
    /// Gets the value at the specified position in the map without throwing index out of range exceptions.
    /// </summary>
    /// <param name="map">The map array.</param>
    /// <param name="position">The position as a Vector2.</param>
    /// <returns>The value at the specified position.</returns>
    public static float GetValueAt(this float[,] map, Vector2 position)
    {
        return map.GetValueAt(position.Y, position.X);
    }

    /// <summary>
    /// Gets the interpolated value at the specified row and column in the map without throwing index out of range exceptions.
    /// </summary>
    /// <param name="map">The map array.</param>
    /// <param name="row">The row index as a float.</param>
    /// <param name="col">The column index as a float.</param>
    /// <returns>The interpolated value at the specified row and column.</returns>
    public static float GetValueAt(this float[,] map, float row, float col)
    {
        var h = map.GetLength(0);
        var w = map.GetLength(1);
        row = Mathf.Clamp(row, 0, h - 1);
        col = Mathf.Clamp(col, 0, w - 1);
        var row1 = Mathf.FloorToInt(row);
        var row2 = Mathf.CeilToInt(row);
        var col1 = Mathf.FloorToInt(col);
        var col2 = Mathf.CeilToInt(col);
        var v1 = Mathf.Lerp(map[row1, col1], map[row1, col2], col - col1);
        var v2 = Mathf.Lerp(map[row2, col1], map[row2, col2], col - col1);
        return Mathf.Lerp(v1, v2, row - row1);
    }

    public static float GetValueUsingProgress(this float[,] map, float rowProgress, float colProgress)
    {
        var h = map.Height();
        var w = map.Width();
        var row = rowProgress * (h - 1);
        var col = colProgress * (w - 1);
        return map.GetValueAt(row, col);
    }

    public static float GetValueAtCenter(this float[,] map, Vector2I position)
    {
        return map.GetValueAtCenter(position.Y, position.X);
    }

    public static float GetValueAtCenter(this float[,] map, int row, int col)
    {
        return map.GetValueAt(row + 0.5f, col + 0.5f);
    }

    public static float GetProgressOfHeight<T>(this T[,] map, float height)
    {
        int mapHeight = map.Height();

        if (mapHeight <= 0)
            return 0f;

        if (mapHeight == 1)
            return 1f;

        return height / mapHeight;
    }

    public static float GetProgressOfWidth<T>(this T[,] map, float width)
    {
        int mapWidth = map.Width();

        if (mapWidth <= 0)
            return 0f;

        if (mapWidth == 1)
            return 1f;

        return width / mapWidth;
    }
}
