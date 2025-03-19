using System;
using Godot;
using TerrainGenerationApp.Generators;

namespace TerrainGenerationApp.Scenes.GeneratorOptions.Scripts;

public partial class DiamondSquareOptions : BaseGeneratorOptions
{
	private int _seed = 0;
	private int _terrainPower = 8;
	private float _roughness = 5f;

	private Label _terrainPowerLabel;
	private Label _roughnessLabel;
	private Label _seedLabel;
	private Slider _terrainPowerSlider;

	public override void _Ready()
	{
		_terrainPowerLabel = GetNode<Label>("%TerrainPowerLabel");
		_terrainPowerSlider = GetNode<Slider>("%TerrainPowerSlider");
		_roughnessLabel = GetNode<Label>("%RoughnessLabel");
		_seedLabel = GetNode<Label>("%SeedL");
	}

	public override float[,] GenerateMap()
	{
		return DiamondSquare.GenerateMap(_terrainPower, _roughness, _seed);
	}


	private void OnTerrainPowerValueChanged(float value)
	{
		_terrainPower = Mathf.RoundToInt(value);
		var size = (int)Math.Pow(2, _terrainPower) + 1;
		_terrainPowerLabel.Text = _terrainPower.ToString();
		_terrainPowerSlider.TooltipText = $"Size of map (2^terrain_power + 1). Currently: {size}x{size}";
        InvokeParametersChangedEvent();
    }

	private void OnRoughnessValueChanged(float value)
	{
		_roughness = value;
		_roughnessLabel.Text = value.ToString();
        InvokeParametersChangedEvent();
    }

	private void OnSeedValueChanged(float value)
	{
		_seed = Mathf.RoundToInt(value);
		_seedLabel.Text = _seed.ToString();
        InvokeParametersChangedEvent();
    }
}
