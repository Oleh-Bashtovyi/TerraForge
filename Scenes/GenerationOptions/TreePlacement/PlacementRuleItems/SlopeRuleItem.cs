using Godot;
using TerrainGenerationApp.Rules.PlacementRules;

namespace TerrainGenerationApp.Scenes.GenerationOptions.TreePlacement.PlacementRuleItems;

public partial class SlopeRuleItem : BasePlacementRuleItem<SlopeRuleItem>
{
    private LineEdit _lowerBoundLineEdit;
    private LineEdit _upperBoundLineEdit;

    private float _lowerBound;
    private float _upperBound;

    public override void _Ready()
    {
        base._Ready();
        _lowerBoundLineEdit = GetNode<LineEdit>("%LowerBoundLineEdit");
        _upperBoundLineEdit = GetNode<LineEdit>("%UpperBoundLineEdit");
        _lowerBound = 0.1f;
        _upperBound = 0.2f;
        _lowerBoundLineEdit.Text = _lowerBound.ToString();
        _upperBoundLineEdit.Text = _upperBound.ToString();
        _lowerBoundLineEdit.EditingToggled += LowerBoundLineEditOnEditingToggled;
        _upperBoundLineEdit.EditingToggled += UpperBoundLineEditOnEditingToggled;
    }

    public override IPlacementRule GetPlacementRule()
    {
        return new SlopeRule(_lowerBound, _upperBound);
    }

    private void UpperBoundLineEditOnEditingToggled(bool toggledOn)
    {
        if (toggledOn == false)
        {
            var text = _upperBoundLineEdit.Text;

            if (float.TryParse(text, out float result))
            {
                if (!Mathf.IsEqualApprox(result, _upperBound))
                {
                    _upperBound = (float)Mathf.Clamp(result, _lowerBound, 1.0);
                    _upperBoundLineEdit.Text = _upperBound.ToString();
                    _logger.Log($"Upper bound changed to: {_upperBound}");
                    InvokeRuleParametersChangedEvent();
                }
            }
            else
            {
                // Restore original value if parsing fails
                _upperBoundLineEdit.Text = _upperBound.ToString();
            }
        }
    }
    private void LowerBoundLineEditOnEditingToggled(bool toggledOn)
    {
        if (toggledOn == false)
        {
            var text = _lowerBoundLineEdit.Text;
            if (float.TryParse(text, out float result))
            {
                if (!Mathf.IsEqualApprox(result, _lowerBound))
                {
                    _lowerBound = (float)Mathf.Clamp(result, 0.0, _upperBound);
                    _lowerBoundLineEdit.Text = _lowerBound.ToString();
                    _logger.Log($"Lower bound changed to: {_lowerBound}");
                    InvokeRuleParametersChangedEvent();
                }
            }
            else
            {
                // Restore original value if parsing fails
                _lowerBoundLineEdit.Text = _lowerBound.ToString();
            }
        }
    }
}