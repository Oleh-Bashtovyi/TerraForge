using TerrainGenerationApp.Scenes.BuildingBlocks.Attributes;
using TerrainGenerationApp.Scenes.BuildingBlocks.InputLine;

namespace TerrainGenerationApp.Scenes.GeneratorOptions;

public partial class DiamondSquareOptions : BaseGeneratorOptions
{
	private int _seed = 42;
	private int _terrainPower = 5;
	private float _roughness = 3.0f;

	[InputLine(Description = "Terrain power:")]
	[InputLineSlider(1, 10)]
	public int TerrainPower
	{
		get => _terrainPower;
        set => SetAndInvokeParametersChangedEvent(ref _terrainPower, value);
    }

	[InputLine(Description = "Seed:")]
	[InputLineSlider(1, 10_000)]
	public int Seed
	{
		get => _seed;
		set => SetAndInvokeParametersChangedEvent(ref _seed, value);
    }

	[InputLine(Description = "Roughness:")]
	[InputLineSlider(0.1f, 10f, 0.1f)]
	public float Roughness
	{
		get => _roughness;
		set => SetAndInvokeParametersChangedEvent(ref _roughness, value);
    }

	public override void _Ready()
	{
		base._Ready();
		InputLineManager.CreateInputLinesForObject(obj: this, container: this);
	}

	public override float[,] GenerateMap()
	{
        return Domain.Generators.DiamondSquare.GenerateMap(TerrainPower, Roughness, Seed);
    }
}
