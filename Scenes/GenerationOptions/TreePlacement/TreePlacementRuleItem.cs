using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using TerrainGenerationApp.Generators.Trees;
using TerrainGenerationApp.Rules.PlacementRules;
using TerrainGenerationApp.Scenes.GenerationOptions.TreePlacement.PlacementRuleItems;
using TerrainGenerationApp.Scenes.GenerationOptions.TreePlacement.RadiusRuleItems;
using TerrainGenerationApp.Scenes.Loaded;
using TerrainGenerationApp.Utilities;

namespace TerrainGenerationApp.Scenes.GenerationOptions.TreePlacement;

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
    private ColorPickerButton _treeColorPickerButton;
    private OptionButton _modelOptionButton;
    private VBoxContainer _placeRulesVBoxContainer;
	private VBoxContainer _radiusRuleVBoxContainer;
	private LineEdit _treeIdLineEdit;
	private MenuButton _addPlacementRuleButton;
	private Button _addRadiusRuleButton;
	private Button _moveUpButton;
	private Button _moveDownButton;
	private Button _deleteButton;
	private Label _noPlaceRulesLabel;
	private Label _noRadiusRuleLabel;

	private readonly List<IPlacementRuleItem> _placementRules = new();
    private readonly Logger<TreePlacementRuleItem> _logger = new();
	private bool _isDirty = true; 
	private string _treeId;
	private Color _treeColor;
	private TreePlacementRule _cachedRule; 
	private IRadiusRuleItem _radiusRule;

    public event EventHandler OnDeleteButtonPressed;
	public event EventHandler OnMoveUpButtonPressed;
	public event EventHandler OnMoveDownButtonPressed;
	public event EventHandler OnRulesChanged;
	public event EventHandler<TreeColorChangedEventArgs> OnTreeColorChanged;
	public event EventHandler<TreeIdChangedEventArgs> OnTreeIdChanged;

	public string TreeId => _treeId;
	public Color TreeColor => _treeColor;


    private Dictionary<int, Type> _placementRuleTypes = new()
    {
        { 0, typeof(AboveSeaLevelRuleItem) },
        { 1, typeof(SlopeRuleItem) },
        { 2, typeof(NoiseMapRuleItem) }
    };

    private Dictionary<Type, PackedScene> _placementRuleScenes = new()
    {
        { typeof(AboveSeaLevelRuleItem), LoadedScenes.ABOVE_SEA_LEVEL_PLACEMENT_RULE_ITEM_SCENE },
        { typeof(SlopeRuleItem), LoadedScenes.SLOPE_PLACEMENT_RULE_ITEM_SCENE },
        { typeof(NoiseMapRuleItem), LoadedScenes.NOISE_MAP_PLACEMENT_RULE_ITEM_SCENE }
    };



    public override void _Ready()
	{
		_treeIdLineEdit = GetNode<LineEdit>("%TreeIdLineEdit");
		_treeColorPickerButton = GetNode<ColorPickerButton>("%TreeColorPickerButton");
		_addPlacementRuleButton = GetNode<MenuButton>("%AddPlaceRuleButton");
		_addRadiusRuleButton = GetNode<Button>("%AddRadiusRuleButton");
		_moveUpButton = GetNode<Button>("%MoveUpButton");
		_moveDownButton = GetNode<Button>("%MoveDownButton");
		_deleteButton = GetNode<Button>("%DeleteButton");
		_placeRulesVBoxContainer = GetNode<VBoxContainer>("%PlaceRulesVBoxContainer");
		_radiusRuleVBoxContainer = GetNode<VBoxContainer>("%RadiusRuleVBoxContainer");
		_noPlaceRulesLabel = GetNode<Label>("%NoPlaceRulesLabel");
		_noRadiusRuleLabel = GetNode<Label>("%NoRadiusRuleLabel");
        _modelOptionButton = GetNode<OptionButton>("%ModelOptionButton");

        _treeId = "Tree";
		_treeColor = Colors.Purple;
		_treeColorPickerButton.Color = _treeColor;
		_treeIdLineEdit.Text = _treeId;

		_treeColorPickerButton.ColorChanged += TreeColorPickerButtonOnColorChanged;
		_treeIdLineEdit.EditingToggled += TreeIdLineEditOnEditingToggled;
		_moveUpButton.Pressed += MoveUpButtonOnPressed;
		_moveDownButton.Pressed += MoveDownButtonOnPressed;
		_deleteButton.Pressed += DeleteButtonOnPressed;
        _addRadiusRuleButton.Pressed += OnAddRadiusRuleButtonPressed;

        // Populate model option button
        foreach (var (id, name) in TreesLoadedScene.GetTreeNames())
        {
            _modelOptionButton.AddItem(name, id);
        }
        _modelOptionButton.Selected = 0;

        // Populate placement rules popup menu
        _addPlacementRuleButton.AboutToPopup += _addPlacementRuleButton_AboutToPopup;
        var popup = _addPlacementRuleButton.GetPopup();
        popup.AddItem("Above sea rule", 0);
        popup.AddItem("Slope rule", 1);
        popup.AddItem("Noise map rule", 2);
        popup.IdPressed += PlacementRulesPopupMenuOnIdPressed;
    }

    private void _addPlacementRuleButton_AboutToPopup()
    {
        _logger.Log($"<{nameof(TreeId)}: {TreeId}> - POPUP ABOUT TO SHOW");

        var popup = _addPlacementRuleButton.GetPopup();
        var itemsCount = popup.ItemCount;

        for (int index = 0; index < itemsCount; index++)
        {
            var id = popup.GetItemId(index);
            var type = _placementRuleTypes[id];
            if (_placementRules.Any(x => x.GetType() == type))
            {
                popup.SetItemDisabled(index, true);
            }
            else
            {
                popup.SetItemDisabled(index, false);
            }

        }
    }

    private void PlacementRulesPopupMenuOnIdPressed(long id)
    {
        _logger.Log($"<{nameof(TreeId)}: {TreeId}> - PLACEMENT RULE ABOUT TO BE ADD...");

        var idInt = (int)id;
        var type = _placementRuleTypes[idInt];

        if (_placementRules.Any(x => x.GetType() == type))
        {
            _logger.LogError($"<{nameof(TreeId)}: {TreeId}> - PLACEMENT RULE OF TYPE {type} ALREADY EXISTS");
            return;
        }

        var scene = _placementRuleScenes[type];
        AddPlacementRule(scene);
    }


    private void AddPlacementRule(PackedScene placementRuleScene)
    {
        _logger.Log($"<{nameof(TreeId)}: {TreeId}> - ADDING PLACEMENT RULE...");

        var scene = placementRuleScene.Instantiate();

        var item = scene as IPlacementRuleItem;

        if (item == null)
        {
            var message = $"<{nameof(TreeId)}: {TreeId}>---> Can`t ADD PLACEMENT RULE, because it is not " +
                          $"of type {typeof(IPlacementRuleItem)}, " +
                          $"actual type: {scene.GetType()}";
            _logger.LogError(message);
            throw new Exception(message);
        }

        _placeRulesVBoxContainer.AddChild(scene);
        _placementRules.Add(item);

        SubscribePlacementRuleItem(item);
        _noPlaceRulesLabel.Visible = false;
        MarkAsDirty();
    }





    public TreePlacementRule GetTreePlacementRule()
    {
        _logger.LogMethodStart();

        if (_isDirty || _cachedRule == null)
        {
            _logger.Log("Tree placement rule is dirty or null, creating a new one");

            var rules = _placementRules.Select(x => x.GetPlacementRule()).ToList();
            var compositeRule = new CompositePlacementRule(rules);
            var radiusRule = _radiusRule?.GetRadiusRule();
            _cachedRule = new TreePlacementRule(_treeId, compositeRule, radiusRule);
            _isDirty = false;
        }

        _logger.LogMethodEnd();

        return _cachedRule;
    }

    public PackedScene GetModel()
    {
        var index = _modelOptionButton.Selected;
        var id = _modelOptionButton.GetItemId(index);
        return TreesLoadedScene.GetTreeScene(id);
    }

    private void TreeIdLineEditOnEditingToggled(bool toggledOn)
    {
        if (!toggledOn)
        {
            var newId = _treeIdLineEdit.Text;
            if (newId != _treeId)
            {
                _logger.Log($"<{nameof(TreeId)}: {TreeId}> - CHANGING ID to {newId}");
                var eventArgs = new TreeIdChangedEventArgs(_treeId, newId);
                _treeId = newId;
                OnTreeIdChanged?.Invoke(this, eventArgs);
                MarkAsDirty();
            }
        }
    }

    private void TreeColorPickerButtonOnColorChanged(Color newColor)
    {
        _treeColor = newColor;
        var eventArgs = new TreeColorChangedEventArgs(_treeId, newColor);
        _logger.Log($"<{nameof(TreeId)}: {TreeId}> - Changing color of the tree layer to {newColor}");
        OnTreeColorChanged?.Invoke(this, eventArgs);
    }

    private void MarkAsDirty()
    {
        _isDirty = true;
        _logger.Log($"<{nameof(TreeId)}: {TreeId}> - MARKED AS DIRTY!");
        OnRulesChanged?.Invoke(this, EventArgs.Empty);
    }


    private void OnAddRadiusRuleButtonPressed()
    {
        _logger.Log($"<{nameof(TreeId)}: {TreeId}> - ADDING RADIUS RULE...");

        if (_radiusRule != null)
        {
			return;
        }

		var scene = LoadedScenes.CONSTANT_RADIUS_RULE_ITEM_SCENE.Instantiate();
        var item = scene as IRadiusRuleItem;

		if (item == null)
        {
            var message = $"<{nameof(TreeId)}: {TreeId}>---> Can`t ADD RADIUS RULE, because it is not " +
                          $"of type {typeof(IRadiusRuleItem)}, " +
                          $"actual type: {scene.GetType()}";

            _logger.LogError(message);
            throw new Exception(message);
        }

        _radiusRuleVBoxContainer.AddChild(scene);
        _radiusRule = item;

        SubscribeRadiusRuleItem(item);
        
        _noRadiusRuleLabel.Visible = false;
        MarkAsDirty();
    }
    private void SubscribeRadiusRuleItem(IRadiusRuleItem radiusRuleItem)
    {
        radiusRuleItem.OnRuleParametersChanged += OnRadiusRuleItemRuleParametersChanged;
        radiusRuleItem.OnDeleteButtonPressed += OnRadiusRuleItemDeleteButtonPressed;
    }
    private void UnsubscribeRadiusRuleItem(IRadiusRuleItem radiusRuleItem)
    {
        radiusRuleItem.OnRuleParametersChanged -= OnRadiusRuleItemRuleParametersChanged;
        radiusRuleItem.OnDeleteButtonPressed -= OnRadiusRuleItemDeleteButtonPressed;
    }
    private void OnRadiusRuleItemRuleParametersChanged(object sender, EventArgs e)
    {
        _logger.Log($"<{nameof(TreeId)}: {TreeId}> - Some radius rule parameters changed, marking as dirty");
        MarkAsDirty();
    }
    private void OnRadiusRuleItemDeleteButtonPressed(object sender, EventArgs e)
    {
        _logger.Log($"<{nameof(TreeId)}: {TreeId}> - DELETING RADIUS RULE...");

        var scene = sender as Node;
        var item = sender as IRadiusRuleItem;

        if (item == null || scene == null)
        {
            var message = $"<{nameof(TreeId)}: {TreeId}>---> Can`t DELETE RADIUS RULE, because it is not " +
                          $"of types {typeof(IRadiusRuleItem)} and {typeof(Node)}, " +
                          $"actual type: {sender.GetType()}";

            _logger.LogError(message);
            throw new Exception(message);
        }
        _radiusRuleVBoxContainer.RemoveChild(scene);
        _radiusRule = null;

        UnsubscribeRadiusRuleItem(item);

        scene.QueueFree();

        if (_radiusRule == null)
        {
            _noRadiusRuleLabel.Visible = true;
        }

        MarkAsDirty();
    }


    private void SubscribePlacementRuleItem(IPlacementRuleItem item)
    {
        item.OnRuleParametersChanged += PlacementRuleItemOnRuleParametersChanged;
        item.OnDeleteButtonPressed += PlacementRuleItemOnDeleteButtonPressed;
    }
    private void UnsubscribePlacementRuleItem(IPlacementRuleItem item)
    {
        item.OnRuleParametersChanged -= PlacementRuleItemOnRuleParametersChanged;
        item.OnDeleteButtonPressed -= PlacementRuleItemOnDeleteButtonPressed;
    }
    private void PlacementRuleItemOnDeleteButtonPressed(object sender, EventArgs e)
	{
        _logger.Log($"<{nameof(TreeId)}: {TreeId}> - DELETING PLACEMENT RULE...");

        var scene = sender as Node;
		var item = sender as IPlacementRuleItem;

		if (item == null || scene == null)
        {
            var message = $"<{nameof(TreeId)}: {TreeId}>---> Can`t DELETE PLACEMENT RULE, because it is not " +
                          $"of types {typeof(IPlacementRuleItem)} and {typeof(Node)}, " +
                          $"actual type: {sender.GetType()}";
            _logger.LogError(message);
            throw new Exception(message);
		}

		_placeRulesVBoxContainer.RemoveChild(scene);
		_placementRules.Remove(item);

        UnsubscribePlacementRuleItem(item);
		scene.QueueFree();

		if (_placementRules.Count == 0)
		{
			_noPlaceRulesLabel.Visible = true;
		}

		MarkAsDirty();
	}
    private void PlacementRuleItemOnRuleParametersChanged(object sender, EventArgs e)
    {
        _logger.Log($"<{nameof(TreeId)}: {TreeId}> - SOME PLACEMENT RULE CHANGED, MARKING AS DIRTY");
        MarkAsDirty();
    }

    private void DeleteButtonOnPressed()
	{
        _logger.Log($"<{nameof(TreeId)}: {TreeId}> - DELETE BUTTON PRESSED");
		OnDeleteButtonPressed?.Invoke(this, EventArgs.Empty);
	}
	private void MoveDownButtonOnPressed()
	{
        _logger.Log($"<{nameof(TreeId)}: {TreeId}> - MOVE DOWN BUTTON PRESSED");
        OnMoveDownButtonPressed?.Invoke(this, EventArgs.Empty);
	}
	private void MoveUpButtonOnPressed()
	{
        _logger.Log($"<{nameof(TreeId)}: {TreeId}> - MOVE UP BUTTON PRESSED");
        OnMoveUpButtonPressed?.Invoke(this, EventArgs.Empty);
	}
}
