using Godot;
using System;
using TerrainGenerationApp.Domain.Enums;
using TerrainGenerationApp.Domain.Visualization;
using TerrainGenerationApp.Scenes.BuildingBlocks.Attributes;
using TerrainGenerationApp.Scenes.BuildingBlocks.Containers;
using TerrainGenerationApp.Scenes.BuildingBlocks.InputLine;

namespace TerrainGenerationApp.Scenes.CoreModules.DisplayOptions;

public partial class TerrainVisualOptions : Control
{
    private TerrainVisualSettings _settings = new();
    private OptionsContainer _optionsContainer;

    private MapDisplayFormat _curDisplayFormat = MapDisplayFormat.Grey;
    private float _curSlopeThreshold = 0.2f;

    public event Action OnDisplayOptionsChanged;

    [InputLine(Description = "Display format:")]
    [InputLineCombobox(selected: 1, bind: ComboboxBind.Id)]
    [InputOption("Grey",            id: (int)MapDisplayFormat.Grey)]
    [InputOption("Colors",          id: (int)MapDisplayFormat.Colors)]
    [InputOption("Gradient colors", id: (int)MapDisplayFormat.GradientColors)]
    public MapDisplayFormat CurDisplayFormat
    {
        get => _curDisplayFormat;
        set
        {
            _curDisplayFormat = value;
            _settings.SetMapDisplayFormat(value);
            OnDisplayOptionsChanged?.Invoke();
        }
    }

    [InputLine(Description = "Slope threshold:")]
    [InputLineSlider(0.0f, 1.0f, 0.01f)]
    public float CurSlopeThreshold
    {
        get => _curSlopeThreshold;
        set
        {
            value = Mathf.Clamp(value, 0.0f, 1.0f);
            _curSlopeThreshold = value;
            _settings.SetSlopeThreshold(value);
            OnDisplayOptionsChanged?.Invoke();
        }
    }

    public override void _Ready()
    {
        _optionsContainer = GetNode<OptionsContainer>("%OptionsContainer");
        InputLineManager.CreateInputLinesForObject(this, _optionsContainer);
    }

    public void EnableAllOptions()
    {
        _optionsContainer.EnableAllOptions();
    }

    public void DisableAllOptions()
    {
        _optionsContainer.DisableAllOptions();
    }

    /// <summary>
    /// Binds the settings to the options container. Note that direct object changes will not be reflected in the UI.
    /// </summary>
    /// <param name="settings"></param>
    public void BindSettings(TerrainVisualSettings settings)
    {
        _settings = settings ?? new TerrainVisualSettings();
        _curDisplayFormat = _settings.MapDisplayFormat;
        _curSlopeThreshold = _settings.SlopeThreshold;
        _optionsContainer.FindInputLine<InputLineSlider>(nameof(CurSlopeThreshold))?.SetValueNoSignal(_curSlopeThreshold);
        _optionsContainer.FindInputLine<InputLineCombobox>(nameof(CurDisplayFormat))?.SetSelectedById((int)_curDisplayFormat);
    }
}