using System;
using Godot;
using System.Collections.Generic;
using TerrainGenerationApp.Generators;
using TerrainGenerationApp.Scenes.GeneratorOptions.Scripts;
using TerrainGenerationApp.Utilities;
using BaseGeneratorOptions = TerrainGenerationApp.Scenes.GeneratorOptions.Scripts.BaseGeneratorOptions;
using DomainWarpingOptions = TerrainGenerationApp.Scenes.GeneratorOptions.Scripts.DomainWarpingOptions;
using WaterErosionOptions = TerrainGenerationApp.Scenes.GeneratorOptions.Scripts.WaterErosionOptions;

namespace TerrainGenerationApp.Scenes;

public partial class Game : Node3D
{
	private const int MaxSmoothCycles = 10;
	private const float MaxWaterLevel = 1.0f;
	private const float MinWaterLevel = 0.0f;
	private const float MaxSlopeThreshold = 1.0f;
	private const float MinSlopeThreshold = 0.0f;

    public enum DisplayFormats { Grey, Colors, Gradient }


    // ENABLES:
    // enable_insta_changes_apply
    // enable_domain_warping
    // enable_slopes_display
    // enable_water_display

    // CURRENT:
    // _curGenerator         (FOR cur_generators_count = SINGLE)
    // cur_generators        (FOR cur_generators_count = MANY)
    // cur_generators_count  Single/Many
    // cur_display_format    Grey/Color/Gradient/Normals
    // cur_display_map       [,]
    // cur_slopes_map        [,]
    // cur_smooth_cycles     [0; 10]
    // cur_water_level       [0.0; 1.0]
    // cur_slope_threshold   [0.0; 1.0]
    // cur_map_rise_lower_value [0.01; 0.3]

    private float[,] _curDisplayMap = new float[1, 1];
    private float[,] _curSlopesMap = new float[1, 1];
    private float _curChangeMapHeightLevel = 0.01f;
    private float _curSlopeThreshold = 0.2f;
    private float _curWaterLevel = 0.3f;
    private float _curNoiseInfluence = 1.0f;
    private int _curSmoothCycles = 0;

    private bool _enableIslands = false;
    private bool _enableDomainWarping = false;
    private bool _isRedrawOnParametersChanged = false;
    private DisplayFormats _curDisplayFormat = DisplayFormats.Grey;


    private Dictionary<int, BaseGeneratorOptions> _generators = new();
    private BaseGeneratorOptions _curGenerator;
    private Image _texture;
    private ImageTexture _mapTexture;
    private Gradient _terrainGradient;
    private Gradient _waterGradient;

    private DomainWarpingApplier _domainWarpingApplier;
    private WaterErosionApplier _waterErosionApplier;
    private IslandApplier _islandApplier;




	// NODES REFERENCED WITH "%" IN SCENE
	//========================================================================
	// Map and map info
	private Label _cellInfoLabel;
	private TextureRect _mapTextureRect;

	// Select and set generator options
	private OptionButton _generatorDropdownMenu;
	private BaseGeneratorOptions _diamondSquareOptions;
	private BaseGeneratorOptions _worleyOptions;
	private BaseGeneratorOptions _perlinOptions;

	// Display format (Grey, colors, gradient)
	private CheckBox _displayGrey;
	private CheckBox _displayColors;
	private CheckBox _displayGradient;

	// Manipulate map
	private Label _waterLevelLabel;
	private Label _smoothCyclesLabel;
	private Label _mapChangeValueLabel;
	private Label _slopeThresholdLabel;
    private Label _noiseInfluenceLabel;

	// Other
	private CanvasLayer _generationLayer;
	private CanvasLayer _3dLayer;
	private MeshInstance3D _waterMesh;
	private MeshInstance3D _mapMesh;
	private IslandOptions _islandOptions;
	private DomainWarpingOptions _domainWarpingOptions;
	private WaterErosionOptions _waterErosionOptions;


