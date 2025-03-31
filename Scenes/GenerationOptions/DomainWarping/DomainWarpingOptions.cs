using System;
using Godot;
using TerrainGenerationApp.Generators.DomainWarping;

namespace TerrainGenerationApp.Scenes.GenerationOptions.DomainWarping;

public partial class DomainWarpingOptions : VBoxContainer
{
	public event Action ParametersChanged;

    private float _warpingStrength;
	private float _noiseScale;
	private Label _strengthLabel;
	private Label _noiseScaleLabel;
	private Slider _strengthSlider;
    private Slider _noiseScaleSlider;


    private DomainWarpingApplier _domainWarpingApplier;
	public DomainWarpingApplier DomainWarpingApplier
	{
		get => _domainWarpingApplier;
	}

	public override void _Ready()
	{
		_strengthLabel = GetNode<Label>("%StrengthL");
		_noiseScaleLabel = GetNode<Label>("%NoiseScaleL");
		_strengthSlider = GetNode<Slider>("%StrengthSlider");
        _noiseScaleSlider = GetNode<Slider>("%NoiseScaleSlider");
        _domainWarpingApplier = new();
		_domainWarpingApplier.XNoise.Scale = 8;
		_domainWarpingApplier.YNoise.Scale = 8;
	}

    public void DisableAllOptions()
    {
		_strengthSlider.Editable = false;
        _noiseScaleSlider.Editable = false;
    }

    public void EnableAllOptions()
    {
        _strengthSlider.Editable = true;
        _noiseScaleSlider.Editable = true;
    }



    private void OnStrengthSValueChanged(float value)
	{
		_domainWarpingApplier.WarpingStrength = value;
		_strengthLabel.Text = value.ToString();
        ParametersChanged?.Invoke();
    }

	private void OnNoiseScaleSValueChanged(float value)
	{
		_domainWarpingApplier.XNoise.Scale = value;
		_domainWarpingApplier.YNoise.Scale = value;
		_noiseScaleLabel.Text = value.ToString();
		ParametersChanged?.Invoke();
	}
}
