using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using TerrainGenerationApp.Generators;
using TerrainGenerationApp.Scenes.GenerationOptions;
using TerrainGenerationApp.Scenes.GenerationOptions.DomainWarpingOptions;
using TerrainGenerationApp.Scenes.GenerationOptions.IslandOptions;
using TerrainGenerationApp.Scenes.GenerationOptions.TreePlacementOptions;
using TerrainGenerationApp.Scenes.GeneratorOptions.Scripts;
using TerrainGenerationApp.Utilities;

namespace TerrainGenerationApp.Scenes.GameComponents.GenerationMenu;

public partial class MapGenerationMenu : Control
{
	private const int MaxSmoothCycles = 10;

	private float _curChangeMapHeightLevel = 0.01f;
	private float _curNoiseInfluence = 1.0f;
	private int _curSmoothCycles = 0;

	private bool _regenerateOnParametersChanged = false;
	private bool _enableDomainWarping = false;
	private bool _enableIslands = false;
	private bool _enableTrees = false;

	private Dictionary<int, BaseGeneratorOptions> _generators = new();
	private BaseGeneratorOptions _curGenerator;
	private DomainWarpingApplier _domainWarpingApplier;
	private WaterErosionApplier _waterErosionApplier;
	private IslandApplier _islandApplier;
	private GodotThread _waterErosionThread;
	private WorldData _worldData;

	public event Action<MapGenerationResult> OnMapGenerated;


	// NODES REFERENCED WITH "%" IN SCENE
	//========================================================================
	// Select and set generator options
	private OptionButton _generatorDropdownMenu;
	private BaseGeneratorOptions _diamondSquareOptions;
	private BaseGeneratorOptions _worleyOptions;
	private BaseGeneratorOptions _perlinOptions;

	// Map manipulation
	private Label _smoothCyclesLabel;
	private Label _mapChangeValueLabel;
	private Label _noiseInfluenceLabel;
	private Slider _smoothCyclesSlider;
	private Slider _noiseInfluenceSlider;
	private Slider _mapChangeValueSlider;

	// Features
    private CheckBox _autoRegenerateCheckBox;
    private CheckBox _domainWarpingCheckBox;
    private CheckBox _waterErosionCheckbox;
    private CheckBox _islandsOptionsCheckbox;
	private IslandOptions _islandOptions;
	private DomainWarpingOptions _domainWarpingOptions;
	private WaterErosionOptions _waterErosionOptions;
	private TreePlacementOptions _treePlacementOptions;

	public MapDisplayOptions.MapDisplayOptions MapDisplayOptions { get; set; }

	public float CurWaterLevel => MapDisplayOptions.CurWaterLevel;

	public TreesApplier.TreeGenerationResult TreesMaps { get; set; }

    public IWorldData WorldData
    {
		get => _worldData;
    }

	public float CurNoiseInfluence
	{
		get => _curNoiseInfluence;
		private set
		{
			_curNoiseInfluence = value;
			_handleParametersChanged();
		}
	}

	public int CurSmoothCycles
	{
		get => _curSmoothCycles;
		private set
		{
			_curSmoothCycles = Mathf.Clamp(value, 0, MaxSmoothCycles);
			_handleParametersChanged();
		}
	}

	public bool EnableDomainWarping
	{
		get => _enableDomainWarping;
		private set
		{
			_enableDomainWarping = value;
			_handleParametersChanged();
		}
	}

	public bool EnableIslands
	{
		get => _enableIslands;
		private set
		{
			_enableIslands = value;
			_handleParametersChanged();
		}
	}

	public bool EnableTrees
	{
		get => _enableTrees;
		private set
		{
            _enableTrees = value;
			_handleParametersChanged();
		}
	}

	public bool RegenerateOnParametersChanged
	{
		get => _regenerateOnParametersChanged;
		set
		{
			_regenerateOnParametersChanged = value;
			_handleParametersChanged();
		}
	}





