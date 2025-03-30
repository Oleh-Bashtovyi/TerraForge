using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using TerrainGenerationApp.Enums;
using TerrainGenerationApp.Generators.Trees;
using TerrainGenerationApp.Utilities;

namespace TerrainGenerationApp.Scenes.GenerationOptions.TreePlacementOptions;

public partial class TreePlacementOptions : VBoxContainer
{
    private VBoxContainer _rulesContainer;
	private Button _addRuleButton;

    private bool _isRulesCacheDirty = true;
    private TreesApplier _treesApplier;
    private Logger<TreePlacementOptions> _logger = new();
    private List<TreePlacementRule> _cachedRules = new();
	private List<TreePlacementRuleItem> _treePlacementRules;

    public event EventHandler<TreePlacementRuleItem> OnTreePlacementRuleItemAdded;
    public event EventHandler<TreePlacementRuleItem> OnTreePlacementRuleItemRemoved;
    public event EventHandler OnTreePlacementRuleItemsOrderChanged;
    public event EventHandler OnTreePlacementRulesChanged;

    public TreesApplier TreesApplier => _treesApplier;

	public override void _Ready()
	{
		_rulesContainer = GetNode<VBoxContainer>("%RulesContainer");
		_addRuleButton = GetNode<Button>("%AddRuleButton");
		_treePlacementRules = new();
		_treesApplier = new();

		_addRuleButton.Pressed += AddTreePlacementRuleButtonOnPressed;
	}

    public void DisableAllOptions()
    {
        _addRuleButton.Disabled = true;
    }

    public void EnableAllOptions()
    {
        _addRuleButton.Disabled = false;
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

    public Dictionary<string, bool[,]> GenerateTrees(IWorldData worldData)
    {
        var rules = GetRules().ToArray();
        return TreesApplier.GenerateTreesMapsFromRules(worldData, rules);
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

        var scene = LoadedScenes.TREE_PLACEMENT_RULE_ITEM_SCENE.Instantiate();
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

        item.OnDeleteButtonPressed += TreePlacementRuleItemOnDeleteButtonPressed;
        item.OnMoveDownButtonPressed += TreePlacementRuleItemOnMoveDownButtonPressed;
        item.OnMoveUpButtonPressed += TreePlacementRuleItemOnMoveUpButtonPressed;
        item.OnRulesChanged += TreePlacementRuleItemOnRulesChanged;
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
        item.QueueFree();
        _isRulesCacheDirty = true;

        OnTreePlacementRuleItemRemoved?.Invoke(this, item);
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
