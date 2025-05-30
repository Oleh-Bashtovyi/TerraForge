using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using TerrainGenerationApp.Domain.Rules.PlacementRules;
using TerrainGenerationApp.Domain.Rules.TreeRules;
using TerrainGenerationApp.Domain.Utils;
using TerrainGenerationApp.Scenes.BuildingBlocks.Attributes;
using TerrainGenerationApp.Scenes.BuildingBlocks.Containers;
using TerrainGenerationApp.Scenes.BuildingBlocks.InputLine;
using TerrainGenerationApp.Scenes.FeatureOptions.TreePlacement.PlacementRuleItems;
using TerrainGenerationApp.Scenes.FeatureOptions.TreePlacement.RadiusRuleItems;
using TerrainGenerationApp.Scenes.LoadedScenes;

namespace TerrainGenerationApp.Scenes.FeatureOptions.TreePlacement;

public class TreeLayerColorChangedEventArgs(string layerId, Color newColor)
{
	public Color NewColor { get; } = newColor;
	public string LayerId { get; } = layerId;
}

public partial class TreePlacementRuleItem : PanelContainer, IOptionsToggleable, ILastUsedConfigProvider
{
    private const string LAYER_NAME_TOOLTIP = "This name is used to identify layer by placement or radius rules.\n" +
                                              "Layer name is not reuqired in general";

    private const string OVERWRITE_LAYERS_TOOLTIP =
        "If true, this layer tree will overwrite all trees that was placed by previous layers.";

    private ColorPickerButton _treeColorPickerButton;
    private OptionsContainer _optionsContainer;
    private VBoxContainer _placeRulesVBoxContainer;
	private VBoxContainer _radiusRuleVBoxContainer;
	private MenuButton _addPlacementRuleButton;
	private Button _addRadiusRuleButton;
	private Button _moveUpButton;
	private Button _moveDownButton;
	private Button _deleteButton;
	private Label _noPlaceRulesLabel;
	private Label _noRadiusRuleLabel;

	private readonly List<IPlacementRuleItem> _placementRules = new();
    private readonly Logger<TreePlacementRuleItem> _logger = new();
    private bool _overwriteLayers;
	private bool _isDirty = true; 
	private string _layerId;
    private string _layerName;
    private int _selected3DModelItem;
	private Color _treeColor;
	private TreePlacementRule _cachedRule; 
	private IRadiusRuleItem _radiusRule;

	public event EventHandler OnRulesChanged;
    public event EventHandler OnDeleteButtonPressed;
	public event EventHandler OnMoveUpButtonPressed;
	public event EventHandler OnMoveDownButtonPressed;
	public event EventHandler<TreeLayerColorChangedEventArgs> OnTreeColorChanged;

    private bool IsLoading { get; set; }

	public Color TreeColor => _treeColor;

    public string LayerId => _layerId;

    [InputLine(Description = "Tree layer name:", Tooltip = LAYER_NAME_TOOLTIP)]
    [InputLineText(maxLength: 25)]
    public string LayerName
    {
        get => _layerName;
        set
        {
            _logger.Log($"<{nameof(LayerId)}: {LayerId}> - CHANGING NAME to {value}");
            _layerName = value;
            MarkAsDirty();
        }
    }

    [InputLine(Description = "3D model:", Id = "3DModelSelection")]
    [InputLineCombobox(bind: ComboboxBind.Id)]
    public int Selected3DModelItem
    {
        get => _selected3DModelItem;
        set => _selected3DModelItem = value;
    }

    [InputLine(Description = "Overwrite previous layers:", Tooltip = OVERWRITE_LAYERS_TOOLTIP)]
    [InputLineCheckBox(checkboxType: CheckboxType.CheckButton)]
    public bool OverwriteLayers
    {
        get => _overwriteLayers;
        set
        {
            _overwriteLayers = value;
            MarkAsDirty();
        }
    }

