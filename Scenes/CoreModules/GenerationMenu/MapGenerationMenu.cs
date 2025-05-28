using Godot;
using System;
using System.Collections.Generic;
using TerrainGenerationApp.Domain.Extensions;
using TerrainGenerationApp.Domain.Generators.DomainWarping;
using TerrainGenerationApp.Domain.Generators.Islands;
using TerrainGenerationApp.Domain.Generators.Trees;
using TerrainGenerationApp.Domain.Utils;
using TerrainGenerationApp.Domain.Utils.TerrainUtils;
using TerrainGenerationApp.Scenes.BuildingBlocks.Attributes;
using TerrainGenerationApp.Scenes.BuildingBlocks.Containers;
using TerrainGenerationApp.Scenes.BuildingBlocks.InputLine;
using TerrainGenerationApp.Scenes.FeatureOptions.DomainWarping;
using TerrainGenerationApp.Scenes.FeatureOptions.Island;
using TerrainGenerationApp.Scenes.FeatureOptions.Moisture;
using TerrainGenerationApp.Scenes.FeatureOptions.TreePlacement;
using TerrainGenerationApp.Scenes.GeneratorOptions;


namespace TerrainGenerationApp.Scenes.CoreModules.GenerationMenu;

public partial class MapGenerationMenu : Control, IOptionsToggleable, ILastUsedConfigProvider
{
    private const string SELECTED_GENERATOR_TOOLTIP = "The selected generator for base height map generation.";
    private const string MAP_INTERPOLATION_TOOLTIP = "The selected interpolation method for the generated map.";
    private const string DISCRETE_STEPS_TOOLTIP = "The number of discrete steps for the discrete interpolation method.";
    private const string HEIGHT_INFLUENCE_TOOLTIP = "The influence of the height on the generated map.";
    private const string SMOOTH_CYCLES_TOOLTIP = "The number of smoothing cycles to apply to the generated map.";
    private const string SEA_LEVEL_TOOLTIP = "The sea level for the generated map. Values below this level are considered water.";
    private const string DOMAIN_WARPING_TOOLTIP = "Enable or disable domain warping for the generated map.";
    private const string ISLANDS_TOOLTIP = "Enable or disable islands for the generated map.";
    private const string TREES_TOOLTIP = "Enable or disable trees generation for the generated map.";
    private const string MOISTURE_TOOLTIP = "Enable or disable moisture generation for the generated map.";
    private const string REDRAW_ON_PARAMETERS_CHANGED_TOOLTIP = "Instantly redraw 2D picture on parameters changed. It does not redraw tree layer!";
	private const int MaxSmoothCycles = 10;

    public enum MapInterpolationType
    {
        Linear,
        HighlightHighValues,
        HighlightLowValues,
        HighlightExtremes,
        Discrete
    }

    private BaseGeneratorOptions _diamondSquareOptions;
    private BaseGeneratorOptions _worleyOptions;
    private BaseGeneratorOptions _perlinOptions;
    private BaseGeneratorOptions _simplexOptions;
    private BaseGeneratorOptions _valueNoiseOptions;
    private OptionsContainer _adjustmentsContainer;
    private OptionsContainer _generatorsContainer;
    private CheckBox _domainWarpingCheckBox;
    private CheckBox _islandsOptionsCheckbox;
    private CheckBox _treeOptionsCheckbox;
    private CheckBox _moistureCheckBox;
    private IslandOptions _islandOptions;
    private DomainWarpingOptions _domainWarpingOptions;
    private TreePlacementOptions _treePlacementOptions;
    private MoistureOptions _moistureOptions;

    private readonly Logger<MapGenerationMenu> _logger = new();
    private int _discreteSteps = 15;
    private int _curSmoothCycles = 2;
    private float _curNoiseInfluence = 1.0f;
    private float _curSeaLevel = 0.2f;
	private bool _enableDomainWarping;
	private bool _enableIslands;
	private bool _enableTrees;
	private bool _enableMoisture;
    private bool _redrawOnParametersChanged = true;
    private MapInterpolationType _mapInterpolationType;
	private BaseGeneratorOptions _selectedGenerator;
	private DomainWarpingApplier _domainWarpingApplier;
	private IslandApplier _islandApplier;
    private TreesApplier _treesApplier;

