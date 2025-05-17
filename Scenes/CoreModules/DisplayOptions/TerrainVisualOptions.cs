using Godot;
using System;
using TerrainGenerationApp.Domain.Enums;
using TerrainGenerationApp.Domain.Visualization;
using TerrainGenerationApp.Scenes.BuildingBlocks.Attributes;
using TerrainGenerationApp.Scenes.BuildingBlocks.Containers;
using TerrainGenerationApp.Scenes.BuildingBlocks.InputLine;

namespace TerrainGenerationApp.Scenes.CoreModules.DisplayOptions;

public partial class TerrainVisualOptions : OptionsContainer
{
	private TerrainVisualSettings _settings = new();

	private MapDisplayFormat _curDisplayFormat = MapDisplayFormat.Grey;
	private float _curSlopeThreshold = 0.2f;
    private float _moistureInfluence = 0.8f;
    private bool _includeMoisture = false;

    public event Action OnDisplayOptionsChanged;

	[InputLine(Description = "Display format:")]
	[InputLineCombobox(selected: 1, bind: ComboboxBind.Id)]
	[InputOption("Grey",            id: (int)MapDisplayFormat.Grey)]
	[InputOption("Colors",          id: (int)MapDisplayFormat.Colors)]
	[InputOption("Gradient colors", id: (int)MapDisplayFormat.GradientColors)]
	public MapDisplayFormat CurDisplayFormat
	{
		get => _curDisplayFormat;
		set
		{
			_curDisplayFormat = value;
			_settings.SetMapDisplayFormat(value);
			OnDisplayOptionsChanged?.Invoke();
		}
	}

	[InputLine(Description = "Slope threshold:")]
	[InputLineSlider(0.0f, 1.0f, 0.01f)]
	public float CurSlopeThreshold
	{
		get => _curSlopeThreshold;
		set
		{
			value = Mathf.Clamp(value, 0.0f, 1.0f);
			_curSlopeThreshold = value;
			_settings.SetSlopeThreshold(value);
			OnDisplayOptionsChanged?.Invoke();
		}
	}

    [InputLine(Description = "Moisture influence:")]
    [InputLineSlider(0.0f, 1.0f, 0.01f)]
    public float MoistureInfluence
    {
        get => _moistureInfluence;
        set
        {
            value = Mathf.Clamp(value, 0.0f, 1.0f);
            _moistureInfluence = value;
            _settings.SetMoistureInfluence(value);
            OnDisplayOptionsChanged?.Invoke();
        }
    }

    [InputLine(Description = "Include moisture:")]
    [InputLineCheckBox(checkboxType: CheckboxType.CheckButton)]
    public bool IncludeMoisture
    {
        get => _includeMoisture;
        set
        {
            _includeMoisture = value;
            _settings.SetIncludeMoisture(value);
            OnDisplayOptionsChanged?.Invoke();
        }
    }

    public override void _Ready()
	{
        base._Ready();
        InputLineManager.CreateInputLinesForObject(this, this);
	}

    /// <summary>
    /// Binds the settings to the options container. Note that direct object changes will not be reflected in the UI.
    /// If settings object was changed outside of this class, call <see cref="UpdateUi"/> to refresh the UI.
    /// </summary>
    /// <param name="settings"></param>
    public void BindSettings(TerrainVisualSettings settings)
	{
		_settings = settings ?? new TerrainVisualSettings();
        UpdateUi();
    }

    /// <summary>
    /// Updates the UI elements to reflect the current settings values.
    /// Note that direct settings object changes will not be reflected in the UI. So this method should be called.
    /// </summary>
    public void UpdateUi()
    {
        _curDisplayFormat = _settings.MapDisplayFormat;
        _curSlopeThreshold = _settings.SlopeThreshold;
        _moistureInfluence = _settings.MoistureInfluence;
        _includeMoisture = _settings.IncludeMoisture;
        FindInputLine<InputLineCheckbox>(nameof(IncludeMoisture))?.SetValue(_includeMoisture, invokeEvent: false);
        FindInputLine<InputLineSlider>(nameof(CurSlopeThreshold))?.SetValue(_curSlopeThreshold, invokeEvent:false);
        FindInputLine<InputLineSlider>(nameof(MoistureInfluence))?.SetValue(_moistureInfluence, invokeEvent: false);
        FindInputLine<InputLineCombobox>(nameof(CurDisplayFormat))?.SetSelectedById((int)_curDisplayFormat);
    }
}
