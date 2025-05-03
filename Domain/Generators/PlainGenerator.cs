namespace TerrainGenerationApp.Domain.Generators;

public class PlainGenerator
{
    public static float[,] GenerateMap(int mapHeight, int mapWidth, float height)
    {
        var map = new float[mapHeight, mapWidth];

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                map[y, x] = height;
            }
        }

        return map;
    }
}