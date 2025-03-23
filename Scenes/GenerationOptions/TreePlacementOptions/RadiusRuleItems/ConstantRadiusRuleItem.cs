using Godot;
using System;
using TerrainGenerationApp.RadiusRules;

namespace TerrainGenerationApp.Scenes.GenerationOptions.TreePlacementOptions.RadiusRuleItems;

public partial class ConstantRadiusRuleItem : PanelContainer, IRadiusRuleItem
{
    private LineEdit _radiusLineEdit;
    private Button _deleteButton;

    private float _radius;

    public event EventHandler OnDeleteButtonPressed;
    public event EventHandler OnRuleParametersChanged;

    public IRadiusRule GetRadiusRule()
    {
        return new ConstantRadiusRule(_radius); 
    }


    public override void _Ready()
    {
        _radiusLineEdit = GetNode<LineEdit>("%RadiusLineEdit");
        _deleteButton = GetNode<Button>("%DeleteButton");
        _deleteButton.Pressed += DeleteButtonOnPressed;
        _radius = 5.0f;
        _radiusLineEdit.Text = _radius.ToString();
        _radiusLineEdit.EditingToggled += RadiusLineEditOnEditingToggled;
    }

    private void RadiusLineEditOnEditingToggled(bool toggledOn)
    {
        if (toggledOn == false)
        {
            var text = _radiusLineEdit.Text;
            if (float.TryParse(text, out float result))
            {
                if (!Mathf.IsEqualApprox(result, _radius))
                {
                    _radius = result;
                    _radiusLineEdit.Text = _radius.ToString();
                    GD.Print($"<{nameof(ConstantRadiusRuleItem)}><{nameof(RadiusLineEditOnEditingToggled)}>---> " +
                             $"Radius changed to {_radius}, invoking  {nameof(OnRuleParametersChanged)} event");
                    OnRuleParametersChanged?.Invoke(this, EventArgs.Empty);
                }
            }
            else
            {
                // Restore original value if parsing fails
                _radiusLineEdit.Text = _radius.ToString();
                GD.PrintErr($"<{nameof(ConstantRadiusRuleItem)}><{nameof(RadiusLineEditOnEditingToggled)}>---> " +
                            $"Invalid radius value entered: {text}, reverting to {_radius}");
            }
        }
    }

    private void DeleteButtonOnPressed()
    {
        GD.Print($"<{nameof(ConstantRadiusRuleItem)}><{nameof(DeleteButtonOnPressed)}>---> " +
                 $"Delete button pressed, invoking  {nameof(OnDeleteButtonPressed)} event");
        OnDeleteButtonPressed?.Invoke(this, EventArgs.Empty);
    }
}