using System;
using TerrainGenerationApp.PlacementRules;

namespace TerrainGenerationApp.Scenes.GenerationOptions.TreePlacementOptions.PlacementRuleItems;

public interface IPlacementRuleItem
{
	public event EventHandler OnRuleParametersChanged;
	public event EventHandler OnDeleteButtonPressed;

	public IPlacementRule GetPlacementRule();
}
