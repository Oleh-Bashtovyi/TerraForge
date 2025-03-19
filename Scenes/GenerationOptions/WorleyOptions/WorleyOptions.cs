using Godot;
using TerrainGenerationApp.Generators;

namespace TerrainGenerationApp.Scenes.GeneratorOptions.Sharp;

public partial class WorleyOptions : Scripts.BaseGeneratorOptions
{
	private int _mapHeight = 100;
	private int _mapWidth = 100;
	private int _seed;
	private int _dotsCount = 100;
	private float _maxIntensity = 100;
	private bool _invert;

	private Label _mapHeightLabel;
	private Label _mapWidthLabel;
	private Label _dotsCountLabel;
	private Label _maxIntensityLabel;
	private Label _seedLabel;

	public override void _Ready()
	{
		_mapHeightLabel = GetNode<Label>("%MapHeightLabel");
		_mapWidthLabel = GetNode<Label>("%MapWidthLabel");
		_dotsCountLabel = GetNode<Label>("%DotsCountLabel");
		_maxIntensityLabel = GetNode<Label>("%MaxIntensityLabel");
		_seedLabel = GetNode<Label>("%SeedL");
	}

	public override float[,] GenerateMap()
	{
		return WorleyNoise.GenerateMap(_mapHeight, _mapWidth, _dotsCount, _maxIntensity, _invert, _seed);
	}

	private void OnMapHeightValueChanged(float value)
	{
		_mapHeight = Mathf.RoundToInt(value);
		_mapHeightLabel.Text = _mapHeight.ToString();
        InvokeParametersChangedEvent();
}

	private void OnMapWidthValueChanged(float value)
	{
		_mapWidth = Mathf.RoundToInt(value);
		_mapWidthLabel.Text = _mapWidth.ToString();
        InvokeParametersChangedEvent();
}

	private void OnDotsCountValueChanged(float value)
	{
		_dotsCount = Mathf.RoundToInt(value);
		_dotsCountLabel.Text = _dotsCount.ToString();
        InvokeParametersChangedEvent();
}

	private void OnMaxIntensityValueChanged(float value)
	{
		_maxIntensity = value;
		_maxIntensityLabel.Text = value.ToString();
        InvokeParametersChangedEvent();
}

	private void OnInvertToggled(bool toggledOn)
	{
		_invert = toggledOn;
        InvokeParametersChangedEvent();
}

	private void OnSeedValueChanged(float value)
	{
		_seed = Mathf.RoundToInt(value);
		_seedLabel.Text = _seed.ToString();
        InvokeParametersChangedEvent();
    }
}
