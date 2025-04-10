using System;
using Godot;

namespace TerrainGenerationApp.Scenes.BuildingBlocks.InputLine;

public partial class InputLineVector2 : BaseInputLine
{
    private InputLineSlider _xInput;
    private InputLineSlider _yInput;

    public Action<Vector2> OnValueChanged;

    private InputLineSlider XInput
    {
        get
        {
            _xInput = GetNode<InputLineSlider>("%XInput");
            return _xInput;
        }
    }

    private InputLineSlider YInput
    {
        get
        {
            _yInput = GetNode<InputLineSlider>("%YInput");
            return _yInput;
        }
    }

    public override void _Ready()
    {
        XInput.OnValueChanged += OnInputsChanged;
        YInput.OnValueChanged += OnInputsChanged;
    }

    public Vector2 GetValue()
    {
        var x = XInput.Value;
        var y = YInput.Value;
        return new Vector2(x, y);
    }

    private void OnInputsChanged(float value)
    {
        InvokeInputsChanged();
    }

    public void SetValue(Vector2 value)
    {
        XInput.SetValueNoSignal(value.X);
        YInput.SetValueNoSignal(value.Y);
    }

    public override void EnableInput()
    {
        XInput.EnableInput();
        YInput.EnableInput();
    }

    public override void DisableInput()
    {
        XInput.DisableInput();
        YInput.DisableInput();
    }

    public override void SetFontSize(int size)
    {
        base.SetFontSize(size);
        XInput.SetFontSize(size);
        YInput.SetFontSize(size);
    }

    public void ConfigureXInput(string description, float min, float max, float step, bool round, 
        string format = null, string localisation = null)
    {
        XInput.SetDescription(description);
        XInput.SetRange(min, max);
        XInput.SetStep(step);
        XInput.SetRounded(round);
        if (format != null)
        {
            XInput.SetTextFormat(format);
        }
    }

    public void ConfigureYInput(string description, float min, float max, float step, bool round,
        string format = null, string localisation = null)
    {
        YInput.SetDescription(description);
        YInput.SetRange(min, max);
        YInput.SetStep(step);
        YInput.SetRounded(round);
        if (format != null)
        {
            YInput.SetTextFormat(format);
        }
    }

    private void InvokeInputsChanged()
    {
        var vector = GetValue();
        OnValueChanged?.Invoke(vector);
    }
}