	// PROPERTIES
	//========================================================================
	public DisplayFormats CurDisplayFormat
	{
		get => _curDisplayFormat;
		set
		{
			if (_curDisplayFormat != value)
			{
				_curDisplayFormat = value;
				if (value == DisplayFormats.Colors)
				{
					_terrainGradient.InterpolationMode = Gradient.InterpolationModeEnum.Constant;
					_waterGradient.InterpolationMode = Gradient.InterpolationModeEnum.Constant;
				}
				else if (value == DisplayFormats.Gradient)
				{
					_terrainGradient.InterpolationMode = Gradient.InterpolationModeEnum.Linear;
					_waterGradient.InterpolationMode = Gradient.InterpolationModeEnum.Linear;
				}
				if (IsNodeReady())
				{
					_redraw2dMap();
				}
			}
		}
	}

	public float CurWaterLevel
	{
		get => _curWaterLevel;
		set
		{
			value = Mathf.Clamp(value, MinWaterLevel, MaxWaterLevel);
			if (!Mathf.IsEqualApprox(value, _curWaterLevel))
			{
				_curWaterLevel = value;
				if (IsNodeReady())
				{
					_redraw2dMap();
				}
			}
		}
	}

    public float CurNoiseInfluence
    {
        get => _curNoiseInfluence;
        set
        {
            if (!Mathf.IsEqualApprox(value, _curNoiseInfluence))
            {
				//GD.Print("INFLUENCE CHANGED");
                _curNoiseInfluence = value;
                if (IsNodeReady())
                {
                    _generateMap();
                }
            }
        }
    }

public float CurSlopeThreshold
	{
		get => _curSlopeThreshold;
		set
		{
			value = Mathf.Clamp(value, MinSlopeThreshold, MaxSlopeThreshold);
			if (!Mathf.IsEqualApprox(value, _curSlopeThreshold))
			{
				_curSlopeThreshold = value;
				if (IsNodeReady())
				{
					_redraw2dMap();
				}
			}
		}
	}

	public int CurSmoothCycles
	{
		get => _curSmoothCycles;
		set
		{
			value = Mathf.Clamp(value, 0, MaxSmoothCycles);
			if (value != _curSmoothCycles)
			{
				_curSmoothCycles = value;
				if (IsNodeReady())
				{
					_generateMap();
				}
			}
		}
	}

	public bool IsRedrawOnParametersChanged
	{
		get => _isRedrawOnParametersChanged;
		set
		{
			if (value != _isRedrawOnParametersChanged)
			{
				_isRedrawOnParametersChanged = value;
				if (value && IsNodeReady())
				{
					_redraw2dMap();
				}
			}
		}
	}

	public bool EnableDomainWarping
	{
		get => _enableDomainWarping;
		set
		{
			if (value != _enableDomainWarping)
			{
				_enableDomainWarping = value;
				if (IsNodeReady())
				{
					_generateMap();
				}
			}
		}
	}

    public bool EnableIslands
    {
        get => _enableIslands;
        set
        {
            if (value != _enableIslands)
            {
                _enableIslands = value;
                if (IsNodeReady())
                {
                    _generateMap();
                }
            }
        }
    }


