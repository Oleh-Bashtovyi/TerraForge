using System;
using TerrainGenerationApp.RadiusRules;

namespace TerrainGenerationApp.Scenes.GenerationOptions.TreePlacementOptions.RadiusRuleItems;

public interface IRadiusRuleItem
{
    public event EventHandler OnRuleParametersChanged;
    public event EventHandler OnDeleteButtonPressed;

    public IRadiusRule GetRadiusRule();
}
