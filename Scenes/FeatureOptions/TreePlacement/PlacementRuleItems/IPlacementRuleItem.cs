using System;
using TerrainGenerationApp.Domain.Rules.PlacementRules;

namespace TerrainGenerationApp.Scenes.FeatureOptions.TreePlacement.PlacementRuleItems;

public interface IPlacementRuleItem
{
	public event EventHandler OnRuleParametersChanged;
	public event EventHandler OnDeleteButtonPressed;

	public IPlacementRule GetPlacementRule();
    public void EnableAllOptions();
    public void DisableAllOptions();
}