	public override void _Ready()
	{
		// Select and set generator options
		_generatorDropdownMenu = GetNode<OptionButton>("%GeneratorDropdownMenu");
		_diamondSquareOptions = GetNode<BaseGeneratorOptions>("%DiamondSquareOptions");
		_perlinOptions = GetNode<BaseGeneratorOptions>("%PerlinOptions");
		_worleyOptions = GetNode<BaseGeneratorOptions>("%WorleyOptions");

		// Manipulate map
		_smoothCyclesLabel = GetNode<Label>("%SmoothCyclesLabel");
		_mapChangeValueLabel = GetNode<Label>("%MapChangeValueLabel");
		_noiseInfluenceLabel = GetNode<Label>("%NoiseInfluenceLabel");
		_smoothCyclesSlider = GetNode<Slider>("%SmoothCyclesSlider");
		_noiseInfluenceSlider = GetNode<Slider>("%NoiseInfluenceSlider");
		_mapChangeValueSlider = GetNode<Slider>("%MapChangeValueSlider");
		_smoothCyclesSlider.ValueChanged += _on_smooth_cycles_slider_value_changed;
		_noiseInfluenceSlider.ValueChanged += _on_noise_influence_slider_value_changed;
		_mapChangeValueSlider.ValueChanged += _on_map_change_value_slider_value_changed;
		
        // Features
        // GET AND SUBSCRIBE CHECKBOXES
        _autoRegenerateCheckBox = GetNode<CheckBox>("%AutoRegenerateCheckBox");
        _domainWarpingCheckBox = GetNode<CheckBox>("%DomainWarpingCheckBox");
        _waterErosionCheckbox = GetNode<CheckBox>("%WaterErosionCheckBox");
        _islandsOptionsCheckbox = GetNode<CheckBox>("%IslandOptionsCheckBox");
        _treePlacementOptions = GetNode<TreePlacementOptions>("%TreePlacementOptions");
        _domainWarpingCheckBox.Toggled += _on_domain_warping_check_box_toggled;
        _waterErosionCheckbox.Toggled += _on_water_erosion_check_box_toggled;
        _islandsOptionsCheckbox.Toggled += _on_island_options_check_box_toggled;
        RegenerateOnParametersChanged = true;
        _autoRegenerateCheckBox.ButtonPressed = true;
        _autoRegenerateCheckBox.Toggled += (toggleOn) => RegenerateOnParametersChanged = toggleOn;
        // GET AND SUBSCRIBE FEATURES OPTIONS
        _domainWarpingOptions = GetNode<DomainWarpingOptions>("%DomainWarpingOptions");
		_waterErosionOptions = GetNode<WaterErosionOptions>("%WaterErosionOptions");
		_islandOptions = GetNode<IslandOptions>("%IslandOptions");
		_domainWarpingApplier = _domainWarpingOptions.DomainWarpingApplier;
		_waterErosionApplier = _waterErosionOptions.WaterErosionApplier;
		_islandApplier = _islandOptions.IslandApplier;
		_domainWarpingOptions.ParametersChanged += _handleParametersChanged;
		_waterErosionApplier.IterationPassed += OnWaterErosionIterationPassed;
		_islandOptions.ParametersChanged += _handleParametersChanged;
        _worldData = new WorldData();

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
		_generatorDropdownMenu.ItemSelected += _onGeneratorDropdownMenuItemSelected;


        _treePlacementOptions.OnTreePlacementRuleItemAdded += (sender, item) =>
        {
            GD.Print("======================================");
			GD.Print("OnTreePlacementRuleItemAdded");
            GenerateMap();
        };

        _treePlacementOptions.OnRulesChanged += (sender, args) =>
        {
            GD.Print("======================================");
            GD.Print("OnRulesChanged");
            GenerateMap();
        };


    }


	private void OnWaterErosionIterationPassed(float[,] map)
	{
		//CallDeferred(nameof(_redraw2dMap));
	}



	private void _connectParametersChanged()
	{
		foreach (int key in _generators.Keys)
		{
			var options = _generators[key];
			options.ParametersChanged += _handleParametersChanged;
		}
	}


	public Dictionary<string, Color> TreesColorsTemp { get; set; } 

	public void GenerateMap()
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

        if (EnableIslands)
        {
            map = _islandApplier.ApplyIslands(map);
        }

        if (EnableDomainWarping)
		{
			map = _domainWarpingApplier.ApplyWarping(map);
		}


		_worldData.SetTerrain(map);

		var rules = _treePlacementOptions.GetRules.ToArray();
		GD.Print($"Rules count: {rules.Length}");

        var treeMaps = TreesApplier.GenerateTreesMapsFromRules(map, _worldData, rules);

        _worldData.TreeMaps.Clear();

        foreach (var item in treeMaps.TreeMaps)
        {
			GD.Print($"Inserting: {item.Key}, Height: {item.Value.GetLength(0)}");
            _worldData.TreeMaps.Add(item.Key, item.Value);
        }

        TreesColorsTemp = _treePlacementOptions.GetTreeIdColors();

 //TreesMaps= TreesApplier.GenerateTreesMapsFromRules(CurTerrainMap, this, rules);


        //CurTreesMap = TreesApplier.GenerateTreesMap(map, 30, canPlaceFunction, minDistanceFunction);



        //TreesMaps = new MapGenerationResult(CurTerrainMap, CurSlopesMap, CurTreesMap);
        //GD.Print("MAP GENERATED");
        OnMapGenerated?.Invoke(null);
	}



	private void _hideAllOptions()
	{
		_diamondSquareOptions.Visible = false;
		_worleyOptions.Visible = false;
		_perlinOptions.Visible = false;
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
		//_waterErosionApplier.BeginApplyingErosion(_curTerrainMap, CurWaterLevel);

		//CallDeferred(nameof(MMM));
	}


	private void MMM()
	{
		_waterErosionOptions.MakeAllOptionsAvailable();
	}


	private void _handleParametersChanged()
	{
		if (RegenerateOnParametersChanged)
		{
			GenerateMap();
		}
	}


    #region EventsHandling
    private void _on_lower_map_button_pressed()
    {
        //MapHelpers.AddHeight(_curTerrainMap, -_curChangeMapHeightLevel);
        //_redraw2dMap();
    }
    private void _on_raise_map_button_pressed()
    {
        //MapHelpers.AddHeight(_curTerrainMap, _curChangeMapHeightLevel);
        //_redraw2dMap();
    }
    private void _onGeneratorDropdownMenuItemSelected(long index)
    {
        _hideAllOptions();

        int id = _generatorDropdownMenu.GetItemId((int)index);
        var cur = _generators[id];

        if (cur != null)
        {
            cur.Visible = true;
        }
        _curGenerator = cur;
    }
    private void _on_map_change_value_slider_value_changed(double value)
	{
		_curChangeMapHeightLevel = (float)value;
		_mapChangeValueLabel.Text = _curChangeMapHeightLevel.ToString();
	}
	private void _on_smooth_cycles_slider_value_changed(double value)
	{
		CurSmoothCycles = Mathf.RoundToInt(value);
		_smoothCyclesLabel.Text = CurSmoothCycles.ToString();
	}
	private void _on_noise_influence_slider_value_changed(double value)
	{
		CurNoiseInfluence = (float)value;
		_noiseInfluenceLabel.Text = CurNoiseInfluence.ToString();
	}
	private void _on_water_erosion_check_box_toggled(bool toggledOn)
	{
		_waterErosionOptions.Visible = toggledOn;
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
    #endregion
}
