using TerrainGenerationApp.Domain.Enums;
using TerrainGenerationApp.Domain.Generators;
using TerrainGenerationApp.Scenes.BuildingBlocks.Attributes;

namespace TerrainGenerationApp.Scenes.GeneratorOptions;

// ReSharper disable InconsistentNaming
public partial class NoiseGeneratorOptionsBase : BaseGeneratorOptions
{
    private const string MAP_HEIGHT_TOOLTIP = "The height of the map, which determines the number of rows in the generated noise map.";
    private const string MAP_WIDTH_TOOLTIP = "The width of the map, which determines the number of columns in the generated noise map.";
    private const string SEED_TOOLTIP = "The seed for the random number generator.";
    private const string OCTAVES_TOOLTIP = "The number of octaves to use in the noise generation. More octaves result in more detail.";
    private const string FREQUENCY_TOOLTIP = "The frequency of the noise. Higher values result in more rapid changes in the noise.";
    private const string FRACTAL_TYPE_TOOLTIP = "The type of fractal noise to generate. Different types produce different patterns.";
    private const string LACUNARITY_TOOLTIP = "The lacunarity of the noise. Higher values result in more detail.";
    private const string PERSISTENCE_TOOLTIP = "The persistence of the noise. Higher values result in more detail.";
    private const string OFFSET_X_TOOLTIP = "The X offset for the noise generation.";
    private const string OFFSET_Y_TOOLTIP = "The Y offset for the noise generation.";
    private const string USE_DOMAIN_WARPING_TOOLTIP = "Whether to use domain warping for the noise generation.";
    private const string WARPING_SIZE_TOOLTIP = "The size of the warping effect.";
    private const string WARPING_STRENGTH_TOOLTIP = "The strength of the warping effect.";

    protected readonly NoiseMapGenerator Generator;
    private int _mapHeight = 150;
    private int _mapWidth = 150;

    [InputLine(Description = "Map height:", Tooltip = MAP_HEIGHT_TOOLTIP)]
    [InputLineSlider(2, 512)]
    public int MapHeight
    {
        get => _mapHeight;
        set
        {
            _mapHeight = value;
            InvokeParametersChangedEvent();
        }
    }

    [InputLine(Description = "Map width:", Tooltip = MAP_WIDTH_TOOLTIP)]
    [InputLineSlider(2, 512)]
    public int MapWidth
    {
        get => _mapWidth;
        set
        {
            _mapWidth = value;
            InvokeParametersChangedEvent();
        }
    }

    [InputLine(Description = "Seed:", Tooltip = SEED_TOOLTIP)]
    [InputLineSlider(0, 10_000)]
    public int Seed
    {
        get => Generator.Seed;
        set
        {
            Generator.Seed = value;
            InvokeParametersChangedEvent();
        }
    }

    [InputLine(Description = "Octaves:", Tooltip = OCTAVES_TOOLTIP)]
    [InputLineSlider(1, 10)]
    public int Octaves
    {
        get => Generator.Octaves;
        set
        {
            Generator.Octaves = value;
            InvokeParametersChangedEvent();
        }
    }

    [InputLine(Description = "Frequency:", Tooltip = FREQUENCY_TOOLTIP)]
    [InputLineSlider(0.00001f, 1f, 0.00001f, format: "0.#####")]
    public float Frequency
    {
        get => Generator.Frequency;
        set
        {
            Generator.Frequency = value;
            InvokeParametersChangedEvent();
        }
    }

    [InputLine(Description = "Fractal type:", Tooltip = FRACTAL_TYPE_TOOLTIP, IsVisible = false)]
    [InputLineCombobox(selected: 0, bind: ComboboxBind.Id)]
    [InputOption("FBM", id: (int)FractalType.Fbm)]
    [InputOption("Ridged", id: (int)FractalType.Ridged)]
    public FractalType Fractal
    {
        get => Generator.Fractal;
        set
        {
            Generator.Fractal = value;
            InvokeParametersChangedEvent();
        }
    }


    [InputLine(Description = "Lacunarity:", Tooltip = LACUNARITY_TOOLTIP)]
    [InputLineSlider(0.001f, 10f, 0.001f, format: "0.###")]
    public float Lacunarity
    {
        get => Generator.Lacunarity;
        set
        {
            Generator.Lacunarity = value;
            InvokeParametersChangedEvent();
        }
    }

    [InputLine(Description = "Persistence:", Tooltip = PERSISTENCE_TOOLTIP)]
    [InputLineSlider(0.001f, 3f, 0.001f, format: "0.###")]
    public float Persistence
    {
        get => Generator.Persistence;
        set
        {
            Generator.Persistence = value;
            InvokeParametersChangedEvent();
        }
    }

    [InputLine(Description = "Offset X:", Tooltip = OFFSET_X_TOOLTIP)]
    [InputLineSlider(-1000f, 1000f, 0.1f)]
    public float OffsetX
    {
        get => Generator.Offset.X;
        set
        {
            var offset = Generator.Offset;
            offset.X = value;
            Generator.Offset = offset;
            InvokeParametersChangedEvent();
        }
    }

    [InputLine(Description = "Offset Y:", Tooltip = OFFSET_Y_TOOLTIP)]
    [InputLineSlider(-1000f, 1000f, 0.1f)]
    public float OffsetY
    {
        get => Generator.Offset.Y;
        set
        {
            var offset = Generator.Offset;
            offset.Y = value;
            Generator.Offset = offset;
            InvokeParametersChangedEvent();
        }
    }

    [InputLine(Description = "Use domain warping", Tooltip = USE_DOMAIN_WARPING_TOOLTIP)]
    [InputLineCheckBox]
    public bool UseDomainWarping
    {
        get => Generator.EnableWarping;
        set
        {
            Generator.EnableWarping = value;
            InvokeParametersChangedEvent();
        }
    }

    [InputLine(Description = "Warping size:", Tooltip = WARPING_SIZE_TOOLTIP)]
    [InputLineSlider(0.1f, 2.0f, 0.01f, format: "0.##")]
    public float WarpingSize
    {
        get => Generator.WarpingSize;
        set
        {
            Generator.WarpingSize = value;
            InvokeParametersChangedEvent();
        }
    }

    [InputLine(Description = "Warping strength:", Tooltip = WARPING_STRENGTH_TOOLTIP)]
    [InputLineSlider(0.1f, 5.0f, 0.01f, format: "0.##")]
    public float WarpingStrength
    {
        get => Generator.WarpingStrength;
        set
        {
            Generator.WarpingStrength = value;
            InvokeParametersChangedEvent();
        }
    }

    public NoiseGeneratorOptionsBase(NoiseMapGenerator generator)
    {
        Generator = generator;
        Generator.EnableWarping = false;
    }

    public override float[,] GenerateMap()
    {
        return Generator.GenerateMap(_mapHeight, _mapWidth);
    }
}