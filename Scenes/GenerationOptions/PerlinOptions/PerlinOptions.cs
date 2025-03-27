using Godot;
using TerrainGenerationApp.Generators;

namespace TerrainGenerationApp.Scenes.GenerationOptions.PerlinOptions;

public partial class PerlinOptions : BaseGeneratorOptions
{
	private PerlinNoise _generator;
	private int _seed = 0;

	// UI Element references
	private Label _mapHeightLabel;
	private Label _mapWidthLabel;
	private Label _offsetXLabel;
	private Label _offsetYLabel;
	private Label _scaleLabel;
	private Label _persistenceLabel;
	private Label _lacunarityLabel;
	private Label _octavesLabel;
	private Label _seedLabel;
	private Label _warpingStrengthLabel;
	private Label _warpingSizeLabel;
	private Slider _mapHeightSlider;
    private Slider _mapWidthSlider;
    private Slider _offsetXSlider;
    private Slider _offsetYSlider;
    private Slider _scaleSlider;
	private Slider _lacunaritySlider;
	private Slider _persistanceSlider;
    private Slider _octavesSlider;
    private Slider _seedSlider;
    private Slider _warpingStrengthSlider;
    private Slider _warpingSizeSlider;
    private CheckButton _useWarpingCheckButton;

	public PerlinNoise Generator
	{
		get => _generator;
		set => _generator = value;
	}

	public override void _Ready()
	{
		if (_generator == null)
		{
			_generator = new PerlinNoise();
		}

		// Get UI references
		_mapHeightLabel = GetNode<Label>("%MapHeightLabel");
		_mapWidthLabel = GetNode<Label>("%MapWidthLabel");
		_offsetXLabel = GetNode<Label>("%OffsetXLabel");
		_offsetYLabel = GetNode<Label>("%OffsetYLabel");
		_scaleLabel = GetNode<Label>("%ScaleLabel");
		_persistenceLabel = GetNode<Label>("%PersistanceLabel");
		_lacunarityLabel = GetNode<Label>("%LacunarityLabel");
		_octavesLabel = GetNode<Label>("%OctavesLabel");
		_seedLabel = GetNode<Label>("%SeedL");
		_warpingStrengthLabel = GetNode<Label>("%WarpingStrengthL");
		_warpingSizeLabel = GetNode<Label>("%WarpingSizeL");
		_useWarpingCheckButton = GetNode<CheckButton>("%UseWarpingCheckButton");
		_mapHeightSlider = GetNode<Slider>("%MapHeightSlider");
        _mapWidthSlider = GetNode<Slider>("%MapWidthSlider");
        _offsetXSlider = GetNode<Slider>("%OffsetXSlider");
        _offsetYSlider = GetNode<Slider>("%OffsetYSlider");
        _scaleSlider = GetNode<Slider>("%ScaleSlider");
        _persistanceSlider = GetNode<Slider>("%PersistanceSlider");
        _lacunaritySlider = GetNode<Slider>("%LacunaritySlider");
        _octavesSlider = GetNode<Slider>("%OctavesSlider");
        _seedSlider = GetNode<Slider>("%SeedSlider");
        _warpingStrengthSlider = GetNode<Slider>("%WarpingStrengthSlider");
        _warpingSizeSlider = GetNode<Slider>("%WarpingSizeSlider");
    }


	public override float[,] GenerateMap()
	{
		return _generator.GenerateMap();
	}

    public override void DisableAllOptions()
    {
        _mapHeightSlider.Editable = false;
        _mapWidthSlider.Editable = false;
        _offsetXSlider.Editable = false;
        _offsetYSlider.Editable = false;
        _scaleSlider.Editable = false;
        _persistanceSlider.Editable = false;
        _lacunaritySlider.Editable = false;
        _octavesSlider.Editable = false;
        _seedSlider.Editable = false;
        _warpingStrengthSlider.Editable = false;
        _warpingSizeSlider.Editable = false;
    }

    public override void EnableAllOptions()
    {
        _mapHeightSlider.Editable = true;
        _mapWidthSlider.Editable = true;
        _offsetXSlider.Editable = true;
        _offsetYSlider.Editable = true;
        _scaleSlider.Editable = true;
        _persistanceSlider.Editable = true;
        _lacunaritySlider.Editable = true;
        _octavesSlider.Editable = true;
        _seedSlider.Editable = true;
        _warpingStrengthSlider.Editable = true;
        _warpingSizeSlider.Editable = true;
    }


    private void OnMapHeightSliderValueChanged(float value)
	{
		_generator.MapHeight = Mathf.RoundToInt(value);
		_mapHeightLabel.Text = _generator.MapHeight.ToString();
        InvokeParametersChangedEvent();
}

	private void OnMapWidthSliderValueChanged(float value)
	{
		_generator.MapWidth = Mathf.RoundToInt(value);
		_mapWidthLabel.Text = _generator.MapWidth.ToString();
        InvokeParametersChangedEvent();
}

	private void OnOffsetXSliderValueChanged(float value)
	{
		Vector2 offset = _generator.Offset;
		offset.X = Mathf.RoundToInt(value);
		_generator.Offset = offset;
		_offsetXLabel.Text = offset.X.ToString();
        InvokeParametersChangedEvent();
}

	private void OnOffsetYSliderValueChanged(float value)
	{
		Vector2 offset = _generator.Offset;
		offset.Y = Mathf.RoundToInt(value);
		_generator.Offset = offset;
		_offsetYLabel.Text = offset.Y.ToString();
        InvokeParametersChangedEvent();
}

	private void OnScaleSliderValueChanged(float value)
	{
		_generator.Scale = value;
		_scaleLabel.Text = value.ToString();
        InvokeParametersChangedEvent();
}

	private void OnPersistanceSliderValueChanged(float value)
	{
		_generator.Persistance = value;
		_persistenceLabel.Text = value.ToString();
        InvokeParametersChangedEvent();
}

	private void OnLacunaritySliderValueChanged(float value)
	{
		_generator.Lacunarity = value;
		_lacunarityLabel.Text = value.ToString();
        InvokeParametersChangedEvent();
}

	private void OnOctavesSliderValueChanged(float value)
	{
		_generator.Octaves = Mathf.RoundToInt(value);
		_octavesLabel.Text = _generator.Octaves.ToString();
        InvokeParametersChangedEvent();
}

	private void OnSeedSValueChanged(float value)
	{
		_seed = Mathf.RoundToInt(value);
		_seedLabel.Text = _seed.ToString();
		_generator.Seed = _seed;
        InvokeParametersChangedEvent();
    }


	private void OnWarpingStrengthSValueChanged(float value)
	{
		_generator.WarpingStrength = value;
		_warpingStrengthLabel.Text = value.ToString();
		InvokeParametersChangedEvent();
	}

	private void OnWarpingSizeSValueChanged(float value)
	{
		_generator.WarpingSize = value;
		_warpingSizeLabel.Text = value.ToString();
        InvokeParametersChangedEvent();
}

	private void OnUseWarpingCheckButtonToggled(bool toggledOn)
	{
		_generator.EnableWarping = toggledOn;
        InvokeParametersChangedEvent();
}



}
