using Godot;
using System;
using TerrainGenerationApp.Rules.PlacementRules;
using TerrainGenerationApp.Utilities;

namespace TerrainGenerationApp.Scenes.GenerationOptions.TreePlacement.PlacementRuleItems;

public partial class BasePlacementRuleItem<TLoggerType> : PanelContainer, IPlacementRuleItem where TLoggerType : class
{
    protected readonly Logger<TLoggerType> _logger = new();
    protected Button _deleteButton;

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
        _logger.Log($"Invoking {nameof(OnRuleParametersChanged)}");
        OnRuleParametersChanged?.Invoke(this, EventArgs.Empty);
    }

    private void DeleteButtonOnPressed()
    {
        _logger.Log($"Invoking {nameof(OnDeleteButtonPressed)}");
        OnDeleteButtonPressed?.Invoke(this, EventArgs.Empty);
    }
}
