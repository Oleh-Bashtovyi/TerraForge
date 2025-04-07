using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using TerrainGenerationApp.Domain.Core;
using TerrainGenerationApp.Domain.Enums;
using TerrainGenerationApp.Domain.Generators.Trees;
using TerrainGenerationApp.Domain.Rules.TreeRules;
using TerrainGenerationApp.Domain.Utils;
using TerrainGenerationApp.Scenes.BuildingBlocks.Attributes;
using TerrainGenerationApp.Scenes.BuildingBlocks.InputLine;
using TerrainGenerationApp.Scenes.LoadedScenes;

namespace TerrainGenerationApp.Scenes.FeatureOptions.TreePlacement;

public partial class TreePlacementOptions : VBoxContainer
{
    private BuildingBlocks.OptionsContainer _optionsContainer;
    private VBoxContainer _rulesContainer;
	private Button _addRuleButton;

    private readonly Logger<TreePlacementOptions> _logger = new();
	private readonly List<TreePlacementRuleItem> _treePlacementRules = new();
    private readonly TreesApplier _treesApplier = new();
    private List<TreePlacementRule> _cachedRules = new();
    private bool _isRulesCacheDirty = true;
    private float _frequency = 1.0f;

    public event EventHandler<TreePlacementRuleItem> OnTreePlacementRuleItemAdded;
    public event EventHandler<TreePlacementRuleItem> OnTreePlacementRuleItemRemoved;
    public event EventHandler OnTreePlacementRuleItemsOrderChanged;
    public event EventHandler OnTreePlacementRulesChanged;

    [InputLine(Description = "Frequency")]
    [InputLineSlider(0.0f, 5.0f, 0.1f)]
    public float Frequency
    {
        get => _frequency;
        set
        {
            _frequency = value;
        }
    }

    public TreesApplier TreesApplier => _treesApplier;

	public override void _Ready()
	{
        base._Ready();
        _optionsContainer = GetNode<BuildingBlocks.OptionsContainer>("%OptionsContainer");
        _rulesContainer = GetNode<VBoxContainer>("%RulesContainer");
		_addRuleButton = GetNode<Button>("%AddRuleButton");
        InputLineManager.CreateInputLinesForObject(this, _optionsContainer);

        _addRuleButton.Pressed += AddTreePlacementRuleButtonOnPressed;
	}

    public void EnableAllOptions()
    {
        _optionsContainer.EnableAllOptions();
        _addRuleButton.Disabled = false;
        _treePlacementRules.ForEach(x => x.EnableAllOptions());
    }

    public void DisableAllOptions()
    {
        _optionsContainer.DisableAllOptions();
        _addRuleButton.Disabled = true;
        _treePlacementRules.ForEach(x => x.DisableAllOptions());
    }

    public List<TreePlacementRule> GetRules()
    {
        if (_isRulesCacheDirty)
        {
            _cachedRules = _treePlacementRules.Select(x => x.GetTreePlacementRule()).ToList();
            _isRulesCacheDirty = false;
        }

        return _cachedRules;
    }

    public Dictionary<string, Color> GetTreesColors()
    {
        return _treePlacementRules.ToDictionary(x => x.TreeId, x => x.TreeColor);
    }

    public Dictionary<string, PackedScene> GetTreesModels()
    {
        return _treePlacementRules.ToDictionary(x => x.TreeId, x => x.GetModel());
    }

    public List<TreesLayer> GenerateTrees(IWorldData worldData)
    {
        var rules = GetRules().ToArray();
        return TreesApplier.GenerateTreesLayers(worldData, rules, _frequency);
    }