    private Dictionary<int, Type> _placementRuleTypes = new()
    {
        { 0, typeof(AboveSeaLevelRuleItem) },
        { 1, typeof(SlopeRuleItem) },
        { 2, typeof(NoiseMapRuleItem) },
        { 3, typeof(NoTreeLayersInRadiusRuleItem)},
        { 4, typeof(MoistureRuleItem)},
        { 5, typeof(WaterInRadiusRuleItem) }
    };

    private Dictionary<Type, PackedScene> _placementRuleScenes = new()
    {
        { typeof(AboveSeaLevelRuleItem), TreePlacementRuleLoadedScenes.ABOVE_SEA_LEVEL_PLACEMENT_RULE_ITEM_SCENE },
        { typeof(SlopeRuleItem), TreePlacementRuleLoadedScenes.SLOPE_PLACEMENT_RULE_ITEM_SCENE },
        { typeof(NoiseMapRuleItem), TreePlacementRuleLoadedScenes.NOISE_MAP_PLACEMENT_RULE_ITEM_SCENE },
        { typeof(NoTreeLayersInRadiusRuleItem), TreePlacementRuleLoadedScenes.NO_TREE_LAYER_IN_RADIUS_RULE_ITEM_SCENE},
        { typeof(MoistureRuleItem), TreePlacementRuleLoadedScenes.MOISTURE_RULE_ITEM_SCENE },
        { typeof(WaterInRadiusRuleItem), TreePlacementRuleLoadedScenes.WATER_IN_RADIUS_RULE_ITEM_SCENE }
    };

    public override void _Ready()
	{
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
        _optionsContainer = GetNode<OptionsContainer>("%OptionsContainer");
        InputLineManager.CreateInputLinesForObject(this, _optionsContainer);

        var modelsCombobox = _optionsContainer.FindInputLine<InputLineCombobox>("3DModelSelection");
        modelsCombobox!.AddOptions(TreeModelLoadedScenes.GetTreeNames());

		_treeColor = Colors.Purple;
		_treeColorPickerButton.Color = _treeColor;

		_treeColorPickerButton.ColorChanged += TreeColorPickerButtonOnColorChanged;
		_moveUpButton.Pressed += MoveUpButtonOnPressed;
		_moveDownButton.Pressed += MoveDownButtonOnPressed;
		_deleteButton.Pressed += DeleteButtonOnPressed;
        _addRadiusRuleButton.Pressed += AddRadiusRuleOnButtonPressed;

        // Populate placement rules popup menu
        _addPlacementRuleButton.AboutToPopup += AddPlacementRuleButtonOnAboutToPopup;
        var popup = _addPlacementRuleButton.GetPopup();
        popup.AddItem("Above sea rule", 0);
        popup.AddItem("Slope rule", 1);
        popup.AddItem("Noise map rule", 2);
        popup.AddItem("No tree layers in radius rule", 3);
        popup.AddItem("Moisture rule", 4);
        popup.AddItem("Water in radius rule", 5);
        popup.IdPressed += PlacementRulesPopupMenuOnIdPressed;
    }

    public TreePlacementRule GetTreePlacementRule()
    {
        if (_isDirty || _cachedRule == null)
        {
            _logger.Log("Tree placement rule is dirty or null, creating a new one");

            var rules = _placementRules.Select(x => x.GetPlacementRule()).ToList();
            var compositeRule = new CompositePlacementRule(rules);
            var radiusRule = _radiusRule?.GetRadiusRule();
            _cachedRule = new TreePlacementRule(LayerId, LayerName, compositeRule, radiusRule, OverwriteLayers);
            _isDirty = false;
        }

        return _cachedRule;
    }
    
    public PackedScene GetModel()
    {
        return TreeModelLoadedScenes.GetTreeScene(Selected3DModelItem);
    }

    public void EnableOptions()
    {
        _treeColorPickerButton.Disabled = false;
        _addPlacementRuleButton.Disabled = false;
        _addRadiusRuleButton.Disabled = false;
        _deleteButton.Disabled = false;
        _moveUpButton.Disabled = false;
        _moveDownButton.Disabled = false;
        _optionsContainer.EnableOptions();
        _radiusRule?.EnableOptions();
        _placementRules.ForEach(x => x.EnableOptions());
    }
    
