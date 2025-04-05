using System;
using Godot;

namespace TerrainGenerationApp.Scenes.BuildingBlocks;

public partial class InputLine : HBoxContainer
{
    private Label _descriptionLabel;
    private LineEdit _inputLineEdit;
    private Slider _slider;
    private string _textFormat = "0.##";
    private string _description = string.Empty;
    private string _tooltip = string.Empty;

    public event Action<float> OnValueChanged;

    private Label GetDescriptionLabel()
    {
        GD.Print("Children count: " + GetChildCount());
        _descriptionLabel ??= GetNode<Label>("%DescriptionLabel");
        return _descriptionLabel;
    }

    private LineEdit GetInputLineEdit()
    {
        _inputLineEdit ??= GetNode<LineEdit>("%InputLineEdit");
        return _inputLineEdit;
    }

    private Slider GetSlider()
    {
        _slider = GetNode<Slider>("%Slider");
        return _slider;
    }

    public override void _Ready()
    {
        var slider = GetSlider();

        if (slider != null)
        {
            slider.ValueChanged += OnSliderValueChanged;
        }
    }

    public void EnableInput()
    {
        var inputLineEdit = GetInputLineEdit();
        var slider = GetSlider();

        if (inputLineEdit != null) inputLineEdit.Editable = true;
        if (slider != null) slider.Editable = true;
    }

    public void DisableInput()
    {
        var inputLineEdit = GetInputLineEdit();
        var slider = GetSlider();

        if (inputLineEdit != null) inputLineEdit.Editable = false;
        if (slider != null) slider.Editable = false;
    }

    private void OnSliderValueChanged(double value)
    {
        var val = (float)value;
        var inputLineEdit = GetInputLineEdit();
        if (inputLineEdit != null)
        {
            inputLineEdit.Text = val.ToString(_textFormat);
            OnValueChanged?.Invoke(val);
        }
    }

    public void SetDescription(string description)
    {
        var descriptionLabel = GetDescriptionLabel();
        descriptionLabel.Text = description;
    }

    public void SetRange(float minValue, float maxValue)
    {
        var slider = GetSlider();
        var inputLineEdit = GetInputLineEdit();

        if (slider != null)
        {
            slider.MinValue = minValue;
            slider.MaxValue = maxValue;

            if (inputLineEdit != null)
            {
                inputLineEdit.Text = slider.Value.ToString(_textFormat);
            }
        }
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
        var slider = GetSlider();
        if (slider != null)
        {
            slider.Rounded = rounded;
        }
    }

    public void SetStep(float step)
    {
        var slider = GetSlider();
        if (slider != null)
        {
            slider.Step = step;
        }
    }

    public void SetValue(float value)
    {
        var slider = GetSlider();
        var inputLineEdit = GetInputLineEdit();

        if (slider != null)
        {
            slider.Value = value;

            if (inputLineEdit != null)
            {
                inputLineEdit.Text = value.ToString(_textFormat);
            }
        }
    }

    public void SetTextFormat(string textFormat)
    {
        _textFormat = textFormat;

        var slider = GetSlider();
        var inputLineEdit = GetInputLineEdit();

        if (slider != null && inputLineEdit != null)
        {
            inputLineEdit.Text = slider.Value.ToString(_textFormat);
        }
    }
}
