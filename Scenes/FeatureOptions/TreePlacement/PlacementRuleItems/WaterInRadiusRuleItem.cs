using TerrainGenerationApp.Domain.Rules.PlacementRules;
using TerrainGenerationApp.Scenes.BuildingBlocks.Attributes;
using TerrainGenerationApp.Scenes.BuildingBlocks.InputLine;

namespace TerrainGenerationApp.Scenes.FeatureOptions.TreePlacement.PlacementRuleItems;

public partial class WaterInRadiusRuleItem : BasePlacementRuleItem<WaterInRadiusRuleItem>
{
	private float _radius = 5.0f;

	[InputLine(Description = "Search radius:")]
	[InputLineSlider(0.0f, 20.0f, 0.5f, format: "0.#")]
	public float Radius
	{
		get => _radius;
		set
		{
			_radius = value;
			Logger.Log($"Water search radius changed to: {_radius}");
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
		return new WaterInRadius(_radius);
	}
}
