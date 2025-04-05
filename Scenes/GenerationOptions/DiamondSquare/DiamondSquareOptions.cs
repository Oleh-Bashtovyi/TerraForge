using TerrainGenerationApp.Scenes.BuildingBlocks;
using TerrainGenerationApp.Scenes.BuildingBlocks.Attributes;

namespace TerrainGenerationApp.Scenes.GenerationOptions.DiamondSquare;

public partial class DiamondSquareOptions : BaseGeneratorOptions
{
    private int _seed = 42;
	private int _terrainPower = 5;
	private float _roughness = 3.0f;

    [LineInputValue(Description = "Terrain power:")]
    [InputRange(1, 10)]
    [Rounded]
    [Step(1.0f)]
    public int TerrainPower
    {
        get => _terrainPower;
        set
        {
            _terrainPower = value;
            InvokeParametersChangedEvent();
        }
    }

    [LineInputValue(Description = "Seed:")]
    [InputRange(0, 10000)]
    [Rounded]
    [Step(1.0f)]
    public int Seed
    {
        get => _seed;
        set
        {
            _seed = value;
            InvokeParametersChangedEvent();
        }
    }

    [LineInputValue(Description = "Roughness:")]
    [InputRange(0.1f, 10f)]
    [Step(0.1f)]
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
        InputLineManager.CreateInputLinesForObject(this, this);
    }

    public override float[,] GenerateMap()
	{
		return Domain.Generators.DiamondSquare.GenerateMap(TerrainPower, Roughness, Seed);
	}
}
