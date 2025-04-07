using TerrainGenerationApp.Domain.Rules.PlacementRules;
using TerrainGenerationApp.Scenes.BuildingBlocks.Attributes;
using TerrainGenerationApp.Scenes.BuildingBlocks.InputLine;

namespace TerrainGenerationApp.Scenes.FeatureOptions.TreePlacement.PlacementRuleItems;

public partial class SlopeRuleItem : BasePlacementRuleItem<SlopeRuleItem>
{
    private float _lowerBound = 0.01f;
    private float _upperBound = 0.15f;

    [InputLine(Description = "Lower bound:")]
    [InputLineSlider(0.0f, 1.0f, 0.001f)]
    [InputLineTextFormat("0.###")]
    public float LowerBound
    {
        get => _lowerBound;
        set
        {
            _lowerBound = value;
            Logger.Log($"Lower bound changed to: {_lowerBound}");
            InvokeRuleParametersChangedEvent();
        }
    }

    [InputLine(Description = "Upper bound:")]
    [InputLineSlider(0.0f, 1.0f, 0.001f)]
    [InputLineTextFormat("0.###")]
    public float UpperBound
    {
        get => _upperBound;
        set
        {
            _upperBound = value;
            Logger.Log($"Upper bound changed to: {_upperBound}");
            InvokeRuleParametersChangedEvent();
        }
    }

    public override void _Ready()
    {
        base._Ready();
        InputLineManager.CreateInputLinesForObject(this, OptionsContainer);
    }

    public override IPlacementRule GetPlacementRule()
    {
        return new SlopeRule(_lowerBound, _upperBound);
    }
}