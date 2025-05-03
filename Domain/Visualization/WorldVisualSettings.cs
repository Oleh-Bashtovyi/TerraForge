namespace TerrainGenerationApp.Domain.Visualization;

public class WorldVisualSettings : IWorldVisualSettings
{
    public TerrainVisualSettings TerrainSettings { get; } = new();
    public TreeVisualSettings TreeSettings { get; } = new();
}