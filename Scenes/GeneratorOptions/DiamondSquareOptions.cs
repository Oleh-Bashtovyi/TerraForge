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
		set
		{
			_terrainPower = value;
			UpdateOptionValue(nameof(TerrainPower), TerrainPower);
			InvokeParametersChangedEvent();
		}
	}

	[InputLine(Description = "Seed:")]
	[InputLineSlider(1, 10_000)]
	public int Seed
	{
		get => _seed;
		set
		{
			_seed = value;
			InvokeParametersChangedEvent();
		}
	}

	[InputLine(Description = "Roughness:")]
	[InputLineSlider(0.1f, 10f, 0.1f)]
	public float Roughness
	{
		get => _roughness;
		set
		{
			_roughness = value;
			InvokeParametersChangedEvent();
		}
	}

	public override void _Ready()
	{
		base._Ready();
		InputLineManager.CreateInputLinesForObject(obj: this, container: this);
		UpdateOptionValue(nameof(TerrainPower), TerrainPower);
		UpdateOptionValue(nameof(Seed), Seed);
		UpdateOptionValue(nameof(Roughness), Roughness);
	}

	public override float[,] GenerateMap()
	{
		var map = Domain.Generators.DiamondSquare.GenerateMap(TerrainPower, Roughness, Seed);
        UpdateCurrentOptionsAsLastUsed();
        return map;
    }
}
