using Godot;
using System.Collections.Generic;
using TerrainGenerationApp.Domain.Extensions;
using TerrainGenerationApp.Domain.Rules.PlacementRules;
using TerrainGenerationApp.Scenes.BuildingBlocks.Attributes;
using TerrainGenerationApp.Scenes.BuildingBlocks.InputLine;
using TerrainGenerationApp.Scenes.GeneratorOptions.PerlinNoise;

namespace TerrainGenerationApp.Scenes.FeatureOptions.TreePlacement.PlacementRuleItems;

public partial class NoiseMapRuleItem : BasePlacementRuleItem<NoiseMapRuleItem>
{
    private PerlinOptions _perlinOptions;
    private TextureRect _noiseTextureRect;

    private Image _noiseImage;
    private ImageTexture _noiseTexture;
    private float _noiseThreshold = 0.5f;
    private bool _sizeChanged = true;
    
    [InputLine(Description = "Noise threshold:")]
    [InputLineSlider(0.0f, 1.0f, 0.001f, format: "0.###")]
    public float NoiseThreshold
    {
        get => _noiseThreshold;
        set
        {
            _noiseThreshold = value;
            Logger.Log($"Lower bound changed to: {_noiseThreshold}");
            InvokeRuleParametersChangedEvent();
        }
    }

    public override void _Ready()
    {
        base._Ready();
        InputLineManager.CreateInputLinesForObject(this, OptionsContainer);
        _perlinOptions = GetNode<PerlinOptions>("%PerlinOptions");
        _noiseTextureRect = GetNode<TextureRect>("%NoiseTextureRect");
        _perlinOptions.ParametersChanged += PerlinOptionsOnParametersChanged;

        _noiseImage = Image.CreateEmpty(1, 1, false, Image.Format.Rgb8);
        _noiseTexture = ImageTexture.CreateFromImage(_noiseImage);
        _noiseTextureRect.Texture = _noiseTexture;
        RedrawMap();
    }

    public override void EnableOptions()
    {
        base.EnableOptions();
        _perlinOptions.EnableOptions();
    }

    public override void DisableOptions()
    {
        base.DisableOptions();
        _perlinOptions.DisableOptions();
    }

    private void PerlinOptionsOnParametersChanged()
    {
        InvokeParametersChangedEvent();
    }

    public override IPlacementRule GetPlacementRule()
    {
        var map = _perlinOptions.GenerateMap();
        var rule = new NoiseMapRule(map, _noiseThreshold);
        return rule;
    }

    private void RedrawMap()
    {
        var curImageSize = _noiseImage.GetSize();

        if (curImageSize.X != _perlinOptions.MapWidth || curImageSize.Y != _perlinOptions.MapHeight)
        {
            _sizeChanged = true;
            _noiseImage.Resize(_perlinOptions.MapWidth, _perlinOptions.MapHeight);
        }

        var map = _perlinOptions.GenerateMap();

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

    public override Dictionary<string, object> GetLastUsedConfig()
    {
        var dict = base.GetLastUsedConfig();
        dict["Noise"] = _perlinOptions.GetLastUsedConfig();
        return dict;
    }

    public override void UpdateCurrentConfigAsLastUsed()
    {
        base.UpdateCurrentConfigAsLastUsed();
        _perlinOptions.UpdateCurrentConfigAsLastUsed();
    }

    protected override void LoadConfigInternal(Dictionary<string, object> config)
    {
        base.LoadConfigInternal(config);

        if (config.GetValueOrDefault("Noise") is Dictionary<string, object> noiseConfig)
        {
            _perlinOptions.LoadConfigFrom(noiseConfig);
            RedrawMap();
        }
    }
}