using Godot;
using System;
using TerrainGenerationApp.Domain.Rules.PlacementRules;
using TerrainGenerationApp.Domain.Utils;

namespace TerrainGenerationApp.Scenes.GenerationOptions.TreePlacement.PlacementRuleItems;

public partial class BasePlacementRuleItem<TLoggerType> : PanelContainer, IPlacementRuleItem where TLoggerType : class
{
    private Button _deleteButton;

    protected readonly Logger<TLoggerType> Logger = new();

    public event EventHandler OnRuleParametersChanged;
    public event EventHandler OnDeleteButtonPressed;

    public override void _Ready()
    {
        _deleteButton = GetNode<Button>("%DeleteButton");
        _deleteButton.Pressed += DeleteButtonOnPressed;
    }

    public virtual IPlacementRule GetPlacementRule()
    {
        throw new NotImplementedException("Placement rule must be implemented in child class");
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
