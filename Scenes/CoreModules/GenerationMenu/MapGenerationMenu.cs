using Godot;
using System;
using System.Collections.Generic;
using TerrainGenerationApp.Domain.Extensions;
using TerrainGenerationApp.Domain.Generators.DomainWarping;
using TerrainGenerationApp.Domain.Generators.Islands;
using TerrainGenerationApp.Domain.Generators.Trees;
using TerrainGenerationApp.Domain.Generators.WaterErosion;
using TerrainGenerationApp.Domain.Utils.TerrainUtils;
using TerrainGenerationApp.Scenes.BuildingBlocks.Attributes;
using TerrainGenerationApp.Scenes.BuildingBlocks.Containers;
using TerrainGenerationApp.Scenes.BuildingBlocks.InputLine;
using TerrainGenerationApp.Scenes.FeatureOptions.DomainWarping;
using TerrainGenerationApp.Scenes.FeatureOptions.Island;
using TerrainGenerationApp.Scenes.FeatureOptions.TreePlacement;
using TerrainGenerationApp.Scenes.FeatureOptions.WaterErosion;
using TerrainGenerationApp.Scenes.GeneratorOptions;


namespace TerrainGenerationApp.Scenes.CoreModules.GenerationMenu;

public partial class MapGenerationMenu : Control
{
    public enum MapInterpolationType
    {
        Linear,
        HighlightHighValues,
        HighlightLowValues,
        HighlightExtremes,
        Discrete
    }

	private const int MaxSmoothCycles = 10;

    private BaseGeneratorOptions _diamondSquareOptions;
    private BaseGeneratorOptions _worleyOptions;
    private BaseGeneratorOptions _perlinOptions;
    private BaseGeneratorOptions _simplexOptions;
    private OptionsContainer _adjustmentsContainer;
    private OptionsContainer _generatorsContainer;
    private CheckBox _domainWarpingCheckBox;
    private CheckBox _waterErosionCheckbox;
    private CheckBox _islandsOptionsCheckbox;
    private CheckBox _treeOptionsCheckbox;
    private IslandOptions _islandOptions;
    private DomainWarpingOptions _domainWarpingOptions;
    private WaterErosionOptions _waterErosionOptions;
    private TreePlacementOptions _treePlacementOptions;

    private int _discreteSteps = 10;
    private int _curSmoothCycles = 2;
    private float _curNoiseInfluence = 1.0f;
    private float _curSeaLevel = 0.2f;
	private bool _enableDomainWarping;
	private bool _enableIslands;
	private bool _enableTrees;
    private MapInterpolationType _mapInterpolationType;
	private BaseGeneratorOptions _selectedGenerator;
	private DomainWarpingApplier _domainWarpingApplier;
	private WaterErosionApplier _waterErosionApplier;
	private IslandApplier _islandApplier;
    private TreesApplier _treesApplier;

    public event EventHandler OnWaterLevelChanged;
    public event EventHandler GenerationParametersChanged;

    [InputLine(Description = "Generator:", Category = "Generator selection", Id = "GeneratorOptions")]
    [InputLineCombobox(selected: 0, bind: ComboboxBind.Id)]
    [InputOption("Diamond square", id: 1)]
    [InputOption("Perlin noise",   id: 2)]
    [InputOption("Worley noise",   id: 3)]
    [InputOption("Simplex noise",  id: 4)]
    public int SelectedGeneratorItemId
    {
        set
        {
            _selectedGenerator = value switch
            {
                1 => _diamondSquareOptions,
                2 => _perlinOptions,
                3 => _worleyOptions,
                4 => _simplexOptions,
                _ => _selectedGenerator
            };

            _diamondSquareOptions.Hide();
            _perlinOptions.Hide();
            _worleyOptions.Hide();
            _simplexOptions.Hide();

            if (_selectedGenerator != null)
            {
                _selectedGenerator.Show();
            }
        }
    }

