using System;
using Godot;
using TerrainGenerationApp.Domain.Enums;
using TerrainGenerationApp.Scenes.BuildingBlocks;
using TerrainGenerationApp.Scenes.BuildingBlocks.Attributes;
using TerrainGenerationApp.Scenes.BuildingBlocks.InputLine;
using OptionsContainer = TerrainGenerationApp.Scenes.BuildingBlocks.Containers.OptionsContainer;

namespace TerrainGenerationApp.Scenes.CoreModules.DisplayOptions;

public partial class TerrainVisualizationOptions : Control
{
    private CheckBox _displayGrey;
    private CheckBox _displayColors;
    private CheckBox _displayGradient;
    private OptionsContainer _optionsContainer;

    private MapDisplayFormat _curDisplayFormat = MapDisplayFormat.Grey;
    private float _curSlopeThreshold = 0.2f;

    public event Action OnDisplayOptionsChanged;

    public MapDisplayFormat CurDisplayFormat
    {
        get => _curDisplayFormat;
        set
        {
            _curDisplayFormat = value;
            OnDisplayOptionsChanged?.Invoke();
        }
    }

    [InputLine(Description = "Slope threshold")]
    [InputLineSlider(0.0f, 1.0f, 0.01f)]
    public float CurSlopeThreshold
    {
        get => _curSlopeThreshold;
        set
        {
            value = Mathf.Clamp(value, 0.0f, 1.0f);
            _curSlopeThreshold = value;
            OnDisplayOptionsChanged?.Invoke();
        }
    }

    public override void _Ready()
    {
        _displayGrey = GetNode<CheckBox>("%DisplayGrey");
        _displayColors = GetNode<CheckBox>("%DisplayColors");
        _displayGradient = GetNode<CheckBox>("%DisplayGradient");
        _optionsContainer = GetNode<OptionsContainer>("%OptionsContainer");
        InputLineManager.CreateInputLinesForObject(this, _optionsContainer);

        _displayGradient.ButtonPressed = false;
        _displayGrey.ButtonPressed = false;
        _displayColors.ButtonPressed = true;
        _displayGrey.Toggled += (_) => CurDisplayFormat = MapDisplayFormat.Grey;
        _displayColors.Toggled += (_) => CurDisplayFormat = MapDisplayFormat.Colors;
        _displayGradient.Toggled += (_) => CurDisplayFormat = MapDisplayFormat.GradientColors;
        CurDisplayFormat = MapDisplayFormat.Colors;
    }

    public void EnableAllOptions()
    {
        _optionsContainer.EnableAllOptions();
        _displayGrey.Disabled = false;
        _displayColors.Disabled = false;
        _displayGradient.Disabled = false;
    }

    public void DisableAllOptions()
    {
        _optionsContainer.DisableAllOptions();
        _displayGrey.Disabled = true;
        _displayColors.Disabled = true;
        _displayGradient.Disabled = true;
    }
}