namespace TerrainGenerationApp.Scenes.GenerationOptions;

public partial class BaseGeneratorOptions : OptionsContainer
{
    public virtual float[,] GenerateMap()
    {
        return new float[1, 1];
    }
}
