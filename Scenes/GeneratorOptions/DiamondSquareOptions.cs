using TerrainGenerationApp.Scenes.BuildingBlocks.Attributes;
using TerrainGenerationApp.Scenes.BuildingBlocks.InputLine;

namespace TerrainGenerationApp.Scenes.GeneratorOptions;

public partial class DiamondSquareOptions : BaseGeneratorOptions
{
	private const string TERRAIN_POWER_TOOLTIP = "The power of the terrain, which determines the size of the map.\n" +
                                                    "A higher value results in a larger map with more detail.\n" +
                                                    "Map size formula: 2^(terrain_power) + 1";

	private const string ROUGHNESS_TOOLTIP = "The roughness of the terrain, which affects the height variation.\n" +
                                             "A higher value results in a more rugged terrain.";

    private const string SEED_TOOLTIP = "The seed for the random number generator.";

	private const string ROUGHNESS_DEGRADATION_TOOLTIP = "The degradation of roughness for each iteration.\n" +
														 "A lower value results in smoother terrain.";

	private const string MIN_ROUGHNESS_TOOLTIP = "The minimum roughness value to prevent the terrain from becoming too smooth.";

    private int _seed = 42;
	private int _terrainPower = 7;
	private float _roughness = 3.0f;
	private float _roughnessDegradation = 0.5f;
	private float _minRoughness = 0.1f;

    [InputLine(Description = "Terrain power:", Tooltip = TERRAIN_POWER_TOOLTIP)]
	[InputLineSlider(1, 10)]
	public int TerrainPower
	{
		get => _terrainPower;
        set => SetAndInvokeParametersChangedEvent(ref _terrainPower, value);
    }

	[InputLine(Description = "Seed:", Tooltip = SEED_TOOLTIP)]
	[InputLineSlider(0, 10_000)]
	public int Seed
	{
		get => _seed;
		set => SetAndInvokeParametersChangedEvent(ref _seed, value);
    }

	[InputLine(Description = "Roughness:", Tooltip = ROUGHNESS_TOOLTIP)]
	[InputLineSlider(0.1f, 10f, 0.1f)]
	public float Roughness
	{
		get => _roughness;
		set => SetAndInvokeParametersChangedEvent(ref _roughness, value);
    }

	[InputLine(Description = "Roughness degradation:", Tooltip = ROUGHNESS_DEGRADATION_TOOLTIP)]
	[InputLineSlider(0.01f, 1f, 0.01f)]
    public float RoughnessDegradation
	{
		get => _roughnessDegradation;
		set => SetAndInvokeParametersChangedEvent(ref _roughnessDegradation, value);
    }

    [InputLine(Description = "Minimum roughness:", Tooltip = MIN_ROUGHNESS_TOOLTIP)]
    [InputLineSlider(0.01f, 1f, 0.01f)]
    public float MinRoughness
    {
		get => _minRoughness;
		set => SetAndInvokeParametersChangedEvent(ref _minRoughness, value);
    }


    public override void _Ready()
	{
		base._Ready();
		InputLineManager.CreateInputLinesForObject(obj: this, container: this);
	}

	public override float[,] GenerateMap()
	{
        return Domain.Generators.DiamondSquare.GenerateMap(TerrainPower, Roughness, Seed, RoughnessDegradation, MinRoughness);
    }
}