    public void DisableOptions()
    {
        _treeColorPickerButton.Disabled = true;
        _addPlacementRuleButton.Disabled = true;
        _addRadiusRuleButton.Disabled = true;
        _deleteButton.Disabled = true;
        _moveUpButton.Disabled = true;
        _moveDownButton.Disabled = true;
        _optionsContainer.DisableOptions();
        _placementRules.ForEach(x => x.DisableOptions());
        _radiusRule?.DisableOptions();
    }

    public Dictionary<string, object> GetLastUsedConfig()
    {
        var result = new Dictionary<string, object>();
        var placementRules = new List<Dictionary<string, object>>();

        foreach (var item in _placementRules)
        {
            var itemConfig = item.GetLastUsedConfig();
            itemConfig["ItemTypeId"] = _placementRuleTypes.First(x => x.Value == item.GetType()).Key;
            placementRules.Add(itemConfig);
        }
        result["PlacementRules"] = placementRules;

        if (_radiusRule != null)
        {
            result["RadiusRuleType"] = "ConstantRule";
            result["RadiusRule"] = _radiusRule.GetLastUsedConfig();
        }

        var options = _optionsContainer.GetLastUsedConfig();
        result["Options"] = options;

        result["LayerId"] = LayerId;
        result["LayerColor"] = new[] { _treeColor.R, _treeColor.G, _treeColor.B };

        return result;
    }

    public void UpdateCurrentConfigAsLastUsed()
    {
        _optionsContainer.UpdateCurrentConfigAsLastUsed();
        _radiusRule?.UpdateCurrentConfigAsLastUsed();
        _placementRules.ForEach(x => x.UpdateCurrentConfigAsLastUsed());
    }

    public void LoadConfigFrom(Dictionary<string, object> config)
    {
        try
        {
            IsLoading = true;

            foreach (var child in _radiusRuleVBoxContainer.GetChildren())
            {
                if (child is IRadiusRuleItem)
                {
                    RemoveRadiusRule(child);
                }
            }

            foreach (var child in _placeRulesVBoxContainer.GetChildren())
            {
                if (child is IPlacementRuleItem)
                {
                    RemovePlacementRule(child);
                }
            }

            if (config.GetValueOrDefault("LayerId") is string layerId)
                SetId(layerId);
            if (config.GetValueOrDefault("LayerColor") is IEnumerable<object> layerColorObject)
            {
                var colorObject = layerColorObject as object[] ?? layerColorObject.ToArray();
                if (colorObject.Count() == 3)
                {
                    var list = colorObject.ToList();

                    if (float.TryParse(list[0]?.ToString(), out float r) &&
                        float.TryParse(list[1]?.ToString(), out float g) &&
                        float.TryParse(list[2]?.ToString(), out float b))
                    {
                        _treeColor = new Color(r, g, b);
                        _treeColorPickerButton.Color = _treeColor;
                    }
                }
            }

            if (config.GetValueOrDefault("Options") is Dictionary<string, object> optionsConfig)
                _optionsContainer.LoadConfigFrom(optionsConfig);
            if (config.GetValueOrDefault("RadiusRuleType") is string radiusRuleType)
            {
                AddRadiusRule(TreePlacementRuleLoadedScenes.CONSTANT_RADIUS_RULE_ITEM_SCENE);

                if (config.GetValueOrDefault("RadiusRule") is Dictionary<string, object> radiusRuleConfig)
                {
                    _radiusRule.LoadConfigFrom(radiusRuleConfig);
                }
            }

            if (config.GetValueOrDefault("PlacementRules") is List<object> placementRulesConfig)
            {
                foreach (var placementRuleConfig in placementRulesConfig)
                {
                    if (placementRuleConfig is not Dictionary<string, object> placementConfig)
                    {
                        continue;
                    }

                    if (placementConfig.GetValueOrDefault("ItemTypeId") is not int ItemTypeId)
                    {
                        continue;
                    }

                    var sceneType = _placementRuleTypes.GetValueOrDefault(ItemTypeId);

                    if (sceneType == null)
                    {
                        continue;
                    }

                    var scene = _placementRuleScenes[sceneType];
                    var addedItem = AddPlacementRule(scene);
                    addedItem.LoadConfigFrom(placementConfig);
                }
            }

        }
        finally
        {
            IsLoading = false;
        }
    }

