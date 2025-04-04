namespace TerrainGenerationApp.Data.Structure;

public interface IWorldData
{
    TerrainData TerrainData { get; }
    TreesData TreesData { get; }
    float SeaLevel { get; }
}