    public event Action<float> OnWaterLevelChanged;
    public event Action<bool> TreesGenerationEnabledChanged;
    public event Action GenerationParametersChanged;
    public event Action<bool> OnRedrawOnParametersChangedChanged;

    [InputLine(Description = "Generator:", Category = "Generator selection", Id = "GeneratorOptions", Tooltip = SELECTED_GENERATOR_TOOLTIP)]
    [InputLineCombobox(selected: 0, bind: ComboboxBind.Id)]
    [InputOption("Diamond square", id: 1)]
    [InputOption("Perlin noise",   id: 2)]
    [InputOption("Worley noise",   id: 3)]
    [InputOption("Simplex noise",  id: 4)]
    [InputOption("Value noise",    id: 5)]
    public int SelectedGeneratorItemId
    {
        set
        {
            _selectedGenerator?.Hide();

            _selectedGenerator = value switch
            {
                1 => _diamondSquareOptions,
                2 => _perlinOptions,
                3 => _worleyOptions,
                4 => _simplexOptions,
                5 => _valueNoiseOptions,
                _ => _selectedGenerator
            };

            if (_selectedGenerator != null)
            {
                _selectedGenerator.Show();
            }

            HandleParametersChanged();
        }
    }


    [InputLine(Description = "Map interpolation:", Category = "Adjustments", Id = "MapInterpolation", Tooltip = MAP_INTERPOLATION_TOOLTIP)]
    [InputLineCombobox(selected: 0, bind: ComboboxBind.Id)]
    [InputOption("Linear",               id: (int)MapInterpolationType.Linear)]
    [InputOption("Highlight high areas", id: (int)MapInterpolationType.HighlightHighValues)]
    [InputOption("Highlight low areas",  id: (int)MapInterpolationType.HighlightLowValues)]
    [InputOption("Highlight extremes",   id: (int)MapInterpolationType.HighlightExtremes)]
    //[InputOption("Discrete",             id: (int)MapInterpolationType.Discrete)]
    public MapInterpolationType CurMapInterpolationType
    {
        get => _mapInterpolationType;
        set
        {
            _mapInterpolationType = value;
            HandleParametersChanged();
        }
    }


/*    [InputLine(Description = "Discrete steps:", Category = "Adjustments", Tooltip = DISCRETE_STEPS_TOOLTIP)]
    [InputLineSlider(1, 100)]
    public int DiscreteSteps
    {
        get => _discreteSteps;
        set
        {
            _discreteSteps = value;
            HandleParametersChanged();
        }
    }*/

    [InputLine(Description = "Height influence:", Category = "Adjustments", Tooltip = HEIGHT_INFLUENCE_TOOLTIP)]
    [InputLineSlider(0.01f, 4.0f, 0.01f)]
    public float CurNoiseInfluence
	{
		get => _curNoiseInfluence;
		private set
		{
			_curNoiseInfluence = value;
			HandleParametersChanged();
		}
	}

    [InputLine(Description = "Smooth cycles:", Category = "Adjustments", Tooltip = SMOOTH_CYCLES_TOOLTIP)]
    [InputLineSlider(0, MaxSmoothCycles)]
    public int CurSmoothCycles
	{
		get => _curSmoothCycles;
		private set
		{
			_curSmoothCycles = Mathf.Clamp(value, 0, MaxSmoothCycles);
			HandleParametersChanged();
		}
	}

    [InputLine(Description = "Sea level:", Category = "Adjustments", Tooltip = SEA_LEVEL_TOOLTIP)]
    [InputLineSlider(0.0f, 1.0f, 0.01f)]
    public float CurSeaLevel
    {
        get => _curSeaLevel;
        private set
        {
            _curSeaLevel = (float)Mathf.Clamp(value, 0.0, 1.0);
            OnWaterLevelChanged?.Invoke(_curSeaLevel);
        }
    }


