using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using TerrainGenerationApp.Generators.Trees;
using TerrainGenerationApp.Utilities;

namespace TerrainGenerationApp.Scenes.GenerationOptions.TreePlacementOptions;

public partial class TreePlacementOptions : VBoxContainer
{
    // NODES REFERENCED WITH "%" IN SCENE
    private VBoxContainer _rulesContainer;
	private Button _addRuleButton;

	private List<TreePlacementRuleItem> _treePlacementRules;
    private TreesApplier _treesApplier;
    private bool _isRulesCacheDirty = true;
    private List<TreePlacementRule> _cachedRules = new();

    public event EventHandler<TreePlacementRuleItem> OnTreePlacementRuleItemAdded;
    public event EventHandler<TreePlacementRuleItem> OnTreePlacementRuleItemRemoved;
    public event EventHandler OnTreePlacementRuleItemsOrderChanged;
    public event EventHandler OnTreePlacementRulesChanged;


	public Dictionary<string, Color> GetTreeIdColors()
	{
		return _treePlacementRules.ToDictionary(item => item.TreeId, item => item.GetColor);
	}

    public TreesApplier TreesApplier => _treesApplier;

	public override void _Ready()
	{
		_rulesContainer = GetNode<VBoxContainer>("%RulesContainer");
		_addRuleButton = GetNode<Button>("%AddRuleButton");
		_treePlacementRules = new();
		_treesApplier = new();

		_addRuleButton.Pressed += AddRuleButtonOnPressed;
	}

	private void AddRuleButtonOnPressed()
	{
		var scene = LoadedScenes.TREE_PLACEMENT_RULE_ITEM_SCENE.Instantiate();
		var item = scene as TreePlacementRuleItem;

		if (item == null)
		{
			throw new Exception($"Error in {typeof(TreePlacementOptions)}: " +
								$"Can not ADD placement rule, Instantiated scene is not of type {typeof(TreePlacementRuleItem)}, " +
								$"actual type - {scene.GetType()}");
		}

		_rulesContainer.AddChild(item);
		_treePlacementRules.Add(item);

        item.OnDeleteButtonPressed += TreePlacementRuleItemOnDeleteButtonPressed;
        item.OnMoveDownButtonPressed += TreePlacementRuleItemOnMoveDownButtonPressed;
        item.OnMoveUpButtonPressed += TreePlacementRuleItemOnMoveUpButtonPressed;
        item.OnRulesChanged += TreePlacementRuleItemOnRulesChanged;
        OnTreePlacementRuleItemAdded?.Invoke(this, item);

        _isRulesCacheDirty = true;
    }


    private void TreePlacementRuleItemOnRulesChanged(object sender, EventArgs e)
    {
        _isRulesCacheDirty = true;
        OnTreePlacementRulesChanged?.Invoke(this, EventArgs.Empty);
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


    private void TreePlacementRuleItemOnDeleteButtonPressed(object sender, EventArgs e)
	{
		var item = sender as TreePlacementRuleItem;

		if (item == null)
		{
			throw new Exception($"Error in {typeof(TreePlacementOptions)}: " +
								$"Can`t REMOVE tree placement rule, Instantiated scene is not of type {typeof(TreePlacementRuleItem)}, " +
								$"actual type - {sender.GetType()}");
		}

		_rulesContainer.RemoveChild(item);
		_treePlacementRules.Remove(item);
		OnTreePlacementRuleItemRemoved?.Invoke(this, item);
		item.QueueFree();

        _isRulesCacheDirty = true;
}

	private void TreePlacementRuleItemOnMoveDownButtonPressed(object sender, EventArgs e)
	{
		var sceneToMove = (sender as Node)!;
		var curIndex = sceneToMove.GetIndex();
		_rulesContainer.MoveChild(sceneToMove, (curIndex + 1) % _rulesContainer.GetChildCount());
		OnTreePlacementRuleItemsOrderChanged?.Invoke(this, EventArgs.Empty);

        _isRulesCacheDirty = true;
    }

	private void TreePlacementRuleItemOnMoveUpButtonPressed(object sender, EventArgs e)
	{
		var sceneToMove = (sender as Node)!;
		var curIndex = sceneToMove.GetIndex();
		_rulesContainer.MoveChild(sceneToMove, curIndex - 1);
		OnTreePlacementRuleItemsOrderChanged?.Invoke(this, EventArgs.Empty);

        _isRulesCacheDirty = true;
    }
}
