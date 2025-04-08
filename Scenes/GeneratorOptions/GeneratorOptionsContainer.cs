using TerrainGenerationApp.Scenes.BuildingBlocks.Containers;

namespace TerrainGenerationApp.Scenes.GeneratorOptions;

public partial class BaseGeneratorOptions : OptionsContainer
{
    public virtual float[,] GenerateMap()
    {
        return new float[1, 1];
    }
}
