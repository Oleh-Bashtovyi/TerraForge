namespace TerrainGenerationApp.Domain.Visualization;

public interface IWorldVisualSettings
{
    public TerrainVisualSettings TerrainSettings { get; }
    public TreeVisualSettings TreeSettings { get; }
}