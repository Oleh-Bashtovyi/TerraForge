namespace TerrainGenerationApp.Domain.Core;

public interface IWorldData
{
    TerrainData TerrainData { get; }
    TreesData TreesData { get; }
    float SeaLevel { get; }
}