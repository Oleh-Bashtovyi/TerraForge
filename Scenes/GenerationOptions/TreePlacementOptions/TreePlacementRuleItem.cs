using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using TerrainGenerationApp.Generators.TreePlacement;
using TerrainGenerationApp.PlacementRules;
using TerrainGenerationApp.RadiusRules;
using TerrainGenerationApp.Scenes.GenerationOptions.TreePlacementOptions.PlacementRuleItems;


namespace TerrainGenerationApp.Scenes.GenerationOptions.TreePlacementOptions;
public partial class TreePlacementRuleItem : PanelContainer
{
	private static readonly PackedScene AboveSeaLevelPlacementRuleItemScene = 
		ResourceLoader.Load<PackedScene>(
			"res://Scenes/GenerationOptions/TreePlacementOptions/PlacementRuleItems/AboveSeaLevelRuleItem.tscn");


	private ColorPickerButton _treeColorPickerButton;
	private VBoxContainer _placeRulesVBoxContainer;
	private VBoxContainer _radiusRuleVBoxContainer;
	private LineEdit _treeIdLineEdit;
	private Button _addPlaceRuleButton;
	private Button _addRadiusRuleButton;
	private Button _moveUpButton;
	private Button _moveDownButton;
	private Button _deleteButton;
	private Label _noPlaceRulesLabel;
	private Label _noRadiusRuleLabel;


	private List<IPlacementRuleItem> _placementRules;
	private string _treeId;

	public event EventHandler OnDeleteButtonPressed;
	public event EventHandler OnMoveUpButtonPressed;
	public event EventHandler OnMoveDownButtonPressed;
	public event EventHandler OnRulesChanged;
	public event EventHandler OnRuleAdded;
	public event EventHandler OnRuleRemoved;
	public event EventHandler<TreeColorChangedEventArgs> OnTreeColorChanged;
	public event EventHandler<TreeIdChangedEventArgs> OnTreeIdChanged;

	public class TreeColorChangedEventArgs
	{
		public Color NewColor { get; set; }
		public string TreeId { get; set; }
	}

	public class TreeIdChangedEventArgs
	{
		public string OldTreeId { get; set; }
		public string NewTreeId { get; set; }
	}


	public string TreeId => _treeId;
	public Color GetColor => _treeColorPickerButton.Color;


	public TreePlacementRule GetTreePlacementRule()
	{
		GD.Print("Rules inside:" + _placementRules.Count);
		var rules = _placementRules.Select(x => x.GetPlacementRule()).ToList();
		GD.Print("Rules count collected to composite rule: " + rules.Count);
		var compositeRule = new CompositePlacementRule(rules);
		var radiusRule = new ConstantRadiusRule(5.0f);
		return new TreePlacementRule(_treeId, compositeRule, radiusRule);
	}


	public override void _Ready()
	{
		_treeIdLineEdit = GetNode<LineEdit>("%TreeIdLineEdit");
		_treeColorPickerButton = GetNode<ColorPickerButton>("%TreeColorPickerButton");
		_addPlaceRuleButton = GetNode<Button>("%AddPlaceRuleButton");
		_addRadiusRuleButton = GetNode<Button>("%AddRadiusRuleButton");
		_moveUpButton = GetNode<Button>("%MoveUpButton");
		_moveDownButton = GetNode<Button>("%MoveDownButton");
		_deleteButton = GetNode<Button>("%DeleteButton");
		_placeRulesVBoxContainer = GetNode<VBoxContainer>("%PlaceRulesVBoxContainer");
		_radiusRuleVBoxContainer = GetNode<VBoxContainer>("%RadiusRuleVBoxContainer");
		_noPlaceRulesLabel = GetNode<Label>("%NoPlaceRulesLabel");
		_noRadiusRuleLabel = GetNode<Label>("%NoRadiusRuleLabel");
		_placementRules = new();
		_treeColorPickerButton.Color = Colors.Purple;
		_treeIdLineEdit.Text = "Tree";
		_treeId = "Tree";

		_treeIdLineEdit.EditingToggled += on =>
		{
			if (!on)
			{
				_treeId = _treeIdLineEdit.Text;
				OnRulesChanged?.Invoke(this, EventArgs.Empty);
			}
		};


		_moveUpButton.Pressed += MoveUpButtonOnPressed;
		_moveDownButton.Pressed += MoveDownButtonOnPressed;
		_deleteButton.Pressed += DeleteButtonOnPressed;
		_addPlaceRuleButton.Pressed += AddPlaceRuleButtonOnPressed;
	}


	private void AddPlaceRuleButtonOnPressed()
	{
		// Temporary stub
		// TODO:
		// Still need to create a window for selecting a rule type and 
		// a ban on creating 2 rules with the same type
		if (_placementRules.Any())
		{
			return;
		}

		var scene = AboveSeaLevelPlacementRuleItemScene.Instantiate();
		var item = scene as IPlacementRuleItem;

		if (item == null)
		{
			throw new Exception($"Error in {typeof(TreePlacementRuleItem)}: " +
								$"Can not ADD placement rule, Instantiated scene is not of type {typeof(IPlacementRuleItem)}, " +
								$"actual type - {scene.GetType()}");
		}

		_placeRulesVBoxContainer.AddChild(scene);
		_placementRules.Add(item);

		item.OnRuleParametersChanged += PlacementRuleItemOnOnRuleParametersChanged;
		item.OnDeleteButtonPressed += PlacementRuleItemOnDeleteButtonPressed;

		_noPlaceRulesLabel.Visible = false;
		OnRuleAdded?.Invoke(this, EventArgs.Empty);
		OnRulesChanged?.Invoke(this, EventArgs.Empty);
	}

	private void PlacementRuleItemOnOnRuleParametersChanged(object sender, EventArgs e)
	{
		OnRulesChanged?.Invoke(this, EventArgs.Empty);
	}

	private void PlacementRuleItemOnDeleteButtonPressed(object sender, EventArgs e)
	{
		var scene = sender as Node;
		var item = sender as IPlacementRuleItem;

		if (item == null || scene == null)
		{
			throw new Exception($"Error in {typeof(TreePlacementRuleItem)}: " +
								$"Can not REMOVE placement rule, Instantiated scene is not of type {typeof(IPlacementRuleItem)}, " +
								$"actual type - {sender.GetType()}");
		}

		_placeRulesVBoxContainer.RemoveChild(scene);
		_placementRules.Remove(item);
		scene.QueueFree();

		if (_placementRules.Count == 0)
		{
			_noPlaceRulesLabel.Visible = true;
		}

		OnRuleRemoved?.Invoke(this, EventArgs.Empty);
		OnRulesChanged?.Invoke(this, EventArgs.Empty);
	}

	private void DeleteButtonOnPressed()
	{
		OnDeleteButtonPressed?.Invoke(this, EventArgs.Empty);
	}

	private void MoveDownButtonOnPressed()
	{
		OnMoveDownButtonPressed?.Invoke(this, EventArgs.Empty);
	}

	private void MoveUpButtonOnPressed()
	{
		OnMoveUpButtonPressed?.Invoke(this, EventArgs.Empty);
	}
}
