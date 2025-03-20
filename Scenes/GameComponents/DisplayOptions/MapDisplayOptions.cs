using System;
using Godot;
using TerrainGenerationApp.Enums;

namespace TerrainGenerationApp.Scenes.GameComponents.DisplayOptions;

public partial class MapDisplayOptions : Control
{
    private float _curSlopeThreshold = 0.2f;
    private MapDisplayFormat _curDisplayFormat = MapDisplayFormat.Grey;

    public event Action OnDisplayOptionsChanged;

    // NODES REFERENCED WITH "%" IN SCENE
    private CheckBox _displayGrey;
    private CheckBox _displayColors;
    private CheckBox _displayGradient;
    private Label _slopeThresholdLabel;
    private Slider _slopeThresholdSlider;


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


    public override void _Ready()
    {
        // Display format (Grey, colors, gradient)
        _displayGrey = GetNode<CheckBox>("%DisplayGrey");
        _displayColors = GetNode<CheckBox>("%DisplayColors");
        _displayGradient = GetNode<CheckBox>("%DisplayGradient");
        CurDisplayFormat = MapDisplayFormat.Colors;
        _displayGradient.ButtonPressed = false;
        _displayGrey.ButtonPressed = false;
        _displayColors.ButtonPressed = true;
        _displayGrey.Toggled += (buttonPressed) => CurDisplayFormat = MapDisplayFormat.Grey;
        _displayColors.Toggled += (buttonPressed) => CurDisplayFormat = MapDisplayFormat.Colors;
        _displayGradient.Toggled += (buttonPressed) => CurDisplayFormat = MapDisplayFormat.GradientColors;

        // Display features
        _slopeThresholdLabel = GetNode<Label>("%SlopeThresholdL");
        _slopeThresholdSlider = GetNode<Slider>("%SlopeThresholdSlider");
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