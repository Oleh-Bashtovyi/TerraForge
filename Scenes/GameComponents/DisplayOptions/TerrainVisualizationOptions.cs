using Godot;
using System;
using TerrainGenerationApp.Domain.Enums;
using TerrainGenerationApp.Scenes.BuildingBlocks;
using TerrainGenerationApp.Scenes.BuildingBlocks.Attributes;

namespace TerrainGenerationApp.Scenes.GameComponents.DisplayOptions;

public partial class TerrainVisualizationOptions : Control
{
    private CheckBox _displayGrey;
    private CheckBox _displayColors;
    private CheckBox _displayGradient;
    private VBoxContainer _optionsContainer;

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

    [LineInputValue(Description = "Slope threshold")]
    [InputRange(0.0f, 1.0f)]
    [Step(0.01f)]
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
        _optionsContainer = GetNode<VBoxContainer>("%OptionsContainer");
        InputLineManager.CreateInputLinesForObject(this, _optionsContainer);

        _displayGradient.ButtonPressed = false;
        _displayGrey.ButtonPressed = false;
        _displayColors.ButtonPressed = true;
        _displayGrey.Toggled += (_) => CurDisplayFormat = MapDisplayFormat.Grey;
        _displayColors.Toggled += (_) => CurDisplayFormat = MapDisplayFormat.Colors;
        _displayGradient.Toggled += (_) => CurDisplayFormat = MapDisplayFormat.GradientColors;
        CurDisplayFormat = MapDisplayFormat.Colors;
    }
}