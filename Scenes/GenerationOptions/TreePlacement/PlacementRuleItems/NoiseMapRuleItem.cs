using Godot;
using TerrainGenerationApp.Extensions;
using TerrainGenerationApp.Rules.PlacementRules;

namespace TerrainGenerationApp.Scenes.GenerationOptions.TreePlacement.PlacementRuleItems;

public partial class NoiseMapRuleItem : BasePlacementRuleItem<NoiseMapRuleItem>
{
    private Label _mapHeightLabel;
    private Label _mapWidthLabel;
    private Label _offsetXLabel;
    private Label _offsetYLabel;
    private Label _scaleLabel;
    private Label _persistenceLabel;
    private Label _lacunarityLabel;
    private Label _octavesLabel;
    private Label _seedLabel;
    private Slider _mapHeightSlider;
    private Slider _mapWidthSlider;
    private Slider _offsetXSlider;
    private Slider _offsetYSlider;
    private Slider _scaleSlider;
    private Slider _lacunaritySlider;
    private Slider _persistanceSlider;
    private Slider _octavesSlider;
    private Slider _seedSlider;
    private Label _noiseThresholdLabel;
    private Slider _noiseThresholdSlider;
    private TextureRect _noiseTextureRect;

    private readonly Generators.PerlinNoise _generator = new();
    private Image _noiseImage;
    private ImageTexture _noiseTexture;
    private float _noiseThreshold = 0.5f;
    private bool _sizeChanged = true;

    public override void _Ready()
    {
        base._Ready();
        _mapHeightLabel = GetNode<Label>("%MapHeightLabel");
        _mapWidthLabel = GetNode<Label>("%MapWidthLabel");
        _offsetXLabel = GetNode<Label>("%OffsetXLabel");
        _offsetYLabel = GetNode<Label>("%OffsetYLabel");
        _scaleLabel = GetNode<Label>("%ScaleLabel");
        _persistenceLabel = GetNode<Label>("%PersistanceLabel");
        _lacunarityLabel = GetNode<Label>("%LacunarityLabel");
        _octavesLabel = GetNode<Label>("%OctavesLabel");
        _seedLabel = GetNode<Label>("%SeedLabel");
        _mapHeightSlider = GetNode<Slider>("%MapHeightSlider");
        _mapWidthSlider = GetNode<Slider>("%MapWidthSlider");
        _offsetXSlider = GetNode<Slider>("%OffsetXSlider");
        _offsetYSlider = GetNode<Slider>("%OffsetYSlider");
        _scaleSlider = GetNode<Slider>("%ScaleSlider");
        _persistanceSlider = GetNode<Slider>("%PersistanceSlider");
        _lacunaritySlider = GetNode<Slider>("%LacunaritySlider");
        _octavesSlider = GetNode<Slider>("%OctavesSlider");
        _seedSlider = GetNode<Slider>("%SeedSlider");
        _noiseThresholdLabel = GetNode<Label>("%NoiseThresholdLabel");
        _noiseThresholdSlider = GetNode<Slider>("%NoiseThresholdSlider");
        _noiseTextureRect = GetNode<TextureRect>("%NoiseTextureRect");
        _mapHeightSlider.ValueChanged += OnMapHeightSliderValueChanged;
        _mapWidthSlider.ValueChanged += OnMapWidthSliderValueChanged;
        _offsetXSlider.ValueChanged += OnOffsetXSliderValueChanged;
        _offsetYSlider.ValueChanged += OnOffsetYSliderValueChanged;
        _scaleSlider.ValueChanged += OnScaleSliderValueChanged;
        _persistanceSlider.ValueChanged += OnPersistanceSliderValueChanged;
        _lacunaritySlider.ValueChanged += OnLacunaritySliderValueChanged;
        _octavesSlider.ValueChanged += OnOctavesSliderValueChanged;
        _seedSlider.ValueChanged += OnSeedSValueChanged;
        _noiseThresholdSlider.ValueChanged += OnNoiseThresholdSliderValueChanged;

        _noiseImage = Image.CreateEmpty(1, 1, false, Image.Format.Rgb8);
        _noiseTexture = ImageTexture.CreateFromImage(_noiseImage);
        _noiseTextureRect.Texture = _noiseTexture;
        RedrawMap();
    }