    [InputLine(Description = "Map interpolation:", Category = "Adjustments", Id = "MapInterpolation")]
    [InputLineCombobox(selected: 0, bind: ComboboxBind.Id)]
    [InputOption("Linear",               id: (int)MapInterpolationType.Linear)]
    [InputOption("Highlight high areas", id: (int)MapInterpolationType.HighlightHighValues)]
    [InputOption("Highlight low areas",  id: (int)MapInterpolationType.HighlightLowValues)]
    [InputOption("Highlight extremes",   id: (int)MapInterpolationType.HighlightExtremes)]
    [InputOption("Discrete",             id: (int)MapInterpolationType.Discrete)]
    public MapInterpolationType CurMapInterpolationType
    {
        get => _mapInterpolationType;
        set
        {
            _mapInterpolationType = value;
            HandleParametersChanged();
        }
    }

    [InputLine(Description = "Discrete steps:", Category = "Adjustments")]
    [InputLineSlider(1, 100)]
    public int DiscreteSteps
    {
        get => _discreteSteps;
        set
        {
            _discreteSteps = value;
            HandleParametersChanged();
        }
    }

    [InputLine(Description = "Noise influence:", Category = "Adjustments")]
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

    [InputLine(Description = "Smooth cycles:", Category = "Adjustments")]
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

    [InputLine(Description = "Sea level:", Category = "Adjustments")]
    [InputLineSlider(0.0f, 1.0f, 0.01f)]
    public float CurSeaLevel
    {
        get => _curSeaLevel;
        private set
        {
            _curSeaLevel = (float)Mathf.Clamp(value, 0.0, 1.0);
            OnWaterLevelChanged?.Invoke(this, EventArgs.Empty);
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
			HandleParametersChanged();
		}
	}
    public BaseGeneratorOptions SelectedGenerator => _selectedGenerator;
    public IDomainWarpingApplier DomainWarpingApplier => _domainWarpingApplier;
    public IIslandsApplier IslandsApplier => _islandApplier;
    public IWaterErosionApplier WaterErosionApplier => _waterErosionApplier;
    public ITreesApplier TreesApplier => _treesApplier;
	public TreePlacementOptions TreePlacementOptions => _treePlacementOptions;


