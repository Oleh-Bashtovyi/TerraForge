using TerrainGenerationApp.Domain.Generators.Islands;
using TerrainGenerationApp.Scenes.BuildingBlocks.Attributes;
using TerrainGenerationApp.Scenes.BuildingBlocks.Containers;
using TerrainGenerationApp.Scenes.BuildingBlocks.InputLine;

namespace TerrainGenerationApp.Scenes.FeatureOptions.Island;

public partial class IslandOptions : OptionsContainer
{
    private readonly IslandApplier _islandApplier = new();

    [InputLine(Description = "Island type:", Id = "IslandType")]
    [InputLineCombobox(selected: 0, bind:ComboboxBind.Id)]
    [InputOption("Single At Center", id: (int)IslandApplier.IslandType.SingleAtCenter)]
    [InputOption("Single Randomly",  id: (int)IslandApplier.IslandType.SingleRandomly)]
    [InputOption("Many",             id: (int)IslandApplier.IslandType.Many)]
    public IslandApplier.IslandType SelectedIslandType
    {
        get => _islandApplier.ApplierType;
        set
        {
            _islandApplier.ApplierType = value;
            InvokeParametersChangedEvent();
        }
    }

    [InputLine(Description = "Distance function:", Id = "DistanceFunction")]
    [InputLineCombobox(selected:0, bind:ComboboxBind.Id)]
    [InputOption("Euclidean",         id: (int)IslandApplier.DistanceType.Euclidean)]
    [InputOption("Euclidean Squared", id: (int)IslandApplier.DistanceType.EuclideanSquared)]
    [InputOption("Manhattan",         id: (int)IslandApplier.DistanceType.Manhattan)]
    [InputOption("Diagonal",          id: (int)IslandApplier.DistanceType.Diagonal)]
    [InputOption("Hyperboloid",       id: (int)IslandApplier.DistanceType.Hyperboloid)]
    [InputOption("Blob",              id: (int)IslandApplier.DistanceType.Blob)]
    [InputOption("Square Bump",       id: (int)IslandApplier.DistanceType.SquareBump)]
    public IslandApplier.DistanceType SelectedDistanceFunction
    {
        get => _islandApplier.DistanceFunction;
        set
        {
            _islandApplier.DistanceFunction = value;
            InvokeParametersChangedEvent();
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
        base._Ready();
        InputLineManager.CreateInputLinesForObject(obj: this, container: this);
    }
}