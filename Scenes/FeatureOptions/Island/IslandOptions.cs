using TerrainGenerationApp.Domain.Generators.Islands;
using TerrainGenerationApp.Domain.Utils.TerrainUtils;
using TerrainGenerationApp.Scenes.BuildingBlocks.Attributes;
using TerrainGenerationApp.Scenes.BuildingBlocks.Containers;
using TerrainGenerationApp.Scenes.BuildingBlocks.InputLine;

namespace TerrainGenerationApp.Scenes.FeatureOptions.Island;

public partial class IslandOptions : OptionsContainer
{
    private readonly IslandApplier _islandApplier = new();

    [InputLine(Description = "Center type:", Id = "CenterType")]
    [InputLineCombobox(selected: 0, bind:ComboboxBind.Id)]
    [InputOption("Single",          id: (int)IslandApplier.CenterType.Single)]
    [InputOption("Single random",   id: (int)IslandApplier.CenterType.SingleRandomly)]
    [InputOption("Many random",     id: (int)IslandApplier.CenterType.Many)]
    public IslandApplier.CenterType SelectedCenterType
    {
        get => _islandApplier.ApplierType;
        set
        {
            _islandApplier.ApplierType = value;
            InvokeParametersChangedEvent();
        }
    }

    [InputLine(Description = "Distance function:", Id = "DistanceFunction")]
    [InputLineCombobox(selected:0,    bind:ComboboxBind.Id)]
    [InputOption("Euclidean",         id: (int)DistanceType.Euclidean)]
    [InputOption("Euclidean Squared", id: (int)DistanceType.EuclideanSquared)]
    [InputOption("Manhattan",         id: (int)DistanceType.Manhattan)]
    [InputOption("Diagonal",          id: (int)DistanceType.Diagonal)]
    [InputOption("Hyperboloid",       id: (int)DistanceType.Hyperboloid)]
    public DistanceType SelectedDistanceFunction
    {
        get => _islandApplier.DistanceFunction;
        set
        {
            _islandApplier.DistanceFunction = value;
            InvokeParametersChangedEvent();
        }
    }

    [InputLine(Description = "Centers radius:")]
    [InputLineSlider(1.0f, 250.0f)]
    public float Radius
    {
        get => _islandApplier.RadiusAroundIslands;
        set
        {
            _islandApplier.RadiusAroundIslands = value;
            InvokeParametersChangedEvent();
        }
    }

    [InputLine(Description = "Centers count:")]
    [InputLineSlider(1.0f, 8.0f)]
    public int CentersCount
    {
        get => _islandApplier.CentersCount;
        set
        {
            _islandApplier.CentersCount = value;
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

/*    [InputLine(Description = "Min distance factor:")]
    [InputLineSlider(0.0f, 1.0f, 0.01f)]
    public float MinDistanceFactor
    {
        get => _islandApplier.MinDistanceFactor;
        set
        {
            _islandApplier.MinDistanceFactor = value;
            InvokeParametersChangedEvent();
        }
    }*/

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