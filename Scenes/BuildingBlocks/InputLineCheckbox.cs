using Godot;
using System;

namespace TerrainGenerationApp.Scenes.BuildingBlocks;

public partial class InputLineCheckbox : InputLineBase
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

    public void SetValue(bool value)
    {
        Checkbox.ButtonPressed = value;
    }

    public override void EnableInput()
    {
        Checkbox.Disabled = false;
    }

    public override void DisableInput()
    {
        Checkbox.Disabled = true;
    }

    private void OnCheckboxToggled(bool toggledOn)
    {
        OnValueChanged?.Invoke(toggledOn);
    }
}