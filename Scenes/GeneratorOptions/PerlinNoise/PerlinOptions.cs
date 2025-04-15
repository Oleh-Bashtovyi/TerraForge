using TerrainGenerationApp.Scenes.BuildingBlocks.Attributes;
using TerrainGenerationApp.Scenes.BuildingBlocks.InputLine;

namespace TerrainGenerationApp.Scenes.GeneratorOptions.PerlinNoise;

public partial class PerlinOptions : BaseGeneratorOptions
{
	private readonly Domain.Generators.PerlinNoiseGenerator _generator = new();
    private int _mapHeight = 100;
    private int _mapWidth = 100;

    [InputLine(Description = "Map height:")]
    [InputLineSlider(2, 400)]
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
    [InputLineSlider(2, 400)]
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

    [InputLine(Description = "Use domain warping")]
    [InputLineCheckBox]
    public bool UseDomainWarping
    {
        get => _generator.EnableWarping;
        set
        {
            _generator.EnableWarping = value;
            InvokeParametersChangedEvent();
        }
    }

    [InputLine(Description = "Warping size:")]
    [InputLineSlider(0.1f, 2.0f, 0.1f, format: "0.#")]
    public float WarpingSize
    {
        get => _generator.WarpingSize;
        set
        {
            _generator.WarpingSize = value;
            InvokeParametersChangedEvent();
        }
    }

    [InputLine(Description = "Warping strength:")]
    [InputLineSlider(0.1f, 30.0f, 0.1f, format: "0.#")]
    public float WarpingStrength
    {
        get => _generator.WarpingStrength;
        set
        {
            _generator.WarpingStrength = value;
            InvokeParametersChangedEvent();
        }
    }

    public Domain.Generators.PerlinNoiseGenerator Generator => _generator;

    public override void _Ready()
    {
        base._Ready();
        Generator.EnableWarping = false;
        InputLineManager.CreateInputLinesForObject(obj: this, container: this);
    }

    public override float[,] GenerateMap()
	{
		return _generator.GenerateMap(_mapHeight, _mapWidth);
	}
}
