namespace TerrainGenerationApp.Data.Display;

public interface IWorldVisualizationSettings
{
    public TerrainVisualizationSettings TerrainSettings { get; }
    public TreeVisualizationSettings TreeSettings { get; }
}