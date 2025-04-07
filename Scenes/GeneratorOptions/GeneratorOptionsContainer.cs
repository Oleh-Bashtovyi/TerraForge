namespace TerrainGenerationApp.Scenes.GeneratorOptions;

public partial class BaseGeneratorOptions : BuildingBlocks.OptionsContainer
{
    public virtual float[,] GenerateMap()
    {
        return new float[1, 1];
    }
}
