using Godot;
using System;
using TerrainGenerationApp.Domain.Rules.RadiusRules;
using TerrainGenerationApp.Domain.Utils;

namespace TerrainGenerationApp.Scenes.GenerationOptions.TreePlacement.RadiusRuleItems;

public partial class BaseRadiusRuleItem<TLoggerType> : PanelContainer, IRadiusRuleItem where TLoggerType : class
{
    private Button _deleteButton;
    
    protected readonly Logger<TLoggerType> Logger = new();

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
        Logger.Log($"Invoking {nameof(OnRuleParametersChanged)}");
        OnRuleParametersChanged?.Invoke(this, EventArgs.Empty);
    }

    private void DeleteButtonOnPressed()
    {
        Logger.Log($"Invoking {nameof(OnDeleteButtonPressed)}");
        OnDeleteButtonPressed?.Invoke(this, EventArgs.Empty);
    }
}