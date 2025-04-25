using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TerrainGenerationApp.Domain.Core;
using TerrainGenerationApp.Domain.Enums;
using TerrainGenerationApp.Domain.Generators.Trees;
using TerrainGenerationApp.Domain.Rules.TreeRules;
using TerrainGenerationApp.Domain.Utils;
using TerrainGenerationApp.Scenes.BuildingBlocks.Attributes;
using TerrainGenerationApp.Scenes.BuildingBlocks.Containers;
using TerrainGenerationApp.Scenes.BuildingBlocks.InputLine;
using TerrainGenerationApp.Scenes.LoadedScenes;

namespace TerrainGenerationApp.Scenes.FeatureOptions.TreePlacement;

public partial class TreePlacementOptions : VBoxContainer, IOptionsToggleable, ILastUsedConfigProvider
{
    private OptionsContainer _optionsContainer;
    private VBoxContainer _rulesContainer;
	private Button _addRuleButton;

    private readonly Logger<TreePlacementOptions> _logger = new();
	private readonly List<TreePlacementRuleItem> _treePlacementRules = new();
    private readonly TreesApplier _treesApplier = new();
    private List<TreePlacementRule> _cachedRules = new();
    private bool _isRulesCacheDirty = true;
    private float _frequency = 1.0f;
    private int _idCounter;

    public event EventHandler<TreePlacementRuleItem> OnTreePlacementRuleItemAdded;
    public event EventHandler<TreePlacementRuleItem> OnTreePlacementRuleItemRemoved;
    public event EventHandler OnTreePlacementRuleItemsOrderChanged;
    public event EventHandler OnTreePlacementRulesChanged;

    private bool IsLoading { get; set; }

    [InputLine(Description = "Frequency")]
    [InputLineSlider(0.0f, 5.0f, 0.1f)]
    public float Frequency
    {
        get => _frequency;
        set => _frequency = value;
    }

    public TreesApplier TreesApplier => _treesApplier;

	public override void _Ready()
	{
        base._Ready();
        _optionsContainer = GetNode<OptionsContainer>("%OptionsContainer");
        _rulesContainer = GetNode<VBoxContainer>("%RulesContainer");
		_addRuleButton = GetNode<Button>("%AddRuleButton");
        InputLineManager.CreateInputLinesForObject(obj: this, container: _optionsContainer);
        _addRuleButton.Pressed += AddTreePlacementRuleButtonOnPressed;
	}

    public void EnableOptions()
    {
        _addRuleButton.Disabled = false;
        _optionsContainer.EnableOptions();
        _treePlacementRules.ForEach(x => x.EnableOptions());
    }

    public void DisableOptions()
    {
        _addRuleButton.Disabled = true;
        _optionsContainer.DisableOptions();
        _treePlacementRules.ForEach(x => x.DisableOptions());
    }

    private string GetNextTreeLayerId()
    {
        return (_idCounter++).ToString();
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
        return _treePlacementRules.ToDictionary(x => x.LayerId, x => x.TreeColor);
    }

    public Dictionary<string, PackedScene> GetTreesModels()
    {
        return _treePlacementRules.ToDictionary(x => x.LayerId, x => x.GetModel());
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
        AddTreePlacementRule();
    }

    private TreePlacementRuleItem AddTreePlacementRule()
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

        item.SetId(GetNextTreeLayerId());
        _rulesContainer.AddChild(item);
        _treePlacementRules.Add(item);

        SubscribeEventsToItem(item);
        _isRulesCacheDirty = true;

        InvokeTreePlacementRuleAdded(item);
        return item;
    }


    private void RemoveTreePlacementRule(Node node)
    {
        _logger.Log($"HANDLING {nameof(TreePlacementRuleItem)} DELETION...");

        var item = node as TreePlacementRuleItem;

        if (item == null)
        {
            var message = $"Can`t DELETE TREE PLACEMENT RULE, because it is not " +
                          $"of type {typeof(TreePlacementRuleItem)}, " +
                          $"actual type: {node.GetType()}";
            _logger.Log(message, LogMark.Error);
            throw new Exception(message);
        }

        _rulesContainer.RemoveChild(item);
        _treePlacementRules.Remove(item);

        UnsubscribeEventsFromItem(item);
        _isRulesCacheDirty = true;
        item.QueueFree();
        InvokeTreePlacementRuleRemoved(item);
    }


    private void TreePlacementRuleItemOnDeleteButtonPressed(object sender, EventArgs e)
    {
        RemoveTreePlacementRule(sender as Node);
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




    public Dictionary<string, object> GetLastUsedConfig()
    {
        var result = new Dictionary<string, object>();
        var placementRulesConfig = new List<Dictionary<string, object>>();
        result["Options"] = _optionsContainer.GetLastUsedConfig();
        result["Rules"] = placementRulesConfig;

        foreach (var placementRule in _treePlacementRules)
        {
            placementRulesConfig.Add(placementRule.GetLastUsedConfig());
        }

        return result;
    }

    public void UpdateCurrentConfigAsLastUsed()
    {
        _optionsContainer.UpdateCurrentConfigAsLastUsed();
        _treePlacementRules.ForEach(x => x.UpdateCurrentConfigAsLastUsed());
    }

    public void LoadConfigFrom(Dictionary<string, object> config)
    {
        try
        {
            IsLoading = true;

            foreach (var child in _rulesContainer.GetChildren())
            {
                RemoveTreePlacementRule(child);
            }

            if (config.GetValueOrDefault("Options") is Dictionary<string, object> optionsConfig)
                _optionsContainer.LoadConfigFrom(optionsConfig);

            if (config.GetValueOrDefault("Rules") is List<object> rulesConfig)
            {
                foreach (var item in rulesConfig)
                {
                    if (item is Dictionary<string, object> ruleConfig)
                    {
                        var addedItem = AddTreePlacementRule();
                        addedItem.LoadConfigFrom(ruleConfig);
                    }
                }
            }
        }
        finally
        {
            IsLoading = false;
        }
    }


    private void InvokeTreePlacementRuleAdded(TreePlacementRuleItem item)
    {
        if (!IsLoading)
        {
            OnTreePlacementRuleItemAdded?.Invoke(this, item);
        }
    }

    private void InvokeTreePlacementRuleRemoved(TreePlacementRuleItem item)
    {
        if (!IsLoading)
        {
            OnTreePlacementRuleItemRemoved?.Invoke(this, item);
        }
    }
}
