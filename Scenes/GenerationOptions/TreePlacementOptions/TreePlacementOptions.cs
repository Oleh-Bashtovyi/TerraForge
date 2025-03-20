using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using TerrainGenerationApp.Generators.TreePlacement;

namespace TerrainGenerationApp.Scenes.GenerationOptions.TreePlacementOptions;

public partial class TreePlacementOptions : VBoxContainer
{
	private readonly PackedScene TreePlacementRulesItemScene = 
		ResourceLoader.Load<PackedScene>(
        "res://Scenes/GenerationOptions/TreePlacementOptions/TreePlacementRuleItem.tscn");


	private VBoxContainer _rulesContainer;
	private Button _addRuleButton;


	private List<TreePlacementRuleItem> _rules;


	public IEnumerable<TreePlacementRule> GetRules => _rules.Select(x => x.GetTreePlacementRule());

	public Dictionary<string, Color> GetTreeIdColors()
	{
		return _rules.ToDictionary(item => item.TreeId, item => item.GetColor);
	}


public event EventHandler OnRulesChanged;
	public event EventHandler<TreePlacementRuleItem> OnTreePlacementRuleItemAdded;


	public override void _Ready()
	{
		_rulesContainer = GetNode<VBoxContainer>("%RulesContainer");
		_addRuleButton = GetNode<Button>("%AddRuleButton");
		_rules = new();

		_addRuleButton.Pressed += AddRuleButtonOnPressed;
	}

	private void AddRuleButtonOnPressed()
	{
		var scene = TreePlacementRulesItemScene.Instantiate();
		var item = scene as TreePlacementRuleItem;

		if (item == null)
		{
			throw new Exception($"Error in {typeof(TreePlacementOptions)}: " +
								$"Can not ADD placement rule, Instantiated scene is not of type {typeof(TreePlacementRuleItem)}, " +
								$"actual type - {scene.GetType()}");
		}

		_rulesContainer.AddChild(item);
		_rules.Add(item);

		item.OnDeleteButtonPressed += TreePlacementRuleItemOnDeleteButtonPressed;
		item.OnMoveDownButtonPressed += TreePlacementRuleItemOnMoveDownButtonPressed;
		item.OnMoveUpButtonPressed += TreePlacementRuleItemOnMoveUpButtonPressed;
		item.OnRulesChanged += TreePlacementRuleItemOnRulesChanged;
		OnTreePlacementRuleItemAdded?.Invoke(this, item);
	}

	private void TreePlacementRuleItemOnRulesChanged(object sender, EventArgs e)
	{
		OnRulesChanged?.Invoke(this, EventArgs.Empty);
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
		_rules.Remove(item);
		item.QueueFree();
	}

	private void TreePlacementRuleItemOnMoveDownButtonPressed(object sender, EventArgs e)
	{
		var sceneToMove = (sender as Node)!;
		var curIndex = sceneToMove.GetIndex();
		_rulesContainer.MoveChild(sceneToMove, (curIndex + 1) % _rulesContainer.GetChildCount());
	}

	private void TreePlacementRuleItemOnMoveUpButtonPressed(object sender, EventArgs e)
	{
		var sceneToMove = (sender as Node)!;
		var curIndex = sceneToMove.GetIndex();
		_rulesContainer.MoveChild(sceneToMove, curIndex - 1);
	}
}
