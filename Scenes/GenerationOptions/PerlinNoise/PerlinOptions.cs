using TerrainGenerationApp.Scenes.BuildingBlocks;
using TerrainGenerationApp.Scenes.BuildingBlocks.Attributes;

namespace TerrainGenerationApp.Scenes.GenerationOptions.PerlinNoise;

public partial class PerlinOptions : BaseGeneratorOptions
{
	private readonly Domain.Generators.PerlinNoise _generator = new();

    [InputLine(Description = "Map height:")]
    [InputLineSlider(1, 400)]
    public int MapHeight
    {
        get => _generator.MapHeight;
        set
        {
            _generator.MapHeight = value;
            InvokeParametersChangedEvent();
        }
    }

    [InputLine(Description = "Map width:")]
    [InputLineSlider(1, 400)]
    public int MapWidth
    {
        get => _generator.MapWidth;
        set
        {
            _generator.MapWidth = value;
            InvokeParametersChangedEvent();
        }
    }

    [InputLine(Description = "Seed:")]
    [InputLineSlider(0, 10_000)]
    public int Seed
    {
        get => _generator.Seed;
        set
        {
            _generator.Seed = value;
            InvokeParametersChangedEvent();
        }
    }

    [InputLine(Description = "Octaves:")]
    [InputLineSlider(0, 10)]
    public int Octaves
    {
		get => _generator.Octaves;
        set
        {
			_generator.Octaves = value;
            InvokeParametersChangedEvent();
        }
    }

    [InputLine(Description = "Scale:")]
    [InputLineSlider(0.1f, 100f, 0.1f)]
    public float Frequency
    {
        get => _generator.Scale;
        set
        {
            _generator.Scale = value;
            InvokeParametersChangedEvent();
        }
    }

    [InputLine(Description = "Lacunarity:")]
    [InputLineSlider(0.001f, 10f, 0.001f)]
    public float Lacunarity
    {
        get => _generator.Lacunarity;
        set
        {
            _generator.Lacunarity = value;
            InvokeParametersChangedEvent();
        }
    }

    [InputLine(Description = "Persistence:")]
    [InputLineSlider(0.001f, 3f, 0.001f)]
    public float Persistence
    {
        get => _generator.Persistence;
        set
        {
            _generator.Persistence = value;
            InvokeParametersChangedEvent();
        }
    }

    [InputLine(Description = "Offset X:")]
    [InputLineSlider(-1000f, 1000f, 0.1f)]
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

    [InputLine(Description = "Offset Y:")]
    [InputLineSlider(-1000f, 1000f, 0.1f)]
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
        base._Ready();
        Generator.EnableWarping = false;
        InputLineManager.CreateInputLinesForObject(this, this);
    }

    public override float[,] GenerateMap()
	{
		return _generator.GenerateMap();
	}
}
