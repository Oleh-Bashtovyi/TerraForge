using Godot;
using System;
using TerrainGenerationApp.Domain.Rules.PlacementRules;
using TerrainGenerationApp.Domain.Utils;

namespace TerrainGenerationApp.Scenes.FeatureOptions.TreePlacement.PlacementRuleItems;

public partial class BasePlacementRuleItem<TLoggerType> : PanelContainer, IPlacementRuleItem where TLoggerType : class
{
    private Button _deleteButton;
    private BuildingBlocks.OptionsContainer _optionsContainer;

    protected readonly Logger<TLoggerType> Logger = new();

    public event EventHandler OnRuleParametersChanged;
    public event EventHandler OnDeleteButtonPressed;

    protected BuildingBlocks.OptionsContainer OptionsContainer
    {
        get
        {
            _optionsContainer ??= GetNode<BuildingBlocks.OptionsContainer>("%OptionsContainer");
            return _optionsContainer;
        }
    }

    public override void _Ready()
    {
        _deleteButton = GetNode<Button>("%DeleteButton");
        _deleteButton.Pressed += DeleteButtonOnPressed;
    }

    public virtual IPlacementRule GetPlacementRule()
    {
        throw new NotImplementedException("Placement rule must be implemented in child class");
    }

    public virtual void EnableAllOptions()
    {
        OptionsContainer.EnableAllOptions();
        _deleteButton.Disabled = false;
    }

    public virtual void DisableAllOptions()
    {
        OptionsContainer.DisableAllOptions();
        _deleteButton.Disabled = true;
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
