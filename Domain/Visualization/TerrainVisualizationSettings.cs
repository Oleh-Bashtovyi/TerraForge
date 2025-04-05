using Godot;
using System;
using TerrainGenerationApp.Domain.Core;
using TerrainGenerationApp.Domain.Enums;
using TerrainGenerationApp.Domain.Utils;

namespace TerrainGenerationApp.Domain.Visualization;

public class TerrainVisualizationSettings
{
    private readonly Logger<TerrainVisualizationSettings> _logger = new();
    private readonly Gradient _terrainGradient;
    private readonly Gradient _waterGradient;
    private MapDisplayFormat _mapDisplayFormat;
    private float _slopeThreshold;

    public float SlopeThreshold => _slopeThreshold;
    public MapDisplayFormat MapDisplayFormat => _mapDisplayFormat;

    public TerrainVisualizationSettings()
    {
        _terrainGradient = new Gradient();
        _waterGradient = new Gradient();
        _slopeThreshold = 0.1f;
        ClearTerrainGradient();
        ClearWaterGradient();
        SetMapDisplayFormat(MapDisplayFormat.Colors);
    }

    public void SetMapDisplayFormat(MapDisplayFormat mapDisplayFormat)
    {
        _mapDisplayFormat = mapDisplayFormat;

        if (_mapDisplayFormat == MapDisplayFormat.Colors)
        {
            _terrainGradient.InterpolationMode = Gradient.InterpolationModeEnum.Constant;
            _waterGradient.InterpolationMode = Gradient.InterpolationModeEnum.Constant;
        }
        else if (_mapDisplayFormat == MapDisplayFormat.GradientColors)
        {
            _terrainGradient.InterpolationMode = Gradient.InterpolationModeEnum.Linear;
            _waterGradient.InterpolationMode = Gradient.InterpolationModeEnum.Linear;
        }
    }

    public void SetSlopeThreshold(float slopeThreshold)
    {
        _slopeThreshold = slopeThreshold;
    }

    public Color GetColor(Vector2I pos, IWorldData worldData)
    {
        return GetColor(pos.Y, pos.X, worldData);
    }

    public Color GetColor(int row, int col, IWorldData worldData)
    {
        var h = worldData.TerrainData.HeightAt(row, col);

        switch (_mapDisplayFormat)
        {
            case MapDisplayFormat.Grey:
                return new Color(h, h, h);
            case MapDisplayFormat.Colors:
            case MapDisplayFormat.GradientColors:
                if (h < worldData.SeaLevel)
                {
                    return _waterGradient.Sample(worldData.SeaLevel - h);
                }

                var baseColor = _terrainGradient.Sample(h - worldData.SeaLevel);
                return baseColor;
            default:
                throw new NotImplementedException("Can not handle color get operation idk");
        }
    }


    public void ClearTerrainGradient()
    {
        var pointCount = _terrainGradient.GetPointCount();

        for (int i = 0; i < pointCount; i++)
        {
            _terrainGradient.RemovePoint(0);
        }
    }

    public void ClearWaterGradient()
    {
        var pointCount = _waterGradient.GetPointCount();

        for (int i = 0; i < pointCount; i++)
        {
            _waterGradient.RemovePoint(0);
        }
    }

    public void AddTerrainGradientPoint(float offset, Color color)
    {
        _terrainGradient.AddPoint(offset, color);
    }

    public void AddWaterGradientPoint(float offset, Color color)
    {
        _waterGradient.AddPoint(offset, color);
    }


    public void RedrawTerrainImage(Image image, IWorldData worldData)
    {
        var imageSize = image.GetSize();
        var mapSize = worldData.TerrainData.GetMapSize();

        if (mapSize != imageSize)
        {
            _logger.LogError($"Image size {imageSize} does not match map size {mapSize}");
            throw new ArgumentException($"Terrain image size {imageSize} does not match actual map size: {mapSize}");
        }

        var h = worldData.TerrainData.TerrainMapHeight;
        var w = worldData.TerrainData.TerrainMapWidth;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                image.SetPixel(x, y, GetColor(y, x, worldData));
            }
        }
    }




    private Color GetSlopeColor(Color baseColor, float slope, float elevation, float slopeThreshold)
    {
        // Normalize slope value (to fit within 0-1 range)
        float slopeFactor = MathF.Min(slope * slopeThreshold, 1.0f);

        // Darken based on steepness
        float r = baseColor.R * (1.0f - slopeFactor);
        float g = baseColor.G * (1.0f - slopeFactor);
        float b = baseColor.B * (1.0f - slopeFactor);

        // Brightness correction (slightly brighten at higher elevations)
        float brightnessFactor = 1.0f + elevation * 0.2f;
        r = Math.Clamp(r * brightnessFactor, 0, 1.0f);
        g = Math.Clamp(g * brightnessFactor, 0, 1.0f);
        b = Math.Clamp(b * brightnessFactor, 0, 1.0f);

        return new Color(r, g, b);
    }
}