using Godot;
using TerrainGenerationApp.Domain.Generators;
using TerrainGenerationApp.Domain.Utils.TerrainUtils;
using TerrainGenerationApp.Scenes.BuildingBlocks.Attributes;
using TerrainGenerationApp.Scenes.BuildingBlocks.InputLine;
// ReSharper disable InconsistentNaming

namespace TerrainGenerationApp.Scenes.GeneratorOptions;

public partial class WorleyOptions : BaseGeneratorOptions
{
	private const string MAP_HEIGHT_TOOLTIP = "Map height in pixels. Affects the vertical resolution of the generated map.";
	private const string MAP_WIDTH_TOOLTIP = "Map width in pixels. Affects the horizontal resolution of the generated map.";
	private const string SEED_TOOLTIP = "Seed for random number generation. Changing this value will produce a different map.";
	private const string DOTS_COUNT_TOOLTIP = "Number of feature points (dots) used to generate the Worley noise. " +
                                              "More dots will create a more complex pattern.";
	private const string MAX_DISTANCE_TOOLTIP = "Maximum distance for the Worley noise. " +
                                                "This controls how far apart the dots can influence the noise.";
	private const string INVERT_TOOLTIP = "If true, the generated map will be inverted. " +
										  "This can create a different visual effect by reversing the noise values.";
	private const string DISTANCE_FUNCTION_TOOLTIP = "The distance function used to calculate distances between points. " +
													  "Options include Euclidean, Manhattan and other.";

    private int _mapHeight = 150;
	private int _mapWidth = 150;
	private int _seed = 42;
	private int _dotsCount = 80;
	private float _maxDistance = 20;
	private bool _invert;
	private DistanceType _distanceFunction = DistanceType.Euclidean;

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
		get => _seed;
		set
		{
			_seed = value;
			InvokeParametersChangedEvent();
		}
	}

	[InputLine(Description = "Dots count:", Tooltip = DOTS_COUNT_TOOLTIP)]
	[InputLineSlider(1, 512)]
	public int DotsCount
	{
		get => _dotsCount;
		set
		{
			_dotsCount = value;
			InvokeParametersChangedEvent();
		}
	}

	[InputLine(Description = "Max distance:", Tooltip = MAX_DISTANCE_TOOLTIP)]
	[InputLineSlider(0f, 300f)]
	public float MaxDistance
	{
		get => _maxDistance;
		set
		{
			_maxDistance = value;
			InvokeParametersChangedEvent();
		}
	}

	[InputLine(Description = "Distance function:", Tooltip = DISTANCE_FUNCTION_TOOLTIP)]
	[InputLineCombobox(selected: 0,   bind: ComboboxBind.Id)]
	[InputOption("Euclidean",         id: (int)DistanceType.Euclidean)]
	[InputOption("Euclidean Squared", id: (int)DistanceType.EuclideanSquared)]
	[InputOption("Manhattan",         id: (int)DistanceType.Manhattan)]
	[InputOption("Diagonal",          id: (int)DistanceType.Diagonal)]
	[InputOption("Hyperboloid",       id: (int)DistanceType.Hyperboloid)]
    public DistanceType DistanceFunction
    {
		get => _distanceFunction;
        set
        {
			_distanceFunction = value;
            InvokeParametersChangedEvent();
        }
    }

	[InputLine(Description = "Invert:", Tooltip = INVERT_TOOLTIP)]
	[InputLineCheckBox(checkboxType: CheckboxType.CheckButton)]
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
		InputLineManager.CreateInputLinesForObject(obj: this, container: this);
	}

	public override float[,] GenerateMap()
	{
		return WorleyNoise.GenerateMap(_mapHeight, _mapWidth, _dotsCount, _maxDistance, _invert, _seed, _distanceFunction);
	}
}
