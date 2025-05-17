using TerrainGenerationApp.Domain.Generators;
using TerrainGenerationApp.Scenes.BuildingBlocks.InputLine;

namespace TerrainGenerationApp.Scenes.GeneratorOptions;

public partial class ValueNoiseOptions() : NoiseGeneratorOptionsBase(new PerlinNoiseGenerator())
{
    public override void _Ready()
    {
        base._Ready();
        InputLineManager.CreateInputLinesForObject(obj: this, container: this);
    }
}