    [InputLine(Description = "Redraw on parameter changed:", Category = "Adjustments", Tooltip = REDRAW_ON_PARAMETERS_CHANGED_TOOLTIP)]
    [InputLineCheckBox(checkboxType: CheckboxType.CheckButton)]
    public bool RedrawOnParametersChanged
    {
        get => _redrawOnParametersChanged;
        set
        {
            if (_redrawOnParametersChanged != value)
            {
                _redrawOnParametersChanged = value;
                OnRedrawOnParametersChangedChanged?.Invoke(value);
                HandleParametersChanged();
            }
        }
    }

    public bool EnableDomainWarping
	{
		get => _enableDomainWarping;
		private set
		{
			_enableDomainWarping = value;
			HandleParametersChanged();
		}
	}
	public bool EnableIslands
	{
		get => _enableIslands;
		private set
		{
			_enableIslands = value;
			HandleParametersChanged();
		}
	}
	public bool EnableTrees
	{
		get => _enableTrees;
		private set
		{
            _enableTrees = value;
            TreesGenerationEnabledChanged?.Invoke(_enableTrees);
		}
	}
    public bool EnableMoisture
    {
        get => _enableMoisture;
        private set
        {
            _enableMoisture = value;
            HandleParametersChanged();
        }
    }

    private bool IsLoading { get; set; }

    public BaseGeneratorOptions SelectedGenerator => _selectedGenerator;
    public IDomainWarpingApplier DomainWarpingApplier => _domainWarpingApplier;
    public IIslandsApplier IslandsApplier => _islandApplier;
    public MoistureOptions MoistureOptions => _moistureOptions;
    public ITreesApplier TreesApplier => _treesApplier;
	public TreePlacementOptions TreePlacementOptions => _treePlacementOptions;


    public override void _Ready()
    {
        // Generators and adjustments
        _perlinOptions = GetNode<BaseGeneratorOptions>("%PerlinOptions");
        _worleyOptions = GetNode<BaseGeneratorOptions>("%WorleyOptions");
        _simplexOptions = GetNode<BaseGeneratorOptions>("%SimplexOptions");
        _valueNoiseOptions = GetNode<BaseGeneratorOptions>("%ValueNoiseOptions");
        _diamondSquareOptions = GetNode<BaseGeneratorOptions>("%DiamondSquareOptions");
        _generatorsContainer = GetNode<OptionsContainer>("%GeneratorsContainer");
        _adjustmentsContainer = GetNode<OptionsContainer>("%AdjustmentsContainer");
        _diamondSquareOptions.ParametersChanged += HandleParametersChanged;
        _perlinOptions.ParametersChanged += HandleParametersChanged;
        _worleyOptions.ParametersChanged += HandleParametersChanged;
        _simplexOptions.ParametersChanged += HandleParametersChanged;
        _valueNoiseOptions.ParametersChanged += HandleParametersChanged;
        InputLineManager.CreateInputLinesForObject(this, _adjustmentsContainer, "Adjustments");
        InputLineManager.CreateInputLinesForObject(this, _generatorsContainer, "Generator selection");
        
        // Features enabling
        _domainWarpingCheckBox = GetNode<CheckBox>("%DomainWarpingCheckBox");
        _islandsOptionsCheckbox = GetNode<CheckBox>("%IslandOptionsCheckBox");
        _treeOptionsCheckbox = GetNode<CheckBox>("%TreePlacementCheckBox");
        _moistureCheckBox = GetNode<CheckBox>("%MoistureCheckBox");
        _domainWarpingCheckBox.Toggled += OnDomainWarpingCheckBoxToggled;
        _islandsOptionsCheckbox.Toggled += OnIslandOptionsCheckBoxToggled;
        _treeOptionsCheckbox.Toggled += OnTreeOptionsCheckboxOnToggled;
        _moistureCheckBox.Toggled += OnMoistureCheckBoxOnToggled;

        // Features
        _treePlacementOptions = GetNode<TreePlacementOptions>("%TreePlacementOptions");
        _domainWarpingOptions = GetNode<DomainWarpingOptions>("%DomainWarpingOptions");
        _islandOptions = GetNode<IslandOptions>("%IslandOptions");
        _moistureOptions = GetNode<MoistureOptions>("%MoistureOptions");
        _domainWarpingOptions.ParametersChanged += HandleParametersChanged;
        _islandOptions.ParametersChanged += HandleParametersChanged;
        _moistureOptions.ParametersChanged += HandleParametersChanged;
        _domainWarpingApplier = _domainWarpingOptions.DomainWarpingApplier;
        _islandApplier = _islandOptions.IslandApplier;
        _treesApplier = _treePlacementOptions.TreesApplier;
    }



