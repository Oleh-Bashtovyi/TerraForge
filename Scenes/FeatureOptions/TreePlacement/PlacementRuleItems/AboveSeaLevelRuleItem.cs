using TerrainGenerationApp.Domain.Rules.PlacementRules;
using TerrainGenerationApp.Scenes.BuildingBlocks.Attributes;
using TerrainGenerationApp.Scenes.BuildingBlocks.InputLine;

namespace TerrainGenerationApp.Scenes.FeatureOptions.TreePlacement.PlacementRuleItems;

public partial class AboveSeaLevelRuleItem : BasePlacementRuleItem<AboveSeaLevelRuleItem>
{
    private InputLineSlider _lowerBoundInput;
    private InputLineSlider _upperBoundInput;
    private float _lowerBound = 0.1f;
    private float _upperBound = 0.2f;

    [InputLine(Description = "Lower bound:")]
    [InputLineSlider(0.0f, 1.0f, 0.001f, format: "0.###")]
    public float LowerBound
    {
        get => _lowerBound;
        set
        {
            _lowerBound = value;
            Logger.Log($"Lower bound changed to: {_lowerBound}");
            if (_lowerBound > _upperBound)
            {
                _upperBound = _lowerBound;
                _upperBoundInput?.SetValue(_upperBound, invokeEvent:false);
            }
            InvokeRuleParametersChangedEvent();
        }
    }

    [InputLine(Description = "Upper bound:")]
    [InputLineSlider(0.0f, 1.0f, 0.001f, format: "0.###")]
    public float UpperBound
    {
        get => _upperBound;
        set
        {
            _upperBound = value;
            Logger.Log($"Upper bound changed to: {_upperBound}");
            if (_upperBound < _lowerBound)
            {
                _lowerBound = _upperBound;
                _lowerBoundInput?.SetValue(_lowerBound, invokeEvent: false);
            }
            InvokeRuleParametersChangedEvent();
        }
    }

    public override void _Ready()
    {
        base._Ready(); 
        InputLineManager.CreateInputLinesForObject(this, OptionsContainer);
        _lowerBoundInput = OptionsContainer.FindInputLine<InputLineSlider>(nameof(LowerBound));
        _upperBoundInput = OptionsContainer.FindInputLine<InputLineSlider>(nameof(UpperBound));
    }

    public override IPlacementRule GetPlacementRule()
    {
        return new AboveSeaLevelRule(_lowerBound, _upperBound);
    }
}