    public void SetId(string id)
    {
        _layerId = id;
    }



    private void AddPlacementRuleButtonOnAboutToPopup()
    {
        _logger.Log($"<{nameof(LayerId)}: {LayerId}> - POPUP ABOUT TO SHOW");

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
        _logger.Log($"<{nameof(LayerId)}: {LayerId}> - PLACEMENT RULE ABOUT TO BE ADD...");

        var idInt = (int)id;
        var type = _placementRuleTypes[idInt];

        if (_placementRules.Any(x => x.GetType() == type))
        {
            _logger.LogError($"<{nameof(LayerId)}: {LayerId}> - PLACEMENT RULE OF TYPE {type} ALREADY EXISTS");
            return;
        }

        var scene = _placementRuleScenes[type];
        AddPlacementRule(scene);
    }
    private void TreeColorPickerButtonOnColorChanged(Color newColor)
    {
        _treeColor = newColor;
        var eventArgs = new TreeLayerColorChangedEventArgs(LayerId, newColor);
        _logger.Log($"<{nameof(LayerId)}: {LayerId}> - Changing color of the tree layer to {newColor}");
        OnTreeColorChanged?.Invoke(this, eventArgs);
    }


    private IPlacementRuleItem AddPlacementRule(PackedScene placementRuleScene)
    {
        _logger.Log($"<{nameof(LayerId)}: {LayerId}> - ADDING PLACEMENT RULE...");

        var scene = placementRuleScene.Instantiate();
        var item = scene as IPlacementRuleItem;

        if (item == null)
        {
            var message = $"<{nameof(LayerId)}: {LayerId}>---> Can`t ADD PLACEMENT RULE, because it is not " +
                          $"of type {typeof(IPlacementRuleItem)}, " +
                          $"actual type: {scene.GetType()}";
            _logger.LogError(message);
            throw new Exception(message);
        }

        _noPlaceRulesLabel.Visible = false;
        _placeRulesVBoxContainer.AddChild(scene);
        _placementRules.Add(item);
        SubscribePlacementRuleItem(item);
        MarkAsDirty();
        return item;
    }
    private void RemovePlacementRule(Node node)
    {
        _logger.Log($"<{nameof(LayerId)}: {LayerId}> - DELETING PLACEMENT RULE...");

        var item = node as IPlacementRuleItem;

        if (item == null || node == null)
        {
            var message = $"<{nameof(LayerId)}: {LayerId}>---> Can`t DELETE PLACEMENT RULE, because it is not " +
                          $"of type {typeof(IPlacementRuleItem)}, " +
                          $"actual type: {node.GetType()}";
            _logger.LogError(message);
            throw new Exception(message);
        }

        _placeRulesVBoxContainer.RemoveChild(node);
        _placementRules.Remove(item);

        UnsubscribePlacementRuleItem(item);
        node.QueueFree();

        if (_placementRules.Count == 0)
        {
            _noPlaceRulesLabel.Visible = true;
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
        RemovePlacementRule(sender as Node);
    }
    private void PlacementRuleItemOnRuleParametersChanged(object sender, EventArgs e)
    {
        _logger.Log($"<{nameof(LayerId)}: {LayerId}> - SOME PLACEMENT RULE CHANGED, MARKING AS DIRTY");
        MarkAsDirty();
    }

    private void AddRadiusRule(PackedScene radiusRuleScene)
    {
        _logger.Log($"<{nameof(LayerId)}: {LayerId}> - ADDING RADIUS RULE...");

        if (_radiusRule != null)
        {
            return;
        }

        var scene = radiusRuleScene.Instantiate();
        var item = scene as IRadiusRuleItem;

        if (item == null)
        {
            var message = $"<{nameof(LayerId)}: {LayerId}>---> Can`t ADD RADIUS RULE, because it is not " +
                          $"of type {typeof(IRadiusRuleItem)}, " +
                          $"actual type: {scene.GetType()}";
            _logger.LogError(message);
            throw new Exception(message);
        }

        _noRadiusRuleLabel.Visible = false;
        _radiusRuleVBoxContainer.AddChild(scene);
        _radiusRule = item;
        SubscribeRadiusRuleItem(item);
        MarkAsDirty();
    }
    private void RemoveRadiusRule(Node node)
    {
        _logger.Log($"<{nameof(LayerId)}: {LayerId}> - DELETING RADIUS RULE...");

        var item = node as IRadiusRuleItem;

        if (item == null || node == null)
        {
            var message = $"<{nameof(LayerId)}: {LayerId}>---> Can`t DELETE RADIUS RULE, because it is not " +
                          $"of types {typeof(IRadiusRuleItem)}, " +
                          $"actual type: {node.GetType()}";

            _logger.LogError(message);
            throw new Exception(message);
        }
        _radiusRuleVBoxContainer.RemoveChild(node);
        _radiusRule = null;

        UnsubscribeRadiusRuleItem(item);
        node.QueueFree();

        if (_radiusRule == null)
        {
            _noRadiusRuleLabel.Visible = true;
        }

        MarkAsDirty();
    }
    private void AddRadiusRuleOnButtonPressed()
    {
        AddRadiusRule(TreePlacementRuleLoadedScenes.CONSTANT_RADIUS_RULE_ITEM_SCENE);
    }
    private void SubscribeRadiusRuleItem(IRadiusRuleItem radiusRuleItem)
    {
        radiusRuleItem.OnRuleParametersChanged += RadiusRuleItemOnRuleParametersChanged;
        radiusRuleItem.OnDeleteButtonPressed += RadiusRuleItemOnDeleteButtonPressed;
    }
    private void UnsubscribeRadiusRuleItem(IRadiusRuleItem radiusRuleItem)
    {
        radiusRuleItem.OnRuleParametersChanged -= RadiusRuleItemOnRuleParametersChanged;
        radiusRuleItem.OnDeleteButtonPressed -= RadiusRuleItemOnDeleteButtonPressed;
    }
    private void RadiusRuleItemOnRuleParametersChanged(object sender, EventArgs e)
    {
        _logger.Log($"<{nameof(LayerId)}: {LayerId}> - Some radius rule parameters changed, marking as dirty");
        MarkAsDirty();
    }
    private void RadiusRuleItemOnDeleteButtonPressed(object sender, EventArgs e)
    {
        RemoveRadiusRule(sender as Node);
    }

    private void DeleteButtonOnPressed()
	{
        _logger.Log($"<{nameof(LayerId)}: {LayerId}> - DELETE BUTTON PRESSED");
		OnDeleteButtonPressed?.Invoke(this, EventArgs.Empty);
	}
	private void MoveDownButtonOnPressed()
	{
        _logger.Log($"<{nameof(LayerId)}: {LayerId}> - MOVE DOWN BUTTON PRESSED");
        OnMoveDownButtonPressed?.Invoke(this, EventArgs.Empty);
	}
	private void MoveUpButtonOnPressed()
	{
        _logger.Log($"<{nameof(LayerId)}: {LayerId}> - MOVE UP BUTTON PRESSED");
        OnMoveUpButtonPressed?.Invoke(this, EventArgs.Empty);
	}
    private void MarkAsDirty()
    {
        _isDirty = true;
        _logger.Log($"<{nameof(LayerId)}: {LayerId}> - MARKED AS DIRTY!");

        if (!IsLoading)
        {
            OnRulesChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
