using Godot;
using System;
using Godot.Collections;
using TerrainGenerationApp.PlacementRules;

namespace TerrainGenerationApp.Scenes.GenerationOptions.TreePlacementOptions.PlacementRuleItems;

public partial class AboveSeaLevelRuleItem : PanelContainer, IPlacementRuleItem
{
	private LineEdit _lowerBoundLineEdit;
	private LineEdit _upperBoundLineEdit;
	private Button _deleteButton;
	private float _lowerBound;
	private float _upperBound;

	public event EventHandler OnDeleteButtonPressed;
	public event EventHandler OnRuleParametersChanged;


	public override void _Ready()
	{
		_lowerBoundLineEdit = GetNode<LineEdit>("%LowerBoundLineEdit");
		_upperBoundLineEdit = GetNode<LineEdit>("%UpperBoundLineEdit");
		_deleteButton = GetNode<Button>("%DeleteButton");
		_deleteButton.Pressed += DeleteButtonOnPressed;
		_lowerBound = 0.1f;
		_upperBound = 0.2f;
		_lowerBoundLineEdit.Text = _lowerBound.ToString();
		_upperBoundLineEdit.Text = _upperBound.ToString();
		_lowerBoundLineEdit.EditingToggled += LowerBoundLineEditOnEditingToggled;
		_upperBoundLineEdit.EditingToggled += UpperBoundLineEditOnEditingToggled;
	}


    public IPlacementRule GetPlacementRule()
    {
        return new AboveSeaLevelRule(_lowerBound, _upperBound);
    }


    private void UpperBoundLineEditOnEditingToggled(bool toggledon)
    {
        if (toggledon == false)
        {
            var text = _upperBoundLineEdit.Text;

            if (float.TryParse(text, out float result))
            {
                if (!Mathf.IsEqualApprox(result, _upperBound))
                {
                    _upperBound = (float)Mathf.Clamp(result, 0.0, 1.0);
                    _upperBoundLineEdit.Text = _upperBound.ToString();
                    OnRuleParametersChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }


	private void LowerBoundLineEditOnEditingToggled(bool toggledon)
	{
		if (toggledon == false)
		{
			var text = _lowerBoundLineEdit.Text;

			if (float.TryParse(text, out float result))
			{
				if (!Mathf.IsEqualApprox(result, _lowerBound))
				{
					_lowerBound = (float)Mathf.Clamp(result, 0.0, 1.0);
					_lowerBoundLineEdit.Text = _lowerBound.ToString();
					OnRuleParametersChanged?.Invoke(this, EventArgs.Empty);
				}
			}
		}
	}

	private void DeleteButtonOnPressed()
	{
		OnDeleteButtonPressed?.Invoke(this, EventArgs.Empty);
	}
}
