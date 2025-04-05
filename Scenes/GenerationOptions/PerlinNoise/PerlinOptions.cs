using TerrainGenerationApp.Scenes.BuildingBlocks;
using TerrainGenerationApp.Scenes.BuildingBlocks.Attributes;

namespace TerrainGenerationApp.Scenes.GenerationOptions.PerlinNoise;

public partial class PerlinOptions : BaseGeneratorOptions
{
	private readonly Domain.Generators.PerlinNoise _generator = new();

    [LineInputValue(Description = "Map height:")]
    [InputRange(1, 400)]
    [Rounded]
    [Step(1.0f)]
    public int MapHeight
    {
        get => _generator.MapHeight;
        set
        {
            _generator.MapHeight = value;
            InvokeParametersChangedEvent();
        }
    }

    [LineInputValue(Description = "Map width:")]
    [InputRange(1, 400)]
    [Rounded]
    [Step(1.0f)]
    public int MapWidth
    {
        get => _generator.MapWidth;
        set
        {
            _generator.MapWidth = value;
            InvokeParametersChangedEvent();
        }
    }

    [LineInputValue(Description = "Seed:")]
    [InputRange(0, 10000)]
    [Rounded]
    [Step(1.0f)]
    public int Seed
    {
        get => _generator.Seed;
        set
        {
            _generator.Seed = value;
            InvokeParametersChangedEvent();
        }
    }

    [LineInputValue(Description = "Octaves:")]
	[InputRange(1, 10)]
	[Rounded]
    [Step(1.0f)]
    public int Octaves
    {
		get => _generator.Octaves;
        set
        {
			_generator.Octaves = value;
            InvokeParametersChangedEvent();
        }
    }

    [LineInputValue(Description = "Scale:")]
    [InputRange(0.1f, 100f)]
    [Step(0.1f)]
    public float Frequency
    {
        get => _generator.Scale;
        set
        {
            _generator.Scale = value;
            InvokeParametersChangedEvent();
        }
    }

    [LineInputValue(Description = "Lacunarity:")]
    [InputRange(0.001f, 10f)]
    [Step(0.001f)]
    public float Lacunarity
    {
        get => _generator.Lacunarity;
        set
        {
            _generator.Lacunarity = value;
            InvokeParametersChangedEvent();
        }
    }

    [LineInputValue(Description = "Persistence:")]
    [InputRange(0.001f, 1f)]
    [Step(0.001f)]
    public float Persistence
    {
        get => _generator.Persistence;
        set
        {
            _generator.Persistence = value;
            InvokeParametersChangedEvent();
        }
    }

    [LineInputValue(Description = "Offset X:")]
    [InputRange(-1000f, 1000f)]
    [Step(0.1f)]
    public float OffsetX
    {
        get => _generator.Offset.X;
        set
        {
            var offset = _generator.Offset;
            offset.X = value;
            _generator.Offset = offset;
            InvokeParametersChangedEvent();
        }
    }

    [LineInputValue(Description = "Offset Y:")]
    [InputRange(-1000f, 1000f)]
    [Step(0.1f)]
    public float OffsetY
    {
        get => _generator.Offset.Y;
        set
        {
            var offset = _generator.Offset;
            offset.Y = value;
            _generator.Offset = offset;
            InvokeParametersChangedEvent();
        }
    }

    public Domain.Generators.PerlinNoise Generator => _generator;

    public override void _Ready()
    {
        Generator.EnableWarping = false;
        InputLineManager.CreateInputLinesForObject(this, this);
    }

    public override float[,] GenerateMap()
	{
		return _generator.GenerateMap();
	}
}
