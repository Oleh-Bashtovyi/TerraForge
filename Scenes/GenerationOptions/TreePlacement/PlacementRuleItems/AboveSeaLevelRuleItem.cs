using TerrainGenerationApp.Domain.Rules.PlacementRules;
using TerrainGenerationApp.Scenes.BuildingBlocks;
using TerrainGenerationApp.Scenes.BuildingBlocks.Attributes;

namespace TerrainGenerationApp.Scenes.GenerationOptions.TreePlacement.PlacementRuleItems;

public partial class AboveSeaLevelRuleItem : BasePlacementRuleItem<AboveSeaLevelRuleItem>
{
    private float _lowerBound = 0.1f;
    private float _upperBound = 0.2f;

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
        return new AboveSeaLevelRule(_lowerBound, _upperBound);
    }
}