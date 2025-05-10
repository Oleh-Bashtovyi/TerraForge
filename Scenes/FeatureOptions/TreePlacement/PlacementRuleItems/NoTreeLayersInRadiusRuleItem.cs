using Godot;
using System.Collections.Generic;
using System.Linq;
using TerrainGenerationApp.Domain.Rules.PlacementRules;
using TerrainGenerationApp.Scenes.BuildingBlocks.Attributes;
using TerrainGenerationApp.Scenes.BuildingBlocks.Containers;
using TerrainGenerationApp.Scenes.BuildingBlocks.InputLine;
using TerrainGenerationApp.Scenes.LoadedScenes;

namespace TerrainGenerationApp.Scenes.FeatureOptions.TreePlacement.PlacementRuleItems;

public partial class NoTreeLayersInRadiusRuleItem : BasePlacementRuleItem<NoTreeLayersInRadiusRuleItem>
{
    private readonly List<InputLineText> _inputLines = [];
    private OptionsContainer _layerNamesContainer;
    private Button _addButton;
    private Button _clearButton;
    private float _radius = 3.0f;

    protected OptionsContainer LayerNamesContainer
    {
        get
        {
            _layerNamesContainer ??= GetNode<OptionsContainer>("%LayerNamesContainer");
            return _layerNamesContainer;
        }
    }

    protected Button AddButton
    {
        get
        {
            _addButton ??= GetNode<Button>("%AddButton");
            return _addButton;
        }
    }

    protected Button ClearButton
    {
        get
        {
            _clearButton ??= GetNode<Button>("%ClearButton");
            return _clearButton;
        }
    }

    [InputLine(Description = "Radius:")]
    [InputLineSlider(0.0f, 5.0f, 0.1f, format: "0.#")]
    public float Radius
    {
        get => _radius;
        set
        {
            _radius = value;
            InvokeRuleParametersChangedEvent();
        }
    }

    public override void _Ready()
    {
        base._Ready();
        InputLineManager.CreateInputLinesForObject(this, OptionsContainer);
        AddButton.Pressed += AddButton_Pressed;
        ClearButton.Pressed += ClearButton_Pressed;
    }

    public override IPlacementRule GetPlacementRule()
    {
        var layerNames = _inputLines.Where(x => !string.IsNullOrWhiteSpace(x.CurValue)).Select(x => x.CurValue).ToList();
        var rule = new NoTreeLayersInRadiusRule(layerNames, _radius);
        return rule;
    }

    private void ClearButton_Pressed()
    {
        _inputLines.ForEach(x => x.QueueFree());
        _inputLines.Clear();
        InvokeRuleParametersChangedEvent();
    }

    private void AddButton_Pressed()
    {
        AddLayerName();
    }

    private void AddLayerName(string? initialLayerName = null)
    {
        if (_inputLines.Any(x => string.IsNullOrWhiteSpace(x.CurValue)))
            return;

        var inputLine = BuildingBlockLoadedScenes.INPUT_LINE_TEXT.Instantiate<InputLineText>();
        inputLine.SetId("LayerName_1");
        inputLine.SetText(initialLayerName ?? string.Empty);
        inputLine.SetTextLength(15);
        inputLine.SetDescription($"Layer name {_inputLines.Count + 1}:");
        inputLine.OnTextChanged += InputLine_OnTextChanged;
        _inputLines.Add(inputLine);
        LayerNamesContainer.AddChild(inputLine);
    }

    public override void EnableOptions()
    {
        base.EnableOptions();
        LayerNamesContainer.EnableOptions();
        AddButton.Disabled = false;
        ClearButton.Disabled = false;
    }

    public override void DisableOptions()
    {
        base.DisableOptions();
        LayerNamesContainer.DisableOptions();
        AddButton.Disabled = true;
        ClearButton.Disabled = true;
    }

    private void InputLine_OnTextChanged(string obj)
    {
        InvokeRuleParametersChangedEvent();
    }

    public override void UpdateCurrentConfigAsLastUsed()
    {
        base.UpdateCurrentConfigAsLastUsed();
    }

    public override Dictionary<string, object> GetLastUsedConfig()
    {
        var config = base.GetLastUsedConfig();
        config["Layers"] = _inputLines
            .Where(x => !string.IsNullOrWhiteSpace(x.CurValue))
            .Select(x => x.CurValue)
            .ToList();
        return config;
    }

    protected override void LoadConfigInternal(Dictionary<string, object> config)
    {
        base.LoadConfigInternal(config);

        _inputLines.ForEach(x => x.QueueFree());

        var layersObject = config.GetValueOrDefault("Layers");

        if (layersObject is IEnumerable<object> layerNamesObjects)
        {
            foreach (var item in layerNamesObjects)
            {
                if (item is string layerName)
                    AddLayerName(layerName);
            }
        }
        else if (layersObject is string layerName)
        {
            AddLayerName(layerName);
        }
    }
}