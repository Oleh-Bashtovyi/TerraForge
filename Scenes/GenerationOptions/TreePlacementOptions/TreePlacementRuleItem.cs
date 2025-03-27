using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using TerrainGenerationApp.Generators.Trees;
using TerrainGenerationApp.PlacementRules;
using TerrainGenerationApp.Scenes.GenerationOptions.TreePlacementOptions.PlacementRuleItems;
using TerrainGenerationApp.Scenes.GenerationOptions.TreePlacementOptions.RadiusRuleItems;
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
    private PopupMenu _placementRulesPopupMenu;
    private ColorPickerButton _treeColorPickerButton;
	private VBoxContainer _placeRulesVBoxContainer;
	private VBoxContainer _radiusRuleVBoxContainer;
	private LineEdit _treeIdLineEdit;
	private Button _addPlacementRuleButton;
	private Button _addRadiusRuleButton;
	private Button _moveUpButton;
	private Button _moveDownButton;
	private Button _deleteButton;
	private Label _noPlaceRulesLabel;
	private Label _noRadiusRuleLabel;

	private bool _isDirty = true; 
	private string _treeId;
	private Color _treeColor;
	private TreePlacementRule _cachedRule; 
	private IRadiusRuleItem _radiusRule;
	private List<IPlacementRuleItem> _placementRules;

    public event EventHandler OnDeleteButtonPressed;
	public event EventHandler OnMoveUpButtonPressed;
	public event EventHandler OnMoveDownButtonPressed;
	public event EventHandler OnRulesChanged;
	public event EventHandler<TreeColorChangedEventArgs> OnTreeColorChanged;
	public event EventHandler<TreeIdChangedEventArgs> OnTreeIdChanged;

	public string TreeId => _treeId;
	public Color TreeColor => _treeColor;


	public TreePlacementRule GetTreePlacementRule()
	{
		GD.Print("+--GetTreePlacementRule");
        if (_isDirty || _cachedRule == null)
        {
            var rules = _placementRules.Select(x => x.GetPlacementRule()).ToList();
            var compositeRule = new CompositePlacementRule(rules);
            var radiusRule = _radiusRule?.GetRadiusRule();
            GD.Print("| - rules count: " + rules.Count);
            //var radiusRule = new ConstantRadiusRule(5.0f);
            _cachedRule = new TreePlacementRule(_treeId, compositeRule, radiusRule);
            _isDirty = false;
            GD.Print("| it was DIRTY, creating a new one");
        }
        else
        {
			GD.Print("| Cache has up-to-date object");
        }
		GD.Print("+--");

		return _cachedRule;
	}


    public override void _Ready()
	{
		_treeIdLineEdit = GetNode<LineEdit>("%TreeIdLineEdit");
		_treeColorPickerButton = GetNode<ColorPickerButton>("%TreeColorPickerButton");
		_addPlacementRuleButton = GetNode<Button>("%AddPlaceRuleButton");
		_addRadiusRuleButton = GetNode<Button>("%AddRadiusRuleButton");
		_moveUpButton = GetNode<Button>("%MoveUpButton");
		_moveDownButton = GetNode<Button>("%MoveDownButton");
		_deleteButton = GetNode<Button>("%DeleteButton");
		_placeRulesVBoxContainer = GetNode<VBoxContainer>("%PlaceRulesVBoxContainer");
		_radiusRuleVBoxContainer = GetNode<VBoxContainer>("%RadiusRuleVBoxContainer");
		_noPlaceRulesLabel = GetNode<Label>("%NoPlaceRulesLabel");
		_noRadiusRuleLabel = GetNode<Label>("%NoRadiusRuleLabel");
        _placementRulesPopupMenu = GetNode<PopupMenu>("%PlacementRulesPopupMenu");
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
		_addPlacementRuleButton.Pressed += AddPlacementRuleButtonOnPressed;
        _addRadiusRuleButton.Pressed += AddRadiusRuleButtonOnPressed;
    }

    private void AddRadiusRuleButtonOnPressed()
    {
        GD.Print($"<{nameof(TreePlacementRuleItem)}><{nameof(AddRadiusRuleButtonOnPressed)}><{nameof(TreeId)}: {TreeId}>" +
                 $"---> ADDING RADIUS RULE...");

        if (_radiusRule != null)
        {
			return;
        }

		var scene = LoadedScenes.CONSTANT_RADIUS_RULE_ITEM_SCENE.Instantiate();
        var item = scene as IRadiusRuleItem;

		if (item == null)
        {
            var message = $"<{nameof(TreePlacementRuleItem)}><{nameof(AddRadiusRuleButtonOnPressed)}>" +
                          $"<{nameof(TreeId)}: {TreeId}>---> Can`t ADD RADIUS RULE, because it is not " +
                          $"of type {typeof(IRadiusRuleItem)}, " +
                          $"actual type: {scene.GetType()}";
            GD.PrintErr(message);
            throw new Exception(message);
        }

        _radiusRuleVBoxContainer.AddChild(scene);
        _radiusRule = item;

        item.OnRuleParametersChanged += RadiusRuleItemOnRuleParametersChanged;
        item.OnDeleteButtonPressed += RadiusRuleItemOnDeleteButtonPressed;

        _noRadiusRuleLabel.Visible = false;
        MarkAsDirty();
    }


    private void RadiusRuleItemOnDeleteButtonPressed(object sender, EventArgs e)
    {
        GD.Print($"<{nameof(TreePlacementRuleItem)}><{nameof(RadiusRuleItemOnDeleteButtonPressed)}><{nameof(TreeId)}: {TreeId}>" +
                 $"---> DELETING RADIUS RULE...");
        var scene = sender as Node;
        var item = sender as IRadiusRuleItem;
        if (item == null || scene == null)
        {
            var message = $"<{nameof(TreePlacementRuleItem)}><{nameof(RadiusRuleItemOnDeleteButtonPressed)}>" +
                          $"<{nameof(TreeId)}: {TreeId}>---> Can`t DELETE RADIUS RULE, because it is not " +
                          $"of types {typeof(IRadiusRuleItem)} and {typeof(Node)}, " +
                          $"actual type: {sender.GetType()}";
            GD.PrintErr(message);
            throw new Exception(message);
        }
        _radiusRuleVBoxContainer.RemoveChild(scene);
        _radiusRule = null;
        scene.QueueFree();
        if (_radiusRule == null)
        {
            _noRadiusRuleLabel.Visible = true;
        }
        MarkAsDirty();
    }

    private void RadiusRuleItemOnRuleParametersChanged(object sender, EventArgs e)
    {
        GD.Print($"<{nameof(TreePlacementRuleItem)}><{nameof(RadiusRuleItemOnRuleParametersChanged)}><{nameof(TreeId)}: {TreeId}>" +
                 $"---> Some radius rule parameters changed, marking as dirty");
        MarkAsDirty();
    }




    private void AddPlacementRuleButtonOnPressed()
	{
        GD.Print($"<{nameof(TreePlacementRuleItem)}><{nameof(AddPlacementRuleButtonOnPressed)}><{nameof(TreeId)}: {TreeId}>" +
                 $"---> ADDING PLACEMENT RULE...");

        var pos = (_addPlacementRuleButton.GlobalPosition + _addPlacementRuleButton.Size / 2);
        var actualPos = new Vector2I((int)pos.X, (int)pos.Y);
        _placementRulesPopupMenu.Position = actualPos;
        _placementRulesPopupMenu.Show();


        // Temporary stub
        // TODO:
        // Still need to create a window for selecting a rule type and 
        // a ban on creating 2 rules with the same type
        //      if (_placementRules.Any())
        //{
        //	return;
        //}
        Node scene;
        if (_placementRules.Count >= 2)
        {
            return;
        }
        else if (_placementRules.Count == 0)
        {
            scene = LoadedScenes.ABOVE_SEA_LEVEL_PLACEMENT_RULE_ITEM_SCENE.Instantiate();
        }
        else
        {
            scene = LoadedScenes.SLOPE_PLACEMENT_RULE_ITEM_SCENE.Instantiate();
        }

		var item = scene as IPlacementRuleItem;

		if (item == null)
		{
            var message = $"<{nameof(TreePlacementRuleItem)}><{nameof(AddPlacementRuleButtonOnPressed)}>" +
                          $"<{nameof(TreeId)}: {TreeId}>---> Can`t ADD PLACEMENT RULE, because it is not " +
                          $"of type {typeof(IPlacementRuleItem)}, " +
                          $"actual type: {scene.GetType()}";
            GD.PrintErr(message);
            throw new Exception(message);
        }

		_placeRulesVBoxContainer.AddChild(scene);
		_placementRules.Add(item);

		item.OnRuleParametersChanged += PlacementRuleItemOnRuleParametersChanged;
		item.OnDeleteButtonPressed += PlacementRuleItemOnDeleteButtonPressed;

		_noPlaceRulesLabel.Visible = false;
		MarkAsDirty();
	}


    private void TreeIdLineEditOnEditingToggled(bool toggledOn)
    {
        if (!toggledOn)
        {
            var newId = _treeIdLineEdit.Text;
            if (newId != _treeId)
            {
                GD.Print($"<{nameof(TreePlacementRuleItem)}><{nameof(TreeIdLineEditOnEditingToggled)}><{nameof(TreeId)}: {TreeId}>" +
                         $"---> CHANGING ID to {newId}.");
                var eventArgs = new TreeIdChangedEventArgs(_treeId, newId);
                _treeId = newId;
                OnTreeIdChanged?.Invoke(this, eventArgs);
                MarkAsDirty();
            }
        }
    }
    private void PlacementRuleItemOnDeleteButtonPressed(object sender, EventArgs e)
	{
		GD.Print($"<{nameof(TreePlacementRuleItem)}><{nameof(PlacementRuleItemOnDeleteButtonPressed)}><{nameof(TreeId)}: {TreeId}>" +
                 $"---> DELETING PLACEMENT RULE...");

		var scene = sender as Node;
		var item = sender as IPlacementRuleItem;

		if (item == null || scene == null)
        {
            var message = $"<{nameof(TreePlacementRuleItem)}><{nameof(PlacementRuleItemOnDeleteButtonPressed)}>" +
                          $"<{nameof(TreeId)}: {TreeId}>---> Can`t DELETE PLACEMENT RULE, because it is not " +
                          $"of types {typeof(IPlacementRuleItem)} and {typeof(Node)}, " +
                          $"actual type: {sender.GetType()}";
            GD.PrintErr(message);
            throw new Exception(message);
		}

		_placeRulesVBoxContainer.RemoveChild(scene);
		_placementRules.Remove(item);
		scene.QueueFree();

		if (_placementRules.Count == 0)
		{
			_noPlaceRulesLabel.Visible = true;
		}

		MarkAsDirty();
	}
    private void MarkAsDirty()
    {
        _isDirty = true;
        GD.Print($"<{nameof(TreePlacementRuleItem)}><{nameof(MarkAsDirty)}><{nameof(TreeId)}: {TreeId}>---> " +
                 $"MARK AS DIRTY");
        OnRulesChanged?.Invoke(this, EventArgs.Empty);
    }
    private void TreeColorPickerButtonOnColorChanged(Color newColor)
    {
        _treeColor = newColor;
        var eventArgs = new TreeColorChangedEventArgs(_treeId, newColor);
        GD.Print($"<{nameof(TreePlacementRuleItem)}><{nameof(TreeColorPickerButtonOnColorChanged)}><{nameof(TreeId)}: {TreeId}>---> " +
                 $"Changing color of the tree layer to {newColor}");
        OnTreeColorChanged?.Invoke(this, eventArgs);
    }
    private void PlacementRuleItemOnRuleParametersChanged(object sender, EventArgs e)
    {
        GD.Print($"<{nameof(TreePlacementRuleItem)}><{nameof(PlacementRuleItemOnRuleParametersChanged)}><{nameof(TreeId)}: {TreeId}>" +
                 $"---> Some placement rule parameters changed, marking as dirty");
        MarkAsDirty();
    }
    private void DeleteButtonOnPressed()
	{
		GD.Print($"<{nameof(TreePlacementRuleItem)}><{nameof(DeleteButtonOnPressed)}><{nameof(TreeId)}: {TreeId}>---> " +
                 $"DELETE BUTTON PRESSED");
		OnDeleteButtonPressed?.Invoke(this, EventArgs.Empty);
	}
	private void MoveDownButtonOnPressed()
	{
        GD.Print($"<{nameof(TreePlacementRuleItem)}><{nameof(MoveDownButtonOnPressed)}><{nameof(TreeId)}: {TreeId}>---> " +
                 $"MOVE DOWN BUTTON PRESSED");
        OnMoveDownButtonPressed?.Invoke(this, EventArgs.Empty);
	}
	private void MoveUpButtonOnPressed()
	{
        GD.Print($"<{nameof(TreePlacementRuleItem)}><{nameof(MoveUpButtonOnPressed)}><{nameof(TreeId)}: {TreeId}>---> " +
                 $"MOVE UP BUTTON PRESSED");
        OnMoveUpButtonPressed?.Invoke(this, EventArgs.Empty);
	}
}
