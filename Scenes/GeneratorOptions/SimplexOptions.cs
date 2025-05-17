using TerrainGenerationApp.Domain.Generators;
using TerrainGenerationApp.Scenes.BuildingBlocks.InputLine;

namespace TerrainGenerationApp.Scenes.GeneratorOptions;

public partial class SimplexOptions() : NoiseGeneratorOptionsBase(new SimplexNoiseGenerator())
{
    public override void _Ready()
    {
        base._Ready();
        InputLineManager.CreateInputLinesForObject(obj: this, container: this);
    }
}