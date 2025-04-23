using Godot;
using System;
using System.Collections.Generic;

namespace TerrainGenerationApp.Scenes.BuildingBlocks.InputLine;

public class OptionSelectedEventArgs(string label, int id, int index)
{
	public string Label { get; } = label;
    public int Id { get; } = id;
    public int Index { get; } = index;
}

public class SelectedOption(int id, int index)
{
    public int Id { get; } = id;
    public int Index { get; } = index;
}

public partial class InputLineCombobox : BaseInputLine
{
	private OptionButton _optionButton;

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
        OptionButton.ItemSelected += OnOptionButtonItemSelected;
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

    /// <summary>
    /// Sets the selected option in the OptionButton. If invokeEvent is true, the <see cref="OnOptionChanged"/> event will be triggered.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="invokeEvent"></param>
	public void SetSelected(int index, bool invokeEvent = true)
	{
        OptionButton.Select(index);
        
        if (invokeEvent)
        {
            OnOptionButtonItemSelected(OptionButton.Selected);
        }
    }

    /// <summary>
    /// Sets the selected option in the OptionButton by ID. If invokeEvent is true, the <see cref="OnOptionChanged"/> event will be triggered.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="invokeEvent"></param>
    public void SetSelectedById(int id, bool invokeEvent = true)
    {
        var index = OptionButton.GetItemIndex(id);
        
        if (index > -1)
        {
            OptionButton.Select(index);

            if (invokeEvent)
            {
                OnOptionButtonItemSelected(OptionButton.Selected);
            }
        }
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

    public override object GetValueAsObject()
    {
        if (OptionButton.Selected == -1)
        {
            return null;
        }

        var index = OptionButton.Selected;
        var id = OptionButton.GetItemId(index);
        return new SelectedOption(id, index);
    }

    public override bool TrySetValue(object value, bool invokeEvent = true)
    {
        switch (value)
        {
            case SelectedOption selectOption:
                SetSelectedById(selectOption.Id, invokeEvent);
                return true;
            case int id:
                SetSelectedById(id, invokeEvent);
                return true;
            default:
                return false;
        }
    }

    private void OnOptionButtonItemSelected(long index)
	{
        TrackCurrentValue();
		var idx = (int)index;
        var id = _optionButton.GetItemId(idx);
		var text = _optionButton.GetItemText(idx);
		var eventArgs = new OptionSelectedEventArgs(text, id, idx);
        OnOptionChanged?.Invoke(eventArgs);
	}
}
