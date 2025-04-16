using System;
using TerrainGenerationApp.Domain.Extensions;
using TerrainGenerationApp.Domain.Visualization;
using TerrainGenerationApp.Scenes.BuildingBlocks.Attributes;
using TerrainGenerationApp.Scenes.BuildingBlocks.Containers;
using TerrainGenerationApp.Scenes.BuildingBlocks.InputLine;

namespace TerrainGenerationApp.Scenes.CoreModules.DisplayOptions;

public partial class TerrainMeshOptions : OptionsContainer
{
    private TerrainMeshSettings _settings = new();
    private int _gridCellResolution = 5;
    private float _gridCellSize = 1.0f;
    private float _heightScale = 15.0f;
    private MapExtensions.InterpolationType _interpolation = MapExtensions.InterpolationType.Bilinear;

    public event Action OnMeshOptionsChanged;

    [InputLine(Description = "Grid cell size:")]
    [InputLineSlider(0.1f, 3.0f, 0.1f, format: "0.##")]
    public float GridCellSize
    {
        get => _gridCellSize;
        set
        {
            _gridCellSize = value;
            _settings.SetGridCellSize(value);
            OnMeshOptionsChanged?.Invoke();
        }
    }

    [InputLine(Description = "Grid cell resolution:")]
    [InputLineSlider(1, 15)]
    public int GridCellResolution
    {
        get => _gridCellResolution;
        set
        {
            _gridCellResolution = value;
            _settings.SetGridCellResolution(value);
            OnMeshOptionsChanged?.Invoke();
        }
    }

    [InputLine(Description = "Height scale:")]
    [InputLineSlider(0.1f, 100.0f, 0.1f, format: "0.#")]
    public float HeightScale
    {
        get => _heightScale;
        set
        {
            _heightScale = value;
            _settings.SetHeightScale(value);
            OnMeshOptionsChanged?.Invoke();
        }
    }

    [InputLine(Description = "Mesh interpolation:")]
    [InputLineCombobox(selected: 1, bind: ComboboxBind.Id)]
    [InputOption("None",        id: (int)MapExtensions.InterpolationType.None)]
    [InputOption("Linear",      id:(int)MapExtensions.InterpolationType.Bilinear)]
    [InputOption("Smooth step", id: (int)MapExtensions.InterpolationType.SmoothStep)]
    public MapExtensions.InterpolationType Interpolation
    {
        get => _interpolation;
        set
        {
            _interpolation = value;
            _settings.SetInterpolation(value);
            OnMeshOptionsChanged?.Invoke();
        }
    }

    public override void _Ready()
    {
        base._Ready();
        InputLineManager.CreateInputLinesForObject(this, this);
    }

    /// <summary>
    /// Binds the settings to the input lines. Note that direct object changes will not be reflected in the UI.
    /// If settings object was changed outside of this class, call <see cref="UpdateUi"/> to refresh the UI.
    /// </summary>
    /// <param name="settings"></param>
    public void BindSettings(TerrainMeshSettings settings)
    {
        _settings = settings ?? new TerrainMeshSettings(); ;
        UpdateUi();
    }

    /// <summary>
    /// Updates the UI elements to reflect the current settings values.
    /// Note that direct settings object changes will not be reflected in the UI. So this method should be called.
    /// </summary>
    public void UpdateUi()
    {
        _gridCellResolution = _settings.GridCellResolution;
        _gridCellSize = _settings.GridCellSize;
        _heightScale = _settings.HeightScale;
        FindInputLine<InputLineSlider>(nameof(HeightScale))?.SetValue(_heightScale, invokeEvent: false);
        FindInputLine<InputLineSlider>(nameof(GridCellSize))?.SetValue(_gridCellSize, invokeEvent: false);
        FindInputLine<InputLineSlider>(nameof(GridCellResolution))?.SetValue(_gridCellResolution, invokeEvent: false);
    }
}