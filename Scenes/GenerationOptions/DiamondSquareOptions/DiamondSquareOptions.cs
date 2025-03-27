using Godot;
using System;
using TerrainGenerationApp.Generators;

namespace TerrainGenerationApp.Scenes.GenerationOptions.DiamondSquareOptions;

public partial class DiamondSquareOptions : BaseGeneratorOptions
{
	private Label _terrainPowerLabel;
	private Label _roughnessLabel;
	private Label _seedLabel;
	private Slider _terrainPowerSlider;
    private Slider _roughnessSlider;
    private Slider _seedSlider;
	
    private int _seed = 0;
	private int _terrainPower = 8;
	private float _roughness = 5f;


    public override void _Ready()
	{
		_terrainPowerSlider = GetNode<Slider>("%TerrainPowerSlider");
		_roughnessSlider = GetNode<Slider>("%RoughnessSlider");
        _seedSlider = GetNode<Slider>("%SeedSlider");
        _terrainPowerLabel = GetNode<Label>("%TerrainPowerLabel");
		_roughnessLabel = GetNode<Label>("%RoughnessLabel");
		_seedLabel = GetNode<Label>("%SeedLabel");

        _terrainPowerSlider.ValueChanged += OnTerrainPowerSliderValueChanged;
        _roughnessSlider.ValueChanged += OnRoughnessValueChanged;
        _seedSlider.ValueChanged += OnSeedValueChanged;
    }

    private void OnSeedValueChanged(double value)
    {
        _seed = Mathf.RoundToInt(value);
        _seedLabel.Text = _seed.ToString();
        InvokeParametersChangedEvent();
    }

    private void OnRoughnessValueChanged(double value)
    {
        _roughness = (float)value;
        _roughnessLabel.Text = value.ToString();
        InvokeParametersChangedEvent();
    }

    private void OnTerrainPowerSliderValueChanged(double value)
    {
        _terrainPower = Mathf.RoundToInt(value);
        var size = (int)Math.Pow(2, _terrainPower) + 1;
        _terrainPowerLabel.Text = _terrainPower.ToString();
        _terrainPowerSlider.TooltipText = $"Size of map (2^terrain_power + 1). Currently: {size}x{size}";
        InvokeParametersChangedEvent();
    }

    public override float[,] GenerateMap()
	{
		return DiamondSquare.GenerateMap(_terrainPower, _roughness, _seed);
	}

    public override void EnableAllOptions()
    {
        _terrainPowerSlider.Editable = true;
        _roughnessSlider.Editable = true;
        _seedSlider.Editable = true;
    }

    public override void DisableAllOptions()
    {
        _terrainPowerSlider.Editable = false;
        _roughnessSlider.Editable = false;
        _seedSlider.Editable = false;
    }
}
