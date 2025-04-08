using Godot;
using System;
using TerrainGenerationApp.Domain.Rules.RadiusRules;
using TerrainGenerationApp.Domain.Utils;
using TerrainGenerationApp.Scenes.BuildingBlocks.Containers;

namespace TerrainGenerationApp.Scenes.FeatureOptions.TreePlacement.RadiusRuleItems;

public partial class BaseRadiusRuleItem<TLoggerType> : PanelContainer, IRadiusRuleItem where TLoggerType : class
{
    private OptionsContainer _optionsContainer;
    private Button _deleteButton;

    protected readonly Logger<TLoggerType> Logger = new();

    public event EventHandler OnRuleParametersChanged;
    public event EventHandler OnDeleteButtonPressed;

    protected OptionsContainer OptionsContainer
    {
        get
        {
            _optionsContainer ??= GetNode<OptionsContainer>("%OptionsContainer");
            return _optionsContainer;
        }
    }

    public override void _Ready()
    {
        _deleteButton = GetNode<Button>("%DeleteButton");
        _deleteButton.Pressed += DeleteButtonOnPressed;
    }

    public virtual IRadiusRule GetRadiusRule()
    {
        throw new NotImplementedException("Radius rule must be implemented in child class");
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