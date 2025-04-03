using Godot;
using System;
using System.Collections.Generic;
using TerrainGenerationApp.Data;
using TerrainGenerationApp.Enums;

namespace TerrainGenerationApp.Scenes.GameComponents.DisplayOptions;

public partial class MapDisplayOptions : Control, IDisplayOptionsProvider
{
    private CheckBox _displayGrey;
    private CheckBox _displayColors;
    private CheckBox _displayGradient;
    private Label _slopeThresholdLabel;
    private Slider _slopeThresholdSlider;

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
    public Dictionary<string, Color> TreeColors { get; set; } = new();
    public Dictionary<string, PackedScene> TreeModels { get; set; } = new();


    public override void _Ready()
    {
        _displayGrey = GetNode<CheckBox>("%DisplayGrey");
        _displayColors = GetNode<CheckBox>("%DisplayColors");
        _displayGradient = GetNode<CheckBox>("%DisplayGradient");
        _slopeThresholdLabel = GetNode<Label>("%SlopeThresholdL");
        _slopeThresholdSlider = GetNode<Slider>("%SlopeThresholdSlider");
        _displayGradient.ButtonPressed = false;
        _displayGrey.ButtonPressed = false;
        _displayColors.ButtonPressed = true;
        _displayGrey.Toggled += (buttonPressed) => CurDisplayFormat = MapDisplayFormat.Grey;
        _displayColors.Toggled += (buttonPressed) => CurDisplayFormat = MapDisplayFormat.Colors;
        _displayGradient.Toggled += (buttonPressed) => CurDisplayFormat = MapDisplayFormat.GradientColors;
        CurDisplayFormat = MapDisplayFormat.Colors;

        // Display features
        _slopeThresholdSlider.Value = CurSlopeThreshold;
        _slopeThresholdLabel.Text = CurSlopeThreshold.ToString();
        _slopeThresholdSlider.ValueChanged += _on_slope_threshold_slider_value_changed;
    }

    private void _on_slope_threshold_slider_value_changed(double value)
    {
        CurSlopeThreshold = (float)value;
        _slopeThresholdLabel.Text = CurSlopeThreshold.ToString();
    }

    private void NotifyDisplayOptionsChanged()
    {
        OnDisplayOptionsChanged?.Invoke();
    }
}