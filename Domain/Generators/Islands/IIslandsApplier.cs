namespace TerrainGenerationApp.Domain.Generators.Islands;

public interface IIslandsApplier
{
    public float[,] ApplyIslands(float[,] map);
}