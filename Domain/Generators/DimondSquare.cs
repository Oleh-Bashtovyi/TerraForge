using Godot;

namespace TerrainGenerationApp.Domain.Generators;

public static class DiamondSquare
{
    // Diamond-Square algorithm (also known as the midpoint displacement algorithm).
    // This fractal-based method generates realistic-looking terrain heightmaps with mountains, valleys and natural features.
    // The algorithm creates terrain with self-similarity at different scales, producing natural-looking landscapes
    // with controllable roughness and fractal characteristics.
    //
    // Algorithm:
    // 1) Initialize the four corners of the heightmap with random values
    // 2) Repeatedly perform diamond and square steps, each time reducing the chunk size by half:
    //    a) Diamond step: For each square, set the midpoint value to the average of the four corners plus random noise
    //    b) Square step: For each diamond, set the midpoint value to the average of the four adjacent points plus random noise
    // 3) Reduce the roughness value after each iteration to create natural-looking fractal terrain

    public static float[,] GenerateMap(int terrainPower = 5, float roughness = 5.0f, int seed = 0)
    {
        var mapSize = (1 << terrainPower) + 1;
        var map = new float[mapSize, mapSize];
        var rng = new RandomNumberGenerator();
        rng.Seed = (ulong)seed;

        // RANDOM CORNERS
        map[0, 0] = rng.Randf();
        map[0, mapSize - 1] = rng.Randf();
        map[mapSize - 1, 0] = rng.Randf();
        map[mapSize - 1, mapSize - 1] = rng.Randf();

        var chunkSize = mapSize - 1;
        var curRoughness = roughness;
        while (chunkSize > 1)
        {
            var halfSize = chunkSize / 2;
            // DIAMOND STEP
            for (int y = halfSize; y < mapSize; y += chunkSize)
            {
                for (int x = halfSize; x < mapSize; x += chunkSize)
                {
                    var value = 0.0f;
                    var count = 0;
                    // Check top-left
                    if (y - halfSize >= 0 && x - halfSize >= 0)
                    {
                        value += map[y - halfSize, x - halfSize];
                        count++;
                    }
                    // Check top-right
                    if (y - halfSize >= 0 && x + halfSize < mapSize)
                    {
                        value += map[y - halfSize, x + halfSize];
                        count++;
                    }
                    // Check bottom-left
                    if (y + halfSize < mapSize && x - halfSize >= 0)
                    {
                        value += map[y + halfSize, x - halfSize];
                        count++;
                    }
                    // Check bottom-right
                    if (y + halfSize < mapSize && x + halfSize < mapSize)
                    {
                        value += map[y + halfSize, x + halfSize];
                        count++;
                    }
                    float avg = value / count + rng.RandfRange(-curRoughness, curRoughness);
                    map[y, x] = Mathf.Clamp(avg, 0.0f, 1.0f);
                }
            }
            // SQUARE STEP
            for (int y = 0; y < mapSize; y += halfSize)
            {
                for (int x = (y + halfSize) % chunkSize; x < mapSize; x += chunkSize)
                {
                    float value = 0.0f;
                    int count = 0;
                    // Check left
                    if (x - halfSize >= 0)
                    {
                        value += map[y, x - halfSize];
                        count++;
                    }
                    // Check right
                    if (x + halfSize < mapSize)
                    {
                        value += map[y, x + halfSize];
                        count++;
                    }
                    // Check top
                    if (y - halfSize >= 0)
                    {
                        value += map[y - halfSize, x];
                        count++;
                    }
                    // Check bottom
                    if (y + halfSize < mapSize)
                    {
                        value += map[y + halfSize, x];
                        count++;
                    }
                    float avg = value / count + rng.RandfRange(-curRoughness, curRoughness);
                    map[y, x] = Mathf.Clamp(avg, 0.0f, 1.0f);
                }
            }
            chunkSize /= 2;
            curRoughness = Mathf.Max(curRoughness / 2.0f, 0.1f);
        }
        return map;
    }
}