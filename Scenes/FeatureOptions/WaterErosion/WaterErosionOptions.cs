using Godot;
using TerrainGenerationApp.Domain.Generators.WaterErosion;

namespace TerrainGenerationApp.Scenes.FeatureOptions.WaterErosion;

public partial class WaterErosionOptions : VBoxContainer
{
	private Label _iterationsLabel;
	private Label _rainPowerLabel;
	private Label _rainChanceLabel;
	private Slider _iterationSlider;
	private Slider _rainPowerSlider;
	private Slider _rainChanceSlider;
	private OptionButton _rainTypeOptions;
	private OptionButton _rainRateTypeOptions;

	private WaterErosionApplier _waterErosionApplier;

	public WaterErosionApplier WaterErosionApplier
	{
		get => _waterErosionApplier;
	}

	public override void _Ready()
	{
		_iterationsLabel = GetNode<Label>("%IterationsLabel");
		_rainChanceLabel = GetNode<Label>("%RainChanceLabel");
		_rainPowerLabel = GetNode<Label>("%RainPowerLabel");
		_iterationSlider = GetNode<Slider>("%IterationsSlider");
		_rainChanceSlider = GetNode<Slider>("%RainChanceSlider");
		_rainPowerSlider = GetNode<Slider>("%RainPowerSlider");
		_rainTypeOptions = GetNode<OptionButton>("%RainTypeOptions");
		_rainRateTypeOptions = GetNode<OptionButton>("%RainRateTypeOptions");

		_rainTypeOptions.Clear();
		_rainTypeOptions.AddItem("Static values", 1);
		_rainTypeOptions.AddItem("Random values", 2);
		
		_rainRateTypeOptions.Clear();
		_rainRateTypeOptions.AddItem("Every iteration", 1);
		_rainRateTypeOptions.AddItem("Randomly", 2);
		_rainRateTypeOptions.AddItem("Once", 3);

		MakeAllOptionsAvailable();

		_waterErosionApplier = new WaterErosionApplier();
		_waterErosionApplier.Iterations = 30;
		_waterErosionApplier.RainChance = 0.5f;
		_waterErosionApplier.RainPower = 0.01f;
		_waterErosionApplier.RainRateType = RainRateType.EveryIteration;
		_waterErosionApplier.RainType = RainType.StaticValues;
	}


    public void DisableAllOptions()
    {
		_iterationSlider.Editable = false;
        _rainChanceSlider.Editable = false;
        _rainPowerSlider.Editable = false;
        _rainRateTypeOptions.Disabled = true;
        _rainTypeOptions.Disabled = true;
    }

    public void EnableAllOptions()
    {
        _iterationSlider.Editable = true;
        _rainChanceSlider.Editable = true;
        _rainPowerSlider.Editable = true;
        _rainRateTypeOptions.Disabled = false;
        _rainTypeOptions.Disabled = false;
    }



    private void _on_iterations_slider_value_changed(float value)
	{
		_waterErosionApplier.Iterations = Mathf.RoundToInt(value);
		_iterationsLabel.Text = _waterErosionApplier.Iterations.ToString();
	}

	private void _on_rain_power_slider_value_changed(float value)
	{
		_waterErosionApplier.RainPower = value;
		_rainPowerLabel.Text = _waterErosionApplier.RainPower.ToString();
	}

	private void _on_rain_chance_slider_value_changed(float value)
	{
		//GD.Print(value);
		_waterErosionApplier.RainChance = value;
		//GD.Print(_waterErosionApplier.RainChance);
		_rainChanceLabel.Text = _waterErosionApplier.RainChance.ToString();
	}



	private void _on_rain_rate_type_options_item_selected(int index)
	{
		var itemId = _rainRateTypeOptions.GetItemId(index);

		_waterErosionApplier.RainRateType = itemId switch
		{
			1 => RainRateType.EveryIteration,
			2 => RainRateType.Randomly,
			3 => RainRateType.Once,
			_ => RainRateType.Once 
		};
	}
	
	private void _on_rain_type_options_item_selected(int index)
	{
		var itemId = _rainTypeOptions.GetItemId(index);

		_waterErosionApplier.RainType = itemId switch
		{
			1 => RainType.StaticValues,
			2 => RainType.RandomValues,
			_ => RainType.StaticValues
		};
	}
	
	
	public void MakeAllOptionsUnavailable()
	{
		_iterationSlider.Editable = false;
		_rainChanceSlider.Editable = false;
		_rainPowerSlider.Editable = false;
		_rainRateTypeOptions.Disabled = true;
		_rainTypeOptions.Disabled = true;
	}
	
	public void MakeAllOptionsAvailable()
	{
		_iterationSlider.Editable = true;
		_rainChanceSlider.Editable = true;
		_rainPowerSlider.Editable = true;
		_rainRateTypeOptions.Disabled = false;
		_rainTypeOptions.Disabled = false;
    }
}
