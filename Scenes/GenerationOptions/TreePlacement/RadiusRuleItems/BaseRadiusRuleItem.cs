using Godot;
using System;
using TerrainGenerationApp.Rules.RadiusRules;
using TerrainGenerationApp.Utilities;

namespace TerrainGenerationApp.Scenes.GenerationOptions.TreePlacement.RadiusRuleItems;

public partial class BaseRadiusRuleItem<TLoggerType> : PanelContainer, IRadiusRuleItem where TLoggerType : class
{
    protected readonly Logger<TLoggerType> _logger = new();
    protected Button _deleteButton;

    public event EventHandler OnRuleParametersChanged;
    public event EventHandler OnDeleteButtonPressed;

    public virtual IRadiusRule GetRadiusRule()
    {
        throw new NotImplementedException("Radius rule must be implemented in child class");
    }

    public override void _Ready()
    {
        _deleteButton = GetNode<Button>("%DeleteButton");
        _deleteButton.Pressed += DeleteButtonOnPressed;
    }

    protected void InvokeRuleParametersChangedEvent()
    {
        _logger.Log($"Invoking {nameof(OnRuleParametersChanged)}");
        OnRuleParametersChanged?.Invoke(this, EventArgs.Empty);
    }

    private void DeleteButtonOnPressed()
    {
        _logger.Log($"Invoking {nameof(OnDeleteButtonPressed)}");
        OnDeleteButtonPressed?.Invoke(this, EventArgs.Empty);
    }
}