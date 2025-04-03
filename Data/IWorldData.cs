namespace TerrainGenerationApp.Data;

public interface IWorldData
{
    TerrainData TerrainData { get; }
    TreesData TreesData { get; }
    float SeaLevel { get; }
}