namespace TerrainGenerationApp.Domain.Visualization;

public class WorldVisualizationSettings : IWorldVisualizationSettings
{
    public TerrainVisualizationSettings TerrainSettings { get; } = new();
    public TreeVisualizationSettings TreeSettings { get; } = new();
}