	public override void _Ready()
	{
		// Map and map info
		_mapTextureRect = GetNode<TextureRect>("%MapTextureRect");
		_cellInfoLabel = GetNode<Label>("%CellInfoLabel");

		// Select and set generator options
		_generatorDropdownMenu = GetNode<OptionButton>("%GeneratorDropdownMenu");
		_diamondSquareOptions = GetNode<BaseGeneratorOptions>("%DiamondSquareOptions");
		_perlinOptions = GetNode<BaseGeneratorOptions>("%PerlinOptions");
		_worleyOptions = GetNode<BaseGeneratorOptions>("%WorleyOptions");

		// Display format (Grey, colors, gradient)
		_displayGrey = GetNode<CheckBox>("%DisplayGrey");
		_displayColors = GetNode<CheckBox>("%DisplayColors");
		_displayGradient = GetNode<CheckBox>("%DisplayGradient");

		// Manipulate map
		_waterLevelLabel = GetNode<Label>("%WaterLevelLabel");
		_smoothCyclesLabel = GetNode<Label>("%SmoothCyclesLabel");
		_mapChangeValueLabel = GetNode<Label>("%MapChangeValueL");
		_slopeThresholdLabel = GetNode<Label>("%SlopeThresholdL");
        _noiseInfluenceLabel = GetNode<Label>("%NoiseInfluenceLabel");

        // Other
        _waterMesh = GetNode<MeshInstance3D>("%WaterMesh");
		_generationLayer = GetNode<CanvasLayer>("%Generation_Layer");
		_3dLayer = GetNode<CanvasLayer>("%3D_Layer");
		_domainWarpingOptions = GetNode<DomainWarpingOptions>("%DomainWarpingOptions");
		_waterErosionOptions = GetNode<WaterErosionOptions>("%WaterErosionOptions");
        _islandOptions = GetNode<GeneratorOptions.Scripts.IslandOptions>("%IslandOptions");

		// GENERATE GRADIENTS FOR 2D MAPS
		_terrainGradient = new Gradient();
		_waterGradient = new Gradient();
		_terrainGradient.RemovePoint(1);
		_waterGradient.RemovePoint(1);
		foreach (var heightColor in ColorPallets.DefaultTerrainColors)
		{
			var gradOffset = heightColor.Key;
			_terrainGradient.AddPoint(gradOffset, heightColor.Value);
		}
		foreach (var heightColor in ColorPallets.DefaultWaterColors)
		{
			var gradOffset = heightColor.Key;
			_waterGradient.AddPoint(gradOffset, heightColor.Value);
		}

		// INIT 2D MAP TEXTURE
		_texture = Image.CreateEmpty(1, 1, false, Image.Format.Rgb8);
		_mapTexture = ImageTexture.CreateFromImage(_texture);
		_mapTextureRect.Texture = _mapTexture;
		CurDisplayFormat = DisplayFormats.Colors;


		// INIT COMBOBOX AND DICTIONARY
		_generators = new Dictionary<int, BaseGeneratorOptions>
		{
			{ 1, _diamondSquareOptions },
			{ 2, _perlinOptions },
			{ 3, _worleyOptions }
		};
		_hideAllOptions();
		_connectParametersChanged();

		var firstSelectedIndex = 0;
		_generatorDropdownMenu.Clear();
		_generatorDropdownMenu.AddItem("Diamond square", 1);
		_generatorDropdownMenu.AddItem("Perlin noise", 2);
		_generatorDropdownMenu.AddItem("Worley noise", 3);
		_generatorDropdownMenu.Selected = firstSelectedIndex;
		var firstItemId = _generatorDropdownMenu.GetItemId(firstSelectedIndex);
		var firstItemOptions = _generators[firstItemId];
		firstItemOptions.Visible = true;
		_curGenerator = firstItemOptions;

		// DISPLAY FORMAT CONNECT 
		_displayGrey.Toggled += (buttonPressed) => CurDisplayFormat = DisplayFormats.Grey;
		_displayColors.Toggled += (buttonPressed) => CurDisplayFormat = DisplayFormats.Colors;
		_displayGradient.Toggled += (buttonPressed) => CurDisplayFormat = DisplayFormats.Gradient;
		IsRedrawOnParametersChanged = true;

		// DOMAIN WARPING AND WATER EROSION CONNECT
		_domainWarpingApplier = _domainWarpingOptions.DomainWarpingApplier;
		_domainWarpingOptions.ParametersChanged += _parameters_changed;
		_waterErosionApplier = _waterErosionOptions.WaterErosionApplier;
		_waterErosionApplier.IterationPassed += OnWaterErosionIterationPassed;
		_islandApplier = _islandOptions.IslandApplier;
        _islandOptions.ParametersChanged += _parameters_changed;
    }


	private void OnWaterErosionIterationPassed(float[,] map)
	{
		CallDeferred(nameof(_redraw2dMap));
	}



	private Color get_terrain_color(float c, float slope)
	{
		if (CurDisplayFormat == DisplayFormats.Grey)
		{
			return new Color(c, c, c, 1.0f);
		}
		if (c <= CurWaterLevel)
		{
			return _waterGradient.Sample(CurWaterLevel - c);
		}
		return _terrainGradient.Sample(c - CurWaterLevel);
	}





	private void _connectParametersChanged()
	{
		foreach (int key in _generators.Keys)
		{
			var options = _generators[key];
			options.ParametersChanged += _parameters_changed;
		}
	}


