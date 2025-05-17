using Godot;
using System;
using System.Collections.Generic;
using TerrainGenerationApp.Domain.Extensions;
using TerrainGenerationApp.Domain.Utils;
using TerrainGenerationApp.Scenes.BuildingBlocks.Containers;
using TerrainGenerationApp.Scenes.BuildingBlocks.InputLine;
using TerrainGenerationApp.Scenes.GeneratorOptions.PerlinNoise;

namespace TerrainGenerationApp.Scenes.FeatureOptions.Moisture;

public partial class MoistureOptions : VBoxContainer, IOptionsToggleable, ILastUsedConfigProvider
{
	protected readonly Logger<MoistureOptions> Logger = new();
	private OptionsContainer _optionsContainer;
	private PerlinOptions _perlinOptions;
	private TextureRect _noiseTextureRect;

	private Image _noiseImage;
	private ImageTexture _noiseTexture;
	private bool _sizeChanged = true;
	private float[,] _moistureMap = new float[1, 1];

	public event Action ParametersChanged;

    protected bool IsLoading { get; set; }

	protected OptionsContainer OptionsContainer
	{
		get
		{
			_optionsContainer ??= GetNode<OptionsContainer>("%OptionsContainer");
			return _optionsContainer;
		}
	}

	protected PerlinOptions PerlineOptions
	{
		get
		{
			_perlinOptions ??= GetNode<PerlinOptions>("%PerlinOptions");
			return _perlinOptions;
		}
	}

	public float[,] MoistureMap => _moistureMap;

	public override void _Ready()
	{
		base._Ready();
		InputLineManager.CreateInputLinesForObject(this, OptionsContainer);
		_noiseTextureRect = GetNode<TextureRect>("%NoiseTextureRect");
		_perlinOptions = GetNode<PerlinOptions>("%PerlinOptions");
		_perlinOptions.ParametersChanged += PerlinOptionsOnParametersChanged;

		_noiseImage = Image.CreateEmpty(1, 1, false, Image.Format.Rgb8);
		_noiseTexture = ImageTexture.CreateFromImage(_noiseImage);
		_noiseTextureRect.Texture = _noiseTexture;
		RedrawMap();
	}

	public void EnableOptions()
	{
		OptionsContainer.EnableOptions();
		PerlineOptions.EnableOptions();
	}

	public void DisableOptions()
	{
		OptionsContainer.DisableOptions();
		PerlineOptions.DisableOptions();
	}

	private void PerlinOptionsOnParametersChanged()
	{
		_moistureMap = PerlineOptions.GenerateMap();
		InvokeParametersChangedEvent();
	}

	private void RedrawMap()
	{
		var curImageSize = _noiseImage.GetSize();

		if (curImageSize.X != _perlinOptions.MapWidth || curImageSize.Y != _perlinOptions.MapHeight)
		{
			_sizeChanged = true;
			_noiseImage.Resize(_perlinOptions.MapWidth, _perlinOptions.MapHeight);
		}

		var map = _moistureMap;

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
        ParametersChanged?.Invoke();

    }

	public Dictionary<string, object> GetLastUsedConfig()
	{
		var dict = OptionsContainer.GetLastUsedConfig();
		dict["Noise"] = PerlineOptions.GetLastUsedConfig();
		return dict;
	}

	public void UpdateCurrentConfigAsLastUsed()
	{
		OptionsContainer.UpdateCurrentConfigAsLastUsed();
		PerlineOptions.UpdateCurrentConfigAsLastUsed();
	}

	public void LoadConfigFrom(Dictionary<string, object> config)
	{
		try
		{
			IsLoading = true;
			OptionsContainer.LoadConfigFrom(config);
			if (config.GetValueOrDefault("Noise") is Dictionary<string, object> noiseConfig)
			{
				PerlineOptions.LoadConfigFrom(noiseConfig);
				RedrawMap();
			}
		}
		catch (Exception e)
		{
			Logger.LogError(e.Message);
			Logger.LogError(e.StackTrace);

		}
		finally
		{
			IsLoading = false;
		}
	}
}
