using System;
using Godot;

namespace TerrainGenerationApp.Scenes.BuildingBlocks;

public partial class InputLineCombobox : InputLineBase
{
	private OptionButton _optionButton;
	private Label _descriptionLabel;

	public event Action<int> OnOptionIdChanged;

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

	public void AddOption(string option)
	{
        OptionButton.AddItem(option);
	}

	public void AddOption(string option, int id)
	{
		GD.Print($"Adding: {option} Id: {id}");
        OptionButton.AddItem(option, id);
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
		var id = _optionButton.GetItemId((int)index);
		OnOptionIdChanged?.Invoke(id);
	}
}