	private void _parameters_changed()
	{
		_generateMap();
	}

	private void _generateMap()
	{
        if (_curGenerator == null)
        {
            GD.PushError("GENERATOR IS NOT SELECTED");
            return;
        }

        var map = _curGenerator.GenerateMap();

        MapHelpers.MultiplyHeight(map, CurNoiseInfluence);

        for (int i = 0; i < CurSmoothCycles; i++)
        {
            map = MapHelpers.SmoothMap(map);
        }

        if (EnableDomainWarping)
        {
            map = _domainWarpingApplier.ApplyWarping(map);
        }

        if (EnableIslands)
        {
            map = _islandApplier.ApplyIslands(map);
        }

        _curDisplayMap = map;
        _curSlopesMap = MapHelpers.GetSlopes(map);
        _resize2dMapTexture();
        _redraw2dMap();
    }




	private void _resize2dMapTexture()
	{
		if (!IsNodeReady() || _curDisplayMap == null)
		{
			return;
		}

		var h = _curDisplayMap.GetLength(0);
		var w = _curDisplayMap.GetLength(1);
		_texture.Resize(w, h);
		_mapTexture.SetImage(_texture);
	}

	private void _onGenerateButtonPressed()
	{
		_generateMap();
	}




	private void _onGeneratorDropdownMenuItemSelected(int index)
	{
		_hideAllOptions();

		int id = _generatorDropdownMenu.GetItemId(index);
		var cur = _generators[id];

		if (cur != null)
		{
			cur.Visible = true;
		}
		_curGenerator = cur;
	}


	private void _redraw2dMap()
	{
		var map = _curDisplayMap;
		var h = map.GetLength(0);
		var w = map.GetLength(1);

		if (CurDisplayFormat == DisplayFormats.Grey)
		{
			for (int y = 0; y < h; y++)
			{
				for (int x = 0; x < w; x++)
				{
					var v = map[y, x];
					_texture.SetPixel(x, y, new Color(v, v, v, 1.0f));
				}
			}
		}
		else
		{
			for (int y = 0; y < h; y++)
			{
				for (int x = 0; x < w; x++)
				{
					// Display water
					if (map[y, x] < CurWaterLevel)
					{
						_texture.SetPixel(x, y, _waterGradient.Sample(CurWaterLevel - map[y, x]));
					}
					//// Display slopes (steep surface in different color)
					//else if (_curSlopesMap[y, x] >= CurSlopeThreshold)
					//{
					//	_texture.SetPixel(x, y, _slopeColor);
					//}
					//// Terrain color based on height
					else
                    {
                        var baseColor = _terrainGradient.Sample(map[y, x] - CurWaterLevel);
                        var slopeBaseColor =
                            GetSlopeColor(baseColor, _curSlopesMap[y, x], map[y, x], CurSlopeThreshold);

                        _texture.SetPixel(x, y, slopeBaseColor);
					}
				}
			}
		}
		_mapTexture.Update(_texture);
	}



    private Color GetSlopeColor(Color baseColor, float slope, float elevation, float slopeThreshold)
    {
        // Нормалізація значення ухилу (щоб вписатись у межі 0-1)
        float slopeFactor = MathF.Min(slope * slopeThreshold, 1.0f);

        // Затемнення залежно від крутизни
        float r = baseColor.R * (1.0f - slopeFactor);
        float g = baseColor.G * (1.0f - slopeFactor);
        float b = baseColor.B * (1.0f - slopeFactor);

        // Корекція яскравості (на великих висотах трохи висвітлюємо)
        float brightnessFactor = 1.0f + (elevation * 0.2f);
        r = Math.Clamp(r * brightnessFactor, 0, 1.0f);
        g = Math.Clamp(g * brightnessFactor, 0, 1.0f);
        b = Math.Clamp(b * brightnessFactor, 0, 1.0f);

        return new Color(r, g, b);
    }




    private void _hideAllOptions()
	{
		_diamondSquareOptions.Visible = false;
		_worleyOptions.Visible = false;
		_perlinOptions.Visible = false;
	}


	private void _showAllOptions()
	{
		_diamondSquareOptions.Visible = true;
		_worleyOptions.Visible = true;
		_perlinOptions.Visible = true;
	}








