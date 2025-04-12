using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

namespace TerrainGenerationApp.Scenes.BuildingBlocks.InputLine;

public class OptionSelectedEventArgs(string label, int id, int index)
{
	public string Label { get; } = label;
    public int Id { get; } = id;
    public int Index { get; } = index;
}

public partial class InputLineCombobox : BaseInputLine
{
	private OptionButton _optionButton;
	private bool _isOptionButtonConnected = false;

    public event Action<OptionSelectedEventArgs> OnOptionChanged;

	public OptionButton OptionButton
	{
		get
		{
			_optionButton ??= GetNode<OptionButton>("%OptionButtonInput");
			return _optionButton;
		}
	}

	public override void _Ready()
	{
        if (!_isOptionButtonConnected)
        {
            OptionButton.ItemSelected += OnOptionButtonItemSelected;
            _isOptionButtonConnected = true;
        }
	}

	public void ClearOptions()
	{
        OptionButton.Clear();
	}

    public int GetOptionsCount()
    {
		return OptionButton.GetItemCount();
    }

	public void AddOption(string label, int id = -1)
	{
        OptionButton.AddItem(label, id);
	}

    /// <summary>
    /// Adds a list of options to the OptionButton. Int - ID, String - Label
    /// </summary>
    /// <param name="dict"></param>
    public void AddOptions(IDictionary<int, string> dict)
    {
        foreach (var pair in dict)
        {
            OptionButton.AddItem(pair.Value, pair.Key);
        }
    }

	public void SetSelected(int index)
	{
		if (!_isOptionButtonConnected)
        {
            _optionButton.ItemSelected += OnOptionButtonItemSelected;
            _isOptionButtonConnected = true;
        }
        OptionButton.Select(index);
        OnOptionButtonItemSelected(OptionButton.Selected);

    }

	public override void EnableInput()
	{
		OptionButton.Disabled = false;
	}

	public override void DisableInput()
	{
		OptionButton.Disabled = true;
	}

    public override void SetFontSize(int size)
    {
        base.SetFontSize(size);
        OptionButton.AddThemeFontSizeOverride("font_size", size);
    }

    private void OnOptionButtonItemSelected(long index)
	{
		var idx = (int)index;
        var id = _optionButton.GetItemId(idx);
		var text = _optionButton.GetItemText(idx);
		var eventArgs = new OptionSelectedEventArgs(text, id, idx);
        OnOptionChanged?.Invoke(eventArgs);
	}
}