    public override IPlacementRule GetPlacementRule()
    {
        var map = _generator.GenerateMap();
        var rule = new NoiseMapRule(map, _noiseThreshold);
        return rule;
    }

    private void RedrawMap()
    {
        if (_sizeChanged)
        {
            _noiseImage.Resize(_generator.MapWidth, _generator.MapHeight);
        }

        var map = _generator.GenerateMap();

        for (int y = 0; y < map.Height(); y++)
        {
            for (int x = 0; x < map.Width(); x++)
            {
                var height = map[y, x];
                _noiseImage.SetPixel(x, y, new Color(height, height, height));
            }
        }

        if (_sizeChanged)
        {
            _noiseTexture.SetImage(_noiseImage);
        }
        else
        {
            _noiseTexture.Update(_noiseImage);
        }

        _sizeChanged = false;
    }


    private void InvokeParametersChangedEvent()
    {
        RedrawMap();
        InvokeRuleParametersChangedEvent();
    }

    private void OnNoiseThresholdSliderValueChanged(double value)
    {
        _noiseThreshold = Mathf.Clamp((float)value, 0f, 1.0f);
        _noiseThresholdLabel.Text = _noiseThreshold.ToString();
        InvokeParametersChangedEvent();
    }

    private void OnMapHeightSliderValueChanged(double value)
    {
        _generator.MapHeight = Mathf.RoundToInt(value);
        _mapHeightLabel.Text = _generator.MapHeight.ToString();
        _sizeChanged = true;
        InvokeParametersChangedEvent();
    }

    private void OnMapWidthSliderValueChanged(double value)
    {
        _generator.MapWidth = Mathf.RoundToInt(value);
        _mapWidthLabel.Text = _generator.MapWidth.ToString();
        _sizeChanged = true;
        InvokeParametersChangedEvent();
    }

    private void OnOffsetXSliderValueChanged(double value)
    {
        Vector2 offset = _generator.Offset;
        offset.X = Mathf.RoundToInt(value);
        _generator.Offset = offset;
        _offsetXLabel.Text = offset.X.ToString();
        InvokeParametersChangedEvent();
    }

    private void OnOffsetYSliderValueChanged(double value)
    {
        Vector2 offset = _generator.Offset;
        offset.Y = Mathf.RoundToInt(value);
        _generator.Offset = offset;
        _offsetYLabel.Text = offset.Y.ToString();
        InvokeParametersChangedEvent();
    }

    private void OnScaleSliderValueChanged(double value)
    {
        _generator.Scale = (float)value;
        _scaleLabel.Text = value.ToString();
        InvokeParametersChangedEvent();
    }

    private void OnPersistanceSliderValueChanged(double value)
    {
        _generator.Persistance = (float)value;
        _persistenceLabel.Text = value.ToString();
        InvokeParametersChangedEvent();
    }

    private void OnLacunaritySliderValueChanged(double value)
    {
        _generator.Lacunarity = (float)value;
        _lacunarityLabel.Text = value.ToString();
        InvokeParametersChangedEvent();
    }

    private void OnOctavesSliderValueChanged(double value)
    {
        _generator.Octaves = Mathf.RoundToInt(value);
        _octavesLabel.Text = _generator.Octaves.ToString();
        InvokeParametersChangedEvent();
    }

    private void OnSeedSValueChanged(double value)
    {
        var _seed = Mathf.RoundToInt(value);
        _seedLabel.Text = _seed.ToString();
        _generator.Seed = _seed;
        InvokeParametersChangedEvent();
    }
}