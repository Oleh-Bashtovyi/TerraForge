using System;
using TerrainGenerationApp.Domain.Rules.RadiusRules;

namespace TerrainGenerationApp.Scenes.GenerationOptions.TreePlacement.RadiusRuleItems;

public interface IRadiusRuleItem
{
    public event EventHandler OnRuleParametersChanged;
    public event EventHandler OnDeleteButtonPressed;

    public IRadiusRule GetRadiusRule();
    public void EnableAllOptions();
    public void DisableAllOptions();
}
