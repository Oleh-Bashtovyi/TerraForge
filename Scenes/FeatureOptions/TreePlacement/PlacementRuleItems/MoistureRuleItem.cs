using TerrainGenerationApp.Domain.Rules.PlacementRules;
using TerrainGenerationApp.Scenes.BuildingBlocks.Attributes;
using TerrainGenerationApp.Scenes.BuildingBlocks.InputLine;

namespace TerrainGenerationApp.Scenes.FeatureOptions.TreePlacement.PlacementRuleItems;

public partial class MoistureRuleItem : BasePlacementRuleItem<MoistureRuleItem>
{
	private InputLineSlider _minMoistureInput;
	private InputLineSlider _maxMoistureInput;
	private float _minMoisture = 0.2f;
	private float _maxMoisture = 0.8f;

	[InputLine(Description = "Min moisture:")]
	[InputLineSlider(0.0f, 1.0f, 0.01f, format: "0.##")]
	public float MinMoisture
	{
		get => _minMoisture;
		set
		{
			_minMoisture = value;
			Logger.Log($"Min moisture changed to: {_minMoisture}");
			if (_minMoisture > _maxMoisture)
			{
				_maxMoisture = _minMoisture;
				_maxMoistureInput?.SetValue(_maxMoisture, invokeEvent: false);
			}
			InvokeRuleParametersChangedEvent();
		}
	}

	[InputLine(Description = "Max moisture:")]
	[InputLineSlider(0.0f, 1.0f, 0.01f, format: "0.##")]
	public float MaxMoisture
	{
		get => _maxMoisture;
		set
		{
			_maxMoisture = value;
			Logger.Log($"Max moisture changed to: {_maxMoisture}");
			if (_maxMoisture < _minMoisture)
			{
				_minMoisture = _maxMoisture;
				_minMoistureInput?.SetValue(_minMoisture, invokeEvent: false);
			}
			InvokeRuleParametersChangedEvent();
		}
	}

	public override void _Ready()
	{
		base._Ready();
		InputLineManager.CreateInputLinesForObject(this, OptionsContainer);
		_minMoistureInput = OptionsContainer.FindInputLine<InputLineSlider>(nameof(MinMoisture));
		_maxMoistureInput = OptionsContainer.FindInputLine<InputLineSlider>(nameof(MaxMoisture));
	}

	public override IPlacementRule GetPlacementRule()
	{
		return new MoistureRule(_minMoisture, _maxMoisture);
	}
}
