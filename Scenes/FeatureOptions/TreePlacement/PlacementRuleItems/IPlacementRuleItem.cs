using System;
using TerrainGenerationApp.Domain.Rules.PlacementRules;
using TerrainGenerationApp.Scenes.BuildingBlocks.Containers;

namespace TerrainGenerationApp.Scenes.FeatureOptions.TreePlacement.PlacementRuleItems;

public interface IPlacementRuleItem : ILastUsedConfigProvider, IOptionsToggleable
{
	public event EventHandler OnRuleParametersChanged;
	public event EventHandler OnDeleteButtonPressed;

	public IPlacementRule GetPlacementRule();
}
