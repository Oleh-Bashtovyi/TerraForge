using System;
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

    public event Action OnMeshOptionsChanged;

    [InputLine(Description = "Grid cell size:")]
    [InputLineSlider(0.1f, 3.0f, 0.1f, format: "0.##")]
    public float GridCellSize
    {
        get => _gridCellSize;
        set
        {
            _gridCellSize = value;
            _settings.GridCellSize = value;
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
            _settings.GridCellResolution = value;
            OnMeshOptionsChanged?.Invoke();
        }
    }

    [InputLine(Description = "Height scale:")]
    [InputLineSlider(0.1f, 30.0f, 0.1f, format: "0.#")]
    public float HeightScale
    {
        get => _heightScale;
        set
        {
            _heightScale = value;
            _settings.HeightScale = value;
            OnMeshOptionsChanged?.Invoke();
        }
    }

    public override void _Ready()
    {
        base._Ready();
        _settings.HeightScale = 15;
        _settings.GridCellSize = 1;
        _settings.GridCellResolution = 5;
        InputLineManager.CreateInputLinesForObject(this, this);
    }

    /// <summary>
    /// Binds the settings to the input lines. Note that direct object changes will not be reflected in the UI.
    /// </summary>
    /// <param name="settings"></param>
    public void BindSettings(TerrainMeshSettings settings)
    {
        _settings = settings ?? new TerrainMeshSettings(); ;
        _gridCellResolution = _settings.GridCellResolution;
        _gridCellSize = _settings.GridCellSize;
        _heightScale = _settings.HeightScale;
        FindInputLine<InputLineSlider>(nameof(HeightScale))?.SetValueNoSignal(_heightScale);
        FindInputLine<InputLineSlider>(nameof(GridCellSize))?.SetValueNoSignal(_gridCellSize);
        FindInputLine<InputLineSlider>(nameof(GridCellResolution))?.SetValueNoSignal(_gridCellResolution);
    }
}