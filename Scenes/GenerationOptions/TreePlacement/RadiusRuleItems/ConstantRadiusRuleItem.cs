using Godot;
using TerrainGenerationApp.Rules.RadiusRules;

namespace TerrainGenerationApp.Scenes.GenerationOptions.TreePlacement.RadiusRuleItems;

public partial class ConstantRadiusRuleItem : BaseRadiusRuleItem<ConstantRadiusRuleItem>
{
    private LineEdit _radiusLineEdit;

    private float _radius;

    public override IRadiusRule GetRadiusRule()
    {
        return new ConstantRadiusRule(_radius); 
    }

    public override void _Ready()
    {
        base._Ready();
        _radiusLineEdit = GetNode<LineEdit>("%RadiusLineEdit");
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
                    _logger.Log($"Radius changed to {_radius}");
                    InvokeRuleParametersChangedEvent();
                }
            }
            else
            {
                // Restore original value if parsing fails
                _radiusLineEdit.Text = _radius.ToString();
            }
        }
    }
}