    public void ApplySelectedInterpolation(float[,] map)
    {
        for (int y = 0; y < map.Height(); y++)
        {
            for (int x = 0; x < map.Width(); x++)
            {
                switch (CurMapInterpolationType)
                {
                    case MapInterpolationType.Linear:
                        break;
                    case MapInterpolationType.HighlightHighValues:
                        map[y, x] = Interpolations.HighlightHighValues(map[y, x]);
                        break;
                    case MapInterpolationType.HighlightLowValues:
                        map[y, x] = Interpolations.HighlightLowValues(map[y, x]);
                        break;
                    case MapInterpolationType.HighlightExtremes:
                        map[y, x] = Interpolations.HighlightExtremes(map[y, x]);
                        break;
                    case MapInterpolationType.Discrete:
                        map[y, x] = Interpolations.Discrete(map[y, x], _discreteSteps);
                        break;
                }
            }
        }
    }

    public Dictionary<string, object> GetLastUsedConfig()
    {
        var result = new Dictionary<string, object>();

        var selectedGenerator = _generatorsContainer.GetLastUsedConfig();
        result.Add("SelectedGenerator", selectedGenerator);

        var adjustmentsParameters = _adjustmentsContainer.GetLastUsedConfig();
        result.Add("Adjustments", adjustmentsParameters);

        var curGeneratorParameters = _selectedGenerator?.GetLastUsedConfig();
        if (curGeneratorParameters != null)
            result.Add("Generator", curGeneratorParameters);

        // Moisture
        result["MoistureEnabled"] = EnableMoisture;
        if (EnableMoisture)
            result["Moisture"] = MoistureOptions.GetLastUsedConfig();

        // Domain warping
        result["WarpingEnabled"] = EnableDomainWarping;
        if (EnableDomainWarping)
            result["Warping"] = _domainWarpingOptions.GetLastUsedConfig();

        // Islands
        result["IslandsEnabled"] = EnableIslands;
        if (EnableIslands)
            result["Islands"] = _islandOptions.GetLastUsedConfig();

        // Trees
        result["TreesEnabled"] = EnableTrees;
        if (EnableTrees)
            result["TreesPlacement"] = _treePlacementOptions.GetLastUsedConfig();

        return result;
    }

    public void UpdateCurrentConfigAsLastUsed()
    {
        _generatorsContainer.UpdateCurrentConfigAsLastUsed();
        _adjustmentsContainer.UpdateCurrentConfigAsLastUsed();
        _selectedGenerator?.UpdateCurrentConfigAsLastUsed();
        _domainWarpingOptions.UpdateCurrentConfigAsLastUsed();
        _islandOptions.UpdateCurrentConfigAsLastUsed();
        _moistureOptions.UpdateCurrentConfigAsLastUsed();
        _treePlacementOptions.UpdateCurrentConfigAsLastUsed();
    }

