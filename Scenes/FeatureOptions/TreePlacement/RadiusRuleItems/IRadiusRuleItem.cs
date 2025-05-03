using System;
using TerrainGenerationApp.Domain.Rules.RadiusRules;
using TerrainGenerationApp.Scenes.BuildingBlocks.Containers;

namespace TerrainGenerationApp.Scenes.FeatureOptions.TreePlacement.RadiusRuleItems;

public interface IRadiusRuleItem : ILastUsedConfigProvider, IOptionsToggleable
{
    public event EventHandler OnRuleParametersChanged;
    public event EventHandler OnDeleteButtonPressed;

    public IRadiusRule GetRadiusRule();
}
