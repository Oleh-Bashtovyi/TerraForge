using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using TerrainGenerationApp.Domain.Rules.PlacementRules;
using TerrainGenerationApp.Domain.Utils;
using TerrainGenerationApp.Scenes.BuildingBlocks.Containers;

namespace TerrainGenerationApp.Scenes.FeatureOptions.TreePlacement.PlacementRuleItems;

public partial class BasePlacementRuleItem<TLoggerType> : PanelContainer, IPlacementRuleItem where TLoggerType : class
{
    private Button _deleteButton;
    private OptionsContainer _optionsContainer;

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

    public virtual IPlacementRule GetPlacementRule()
    {
        throw new NotImplementedException("Placement rule must be implemented in child class");
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
        return _optionsContainer.GetLastUsedConfig().ToDictionary();
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

    protected virtual void LoadConfigInternal(Dictionary<string, object> config)
    {
        _optionsContainer.LoadConfigFrom(config);
    }
}
