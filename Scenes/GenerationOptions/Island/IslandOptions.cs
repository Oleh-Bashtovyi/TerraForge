using Godot;
using System;
using TerrainGenerationApp.Generators.Islands;

namespace TerrainGenerationApp.Scenes.GenerationOptions.Island;

public partial class IslandOptions : VBoxContainer
{
    public event Action ParametersChanged;

    private Label _radiusLabel;
    private Label _minDistanceFactorLabel;
    private Label _islandsCountLabel;
    private Label _horizontalOffsetLabel;
    private Label _verticalOffsetLabel;
    private Label _seedLabel;
    private Label _mixStrengthLabel;

    private Slider _radiusSlider;
    private Slider _minDistanceFactorSlider;
    private Slider _islandsCountSlider;
    private Slider _horizontalOffsetSlider;
    private Slider _verticalOffsetSlider;
    private Slider _seedSlider;
    private Slider _mixStrengthSlider;

    private OptionButton _islandTypeOptions;
    private OptionButton _distanceFunctionOptions;

    private IslandApplier _islandApplier;

    public IslandApplier IslandApplier
    {
        get => _islandApplier;
    }

    public override void _Ready()
    {
        // Get references to labels
        _radiusLabel = GetNode<Label>("%RadiusLabel");
        _minDistanceFactorLabel = GetNode<Label>("%MinDistanceFactorLabel");
        _islandsCountLabel = GetNode<Label>("%IslandsCountLabel");
        _horizontalOffsetLabel = GetNode<Label>("%HorizontalOffsetLabel");
        _verticalOffsetLabel = GetNode<Label>("%VerticalOffsetLabel");
        _seedLabel = GetNode<Label>("%SeedLabel");
        _mixStrengthLabel = GetNode<Label>("%MixStrengthLabel");

        // Get references to sliders
        _radiusSlider = GetNode<Slider>("%RadiusSlider");
        _minDistanceFactorSlider = GetNode<Slider>("%MinDistanceFactorSlider");
        _islandsCountSlider = GetNode<Slider>("%IslandsCountSlider");
        _horizontalOffsetSlider = GetNode<Slider>("%HorizontalOffsetSlider");
        _verticalOffsetSlider = GetNode<Slider>("%VerticalOffsetSlider");
        _seedSlider = GetNode<Slider>("%SeedSlider");
        _mixStrengthSlider = GetNode<Slider>("%MixStrengthSlider");

        // Get references to option buttons
        _islandTypeOptions = GetNode<OptionButton>("%IslandTypeOptions");
        _distanceFunctionOptions = GetNode<OptionButton>("%DistanceFunctionOptions");

        // Configure island type options
        _islandTypeOptions.Clear();
        _islandTypeOptions.AddItem("Single At Center", (int)IslandApplier.IslandType.SingleAtCenter);
        _islandTypeOptions.AddItem("Single Randomly", (int)IslandApplier.IslandType.SingleRandomly);
        _islandTypeOptions.AddItem("Many", (int)IslandApplier.IslandType.Many);

        // Configure distance function options
        _distanceFunctionOptions.Clear();
        _distanceFunctionOptions.AddItem("Euclidean", (int)IslandApplier.DistanceType.Euclidean);
        _distanceFunctionOptions.AddItem("Euclidean Squared", (int)IslandApplier.DistanceType.EuclideanSquared);
        _distanceFunctionOptions.AddItem("Manhattan", (int)IslandApplier.DistanceType.Manhattan);
        _distanceFunctionOptions.AddItem("Diagonal", (int)IslandApplier.DistanceType.Diagonal);
        _distanceFunctionOptions.AddItem("Hyperboloid", (int)IslandApplier.DistanceType.Hyperboloid);
        _distanceFunctionOptions.AddItem("Blob", (int)IslandApplier.DistanceType.Blob);
        _distanceFunctionOptions.AddItem("Square Bump", (int)IslandApplier.DistanceType.SquareBump);

        // Initialize island applier with default values
        _islandApplier = new IslandApplier
        {
            ApplierType = IslandApplier.IslandType.SingleAtCenter,
            DistanceFunction = IslandApplier.DistanceType.Euclidean,
            RadiusAroundIslands = 100f,
            MinDistanceFactor = 0.2f,
            IslandsCount = 1,
            HorizontalOffsetsToCenter = 0.2f,
            VerticalOffsetsToCenter = 0.2f,
            Seed = 0,
            MixStrength = 0.8f
        };

        // Update labels with initial values
        UpdateLabels();
    }

