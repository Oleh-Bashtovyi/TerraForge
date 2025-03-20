using Godot;
using TerrainGenerationApp.Generators.DomainWarping;

namespace TerrainGenerationApp.Scenes.GenerationOptions.DomainWarpingOptions;

public partial class DomainWarpingOptions : VBoxContainer
{
	[Signal]
	public delegate void ParametersChangedEventHandler();

	private float _warpingStrength;
	private float _noiseScale;
	private Label _strengthLabel;
	private Label _noiseScaleLabel;
	private DomainWarpingApplier _domainWarpingApplier;

	public DomainWarpingApplier DomainWarpingApplier
	{
		get => _domainWarpingApplier;
	}

	public override void _Ready()
	{
		_domainWarpingApplier = new();
		_domainWarpingApplier.XNoise.Scale = 8;
		_domainWarpingApplier.YNoise.Scale = 8;

		_strengthLabel = GetNode<Label>("%StrengthL");
		_noiseScaleLabel = GetNode<Label>("%NoiseScaleL");
	}

	private void OnStrengthSValueChanged(float value)
	{
		_domainWarpingApplier.WarpingStrength = value;
		_strengthLabel.Text = value.ToString();
		EmitSignal(DomainWarpingOptions.SignalName.ParametersChanged);
	}

	private void OnNoiseScaleSValueChanged(float value)
	{
		_domainWarpingApplier.XNoise.Scale = value;
		_domainWarpingApplier.YNoise.Scale = value;
		_noiseScaleLabel.Text = value.ToString();
		EmitSignal(DomainWarpingOptions.SignalName.ParametersChanged);
	}
}
