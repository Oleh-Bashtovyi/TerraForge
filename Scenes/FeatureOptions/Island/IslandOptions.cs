using TerrainGenerationApp.Domain.Generators.Islands;
using TerrainGenerationApp.Scenes.BuildingBlocks.Attributes;
using TerrainGenerationApp.Scenes.BuildingBlocks.Containers;

namespace TerrainGenerationApp.Scenes.FeatureOptions.Island;

public partial class IslandOptions : OptionsContainer
{
    private readonly IslandApplier _islandApplier = new();
    private int _selectedIslandType = 0;
    private int _selectedDistanceFunction = 0;

    [InputLine(Description = "Island type:", Id = "IslandType")]
    [InputLineCombobox(selected: 0, bind:ComboboxBind.Id)]
    [InputOption("Single At Center", 1)]
    [InputOption("Single Randomly", 2)]
    [InputOption("Many", 3)]
    public int SelectedIslandType
    {
        get => _selectedIslandType;
        set
        {
            _selectedIslandType = value;
            _islandApplier.ApplierType = value switch
            {
                0 => IslandApplier.IslandType.SingleAtCenter,
                1 => IslandApplier.IslandType.SingleRandomly,
                2 => IslandApplier.IslandType.Many,
                _ => _islandApplier.ApplierType
            };
        }
    }

    [InputLine(Description = "Distance function:", Id = "DistanceFunction")]
    [InputLineCombobox(selected:0, bind:ComboboxBind.Id)]
    [InputOption("Euclidean", 1)]
    [InputOption("Euclidean Squared", 2)]
    [InputOption("Manhattan", 3)]
    [InputOption("Diagonal", 4)]
    [InputOption("Hyperboloid", 5)]
    [InputOption("Blob", 6)]
    [InputOption("Square Bump", 7)]
    public int SelectedDistanceFunction
    {
        get => _selectedDistanceFunction;
        set
        {
            _selectedDistanceFunction = value;
            _islandApplier.DistanceFunction = value switch
            {
                1 => IslandApplier.DistanceType.Euclidean,
                2 => IslandApplier.DistanceType.EuclideanSquared,
                3 => IslandApplier.DistanceType.Manhattan,
                4 => IslandApplier.DistanceType.Diagonal,
                5 => IslandApplier.DistanceType.Hyperboloid,
                6 => IslandApplier.DistanceType.Blob,
                7 => IslandApplier.DistanceType.SquareBump,
                _ => _islandApplier.DistanceFunction
            };
        }
    }

    [InputLine(Description = "Islands radius:")]
    [InputLineSlider(1.0f, 200.0f)]
    public float Radius
    {
        get => _islandApplier.RadiusAroundIslands;
        set
        {
            _islandApplier.RadiusAroundIslands = value;
            InvokeParametersChangedEvent();
        }
    }

    [InputLine(Description = "Islands count:")]
    [InputLineSlider(1.0f, 8.0f)]
    public int IslandsCount
    {
        get => _islandApplier.IslandsCount;
        set
        {
            _islandApplier.IslandsCount = value;
            InvokeParametersChangedEvent();
        }
    }

    [InputLine(Description = "Seed:")]
    [InputLineSlider(0.0f, 10000.0f)]
    public int Seed
    {
        get => (int)_islandApplier.Seed;
        set
        {
            _islandApplier.Seed = (ulong)value;
            InvokeParametersChangedEvent();
        }
    }

    [InputLine(Description = "Mix strength:")]
    [InputLineSlider(0.0f, 1.0f, 0.01f)]
    public float MixStrength
    {
        get => _islandApplier.MixStrength;
        set
        {
            _islandApplier.MixStrength = value;
            InvokeParametersChangedEvent();
        }
    }

    [InputLine(Description = "Min distance factor:")]
    [InputLineSlider(0.0f, 1.0f, 0.01f)]
    public float MinDistanceFactor
    {
        get => _islandApplier.MinDistanceFactor;
        set
        {
            _islandApplier.MinDistanceFactor = value;
            InvokeParametersChangedEvent();
        }
    }

    [InputLine(Description = "Horizontal offset:")]
    [InputLineSlider(0.0f, 1.0f, 0.01f)]
    public float HorizontalOffset
    {
        get => _islandApplier.HorizontalOffsetsToCenter;
        set
        {
            _islandApplier.HorizontalOffsetsToCenter = value;
            InvokeParametersChangedEvent();
        }
    }

    [InputLine(Description = "Vertical offset:")]
    [InputLineSlider(0.0f, 1.0f, 0.01f)]
    public float VerticalOffset
    {
        get => _islandApplier.VerticalOffsetsToCenter;
        set
        {
            _islandApplier.VerticalOffsetsToCenter = value;
            InvokeParametersChangedEvent();
        }
    }

    public IslandApplier IslandApplier => _islandApplier;

    public override void _Ready()
    {
        BuildingBlocks.InputLine.InputLineManager.CreateInputLinesForObject(this, this);
    }
}