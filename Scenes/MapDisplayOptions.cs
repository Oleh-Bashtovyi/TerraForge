using Godot;
using System;
using TerrainGenerationApp.Scenes;

public partial class MapDisplayOptions : Control
{
    private MapDisplayFormat _curDisplayFormat = MapDisplayFormat.Grey;
    private float _curSlopeThreshold = 0.2f;
    private float _curWaterLevel = 0.3f;

    // NODES REFERENCED WITH "%" IN SCENE
    // Display format (Grey, colors, gradient)
    private CheckBox _displayGrey;
    private CheckBox _displayColors;
    private CheckBox _displayGradient;
    // Display features
    private Label _waterLevelLabel;
    private Label _slopeThresholdLabel;
    private Slider _waterLevelSlider;
    private Slider _slopeThresholdSlider;


    public event Action OnDisplayOptionsChanged;

    // PROPERTIES
    //========================================================================
    public MapDisplayFormat CurDisplayFormat
    {
        get => _curDisplayFormat;
        set
        {
            _curDisplayFormat = value;
            NotifyDisplayOptionsChanged();
        }
    }

    public float CurWaterLevel
    {
        get => _curWaterLevel;
        set
        {
            value = Mathf.Clamp(value, 0.0f, 1.0f);
            _curWaterLevel = value;
            NotifyDisplayOptionsChanged();
        }
    }

    public float CurSlopeThreshold
    {
        get => _curSlopeThreshold;
        set
        {
            value = Mathf.Clamp(value, 0.0f, 1.0f);
            _curSlopeThreshold = value;
            NotifyDisplayOptionsChanged();
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
        _waterLevelLabel = GetNode<Label>("%WaterLevelLabel");
        _slopeThresholdLabel = GetNode<Label>("%SlopeThresholdL");
        _waterLevelSlider = GetNode<Slider>("%WaterLevelSlider");
        _slopeThresholdSlider = GetNode<Slider>("%SlopeThresholdSlider");
        _waterLevelSlider.Value = CurWaterLevel;
        _slopeThresholdSlider.Value = CurSlopeThreshold;
        _waterLevelLabel.Text = CurWaterLevel.ToString();
        _slopeThresholdLabel.Text = CurSlopeThreshold.ToString();
        _waterLevelSlider.ValueChanged += _on_water_level_slider_value_changed;
        _slopeThresholdSlider.ValueChanged += _on_slope_threshold_slider_value_changed;
    }





    private void _on_slope_threshold_slider_value_changed(double value)
    {
        CurSlopeThreshold = (float)value;
        _slopeThresholdLabel.Text = CurSlopeThreshold.ToString();
    }

    private void _on_water_level_slider_value_changed(double value)
    {
        CurWaterLevel = (float)value;
        _waterLevelLabel.Text = CurWaterLevel.ToString();
    }

    private void NotifyDisplayOptionsChanged()
    {
        OnDisplayOptionsChanged?.Invoke();
    }
}
