namespace TerrainGenerationApp.Data.Display;

public class WorldVisualizationSettings : IWorldVisualizationSettings
{
    public TerrainVisualizationSettings TerrainSettings { get; }
    public TreeVisualizationSettings TreeSettings { get; }

    public WorldVisualizationSettings()
    {
        TerrainSettings = new TerrainVisualizationSettings();
        TreeSettings = new TreeVisualizationSettings();
    }
}