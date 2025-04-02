using Godot;

namespace TerrainGenerationApp.Extensions;

public static class MapExtensions
{
    public static int Height<T>(this T[,] map) => map.GetLength(0);

    public static int Width<T>(this T[,] map) => map.GetLength(1);

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

    public static float GetValueAt(this float[,] map, Vector2 position) => map.GetValueAt(position.Y, position.X);

    public static float GetValueAt(this float[,] map, float row, float col)
    {
        row = Mathf.Clamp(row, 0f, map.Height() - 1);
        col = Mathf.Clamp(col, 0f, map.Width() - 1);
        var row1 = Mathf.FloorToInt(row);
        var row2 = Mathf.CeilToInt(row);
        var col1 = Mathf.FloorToInt(col);
        var col2 = Mathf.CeilToInt(col);
        var v1 = Mathf.Lerp(map[row1, col1], map[row1, col2], col - col1);
        var v2 = Mathf.Lerp(map[row2, col1], map[row2, col2], col - col1);
        return Mathf.Lerp(v1, v2, row - row1);
    }

    public static float GetValueUsingIndexProgress(this float[,] map, float rowProgress, float colProgress)
    {
        var row = rowProgress * (map.Height() - 1);
        var col = colProgress * (map.Width() - 1);
        return map.GetValueAt(row, col);
    }

    public static float GetValueAtCenter(this float[,] map, Vector2I position) => map.GetValueAtCenter(position.Y, position.X);

    public static float GetValueAtCenter(this float[,] map, int row, int col) => map.GetValueAt(row + 0.5f, col + 0.5f);




    //public static float GetValueUsingProgress(this float[,] map, float rowProgress, float colProgress)
    //{
    //    var h = map.Height();
    //    var w = map.Width();
    //    var row = rowProgress * (h - 1);
    //    var col = colProgress * (w - 1);
    //    return map.GetValueAt(row, col);
    //}



    /*    /// <summary>
        /// Gets the progress of the specified height in the map.
        /// </summary>
        /// <param name="map">The map array.</param>
        /// <param name="height">The height as a float.</param>
        /// <returns>The progress of the specified height.</returns>
        public static float GetProgressOfHeight<T>(this T[,] map, float height)
        {
            int mapHeight = map.Height();

            if (mapHeight <= 0)
                return 0f;

            if (mapHeight == 1)
                return 1f;

            return height / mapHeight;
        }

        /// <summary>
        /// Gets the progress of the specified width in the map.
        /// </summary>
        /// <param name="map">The map array.</param>
        /// <param name="width">The width as a float.</param>
        /// <returns>The progress of the specified width.</returns>
        public static float GetProgressOfWidth<T>(this T[,] map, float width)
        {
            int mapWidth = map.Width();

            if (mapWidth <= 0)
                return 0f;

            if (mapWidth == 1)
                return 1f;

            return width / mapWidth;
        }


        public static float GetProgressOfHeightIndex<T>(this T[,] map, float index)
        {
            var mapHeight = map.Height();

            if (mapHeight <= 0)
                return 0f;

            if (mapHeight == 1)
                return 1f;

            return index / (mapHeight - 1);
        }

        public static float GetProgressOfWidthIndex<T>(this T[,] map, float index)
        {
            var mapWidth = map.Width();
            if (mapWidth <= 0)
                return 0f;
            if (mapWidth == 1)
                return 1f;
            return index / (mapWidth - 1);
        }*/

    /*    public static float GetValueAtUsingIndexes(this float[,] map, float rowIndex, float colIndex)
        {
            var h = map.Height();
            var w = map.Width();
            rowIndex = Mathf.Clamp(rowIndex, 0, h - 1);
            colIndex = Mathf.Clamp(colIndex, 0, w - 1);
            var row1 = Mathf.FloorToInt(rowIndex);
            var row2 = Mathf.CeilToInt(rowIndex);
            var col1 = Mathf.FloorToInt(colIndex);
            var col2 = Mathf.CeilToInt(colIndex);
            var v1 = Mathf.Lerp(map[row1, col1], map[row1, col2], colIndex - col1);
            var v2 = Mathf.Lerp(map[row2, col1], map[row2, col2], colIndex - col1);
            return Mathf.Lerp(v1, v2, rowIndex - row1);
        }*/



    /*    public static float GetValueAtUsingIndexProgress(this float[,] map, float rowProgress, float colProgress)
        {
            rowProgress = Mathf.Clamp(rowProgress, 0f, 1f);
            colProgress = Mathf.Clamp(colProgress, 0f, 1f);
            var h = map.Height();
            var w = map.Width();

            var rowIndex = rowProgress * (h - 1);
            var colIndex = colProgress * (w - 1);
            var row1 = Mathf.FloorToInt(rowIndex);
            var row2 = Mathf.CeilToInt(rowIndex);
            var col1 = Mathf.FloorToInt(colIndex);
            var col2 = Mathf.CeilToInt(colIndex);
            var rowW = rowIndex - row1;
            var colW = colIndex - col1;

            var h00 = map[row1, col1];
            var h10 = map[row2, col1];
            var h01 = map[row1, col2];
            var h11 = map[row2, col2];

            var h0 = Mathf.Lerp(h00, h10, rowW);
            var h1 = Mathf.Lerp(h01, h11, rowW);

            return Mathf.Lerp(h0, h1, colW);
        }*/
}
