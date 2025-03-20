namespace TerrainGenerationApp.Generators.Islands;

public interface IIslandsApplier
{
    public float[,] ApplyIslands(float[,] map);
}