    private void TreePlacementRuleItemOnRulesChanged(object sender, EventArgs e)
    {
        _logger.Log($"CATCHING {nameof(TreePlacementRuleItem)} RULES CHANGED");

        _isRulesCacheDirty = true;
        OnTreePlacementRulesChanged?.Invoke(this, EventArgs.Empty);
    }
    private void AddTreePlacementRuleButtonOnPressed()
    {
        _logger.Log($"HANDLING {nameof(TreePlacementRuleItem)} ADDING...");

        var scene = TreePlacementRuleLoadedScenes.TREE_PLACEMENT_RULE_ITEM_SCENE.Instantiate();
        var item = scene as TreePlacementRuleItem;

        if (item == null)
        {
            var message = $"Can`t ADD TREE PLACEMENT RULE, because it is not " +
                          $"of type {typeof(TreePlacementRuleItem)}, " +
                          $"actual type: {scene.GetType()}";
            _logger.Log(message, LogMark.Error);
            throw new Exception(message);
        }

        _rulesContainer.AddChild(item);
        _treePlacementRules.Add(item);

        SubscribeEventsToItem(item);
        _isRulesCacheDirty = true;

        OnTreePlacementRuleItemAdded?.Invoke(this, item);
    }
    private void TreePlacementRuleItemOnDeleteButtonPressed(object sender, EventArgs e)
    {
        _logger.Log($"HANDLING {nameof(TreePlacementRuleItem)} DELETION...");

        var item = sender as TreePlacementRuleItem;

        if (item == null)
        {
            var message = $"Can`t DELETE TREE PLACEMENT RULE, because it is not " +
                          $"of type {typeof(TreePlacementRuleItem)}, " +
                          $"actual type: {sender.GetType()}";
            _logger.Log(message, LogMark.Error);
            throw new Exception(message);
        }

        _rulesContainer.RemoveChild(item);
        _treePlacementRules.Remove(item);

        UnsubscribeEventsFromItem(item);
        _isRulesCacheDirty = true;
        item.QueueFree();

        OnTreePlacementRuleItemRemoved?.Invoke(this, item);
    }

    private void SubscribeEventsToItem(TreePlacementRuleItem item)
    {
        item.OnDeleteButtonPressed += TreePlacementRuleItemOnDeleteButtonPressed;
        item.OnMoveDownButtonPressed += TreePlacementRuleItemOnMoveDownButtonPressed;
        item.OnMoveUpButtonPressed += TreePlacementRuleItemOnMoveUpButtonPressed;
        item.OnRulesChanged += TreePlacementRuleItemOnRulesChanged;
    }
    private void UnsubscribeEventsFromItem(TreePlacementRuleItem item)
    {
        item.OnDeleteButtonPressed -= TreePlacementRuleItemOnDeleteButtonPressed;
        item.OnMoveDownButtonPressed -= TreePlacementRuleItemOnMoveDownButtonPressed;
        item.OnMoveUpButtonPressed -= TreePlacementRuleItemOnMoveUpButtonPressed;
        item.OnRulesChanged -= TreePlacementRuleItemOnRulesChanged;
    }

    private void TreePlacementRuleItemOnMoveDownButtonPressed(object sender, EventArgs e)
	{
        _logger.Log($"HANDLING {nameof(TreePlacementRuleItem)} MOVED DOWN");

        var sceneToMove = (sender as Node)!;
		var curIndex = sceneToMove.GetIndex();
		_rulesContainer.MoveChild(sceneToMove, (curIndex + 1) % _rulesContainer.GetChildCount());
        _isRulesCacheDirty = true;
	
        OnTreePlacementRuleItemsOrderChanged?.Invoke(this, EventArgs.Empty);
    }
	private void TreePlacementRuleItemOnMoveUpButtonPressed(object sender, EventArgs e)
	{
        _logger.Log($"HANDLING {nameof(TreePlacementRuleItem)} MOVED UP");

        var sceneToMove = (sender as Node)!;
		var curIndex = sceneToMove.GetIndex();
		_rulesContainer.MoveChild(sceneToMove, curIndex - 1);
        _isRulesCacheDirty = true;
		
        OnTreePlacementRuleItemsOrderChanged?.Invoke(this, EventArgs.Empty);
    }
}
