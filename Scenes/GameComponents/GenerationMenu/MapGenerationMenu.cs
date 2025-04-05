using Godot;
using System;
using System.Collections.Generic;
using TerrainGenerationApp.Domain.Generators.DomainWarping;
using TerrainGenerationApp.Domain.Generators.Islands;
using TerrainGenerationApp.Domain.Generators.Trees;
using TerrainGenerationApp.Domain.Generators.WaterErosion;
using TerrainGenerationApp.Scenes.GenerationOptions;
using TerrainGenerationApp.Scenes.GenerationOptions.DomainWarping;
using TerrainGenerationApp.Scenes.GenerationOptions.Island;
using TerrainGenerationApp.Scenes.GenerationOptions.TreePlacement;
using TerrainGenerationApp.Scenes.GenerationOptions.WaterErosion;


namespace TerrainGenerationApp.Scenes.GameComponents.GenerationMenu;

public partial class MapGenerationMenu : Control
{
	private const int MaxSmoothCycles = 10;

    private OptionButton _generatorDropdownMenu;
    private BaseGeneratorOptions _diamondSquareOptions;
    private BaseGeneratorOptions _worleyOptions;
    private BaseGeneratorOptions _perlinOptions;
    private Label _smoothCyclesLabel;
    private Label _noiseInfluenceLabel;
    private Label _seaLevelLabel;
    private Slider _smoothCyclesSlider;
    private Slider _noiseInfluenceSlider;
    private Slider _seaLevelSlider;
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
	private Dictionary<int, BaseGeneratorOptions> _generators = new();
	private BaseGeneratorOptions _selectedGenerator;
	private DomainWarpingApplier _domainWarpingApplier;
	private WaterErosionApplier _waterErosionApplier;
	private IslandApplier _islandApplier;
    private TreesApplier _treesApplier;

    public event EventHandler OnWaterLevelChanged;
    public event EventHandler GenerationParametersChanged;

    public float CurSeaLevel
    {
        get => _curSeaLevel;
        private set
        {
            _curSeaLevel = (float)Mathf.Clamp(value, 0.0, 1.0);
            GD.Print("SEA LEVEL CHANGED!");
            OnWaterLevelChanged?.Invoke(this, EventArgs.Empty);
        }
    }
	public float CurNoiseInfluence
	{
		get => _curNoiseInfluence;
		private set
		{
			_curNoiseInfluence = value;
			HandleParametersChanged();
		}
	}
	public int CurSmoothCycles
	{
		get => _curSmoothCycles;
		private set
		{
			_curSmoothCycles = Mathf.Clamp(value, 0, MaxSmoothCycles);
			HandleParametersChanged();
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

        // Manipulate map
        _seaLevelLabel = GetNode<Label>("%SeaLevelLabel");
        _smoothCyclesLabel = GetNode<Label>("%SmoothCyclesLabel");
        _noiseInfluenceLabel = GetNode<Label>("%NoiseInfluenceLabel");
        _seaLevelSlider = GetNode<Slider>("%SeaLevelSlider");
        _smoothCyclesSlider = GetNode<Slider>("%SmoothCyclesSlider");
        _noiseInfluenceSlider = GetNode<Slider>("%NoiseInfluenceSlider");
        _seaLevelSlider.ValueChanged += OnSeaLevelSliderOnValueChanged;
        _smoothCyclesSlider.ValueChanged += OnSmoothCyclesSliderValueChanged;
        _noiseInfluenceSlider.ValueChanged += OnNoiseInfluenceSliderValueChanged;

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

    public void DisableAllOptions()
    {
        _selectedGenerator?.DisableAllOptions();
        _noiseInfluenceSlider.Editable = false;
        _smoothCyclesSlider.Editable = false;
        _seaLevelSlider.Editable = false;

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
        _noiseInfluenceSlider.Editable = true;
        _smoothCyclesSlider.Editable = true;
        _seaLevelSlider.Editable = true;

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
    private void OnSeaLevelSliderOnValueChanged(double value)
    {
        CurSeaLevel = (float)value;
        _seaLevelLabel.Text = CurSeaLevel.ToString("0.##");
    }
	private void OnSmoothCyclesSliderValueChanged(double value)
	{
		CurSmoothCycles = Mathf.RoundToInt(value);
		_smoothCyclesLabel.Text = CurSmoothCycles.ToString();
	}
	private void OnNoiseInfluenceSliderValueChanged(double value)
	{
		CurNoiseInfluence = (float)value;
		_noiseInfluenceLabel.Text = CurNoiseInfluence.ToString("0.##");
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
