using System;
using TerrainGenerationApp.Domain.Rules.PlacementRules;

namespace TerrainGenerationApp.Scenes.GenerationOptions.TreePlacement.PlacementRuleItems;

public interface IPlacementRuleItem
{
	public event EventHandler OnRuleParametersChanged;
	public event EventHandler OnDeleteButtonPressed;

	public IPlacementRule GetPlacementRule();
}
