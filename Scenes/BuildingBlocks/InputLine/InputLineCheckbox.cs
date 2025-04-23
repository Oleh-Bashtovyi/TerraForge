#nullable enable
using Godot;
using System;

namespace TerrainGenerationApp.Scenes.BuildingBlocks.InputLine;

public partial class InputLineCheckbox : BaseInputLine
{
    private CheckButton _checkbox;

    public event Action<bool> OnValueChanged;

    public CheckButton Checkbox
    {
        get
        {
            _checkbox ??= GetNode<CheckButton>("%Checkbox");
            return _checkbox;
        }
    }

    public override void _Ready()
    {
        Checkbox.Toggled += OnCheckboxToggled;
    }

    public void SetValue(bool value, bool invokeEvent = true)
    {
        Checkbox.SetPressedNoSignal(value);
        TrackCurrentValue();

        if (invokeEvent)
        {
            OnValueChanged?.Invoke(value);
        }
    }

    public override void EnableInput()
    {
        Checkbox.Disabled = false;
    }

    public override void DisableInput()
    {
        Checkbox.Disabled = true;
    }

    public override object GetValueAsObject()
    {
        return Checkbox.ButtonPressed;
    }

    public override bool TrySetValue(object? value, bool invokeEvent = true)
    {
        switch (value)
        {
            case bool boolValue:
                SetValue(boolValue, invokeEvent);
                return true;
            case int intValue:
                SetValue(intValue != 0, invokeEvent);
                return true;
            case float floatValue:
                SetValue(floatValue != 0, invokeEvent);
                return true;
            case string stringValue:
                if (bool.TryParse(stringValue, out var paresResult))
                {
                    SetValue(paresResult, invokeEvent);
                    return true;
                }
                return false;
            default:
                return false;
        }
    }

    private void OnCheckboxToggled(bool toggledOn)
    {
        TrackCurrentValue();
        OnValueChanged?.Invoke(toggledOn);
    }
}