    public void DisableAllOptions()
    {
        _horizontalOffsetSlider.Editable = false;
        _verticalOffsetSlider.Editable = false;
        _radiusSlider.Editable = false;
        _minDistanceFactorSlider.Editable = false;
        _islandsCountSlider.Editable = false;
        _seedSlider.Editable = false;
        _mixStrengthSlider.Editable = false;
        _distanceFunctionOptions.Disabled = true;
        _islandTypeOptions.Disabled = true;
    }

    public void EnableAllOptions()
    {
        _horizontalOffsetSlider.Editable = true;
        _verticalOffsetSlider.Editable = true;
        _radiusSlider.Editable = true;
        _minDistanceFactorSlider.Editable = true;
        _islandsCountSlider.Editable = true;
        _seedSlider.Editable = true;
        _mixStrengthSlider.Editable = true;
        _distanceFunctionOptions.Disabled = false;
        _islandTypeOptions.Disabled = false;
    }

    private void UpdateLabels()
    {
        _radiusLabel.Text = _islandApplier.RadiusAroundIslands.ToString();
        _minDistanceFactorLabel.Text = _islandApplier.MinDistanceFactor.ToString();
        _islandsCountLabel.Text = _islandApplier.IslandsCount.ToString();
        _horizontalOffsetLabel.Text = _islandApplier.HorizontalOffsetsToCenter.ToString();
        _verticalOffsetLabel.Text = _islandApplier.VerticalOffsetsToCenter.ToString();
        _seedLabel.Text = _islandApplier.Seed.ToString();
        _mixStrengthLabel.Text = _islandApplier.MixStrength.ToString();
    }

    private void _on_radius_slider_value_changed(float value)
    {
        _islandApplier.RadiusAroundIslands = value;
        _radiusLabel.Text = _islandApplier.RadiusAroundIslands.ToString();
        InvokeParametersChangedEvent();
    }

    private void _on_min_distance_factor_slider_value_changed(float value)
    {
        _islandApplier.MinDistanceFactor = value;
        _minDistanceFactorLabel.Text = _islandApplier.MinDistanceFactor.ToString();
        InvokeParametersChangedEvent();
    }

    private void _on_islands_count_slider_value_changed(float value)
    {
        _islandApplier.IslandsCount = Mathf.RoundToInt(value);
        _islandsCountLabel.Text = _islandApplier.IslandsCount.ToString();
        InvokeParametersChangedEvent();
    }

    private void _on_horizontal_offset_slider_value_changed(float value)
    {
        _islandApplier.HorizontalOffsetsToCenter = value;
        _horizontalOffsetLabel.Text = _islandApplier.HorizontalOffsetsToCenter.ToString();
        InvokeParametersChangedEvent();
    }

    private void _on_vertical_offset_slider_value_changed(float value)
    {
        _islandApplier.VerticalOffsetsToCenter = value;
        _verticalOffsetLabel.Text = _islandApplier.VerticalOffsetsToCenter.ToString();
        InvokeParametersChangedEvent();
    }

    private void _on_seed_slider_value_changed(float value)
    {
        _islandApplier.Seed = (ulong)value;
        _seedLabel.Text = _islandApplier.Seed.ToString();
        InvokeParametersChangedEvent();
    }

    private void _on_mix_strength_slider_value_changed(float value)
    {
        _islandApplier.MixStrength = value;
        _mixStrengthLabel.Text = _islandApplier.MixStrength.ToString();
        InvokeParametersChangedEvent();
    }

    private void _on_island_type_options_item_selected(int index)
    {
        var itemId = _islandTypeOptions.GetItemId(index);
        _islandApplier.ApplierType = (IslandApplier.IslandType)itemId;

        // Update UI based on island type
        if (_islandApplier.ApplierType == IslandApplier.IslandType.Many)
        {
            // Enable islands count control
            _islandsCountSlider.Editable = true;
        }
        else
        {
            // Disable islands count control for single island types
            _islandsCountSlider.Editable = false;
        }
        InvokeParametersChangedEvent();
    }

    private void _on_distance_function_options_item_selected(int index)
    {
        var itemId = _distanceFunctionOptions.GetItemId(index);
        _islandApplier.DistanceFunction = (IslandApplier.DistanceType)itemId;
        InvokeParametersChangedEvent();
    }


    protected void InvokeParametersChangedEvent()
    {
        ParametersChanged?.Invoke();
    }
}