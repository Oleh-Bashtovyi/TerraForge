using Godot;
using TerrainGenerationApp.Generators;

namespace TerrainGenerationApp.Scenes.GenerationOptions.Worley;

public partial class WorleyOptions : BaseGeneratorOptions
{
    private Label _mapHeightLabel;
    private Label _mapWidthLabel;
    private Label _dotsCountLabel;
    private Label _maxIntensityLabel;
    private Label _seedLabel;
	private Slider _mapHeightSlider;
    private Slider _mapWidthSlider;
    private Slider _dotsCountSlider;
    private Slider _maxIntensitySlider;
    private Slider _seedSlider;
    private CheckButton _invertCheckBox;

    private int _mapHeight = 100;
	private int _mapWidth = 100;
	private int _seed;
	private int _dotsCount = 100;
	private float _maxIntensity = 100;
	private bool _invert;

	public override void _Ready()
	{
		_mapHeightLabel = GetNode<Label>("%MapHeightLabel");
		_mapWidthLabel = GetNode<Label>("%MapWidthLabel");
		_dotsCountLabel = GetNode<Label>("%DotsCountLabel");
		_maxIntensityLabel = GetNode<Label>("%MaxIntensityLabel");
		_seedLabel = GetNode<Label>("%SeedLabel");
        _mapHeightSlider = GetNode<Slider>("%MapHeightSlider");
        _mapWidthSlider = GetNode<Slider>("%MapWidthSlider");
        _dotsCountSlider = GetNode<Slider>("%DotsCountSlider");
        _maxIntensitySlider = GetNode<Slider>("%MaxIntensitySlider");
        _seedSlider = GetNode<Slider>("%SeedSlider");
        _invertCheckBox = GetNode<CheckButton>("%InvertCheckButton");
		_mapHeightSlider.ValueChanged += OnMapHeightValueChanged;
        _mapWidthSlider.ValueChanged += OnMapWidthValueChanged;
        _dotsCountSlider.ValueChanged += OnDotsCountValueChanged;
        _maxIntensitySlider.ValueChanged += OnMaxIntensityValueChanged;
        _seedSlider.ValueChanged += OnSeedValueChanged;
        _invertCheckBox.Toggled += OnInvertToggled;
    }

	public override float[,] GenerateMap()
	{
		return WorleyNoise.GenerateMap(_mapHeight, _mapWidth, _dotsCount, _maxIntensity, _invert, _seed);
	}

	private void OnMapHeightValueChanged(double value)
	{
		_mapHeight = Mathf.RoundToInt(value);
		_mapHeightLabel.Text = _mapHeight.ToString();
        InvokeParametersChangedEvent();
}

	private void OnMapWidthValueChanged(double value)
	{
		_mapWidth = Mathf.RoundToInt(value);
		_mapWidthLabel.Text = _mapWidth.ToString();
        InvokeParametersChangedEvent();
}
	
    private void OnDotsCountValueChanged(double value)
	{
		_dotsCount = Mathf.RoundToInt(value);
		_dotsCountLabel.Text = _dotsCount.ToString();
        InvokeParametersChangedEvent();
}

	private void OnMaxIntensityValueChanged(double value)
	{
		_maxIntensity = (float)value;
		_maxIntensityLabel.Text = _maxIntensity.ToString();
        InvokeParametersChangedEvent();
    }

	private void OnSeedValueChanged(double value)
	{
		_seed = Mathf.RoundToInt(value);
		_seedLabel.Text = _seed.ToString();
        InvokeParametersChangedEvent();
    }

	private void OnInvertToggled(bool toggledOn)
	{
		_invert = toggledOn;
        InvokeParametersChangedEvent();
    }
}