	private void _show_options(int index)
	{
		var itemId = _generatorDropdownMenu.GetItemId(index);
		var itemOptions = _generators[index];
		if (itemOptions != null)
		{
			itemOptions.Visible = true;
		}
	}

	private void _on_show_in_3d_button_pressed()
	{
		//_generationLayer.Hide();
		//_3dLayer.Show();
		//_generate_mesh();
	}

	private void _on_back_button_pressed()
	{
		_generationLayer.Show();
		_3dLayer.Hide();
	}






    private void _on_water_erosion_check_box_toggled(bool toggledOn)
	{
		GetNode<Control>("%WaterErosionOptions").Visible = toggledOn;
	}


	private GodotThread _waterErosionThread;

	private void _on_erosion_simulation_button_pressed()
	{
		if (_waterErosionThread != null)
		{
			if (_waterErosionThread.IsAlive())
			{
				return;
			}

			_waterErosionThread.Free();
		}

		_waterErosionOptions.MakeAllOptionsUnavailable();
		_waterErosionThread = new GodotThread();
		_waterErosionThread.Start(Callable.From(begin_erosion_simulation));
	}


	private void begin_erosion_simulation()
	{
		_waterErosionApplier.BeginApplyingErosion(_curDisplayMap, CurWaterLevel);

		CallDeferred(nameof(MMM));
	}


	private void MMM()
	{
		_waterErosionOptions.MakeAllOptionsAvailable();
	}
















	private void _on_map_change_value_s_value_changed(float value)
	{
		_curChangeMapHeightLevel = value;
		_mapChangeValueLabel.Text = value.ToString();
	}

	private void _on_lower_map_button_pressed()
	{
		MapHelpers.AddHeight(_curDisplayMap, -_curChangeMapHeightLevel);
		_redraw2dMap();
	}

	private void _on_raise_map_button_pressed()
	{
		MapHelpers.AddHeight(_curDisplayMap, _curChangeMapHeightLevel);
		_redraw2dMap();
	}


	private void _on_map_texture_rect_gui_input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion)
		{
			Vector2 localPosition = _mapTextureRect.GetLocalMousePosition();
			int h = _curDisplayMap.GetLength(0);
			int w = _curDisplayMap.GetLength(1);

			// Convert coordinates to map grid coordinates
			int cellX = (int)(localPosition.X / _mapTextureRect.Size.X * w);
			int cellY = (int)(localPosition.Y / _mapTextureRect.Size.Y * h);

			// Check boundaries
			if (cellX >= 0 && cellX < w && cellY >= 0 && cellY < h)
			{
				float height = _curDisplayMap[cellY, cellX];  // Get height from the heat map
				float slope = _curSlopesMap[cellY, cellX];
				_cellInfoLabel.Text = string.Format("Cell: [ {0} ; {1} ] <---> Value: {2} <---> Slope: {3}", cellX, cellY, height, slope);
			}
		}
	}

	private void _on_map_texture_rect_mouse_exited()
	{
		_cellInfoLabel.Text = "";
	}




    private void _on_water_level_slider_value_changed(float value)
    {
        CurWaterLevel = value;
        _waterLevelLabel.Text = value.ToString();
    }

    private void _on_smooth_cycles_slider_value_changed(float value)
    {
        CurSmoothCycles = Mathf.RoundToInt(value);
        _smoothCyclesLabel.Text = CurSmoothCycles.ToString();
    }

    private void _on_noise_influence_slider_value_changed(float value)
    {
        CurNoiseInfluence = value;
        _noiseInfluenceLabel.Text = CurNoiseInfluence.ToString();
    }

    private void _on_slope_threshold_slider_value_changed(float value)
	{
		CurSlopeThreshold = value;
		_slopeThresholdLabel.Text = CurSlopeThreshold.ToString();
	}

    private void _on_domain_warping_check_box_toggled(bool toggledOn)
    {
        _domainWarpingOptions.Visible = toggledOn;
        EnableDomainWarping = toggledOn;
    }
    private void _on_island_options_check_box_toggled(bool toggledOn)
    {
        _islandOptions.Visible = toggledOn;
        EnableIslands = toggledOn;
    }
}
