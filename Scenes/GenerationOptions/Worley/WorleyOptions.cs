using TerrainGenerationApp.Domain.Generators;
using TerrainGenerationApp.Scenes.BuildingBlocks;
using TerrainGenerationApp.Scenes.BuildingBlocks.Attributes;

namespace TerrainGenerationApp.Scenes.GenerationOptions.Worley;

public partial class WorleyOptions : BaseGeneratorOptions
{
    private int _mapHeight = 100;
	private int _mapWidth = 100;
	private int _seed = 42;
	private int _dotsCount = 100;
	private float _maxIntensity = 100;
	private bool _invert = false;

    [InputLine(Description = "Map height:")]
    [InputLineSlider(1, 400)]
    public int MapHeight
    {
        get => _mapHeight;
        set
        {
            _mapHeight = value;
            InvokeParametersChangedEvent();
        }
    }

    [InputLine(Description = "Map width:")]
    [InputLineSlider(1, 400)]
    public int MapWidth
    {
        get => _mapWidth;
        set
        {
            _mapWidth = value;
            InvokeParametersChangedEvent();
        }
    }

    [InputLine(Description = "Seed:")]
    [InputLineSlider(0, 10_000)]
    public int Seed
    {
        get => _seed;
        set
        {
            _seed = value;
            InvokeParametersChangedEvent();
        }
    }

    [InputLine(Description = "Dots count:")]
    [InputLineSlider(1, 400)]
    public int DotsCount
    {
        get => _dotsCount;
        set
        {
            _dotsCount = value;
            InvokeParametersChangedEvent();
        }
    }

    [InputLine(Description = "Max intensity:")]
    [InputLineSlider(0f, 1000f)]
    public float MaxIntensity
    {
        get => _maxIntensity;
        set
        {
            _maxIntensity = value;
            InvokeParametersChangedEvent();
        }
    }

    [InputLine(Description = "Invert:")]
    public bool Invert
    {
        get => _invert;
        set
        {
            _invert = value;
            InvokeParametersChangedEvent();
        }
    }

    public override void _Ready()
	{
        base._Ready();
        InputLineManager.CreateInputLinesForObject(this, this);
    }

	public override float[,] GenerateMap()
	{
		return WorleyNoise.GenerateMap(_mapHeight, _mapWidth, _dotsCount, _maxIntensity, _invert, _seed);
	}
}
