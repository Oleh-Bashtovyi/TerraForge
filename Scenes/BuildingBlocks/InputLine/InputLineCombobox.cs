using Godot;
using System;

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
		_optionButton = GetNode<OptionButton>("%OptionButtonInput");
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

	public void SetSelected(int index)
	{
        OptionButton.Select(index);
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