    public void LoadConfigFrom(Dictionary<string, object> config)
    {
        try
        {
            IsLoading = true;
            _logger.Log("Starting loading generation options from config....");

            // ADJUSTMENTS
            if (config.GetValueOrDefault("Adjustments") is Dictionary<string, object> adjustmentsConfig)
            {
                _logger.Log("Loading adjustments...");
                _adjustmentsContainer.LoadConfigFrom(adjustmentsConfig);
            }

            // SELECTED GENERATOR
            if (config.GetValueOrDefault("SelectedGenerator") is Dictionary<string, object> selectedGeneratorConfig)
            {
                _logger.Log("Loading selected generator...");
                _generatorsContainer.LoadConfigFrom(selectedGeneratorConfig);
            }

            // GENERATOR OPTIONS
            if (config.GetValueOrDefault("Generator") is Dictionary<string, object> generatorConfig)
            {
                _logger.Log("Loading generator options...");
                _selectedGenerator?.LoadConfigFrom(generatorConfig);
            }

            // MOISTURE
            if (config.GetValueOrDefault("MoistureEnabled") is bool moistureEnabled)
            {
                _logger.Log("Loading moisture enabled (bool value)...");
                EnableMoisture = moistureEnabled;
                _moistureCheckBox.ButtonPressed = moistureEnabled;
            }

            if (EnableMoisture && config.GetValueOrDefault("Moisture") is Dictionary<string, object> moistureConfig)
            {
                _logger.Log("Loading moisture feature options...");
                _moistureOptions.LoadConfigFrom(moistureConfig);
            }

            // DOMAIN WARPING
            if (config.GetValueOrDefault("WarpingEnabled") is bool warpingEnabled)
            {
                _logger.Log("Loading warping enabled (bool value)...");
                EnableDomainWarping = warpingEnabled;
                _domainWarpingCheckBox.ButtonPressed = EnableDomainWarping;
            }

            if (EnableDomainWarping && config.GetValueOrDefault("Warping") is Dictionary<string, object> warpingConfig)
            {
                _logger.Log("Loading warping feature options...");
                _domainWarpingOptions.LoadConfigFrom(warpingConfig);
            }

            // ISLANDS
            if (config.GetValueOrDefault("IslandsEnabled") is bool islandsEnabled)
            {
                _logger.Log("Loading islands enabled (bool value)...");
                EnableIslands = islandsEnabled;
                _islandsOptionsCheckbox.ButtonPressed = EnableIslands;
            }

            if (EnableIslands && config.GetValueOrDefault("Islands") is Dictionary<string, object> islandsConfig)
            {
                _logger.Log("Loading islands feature options...");
                _islandOptions.LoadConfigFrom(islandsConfig);
            }

            // TREES
            if (config.GetValueOrDefault("TreesEnabled") is bool treesEnabled)
            {
                _logger.Log("Loading trees enabled (bool value)...");
                EnableTrees = treesEnabled;
                _treeOptionsCheckbox.ButtonPressed = EnableTrees;
            }

            if (EnableTrees && config.GetValueOrDefault("TreesPlacement") is Dictionary<string, object> treesConfig)
            {
                _logger.Log("Loading trees feature options...");
                _treePlacementOptions.LoadConfigFrom(treesConfig);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    public void DisableOptions()
    {
        _selectedGenerator?.DisableOptions();
        _adjustmentsContainer.DisableOptions();
        _domainWarpingCheckBox.Disabled = true;
        _islandsOptionsCheckbox.Disabled = true;
        _treeOptionsCheckbox.Disabled = true;
        _moistureCheckBox.Disabled = true;
        _moistureOptions.DisableOptions();
        _domainWarpingOptions.DisableOptions();
        _treePlacementOptions.DisableOptions();
        _islandOptions.DisableOptions();
    }
    
    public void EnableOptions()
    {
        _selectedGenerator?.EnableOptions();
        _adjustmentsContainer.EnableOptions();
        _domainWarpingCheckBox.Disabled = false;
        _islandsOptionsCheckbox.Disabled = false;
        _treeOptionsCheckbox.Disabled = false;
        _moistureCheckBox.Disabled = false;
        _domainWarpingOptions.EnableOptions();
        _treePlacementOptions.EnableOptions();
        _moistureOptions.EnableOptions();
        _islandOptions.EnableOptions();
    }

    private void HandleParametersChanged()
    {
        if(!IsLoading) GenerationParametersChanged?.Invoke();
    }
	
    private void OnDomainWarpingCheckBoxToggled(bool toggledOn)
	{
		_domainWarpingOptions.Visible = toggledOn;
		EnableDomainWarping = toggledOn;
	}
	
    private void OnIslandOptionsCheckBoxToggled(bool toggledOn)
	{
		_islandOptions.Visible = toggledOn;
		EnableIslands = toggledOn;
	}
    private void OnTreeOptionsCheckboxOnToggled(bool toggledOn)
    {
        _treePlacementOptions.Visible = toggledOn;
        EnableTrees = toggledOn;
    }

    private void OnMoistureCheckBoxOnToggled(bool toggledOn)
    {
        _moistureOptions.Visible = toggledOn;
        EnableMoisture = toggledOn;
    }
}
