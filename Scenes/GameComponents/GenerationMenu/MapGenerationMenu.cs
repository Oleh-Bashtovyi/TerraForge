using Godot;
using System;
using System.Collections.Generic;
using TerrainGenerationApp.Domain.Extensions;
using TerrainGenerationApp.Domain.Generators.DomainWarping;
using TerrainGenerationApp.Domain.Generators.Islands;
using TerrainGenerationApp.Domain.Generators.Trees;
using TerrainGenerationApp.Domain.Generators.WaterErosion;
using TerrainGenerationApp.Domain.Utils.TerrainUtils;
using TerrainGenerationApp.Scenes.BuildingBlocks;
using TerrainGenerationApp.Scenes.BuildingBlocks.Attributes;
using TerrainGenerationApp.Scenes.GenerationOptions;
using TerrainGenerationApp.Scenes.GenerationOptions.DomainWarping;
using TerrainGenerationApp.Scenes.GenerationOptions.Island;
using TerrainGenerationApp.Scenes.GenerationOptions.TreePlacement;
using TerrainGenerationApp.Scenes.GenerationOptions.WaterErosion;


namespace TerrainGenerationApp.Scenes.GameComponents.GenerationMenu;

public partial class MapGenerationMenu : Control
{
    public enum MapInterpolationType
    {
        [OptionDescription("Linear")]
        Linear,
        [OptionDescription("Highlight high areas")]
        HighlightHighValues,
        [OptionDescription("Highlight low areas")]
        HighlightLowValues,
        [OptionDescription("Highlight extremes")]
        HighlightExtremes
    }

	private const int MaxSmoothCycles = 10;

    private OptionButton _generatorDropdownMenu;
    private BaseGeneratorOptions _diamondSquareOptions;
    private BaseGeneratorOptions _worleyOptions;
    private BaseGeneratorOptions _perlinOptions;
    private VBoxContainer _adjustmentsContainer;
    private CheckBox _domainWarpingCheckBox;
    private CheckBox _waterErosionCheckbox;
    private CheckBox _islandsOptionsCheckbox;
    private CheckBox _treeOptionsCheckbox;
    private IslandOptions _islandOptions;
    private DomainWarpingOptions _domainWarpingOptions;
    private WaterErosionOptions _waterErosionOptions;
    private TreePlacementOptions _treePlacementOptions;

	private int _curSmoothCycles = 0;
    private float _curNoiseInfluence = 1.0f;
    private float _curSeaLevel = 0.2f;
	private bool _enableDomainWarping = false;
	private bool _enableIslands = false;
	private bool _enableTrees = false;
    private MapInterpolationType _mapInterpolationType = MapInterpolationType.Linear;
    private Dictionary<int, BaseGeneratorOptions> _generators = new();
	private BaseGeneratorOptions _selectedGenerator;
	private DomainWarpingApplier _domainWarpingApplier;
	private WaterErosionApplier _waterErosionApplier;
	private IslandApplier _islandApplier;
    private TreesApplier _treesApplier;

    public event EventHandler OnWaterLevelChanged;
    public event EventHandler GenerationParametersChanged;

    [InputLine(Description = "Map interpolation:", Category = "Adjustments")]
    public MapInterpolationType CurMapInterpolationType
    {
        get => _mapInterpolationType;
        set
        {
            _mapInterpolationType = value;
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
        // Select and set generator options
        _perlinOptions = GetNode<BaseGeneratorOptions>("%PerlinOptions");
        _worleyOptions = GetNode<BaseGeneratorOptions>("%WorleyOptions");
        _diamondSquareOptions = GetNode<BaseGeneratorOptions>("%DiamondSquareOptions");
        _generatorDropdownMenu = GetNode<OptionButton>("%GeneratorDropdownMenu");

        // Adjustments
        _adjustmentsContainer = GetNode<VBoxContainer>("%AdjustmentsContainer");
        InputLineManager.CreateInputLinesForObject(this, _adjustmentsContainer, "Adjustments");

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


        // Generator combobox
        _generators = new Dictionary<int, BaseGeneratorOptions>
        {
            { 1, _diamondSquareOptions },
            { 2, _perlinOptions },
            { 3, _worleyOptions }
        };
        HideAllGeneratorsOptions();

        foreach (var options in _generators.Values)
        {
            options.ParametersChanged += HandleParametersChanged;
        }

        var firstSelectedIndex = 0;
        _generatorDropdownMenu.Clear();
        _generatorDropdownMenu.AddItem("Diamond square", 1);
        _generatorDropdownMenu.AddItem("Perlin noise", 2);
        _generatorDropdownMenu.AddItem("Worley noise", 3);
        _generatorDropdownMenu.Selected = firstSelectedIndex;
        var firstItemId = _generatorDropdownMenu.GetItemId(firstSelectedIndex);
        var firstItemOptions = _generators[firstItemId];
        firstItemOptions.Visible = true;
        _selectedGenerator = firstItemOptions;
        _generatorDropdownMenu.ItemSelected += OnGeneratorDropdownMenuItemSelected;
    }

    public void ApplySelectedInterpolation(float[,] map)
    {
        GD.Print("Now using: " + CurMapInterpolationType);
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
                }
            }
        }
    }

    public void DisableAllOptions()
    {
        _selectedGenerator?.DisableAllOptions();

        foreach (var child in _adjustmentsContainer.GetChildren())
        {
            if (child is InputLineBase inputLine)
            {
                inputLine.DisableInput();
            }
        }

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

        foreach (var child in _adjustmentsContainer.GetChildren())
        {
            if (child is InputLineBase inputLine)
            {
                inputLine.EnableInput();
            }
        }

        _domainWarpingCheckBox.Disabled = false;
        _waterErosionCheckbox.Disabled = false;
        _islandsOptionsCheckbox.Disabled = false;
        _treeOptionsCheckbox.Disabled = false;
        _domainWarpingOptions.EnableAllOptions();
        _waterErosionOptions.EnableAllOptions();
        _islandOptions.EnableAllOptions();
        _treePlacementOptions.EnableAllOptions();
    }
	private void HideAllGeneratorsOptions()
	{
		foreach (var generator in _generators.Values)
        {
            generator.Visible = false;
        }
	}
    private void HandleParametersChanged()
    {
        GenerationParametersChanged?.Invoke(this, EventArgs.Empty);
    }
    
    private void OnGeneratorDropdownMenuItemSelected(long index)
    {
        HideAllGeneratorsOptions();

        int id = _generatorDropdownMenu.GetItemId((int)index);
        var cur = _generators[id];

        if (cur != null)
        {
            cur.Visible = true;
        }
        _selectedGenerator = cur;
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
