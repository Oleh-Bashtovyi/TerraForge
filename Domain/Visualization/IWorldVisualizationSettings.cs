namespace TerrainGenerationApp.Domain.Visualization;

public interface IWorldVisualizationSettings
{
    public TerrainVisualizationSettings TerrainSettings { get; }
    public TreeVisualizationSettings TreeSettings { get; }
}