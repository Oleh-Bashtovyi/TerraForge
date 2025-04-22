using Godot;
using System;

namespace TerrainGenerationApp.Scenes.BuildingBlocks.InputLine;

public partial class InputLineSlider : BaseInputLine
{
    private LineEdit _inputLineEdit;
    private Slider _slider;
    private string _textFormat = "0.##";
    private string _description = string.Empty;
    private string _tooltip = string.Empty;

    public event Action<float> OnValueChanged;

    private LineEdit LineEdit
    {
        get
        {
            _inputLineEdit ??= GetNode<LineEdit>("%InputLineEdit");
            return _inputLineEdit;
        }
    }

    private Slider Slider
    {
        get
        {
            _slider = GetNode<Slider>("%Slider");
            return _slider;
        }
    }

    public float Value => (float)Slider.Value;

    public override void _Ready()
    {
        Slider.ValueChanged += OnSliderValueChanged;
        LineEdit.TextSubmitted += OnLineEditTextSubmitted;
    }

    private void OnLineEditTextSubmitted(string newText)
    {
        var normalizedText = newText.Trim().Replace(',', '.');

        if (float.TryParse(normalizedText, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var value))
        {
            Slider.Value = value;
        }

        LineEdit.Text = Slider!.Value.ToString(_textFormat);
    }

    public override void EnableInput()
    { 
        LineEdit.Editable = true;
        Slider.Editable = true;
    }

    public override void DisableInput()
    {
        LineEdit.Editable = false;
        Slider.Editable = false;
    }

    private  void OnSliderValueChanged(double value)
    {
        var val = (float)value;
        LineEdit.Text = val.ToString(_textFormat);
        OnValueChanged?.Invoke(val);
    }


    public void SetRange(float minValue, float maxValue)
    {
        Slider.MinValue = minValue;
        Slider.MaxValue = maxValue; 
        LineEdit.Text = Slider.Value.ToString(_textFormat);
    }

    public override void SetFontSize(int size)
    {
        base.SetFontSize(size);
        LineEdit.AddThemeFontSizeOverride("font_size", size);
    }

    public void SetAsInt()
    {
        SetRounded(true);
    }

    public void SetAsFloat()
    {
        SetRounded(false);
    }

    public void SetRounded(bool rounded)
    {
        Slider.Rounded = rounded;
    }

    public void SetStep(float step)
    {
        Slider.Step = step;
    }

    public void SetValue(float value, bool invokeEvent = true)
    {
        if (invokeEvent)
        {
            Slider.Value = value;
            LineEdit.Text = Slider.Value.ToString(_textFormat);
        }
        else
        {
            Slider.SetValueNoSignal(value);
            LineEdit.Text = Slider.Value.ToString(_textFormat);
        }
    }

    public void SetValueNoSignal(float value)
    {
        Slider.SetValueNoSignal(value);
        LineEdit.Text = Slider.Value.ToString(_textFormat);
    }

    public void SetTextFormat(string textFormat)
    {
        _textFormat = textFormat;
        LineEdit.Text = Slider.Value.ToString(_textFormat);
    }

    public override bool TrySetValue(object value, bool invokeEvent = true)
    {
        switch (value)
        {
            case int intValue:
                SetValue(intValue, invokeEvent);
                return true;
            case float floatValue:
                SetValue(floatValue, invokeEvent);
                return true;
            case string stringValue:
                if (float.TryParse(stringValue, out float parsedValue))
                {
                    SetValue(parsedValue, invokeEvent);
                    return true;
                }
                return false;
            default:
                return false;
        }
    }
}