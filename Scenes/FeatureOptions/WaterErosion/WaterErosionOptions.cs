using TerrainGenerationApp.Domain.Generators.WaterErosion;
using TerrainGenerationApp.Scenes.BuildingBlocks.Attributes;
using TerrainGenerationApp.Scenes.BuildingBlocks.Containers;
using TerrainGenerationApp.Scenes.BuildingBlocks.InputLine;

namespace TerrainGenerationApp.Scenes.FeatureOptions.WaterErosion;

public partial class WaterErosionOptions : OptionsContainer
{
	private readonly WaterErosionApplier _waterErosionApplier = new();

	[InputLine(Description = "Iterations count:")]
	[InputLineSlider(1, 1000)]
    public int IterationsCount
    {
		get => _waterErosionApplier.Iterations;
        set
        {
			_waterErosionApplier.Iterations = value;
			InvokeParametersChangedEvent();
        }
    }

    [InputLine(Description = "Rain power:")]
    [InputLineSlider(0.0f, 1.0f, 0.01f, format: "0.##")]
    public float RainPower
    {
        get => _waterErosionApplier.RainPower;
        set
        {
            _waterErosionApplier.RainPower = value;
            InvokeParametersChangedEvent();
        }
    }

    [InputLine(Description = "Rain chance:")]
    [InputLineSlider(0.0f, 1.0f, 0.01f, format: "0.##")]
    public float RainChance
    {
		get => _waterErosionApplier.RainChance;
        set
        {
            _waterErosionApplier.RainChance = value;
            InvokeParametersChangedEvent();
        }
    }

    [InputLine(Description = "Rain type:")]
	[InputLineCombobox(selected: 0, bind: ComboboxBind.Id)]
	[InputOption("Static values", id: (int)RainType.StaticValues)]
	[InputOption("Random values", id: (int)RainType.RandomValues)]
    public RainType RainType
    {
        get => _waterErosionApplier.RainType;
        set
        {
            _waterErosionApplier.RainType = value;
            InvokeParametersChangedEvent();
        }
    }

    [InputLine(Description = "Rain rate type:")]
    [InputLineCombobox(selected: 0, bind: ComboboxBind.Id)]
    [InputOption("Once",            id: (int)RainRateType.Once)]
    [InputOption("Every iteration", id: (int)RainRateType.EveryIteration)]
    [InputOption("Randomly",        id: (int)RainRateType.Randomly)]
    public RainRateType RainRateType
    {
        get => _waterErosionApplier.RainRateType;
        set
        {
            _waterErosionApplier.RainRateType = value;
            InvokeParametersChangedEvent();
        }
    }

	public WaterErosionApplier WaterErosionApplier => _waterErosionApplier;

    public override void _Ready()
	{
        base._Ready();
        InputLineManager.CreateInputLinesForObject(obj: this, container: this);
    }
}
