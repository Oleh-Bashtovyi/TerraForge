using System;
using TerrainGenerationApp.Domain.Enums;
using TerrainGenerationApp.Domain.Visualization;

namespace TerrainGenerationApp.Scenes.ViewModels;

public class WorldVisualSettingsViewModel(IWorldVisualSettings worldVisualSettings)
{
    private readonly IWorldVisualSettings _worldVisualSettings = worldVisualSettings;

    public float SlopeThreshold => _worldVisualSettings.TerrainSettings.SlopeThreshold;
    public MapDisplayFormat MapDisplayFormat => _worldVisualSettings.TerrainSettings.MapDisplayFormat;

    public event Action SlopeThresholdChanged;
    public event Action MapDisplayFormatChanged;

    public void SetSlopeThreshold(float slopeThreshold)
    {
        _worldVisualSettings.TerrainSettings.SetSlopeThreshold(slopeThreshold);
        SlopeThresholdChanged?.Invoke();
    }

    public void SetMapDisplayFormat(MapDisplayFormat mapDisplayFormat)
    {
        _worldVisualSettings.TerrainSettings.SetMapDisplayFormat(mapDisplayFormat);
        MapDisplayFormatChanged?.Invoke();
    }
}