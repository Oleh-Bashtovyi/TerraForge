using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using TerrainGenerationApp.Generators.Trees;
using TerrainGenerationApp.PlacementRules;
using TerrainGenerationApp.RadiusRules;
using TerrainGenerationApp.Scenes.GenerationOptions.TreePlacementOptions.PlacementRuleItems;
using TerrainGenerationApp.Utilities;


namespace TerrainGenerationApp.Scenes.GenerationOptions.TreePlacementOptions;


public class TreeColorChangedEventArgs(string treeId, Color newColor)
{
    public Color NewColor { get; } = newColor;
    public string TreeId { get; } = treeId;
}

public class TreeIdChangedEventArgs(string oldTreeId, string newTreeId)
{
    public string OldTreeId { get; } = oldTreeId;
    public string NewTreeId { get; } = newTreeId;
}


public partial class TreePlacementRuleItem : PanelContainer
{   
    // NODES REFERENCED WITH "%" IN SCENE
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
    private Color _treeColor;
	private string _treeId;
    private TreePlacementRule _cachedRule; // Кешований об'єкт правила
    private bool _isDirty = true; // Прапорець, що вказує на необхідність оновлення


    public event EventHandler OnDeleteButtonPressed;
	public event EventHandler OnMoveUpButtonPressed;
	public event EventHandler OnMoveDownButtonPressed;
	public event EventHandler OnRulesChanged;
	public event EventHandler OnRuleAdded;
	public event EventHandler OnRuleRemoved;
	public event EventHandler<TreeColorChangedEventArgs> OnTreeColorChanged;
	public event EventHandler<TreeIdChangedEventArgs> OnTreeIdChanged;




	public string TreeId => _treeId;
	public Color GetColor => _treeColor;


    public TreePlacementRule GetTreePlacementRule()
    {
        if (_isDirty || _cachedRule == null)
        {
            // Створення нового правила або оновлення кешованого
            var rules = _placementRules.Select(x => x.GetPlacementRule()).ToList();
            var compositeRule = new CompositePlacementRule(rules);
            var radiusRule = new ConstantRadiusRule(5.0f);
            _cachedRule = new TreePlacementRule(_treeId, compositeRule, radiusRule);
            _isDirty = false;
        }

        return _cachedRule;
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
		_treeId = "Tree";
        _treeColor = Colors.Purple;
		_treeColorPickerButton.Color = _treeColor;
		_treeIdLineEdit.Text = _treeId;

		_treeColorPickerButton.ColorChanged += TreeColorPickerButtonOnColorChanged;
		_treeIdLineEdit.EditingToggled += TreeIdLineEditOnEditingToggled;
		_moveUpButton.Pressed += MoveUpButtonOnPressed;
		_moveDownButton.Pressed += MoveDownButtonOnPressed;
		_deleteButton.Pressed += DeleteButtonOnPressed;
		_addPlaceRuleButton.Pressed += AddPlaceRuleButtonOnPressed;
	}

    private void MarkAsDirty()
    {
        _isDirty = true;
        OnRulesChanged?.Invoke(this, EventArgs.Empty);
    }


    private void TreeColorPickerButtonOnColorChanged(Color newColor)
    {
        _treeColor = newColor;
        var eventArgs = new TreeColorChangedEventArgs(_treeId, newColor);
		OnTreeColorChanged?.Invoke(this, eventArgs);
    }

    private void TreeIdLineEditOnEditingToggled(bool toggledOn)
    {
        if (!toggledOn)
        {
            var newId = _treeIdLineEdit.Text;
            if (newId != _treeId) // Перевіряємо, чи змінився ID
            {
                var eventArgs = new TreeIdChangedEventArgs(_treeId, newId);
                _treeId = newId;
                OnTreeIdChanged?.Invoke(this, eventArgs);
                MarkAsDirty();
            }
        }
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

		var scene = LoadedScenes.ABOVE_SEA_LEVEL_PLACEMENT_RULE_ITEM_SCENE.Instantiate();
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
        MarkAsDirty();
    }

    private void PlacementRuleItemOnOnRuleParametersChanged(object sender, EventArgs e)
    {
        MarkAsDirty();
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
        MarkAsDirty();
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