    public override void _Ready()
    {
        // Generators and adjustments
        _perlinOptions = GetNode<BaseGeneratorOptions>("%PerlinOptions");
        _worleyOptions = GetNode<BaseGeneratorOptions>("%WorleyOptions");
        _simplexOptions = GetNode<BaseGeneratorOptions>("%SimplexOptions");
        _diamondSquareOptions = GetNode<BaseGeneratorOptions>("%DiamondSquareOptions");
        _generatorsContainer = GetNode<OptionsContainer>("%GeneratorsContainer");
        _adjustmentsContainer = GetNode<OptionsContainer>("%AdjustmentsContainer");
        _diamondSquareOptions.ParametersChanged += HandleParametersChanged;
        _perlinOptions.ParametersChanged += HandleParametersChanged;
        _worleyOptions.ParametersChanged += HandleParametersChanged;
        InputLineManager.CreateInputLinesForObject(this, _adjustmentsContainer, "Adjustments");
        InputLineManager.CreateInputLinesForObject(this, _generatorsContainer, "Generator selection");
        
        // Features enabling
        _domainWarpingCheckBox = GetNode<CheckBox>("%DomainWarpingCheckBox");
        _waterErosionCheckbox = GetNode<CheckBox>("%WaterErosionCheckBox");
        _islandsOptionsCheckbox = GetNode<CheckBox>("%IslandOptionsCheckBox");
        _treeOptionsCheckbox = GetNode<CheckBox>("%TreePlacementCheckBox");
        _domainWarpingCheckBox.Toggled += OnDomainWarpingCheckBoxToggled;
        _waterErosionCheckbox.Toggled += OnWaterErosionCheckBoxToggled;
        _islandsOptionsCheckbox.Toggled += OnIslandOptionsCheckBoxToggled;
        _treeOptionsCheckbox.Toggled += OnTreeOptionsCheckboxOnToggled;

        // Features
        _treePlacementOptions = GetNode<TreePlacementOptions>("%TreePlacementOptions");
        _domainWarpingOptions = GetNode<DomainWarpingOptions>("%DomainWarpingOptions");
        _waterErosionOptions = GetNode<WaterErosionOptions>("%WaterErosionOptions");
        _islandOptions = GetNode<IslandOptions>("%IslandOptions");
        _domainWarpingOptions.ParametersChanged += HandleParametersChanged;
        _islandOptions.ParametersChanged += HandleParametersChanged;
        _domainWarpingApplier = _domainWarpingOptions.DomainWarpingApplier;
        _waterErosionApplier = _waterErosionOptions.WaterErosionApplier;
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
                        map[y, x] = Interpolations.Discrete(map[y, x], DiscreteSteps);
                        break;
                }
            }
        }
    }

    public Dictionary<string, object> GetLastUsedGenerationOptions()
    {
        var result = new Dictionary<string, object>();

        var selectedGenerator = _generatorsContainer.GetLastUsedInputLineValues();
        result.Add("SelectedGenerator", selectedGenerator);

        var adjustmentsParameters = _adjustmentsContainer.GetLastUsedInputLineValues();
        result.Add("Adjustments", adjustmentsParameters);

        var curGeneratorParameters = _selectedGenerator?.GetLastUsedInputLineValues();
        if (curGeneratorParameters != null)
        {
            result.Add("Generator", curGeneratorParameters);
        }

        return result;
    }

    public void UpdateLastUsedOptions()
    {
        _generatorsContainer.UpdateCurrentOptionsAsLastUsed();
        _adjustmentsContainer.UpdateCurrentOptionsAsLastUsed();
        _selectedGenerator?.UpdateCurrentOptionsAsLastUsed();
    }

    public void LoadFromConfiguration(Dictionary<string, object> config)
    {
        if (config.GetValueOrDefault("Adjustments") is Dictionary<string, object> adjustmentsConfig)
            _adjustmentsContainer.LoadInputLineValuesFromConfig(adjustmentsConfig);

        if (config.GetValueOrDefault("SelectedGenerator") is Dictionary<string, object> selectedGeneratorConfig)
            _generatorsContainer.LoadInputLineValuesFromConfig(selectedGeneratorConfig);

        if (config.GetValueOrDefault("Generator") is Dictionary<string, object> generatorConfig)
            _selectedGenerator?.LoadInputLineValuesFromConfig(generatorConfig);
    }

    
    public void DisableAllOptions()
    {
        _selectedGenerator?.DisableAllOptions();
        _adjustmentsContainer.DisableAllOptions();
        _domainWarpingCheckBox.Disabled = true;
        _waterErosionCheckbox.Disabled = true;
        _islandsOptionsCheckbox.Disabled = true;
        _treeOptionsCheckbox.Disabled = true;
		_domainWarpingOptions.DisableAllOptions();
        _waterErosionOptions.DisableAllOptions();
        _islandOptions.DisableAllOptions();
        _treePlacementOptions.DisableAllOptions();
    }
    public void EnableAllOptions()
    {
        _selectedGenerator?.EnableAllOptions();
        _adjustmentsContainer.EnableAllOptions();
        _domainWarpingCheckBox.Disabled = false;
        _waterErosionCheckbox.Disabled = false;
        _islandsOptionsCheckbox.Disabled = false;
        _treeOptionsCheckbox.Disabled = false;
        _domainWarpingOptions.EnableAllOptions();
        _waterErosionOptions.EnableAllOptions();
        _islandOptions.EnableAllOptions();
        _treePlacementOptions.EnableAllOptions();
    }
    private void HandleParametersChanged()
    {
        GenerationParametersChanged?.Invoke(this, EventArgs.Empty);
    }
    
	private void OnWaterErosionCheckBoxToggled(bool toggledOn)
	{
		_waterErosionOptions.Visible = toggledOn;
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
}
