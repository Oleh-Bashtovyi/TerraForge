using TerrainGenerationApp.Domain.Rules.RadiusRules;
using TerrainGenerationApp.Scenes.BuildingBlocks.Attributes;
using TerrainGenerationApp.Scenes.BuildingBlocks.InputLine;

namespace TerrainGenerationApp.Scenes.FeatureOptions.TreePlacement.RadiusRuleItems;

public partial class ConstantRadiusRuleItem : BaseRadiusRuleItem<ConstantRadiusRuleItem>
{
    private float _radius = 3.0f;

    [InputLine(Description = "Radius:")]
    [InputLineSlider(0.0f, 50.0f, 0.1f, format: "0.#")]
    public float Radius
    {
        get => _radius;
        set
        {
            _radius = value;
            Logger.Log($"Radius changed to: {_radius}");
            InvokeRuleParametersChangedEvent();
        }
    }

    public override IRadiusRule GetRadiusRule()
    {
        return new ConstantRadiusRule(_radius);
    }

    public override void _Ready()
    {
        base._Ready();
        InputLineManager.CreateInputLinesForObject(this, OptionsContainer);
    }
}