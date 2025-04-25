using Godot;
using System;
using System.Collections.Generic;
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

    protected bool IsLoading { get; set; }

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

    public virtual void EnableOptions()
    {
        OptionsContainer.EnableOptions();
        _deleteButton.Disabled = false;
    }

    public virtual void DisableOptions()
    {
        OptionsContainer.DisableOptions();
        _deleteButton.Disabled = true;
    }

    protected void InvokeRuleParametersChangedEvent()
    {
        if (!IsLoading)
        {
            Logger.Log($"Invoking {nameof(OnRuleParametersChanged)}");
            OnRuleParametersChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void DeleteButtonOnPressed()
    {
        Logger.Log($"Invoking {nameof(OnDeleteButtonPressed)}");
        OnDeleteButtonPressed?.Invoke(this, EventArgs.Empty);
    }

    public virtual Dictionary<string, object> GetLastUsedConfig()
    {
        return _optionsContainer.GetLastUsedConfig();
    }

    public virtual void UpdateCurrentConfigAsLastUsed()
    {
        _optionsContainer.UpdateCurrentConfigAsLastUsed();
    }

    public void LoadConfigFrom(Dictionary<string, object> config)
    {
        try
        {
            IsLoading = true;
            LoadConfigInternal(config);
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected void LoadConfigInternal(Dictionary<string, object> config)
    {
        _optionsContainer.LoadConfigFrom(config);
    }
}