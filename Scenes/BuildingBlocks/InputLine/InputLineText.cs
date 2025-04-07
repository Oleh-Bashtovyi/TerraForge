using Godot;
using System;

namespace TerrainGenerationApp.Scenes.BuildingBlocks.InputLine;

public partial class InputLineText : BaseInputLine
{
    private LineEdit _inputLineEdit;

    public event Action<string> OnTextChanged;

    public LineEdit InputLineEdit
    {
        get
        {
            _inputLineEdit ??= GetNode<LineEdit>("%InputLineEdit");
            return _inputLineEdit;
        }
    }

    public override void _Ready()
    {
        var lineEdit = InputLineEdit;
        lineEdit.EditingToggled += LineEditOnEditingToggled;
    }

    public void SetText(string text)
    {
        InputLineEdit.Text = text;
        OnTextChanged?.Invoke(InputLineEdit.Text);
    }

    public override void EnableInput()
    {
        InputLineEdit.Editable = true;
    }

    public override void DisableInput()
    {
        InputLineEdit.Editable = false;
    }

    public override void SetFontSize(int size)
    {
        base.SetFontSize(size);
        InputLineEdit.AddThemeFontSizeOverride("font_size", size);
    }

    private void LineEditOnEditingToggled(bool toggledOn)
    {
        if (toggledOn)
        {
            return;
        }

        OnTextChanged?.Invoke(InputLineEdit.Text);
    }
}